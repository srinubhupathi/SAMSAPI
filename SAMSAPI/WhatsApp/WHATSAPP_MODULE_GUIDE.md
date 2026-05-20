# WhatsApp Module — SAMS Club Management System
## Integration with md.enotify.app

---

## Table of Contents
1. [Module Overview](#1-module-overview)
2. [How to Get Keys from md.enotify.app](#2-how-to-get-keys-from-mdenotifyapp)
3. [Configuration (Web.config)](#3-configuration-webconfig)
4. [File Structure](#4-file-structure)
5. [API Endpoints Reference](#5-api-endpoints-reference)
6. [Angular Integration Examples](#6-angular-integration-examples)
7. [Message Templates — Customization](#7-message-templates--customization)
8. [Auto-Trigger from Backend](#8-auto-trigger-from-backend)
9. [Message Preview (English + Telugu)](#9-message-preview-english--telugu)
10. [Troubleshooting](#10-troubleshooting)

---

## 1. Module Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                    WhatsApp Module Architecture                      │
├─────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  Angular Frontend                                                     │
│       │                                                               │
│       │  HTTP POST  (JSON)                                            │
│       ▼                                                               │
│  WhatsAppController.cs   ←──── SAMSManager.cs (auto-trigger)         │
│       │                                                               │
│       ▼                                                               │
│  WhatsAppService.cs                                                   │
│       │  reads templates from ↓                                       │
│       │  Content/Config/whatsapp-templates.json                       │
│       │                                                               │
│       │  POST JSON                                                    │
│       ▼                                                               │
│  https://md.enotify.app/api/send-message                             │
│       │                                                               │
│       ▼                                                               │
│  Member's WhatsApp                                                    │
└─────────────────────────────────────────────────────────────────────┘
```

**Key features:**
- Sends messages in **English + Telugu** (simultaneously in one message)
- Templates are stored in a **JSON config file** — change text without redeploying
- Supports: Membership Fee, Sports Fee, Entrance Fee, Payment Reminders
- **Bulk reminders** — auto-fetch all due members and send in batch
- Fire-and-forget async sending (doesn't block the HTTP response)
- Automatic retry on failure (configurable)

---

## 2. How to Get Keys from md.enotify.app

### Step-by-Step Guide

### Step 1 — Register / Login
1. Go to **https://md.enotify.app**
2. Register with your WhatsApp number + password
3. OR login if you already have an account

---

### Step 2 — Connect Your WhatsApp Device

1. After login, click **"Devices"** or **"Add Device"** in the dashboard
2. You'll see a **QR code**
3. On your WhatsApp phone:
   - Open WhatsApp → Tap the 3-dot menu (⋮)
   - Select **"Linked Devices"**
   - Tap **"Link a Device"**
   - Scan the QR code shown on md.enotify.app
4. Your device will appear as **Connected / Active**

---

### Step 3 — Get the Instance ID

1. In the dashboard, go to **Devices**
2. Find your connected device
3. Copy the **Instance ID** (format: `instance-XXXXXXXX` or a UUID)
4. This is your `WhatsApp_InstanceId`

```
Example: 6BC34D2A-1234-5678-ABCD-9EF012345678
```

---

### Step 4 — Get the Access Token

1. In the dashboard, go to **API Settings** or **Profile → API**
2. Click **"Generate Token"** or **"Create Access Token"**
3. Copy the **Access Token** (a long alphanumeric string)
4. This is your `WhatsApp_AccessToken`

```
Example: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

> ⚠️ **Security Note:** Never commit the Access Token to Git.  
> Use environment variables or encrypted config in production.

---

### Step 5 — Verify API Endpoint

The send-message API endpoint is:
```
POST  https://md.enotify.app/api/send-message
```

Request JSON body:
```json
{
  "instance_id"  : "YOUR_INSTANCE_ID",
  "access_token" : "YOUR_ACCESS_TOKEN",
  "number"       : "919246668725",
  "message"      : "Hello from SAMS!"
}
```

Response on success:
```json
{
  "status"  : true,
  "message" : "Message sent successfully",
  "data"    : "..."
}
```

Response on failure:
```json
{
  "status"  : false,
  "error"   : "Invalid instance ID or token"
}
```

---

### Keys Summary Table

| Key | Where to Find | Example |
|-----|--------------|---------|
| `WhatsApp_InstanceId` | Devices page → copy Instance ID | `6BC34D2A-1234-5678-...` |
| `WhatsApp_AccessToken` | API Settings → Generate Token | `eyJhbGciO...` |
| `WhatsApp_ApiBaseUrl` | Fixed value | `https://md.enotify.app` |
| `WhatsApp_CountryCode` | India = 91 | `91` |

---

## 3. Configuration (Web.config)

Add these keys to the `<appSettings>` section of `Web.config`:

```xml
<!-- WhatsApp Module — md.enotify.app -->
<add key="WhatsApp_Enabled"       value="true"/>
<add key="WhatsApp_ApiBaseUrl"    value="https://md.enotify.app"/>
<add key="WhatsApp_InstanceId"    value="YOUR_INSTANCE_ID_HERE"/>
<add key="WhatsApp_AccessToken"   value="YOUR_ACCESS_TOKEN_HERE"/>
<add key="WhatsApp_CountryCode"   value="91"/>
<add key="WhatsApp_ClubName"      value="Cosmopolitan Club"/>
<add key="WhatsApp_TemplateFile"  value=""/>
```

> Set `WhatsApp_Enabled` to `false` to disable all WhatsApp sending without removing code.

---

## 4. File Structure

```
SAMSAPI/
├── Controllers/
│   └── WhatsAppController.cs          ← REST API endpoints
│
├── WhatsApp/
│   ├── Models/
│   │   └── WhatsAppModels.cs          ← DTOs (request/response)
│   └── Service/
│       ├── WhatsAppService.cs         ← Core service (calls enotify API)
│       └── WhatsAppSAMSIntegration.cs ← Auto-trigger helpers
│
├── Content/Config/
│   └── whatsapp-templates.json        ← All message templates (configurable)
│
└── Web.config                         ← API credentials

angular-whatsapp-service/              ← Angular frontend files
├── whatsapp.model.ts                  ← TypeScript interfaces
└── whatsapp.service.ts                ← Angular service
```

---

## 5. API Endpoints Reference

Base URL: `http://yourserver/SAMSAPI/api/WhatsApp`

---

### 5.1 `POST /SendPaymentSuccess`

Send confirmation after ANY fee payment.

**Request body:**
```json
{
  "PhoneNumber"  : "9246668725",
  "TemplateKey"  : "membership_fee_success",
  "Language"     : "both",
  "Placeholders" : {
    "MemberName"  : "Ravi Kumar",
    "MemberCode"  : "COS-1234",
    "Amount"      : "5000",
    "FeeType"     : "Membership Fee",
    "ReceiptNo"   : "REC-2026-001",
    "PaidDate"    : "26-Mar-2026",
    "PaymentMode" : "Cash"
  }
}
```

**Template keys for payment success:**
| TemplateKey | Use case |
|-------------|----------|
| `membership_fee_success` | Membership fee paid |
| `sports_fee_success` | Sports fee paid (add `SportName` placeholder) |
| `general_fee_success` | Any other fee |
| `welcome_member` | New member registration |

---

### 5.2 `POST /SendPaymentReminder`

Send reminder to a single member.

```json
{
  "PhoneNumber"  : "9246668725",
  "TemplateKey"  : "membership_fee_reminder",
  "Language"     : "both",
  "Placeholders" : {
    "MemberName"  : "Ravi Kumar",
    "MemberCode"  : "COS-1234",
    "DueAmount"   : "5000",
    "DueYear"     : "2025-2026",
    "DueDate"     : "31-Mar-2026",
    "FeeType"     : "Membership Fee"
  }
}
```

---

### 5.3 `POST /SendBulkReminders`

Send reminders to multiple members (backend fetches their dues).

```json
{
  "MemberIds" : [101, 102, 103, 104],
  "FeeType"   : "Membership",
  "Language"  : "both"
}
```

**Response:**
```json
{
  "Success"     : true,
  "Message"     : "Sent: 4, Failed: 0",
  "SentCount"   : 4,
  "FailedCount" : 0,
  "Details"     : [
    { "MemberCode": "COS-101", "MemberName": "Ravi", "Sent": true },
    ...
  ]
}
```

---

### 5.4 `POST /SendRemindersByFeeType?feeType=Membership&language=both`

Auto-fetch **all members** with pending dues and send reminders.  
No need to pass member IDs — the API queries the DB.

---

### 5.5 `POST /SendMembershipFeeAlert?memberId=101&txnId=55`

Shortcut — server looks up member + transaction, sends WhatsApp.  
Call right after `SaveMemberFee` returns.

---

### 5.6 `POST /SendSportsFeeAlert?memberId=101&sportName=Badminton&amount=2000&receiptNo=SREC-001&paymentMode=Cash`

Shortcut for sports fee — server looks up member, sends WhatsApp.

---

### 5.7 `GET /GetTemplates`

Returns the full template JSON (for admin preview in Angular).

---

### 5.8 `POST /ReloadTemplates`

Force-refresh the template cache (after editing the JSON file).

---

### 5.9 `POST /TestConnection?phone=9246668725`

Send a test WhatsApp to verify credentials.

---

### Language Values
| Value | Meaning |
|-------|---------|
| `en`  | English only |
| `te`  | Telugu only |
| `both` | English + Telugu (default, recommended) |

---

## 6. Angular Integration Examples

### Setup — app.module.ts
```typescript
import { HttpClientModule } from '@angular/common/http';
// Add to imports array:
imports: [HttpClientModule, ...]
```

### 6.1 After Membership Fee Payment
```typescript
// fee-payment.component.ts
import { WhatsAppService } from '../services/whatsapp.service';

constructor(private whatsApp: WhatsAppService) {}

onSaveFeeSuccess(member: any, txn: any): void {
  // Option A — Simple (server-side lookup)
  this.whatsApp.alertMembershipFeePaid(member.memberId, txn.id)
    .subscribe({
      next: res => console.log('WhatsApp sent:', res.message),
      error: err => console.warn('WhatsApp failed:', err.message)
    });

  // Option B — Full control from Angular
  this.whatsApp.sendMembershipFeeSuccess({
    memberName:  member.memberName,
    memberCode:  member.memberCode,
    phoneNumber: member.phone,
    amount:      txn.amount,
    feeType:     'Membership Fee',
    receiptNo:   txn.receiptNo,
    paidDate:    new Date().toLocaleDateString('en-IN'),
    paymentMode: txn.paymentMode
  }).subscribe();
}
```

---

### 6.2 After Sports Fee Payment
```typescript
this.whatsApp.sendSportsFeeSuccess({
  memberName:  member.memberName,
  memberCode:  member.memberCode,
  phoneNumber: member.phone,
  amount:      2000,
  feeType:     'Sports Fee',
  sportName:   'Badminton',
  receiptNo:   'SREC-2026-001',
  paidDate:    '26-Mar-2026',
  paymentMode: 'Online'
}).subscribe();
```

---

### 6.3 Send Reminder to One Member
```typescript
this.whatsApp.sendMembershipReminder({
  memberName:  member.memberName,
  memberCode:  member.memberCode,
  phoneNumber: member.phone,
  dueAmount:   member.dueAmount,
  dueYear:     '2025-2026',
  dueDate:     '31-Mar-2026',
  feeType:     'Membership Fee'
}).subscribe();
```

---

### 6.4 Bulk Reminders (from admin screen)
```typescript
// Get selected member IDs from a checkbox list
const selectedIds = this.members
  .filter(m => m.selected)
  .map(m => m.memberId);

this.whatsApp.sendBulkReminders(selectedIds, 'Membership', 'both')
  .subscribe(res => {
    alert(`Sent: ${res.sentCount}, Failed: ${res.failedCount}`);
  });
```

---

### 6.5 Send to ALL Due Members (one click)
```typescript
this.whatsApp.sendRemindersByFeeType('Membership', 'both')
  .subscribe(res => {
    this.toastr.success(`Reminders sent to ${res.sentCount} members`);
  });
```

---

## 7. Message Templates — Customization

Edit `Content/Config/whatsapp-templates.json` to change any message text.

### Available Placeholders

| Placeholder | Meaning | Used in |
|-------------|---------|---------|
| `{MemberName}` | Member's full name | All templates |
| `{MemberCode}` | Member code (e.g. COS-1234) | All templates |
| `{Amount}` | Amount paid | Payment success |
| `{FeeType}` | Type of fee | All templates |
| `{SportName}` | Sport name | Sports fee templates |
| `{PaidDate}` | Date of payment | Payment success |
| `{ReceiptNo}` | Receipt number | Payment success |
| `{PaymentMode}` | Cash / Card / Online | Payment success |
| `{DueAmount}` | Amount due | Reminder templates |
| `{DueYear}` | Due period (e.g. 2025-2026) | Reminder templates |
| `{DueDate}` | Last date to pay | Reminder templates |
| `{ClubName}` | Club name (auto-filled) | All templates |

### After Editing
Call `POST /api/WhatsApp/ReloadTemplates` or restart the application  
to pick up changes without redeployment.

---

## 8. Auto-Trigger from Backend

To automatically send WhatsApp when a fee is saved (without Angular calling a separate endpoint), add these lines to **SAMSManager.cs**:

```csharp
// At the top of SAMSManager.cs:
using SAMSAPI.WhatsApp.Service;

// Inside SaveMemberFee(), after se.SaveChanges():
public MembershipFeeTransaction SaveMemberFee(MembershipFeeTransaction mFee)
{
    // ... existing code ...
    se.SaveChanges();

    // ─── WhatsApp notification ───────────────────────────────────
    try
    {
        Member member = GetMember((int)mFee.MemberId);
        WhatsAppSAMSIntegration.NotifyMembershipFeePaid(member, mFee);
    }
    catch { /* never fail the main flow */ }
    // ────────────────────────────────────────────────────────────

    return mFee;
}
```

The call is **fire-and-forget** — it runs on a background thread and will never break the main API response.

---

## 9. Message Preview (English + Telugu)

### Membership Fee Success
```
Dear Ravi Kumar,

Your *Membership Fee* payment has been successfully received.

📋 *Payment Details:*
• Member Code  : COS-1234
• Fee Type       : Membership Fee
• Amount Paid : ₹5,000/-
• Receipt No    : REC-2026-001
• Date              : 26-Mar-2026
• Mode            : Cash

Thank you for being a valued member of *Cosmopolitan Club*.

─────────────────

ప్రియమైన Ravi Kumar గారికి,

మీ *సభ్యత్వ రుసుము* విజయవంతంగా స్వీకరించబడింది.

📋 *చెల్లింపు వివరాలు:*
• సభ్య కోడ్        : COS-1234
• రుసుము రకం  : Membership Fee
• చెల్లించిన మొత్తం : ₹5,000/-
• రసీదు నంబర్    : REC-2026-001
• తేదీ                 : 26-Mar-2026
• చెల్లింపు విధానం : Cash

*కాస్మోపాలిటన్ క్లబ్* యొక్క విలువైన సభ్యుడిగా ఉన్నందుకు ధన్యవాదాలు.
```

---

### Membership Fee Reminder
```
Dear Ravi Kumar,

This is a *friendly reminder* that your Membership Fee is due.

⚠️ *Due Details:*
• Member Code  : COS-1234
• Due Period     : 2025-2026
• Due Amount  : ₹5,000/-
• Due Date        : 31-Mar-2026

─────────────────

ప్రియమైన Ravi Kumar గారికి,

మీ సభ్యత్వ రుసుము చెల్లించవలసి ఉందని *మనసారా గుర్తు చేస్తున్నాము*.

⚠️ *బాకీ వివరాలు:*
• సభ్య కోడ్       : COS-1234
• బాకీ కాలం     : 2025-2026
• బాకీ మొత్తం  : ₹5,000/-
• చివరి తేదీ      : 31-Mar-2026
```

---

## 10. Troubleshooting

| Problem | Cause | Fix |
|---------|-------|-----|
| `"Invalid instance ID"` | Wrong Instance ID in Web.config | Re-copy from Devices page |
| `"Unauthorized"` | Access token expired | Regenerate token in API Settings |
| `"Device not connected"` | WhatsApp disconnected | Re-scan QR code in md.enotify.app |
| Message not received | Wrong phone number format | Phone should be 10 digits (91 prepended automatically) |
| Templates not updating | Cache not cleared | Call `POST /ReloadTemplates` |
| WhatsApp messages sending but no text | JSON template file path wrong | Set `WhatsApp_TemplateFile` in Web.config |

---

## Checklist

- [ ] Registered on md.enotify.app
- [ ] WhatsApp device connected (QR scanned)
- [ ] `WhatsApp_InstanceId` set in Web.config
- [ ] `WhatsApp_AccessToken` set in Web.config
- [ ] `WhatsApp_ClubName` set to your club name
- [ ] Test connection: `POST /api/WhatsApp/TestConnection?phone=YOUR_NUMBER`
- [ ] Templates reviewed in `whatsapp-templates.json`
- [ ] Angular `WhatsAppService` added to providers
- [ ] `API_BASE` updated in `whatsapp.service.ts`
