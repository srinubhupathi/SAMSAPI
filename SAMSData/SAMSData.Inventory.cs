// ============================================================
// INVENTORY MODULE - EF Entity Classes & Context Extension
// Database: cosmobvrm (SQL Server)
// Date: 2026-03-16
//
// This file manually extends the auto-generated SAMSDataModel.Designer.cs
// (EF4 ObjectContext pattern) to add entity classes and ObjectSet properties
// for the 6 new inventory tables defined in inventory_db_schema.sql.
//
// Entities added:
//   StockItem, RoomTypeStockConfig, InventoryPurchase,
//   InventoryPurchaseDetail, RoomStockTransaction, RoomStockReturn
// ============================================================

using System;
using System.ComponentModel;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace SAMSData
{
    // =========================================================================
    // PARTIAL CONTEXT: Add inventory ObjectSets and AddTo helpers to SAMSEntities
    // =========================================================================
    public partial class SAMSEntities
    {
        // ── StockItems ────────────────────────────────────────────────────────
        public ObjectSet<StockItem> StockItems
        {
            get
            {
                if (_StockItems == null)
                    _StockItems = base.CreateObjectSet<StockItem>("StockItems");
                return _StockItems;
            }
        }
        private ObjectSet<StockItem> _StockItems;

        public void AddToStockItems(StockItem stockItem)
        {
            base.AddObject("StockItems", stockItem);
        }

        // ── RoomTypeStockConfigs ──────────────────────────────────────────────
        public ObjectSet<RoomTypeStockConfig> RoomTypeStockConfigs
        {
            get
            {
                if (_RoomTypeStockConfigs == null)
                    _RoomTypeStockConfigs = base.CreateObjectSet<RoomTypeStockConfig>("RoomTypeStockConfigs");
                return _RoomTypeStockConfigs;
            }
        }
        private ObjectSet<RoomTypeStockConfig> _RoomTypeStockConfigs;

        public void AddToRoomTypeStockConfigs(RoomTypeStockConfig item)
        {
            base.AddObject("RoomTypeStockConfigs", item);
        }

        // ── InventoryPurchases ────────────────────────────────────────────────
        public ObjectSet<InventoryPurchase> InventoryPurchases
        {
            get
            {
                if (_InventoryPurchases == null)
                    _InventoryPurchases = base.CreateObjectSet<InventoryPurchase>("InventoryPurchases");
                return _InventoryPurchases;
            }
        }
        private ObjectSet<InventoryPurchase> _InventoryPurchases;

        public void AddToInventoryPurchases(InventoryPurchase item)
        {
            base.AddObject("InventoryPurchases", item);
        }

        // ── InventoryPurchaseDetails ──────────────────────────────────────────
        public ObjectSet<InventoryPurchaseDetail> InventoryPurchaseDetails
        {
            get
            {
                if (_InventoryPurchaseDetails == null)
                    _InventoryPurchaseDetails = base.CreateObjectSet<InventoryPurchaseDetail>("InventoryPurchaseDetails");
                return _InventoryPurchaseDetails;
            }
        }
        private ObjectSet<InventoryPurchaseDetail> _InventoryPurchaseDetails;

        public void AddToInventoryPurchaseDetails(InventoryPurchaseDetail item)
        {
            base.AddObject("InventoryPurchaseDetails", item);
        }

        // ── RoomStockTransactions ─────────────────────────────────────────────
        public ObjectSet<RoomStockTransaction> RoomStockTransactions
        {
            get
            {
                if (_RoomStockTransactions == null)
                    _RoomStockTransactions = base.CreateObjectSet<RoomStockTransaction>("RoomStockTransactions");
                return _RoomStockTransactions;
            }
        }
        private ObjectSet<RoomStockTransaction> _RoomStockTransactions;

        public void AddToRoomStockTransactions(RoomStockTransaction item)
        {
            base.AddObject("RoomStockTransactions", item);
        }

        // ── RoomStockReturns ──────────────────────────────────────────────────
        public ObjectSet<RoomStockReturn> RoomStockReturns
        {
            get
            {
                if (_RoomStockReturns == null)
                    _RoomStockReturns = base.CreateObjectSet<RoomStockReturn>("RoomStockReturns");
                return _RoomStockReturns;
            }
        }
        private ObjectSet<RoomStockReturn> _RoomStockReturns;

        public void AddToRoomStockReturns(RoomStockReturn item)
        {
            base.AddObject("RoomStockReturns", item);
        }
    }

    // =========================================================================
    // ENTITY: StockItem  →  dbo.StockItems
    // =========================================================================
    [EdmEntityTypeAttribute(NamespaceName = "SAMSModel", Name = "StockItem")]
    [Serializable()]
    [DataContractAttribute(IsReference = true)]
    public partial class StockItem : EntityObject
    {
        public static StockItem CreateStockItem(int stockItemId, string itemName, string unitOfMeasurement, bool isActive)
        {
            var e = new StockItem();
            e.StockItemId       = stockItemId;
            e.ItemName          = itemName;
            e.UnitOfMeasurement = unitOfMeasurement;
            e.IsActive          = isActive;
            return e;
        }

        [EdmScalarPropertyAttribute(EntityKeyProperty = true, IsNullable = false)]
        [DataMemberAttribute()]
        public int StockItemId
        {
            get { return _StockItemId; }
            set
            {
                if (_StockItemId != value)
                {
                    OnStockItemIdChanging(value);
                    ReportPropertyChanging("StockItemId");
                    _StockItemId = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("StockItemId");
                    OnStockItemIdChanged();
                }
            }
        }
        private int _StockItemId;
        partial void OnStockItemIdChanging(int value);
        partial void OnStockItemIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public string ItemName
        {
            get { return _ItemName; }
            set
            {
                OnItemNameChanging(value);
                ReportPropertyChanging("ItemName");
                _ItemName = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("ItemName");
                OnItemNameChanged();
            }
        }
        private string _ItemName;
        partial void OnItemNameChanging(string value);
        partial void OnItemNameChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public string Category
        {
            get { return _Category; }
            set
            {
                OnCategoryChanging(value);
                ReportPropertyChanging("Category");
                _Category = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Category");
                OnCategoryChanged();
            }
        }
        private string _Category;
        partial void OnCategoryChanging(string value);
        partial void OnCategoryChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public string UnitOfMeasurement
        {
            get { return _UnitOfMeasurement; }
            set
            {
                OnUnitOfMeasurementChanging(value);
                ReportPropertyChanging("UnitOfMeasurement");
                _UnitOfMeasurement = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("UnitOfMeasurement");
                OnUnitOfMeasurementChanged();
            }
        }
        private string _UnitOfMeasurement;
        partial void OnUnitOfMeasurementChanging(string value);
        partial void OnUnitOfMeasurementChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<double> ReorderLevel
        {
            get { return _ReorderLevel; }
            set
            {
                OnReorderLevelChanging(value);
                ReportPropertyChanging("ReorderLevel");
                _ReorderLevel = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("ReorderLevel");
                OnReorderLevelChanged();
            }
        }
        private Nullable<double> _ReorderLevel;
        partial void OnReorderLevelChanging(Nullable<double> value);
        partial void OnReorderLevelChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public string Description
        {
            get { return _Description; }
            set
            {
                OnDescriptionChanging(value);
                ReportPropertyChanging("Description");
                _Description = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Description");
                OnDescriptionChanged();
            }
        }
        private string _Description;
        partial void OnDescriptionChanging(string value);
        partial void OnDescriptionChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                OnIsActiveChanging(value);
                ReportPropertyChanging("IsActive");
                _IsActive = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("IsActive");
                OnIsActiveChanged();
            }
        }
        private bool _IsActive;
        partial void OnIsActiveChanging(bool value);
        partial void OnIsActiveChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<int> CreatedBy
        {
            get { return _CreatedBy; }
            set
            {
                OnCreatedByChanging(value);
                ReportPropertyChanging("CreatedBy");
                _CreatedBy = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("CreatedBy");
                OnCreatedByChanged();
            }
        }
        private Nullable<int> _CreatedBy;
        partial void OnCreatedByChanging(Nullable<int> value);
        partial void OnCreatedByChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<DateTime> CreatedOn
        {
            get { return _CreatedOn; }
            set
            {
                OnCreatedOnChanging(value);
                ReportPropertyChanging("CreatedOn");
                _CreatedOn = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("CreatedOn");
                OnCreatedOnChanged();
            }
        }
        private Nullable<DateTime> _CreatedOn;
        partial void OnCreatedOnChanging(Nullable<DateTime> value);
        partial void OnCreatedOnChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<int> UpdatedBy
        {
            get { return _UpdatedBy; }
            set
            {
                OnUpdatedByChanging(value);
                ReportPropertyChanging("UpdatedBy");
                _UpdatedBy = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("UpdatedBy");
                OnUpdatedByChanged();
            }
        }
        private Nullable<int> _UpdatedBy;
        partial void OnUpdatedByChanging(Nullable<int> value);
        partial void OnUpdatedByChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<DateTime> UpdatedOn
        {
            get { return _UpdatedOn; }
            set
            {
                OnUpdatedOnChanging(value);
                ReportPropertyChanging("UpdatedOn");
                _UpdatedOn = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("UpdatedOn");
                OnUpdatedOnChanged();
            }
        }
        private Nullable<DateTime> _UpdatedOn;
        partial void OnUpdatedOnChanging(Nullable<DateTime> value);
        partial void OnUpdatedOnChanged();
    }

    // =========================================================================
    // ENTITY: RoomTypeStockConfig  →  dbo.RoomTypeStockConfigs
    // =========================================================================
    [EdmEntityTypeAttribute(NamespaceName = "SAMSModel", Name = "RoomTypeStockConfig")]
    [Serializable()]
    [DataContractAttribute(IsReference = true)]
    public partial class RoomTypeStockConfig : EntityObject
    {
        public static RoomTypeStockConfig CreateRoomTypeStockConfig(int id, int tariffId, int stockItemId, double quantity)
        {
            var e = new RoomTypeStockConfig();
            e.RoomTypeStockConfigId = id;
            e.TariffId              = tariffId;
            e.StockItemId           = stockItemId;
            e.Quantity              = quantity;
            return e;
        }

        [EdmScalarPropertyAttribute(EntityKeyProperty = true, IsNullable = false)]
        [DataMemberAttribute()]
        public int RoomTypeStockConfigId
        {
            get { return _RoomTypeStockConfigId; }
            set
            {
                if (_RoomTypeStockConfigId != value)
                {
                    OnRoomTypeStockConfigIdChanging(value);
                    ReportPropertyChanging("RoomTypeStockConfigId");
                    _RoomTypeStockConfigId = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("RoomTypeStockConfigId");
                    OnRoomTypeStockConfigIdChanged();
                }
            }
        }
        private int _RoomTypeStockConfigId;
        partial void OnRoomTypeStockConfigIdChanging(int value);
        partial void OnRoomTypeStockConfigIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public int TariffId
        {
            get { return _TariffId; }
            set
            {
                OnTariffIdChanging(value);
                ReportPropertyChanging("TariffId");
                _TariffId = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("TariffId");
                OnTariffIdChanged();
            }
        }
        private int _TariffId;
        partial void OnTariffIdChanging(int value);
        partial void OnTariffIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public int StockItemId
        {
            get { return _StockItemId; }
            set
            {
                OnStockItemIdChanging(value);
                ReportPropertyChanging("StockItemId");
                _StockItemId = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("StockItemId");
                OnStockItemIdChanged();
            }
        }
        private int _StockItemId;
        partial void OnStockItemIdChanging(int value);
        partial void OnStockItemIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public double Quantity
        {
            get { return _Quantity; }
            set
            {
                OnQuantityChanging(value);
                ReportPropertyChanging("Quantity");
                _Quantity = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("Quantity");
                OnQuantityChanged();
            }
        }
        private double _Quantity;
        partial void OnQuantityChanging(double value);
        partial void OnQuantityChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<double> DailyReplenishQty
        {
            get { return _DailyReplenishQty; }
            set
            {
                OnDailyReplenishQtyChanging(value);
                ReportPropertyChanging("DailyReplenishQty");
                _DailyReplenishQty = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("DailyReplenishQty");
                OnDailyReplenishQtyChanged();
            }
        }
        private Nullable<double> _DailyReplenishQty;
        partial void OnDailyReplenishQtyChanging(Nullable<double> value);
        partial void OnDailyReplenishQtyChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<int> CreatedBy
        {
            get { return _CreatedBy; }
            set
            {
                OnCreatedByChanging(value);
                ReportPropertyChanging("CreatedBy");
                _CreatedBy = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("CreatedBy");
                OnCreatedByChanged();
            }
        }
        private Nullable<int> _CreatedBy;
        partial void OnCreatedByChanging(Nullable<int> value);
        partial void OnCreatedByChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<DateTime> CreatedOn
        {
            get { return _CreatedOn; }
            set
            {
                OnCreatedOnChanging(value);
                ReportPropertyChanging("CreatedOn");
                _CreatedOn = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("CreatedOn");
                OnCreatedOnChanged();
            }
        }
        private Nullable<DateTime> _CreatedOn;
        partial void OnCreatedOnChanging(Nullable<DateTime> value);
        partial void OnCreatedOnChanged();
    }

    // =========================================================================
    // ENTITY: InventoryPurchase  →  dbo.InventoryPurchases
    // =========================================================================
    [EdmEntityTypeAttribute(NamespaceName = "SAMSModel", Name = "InventoryPurchase")]
    [Serializable()]
    [DataContractAttribute(IsReference = true)]
    public partial class InventoryPurchase : EntityObject
    {
        public static InventoryPurchase CreateInventoryPurchase(int purchaseId, DateTime purchaseDate)
        {
            var e = new InventoryPurchase();
            e.PurchaseId   = purchaseId;
            e.PurchaseDate = purchaseDate;
            return e;
        }

        [EdmScalarPropertyAttribute(EntityKeyProperty = true, IsNullable = false)]
        [DataMemberAttribute()]
        public int PurchaseId
        {
            get { return _PurchaseId; }
            set
            {
                if (_PurchaseId != value)
                {
                    OnPurchaseIdChanging(value);
                    ReportPropertyChanging("PurchaseId");
                    _PurchaseId = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("PurchaseId");
                    OnPurchaseIdChanged();
                }
            }
        }
        private int _PurchaseId;
        partial void OnPurchaseIdChanging(int value);
        partial void OnPurchaseIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public DateTime PurchaseDate
        {
            get { return _PurchaseDate; }
            set
            {
                OnPurchaseDateChanging(value);
                ReportPropertyChanging("PurchaseDate");
                _PurchaseDate = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("PurchaseDate");
                OnPurchaseDateChanged();
            }
        }
        private DateTime _PurchaseDate;
        partial void OnPurchaseDateChanging(DateTime value);
        partial void OnPurchaseDateChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public string InvoiceNumber
        {
            get { return _InvoiceNumber; }
            set
            {
                OnInvoiceNumberChanging(value);
                ReportPropertyChanging("InvoiceNumber");
                _InvoiceNumber = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("InvoiceNumber");
                OnInvoiceNumberChanged();
            }
        }
        private string _InvoiceNumber;
        partial void OnInvoiceNumberChanging(string value);
        partial void OnInvoiceNumberChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public string VendorName
        {
            get { return _VendorName; }
            set
            {
                OnVendorNameChanging(value);
                ReportPropertyChanging("VendorName");
                _VendorName = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("VendorName");
                OnVendorNameChanged();
            }
        }
        private string _VendorName;
        partial void OnVendorNameChanging(string value);
        partial void OnVendorNameChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<double> TotalAmount
        {
            get { return _TotalAmount; }
            set
            {
                OnTotalAmountChanging(value);
                ReportPropertyChanging("TotalAmount");
                _TotalAmount = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("TotalAmount");
                OnTotalAmountChanged();
            }
        }
        private Nullable<double> _TotalAmount;
        partial void OnTotalAmountChanging(Nullable<double> value);
        partial void OnTotalAmountChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public string Remarks
        {
            get { return _Remarks; }
            set
            {
                OnRemarksChanging(value);
                ReportPropertyChanging("Remarks");
                _Remarks = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Remarks");
                OnRemarksChanged();
            }
        }
        private string _Remarks;
        partial void OnRemarksChanging(string value);
        partial void OnRemarksChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<int> CreatedBy
        {
            get { return _CreatedBy; }
            set
            {
                OnCreatedByChanging(value);
                ReportPropertyChanging("CreatedBy");
                _CreatedBy = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("CreatedBy");
                OnCreatedByChanged();
            }
        }
        private Nullable<int> _CreatedBy;
        partial void OnCreatedByChanging(Nullable<int> value);
        partial void OnCreatedByChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<DateTime> CreatedOn
        {
            get { return _CreatedOn; }
            set
            {
                OnCreatedOnChanging(value);
                ReportPropertyChanging("CreatedOn");
                _CreatedOn = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("CreatedOn");
                OnCreatedOnChanged();
            }
        }
        private Nullable<DateTime> _CreatedOn;
        partial void OnCreatedOnChanging(Nullable<DateTime> value);
        partial void OnCreatedOnChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<int> UpdatedBy
        {
            get { return _UpdatedBy; }
            set
            {
                OnUpdatedByChanging(value);
                ReportPropertyChanging("UpdatedBy");
                _UpdatedBy = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("UpdatedBy");
                OnUpdatedByChanged();
            }
        }
        private Nullable<int> _UpdatedBy;
        partial void OnUpdatedByChanging(Nullable<int> value);
        partial void OnUpdatedByChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<DateTime> UpdatedOn
        {
            get { return _UpdatedOn; }
            set
            {
                OnUpdatedOnChanging(value);
                ReportPropertyChanging("UpdatedOn");
                _UpdatedOn = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("UpdatedOn");
                OnUpdatedOnChanged();
            }
        }
        private Nullable<DateTime> _UpdatedOn;
        partial void OnUpdatedOnChanging(Nullable<DateTime> value);
        partial void OnUpdatedOnChanged();
    }

    // =========================================================================
    // ENTITY: InventoryPurchaseDetail  →  dbo.InventoryPurchaseDetails
    // =========================================================================
    [EdmEntityTypeAttribute(NamespaceName = "SAMSModel", Name = "InventoryPurchaseDetail")]
    [Serializable()]
    [DataContractAttribute(IsReference = true)]
    public partial class InventoryPurchaseDetail : EntityObject
    {
        public static InventoryPurchaseDetail CreateInventoryPurchaseDetail(int purchaseDetailId, int purchaseId, int stockItemId, double quantity)
        {
            var e = new InventoryPurchaseDetail();
            e.PurchaseDetailId = purchaseDetailId;
            e.PurchaseId       = purchaseId;
            e.StockItemId      = stockItemId;
            e.Quantity         = quantity;
            return e;
        }

        [EdmScalarPropertyAttribute(EntityKeyProperty = true, IsNullable = false)]
        [DataMemberAttribute()]
        public int PurchaseDetailId
        {
            get { return _PurchaseDetailId; }
            set
            {
                if (_PurchaseDetailId != value)
                {
                    OnPurchaseDetailIdChanging(value);
                    ReportPropertyChanging("PurchaseDetailId");
                    _PurchaseDetailId = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("PurchaseDetailId");
                    OnPurchaseDetailIdChanged();
                }
            }
        }
        private int _PurchaseDetailId;
        partial void OnPurchaseDetailIdChanging(int value);
        partial void OnPurchaseDetailIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public int PurchaseId
        {
            get { return _PurchaseId; }
            set
            {
                OnPurchaseIdChanging(value);
                ReportPropertyChanging("PurchaseId");
                _PurchaseId = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("PurchaseId");
                OnPurchaseIdChanged();
            }
        }
        private int _PurchaseId;
        partial void OnPurchaseIdChanging(int value);
        partial void OnPurchaseIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public int StockItemId
        {
            get { return _StockItemId; }
            set
            {
                OnStockItemIdChanging(value);
                ReportPropertyChanging("StockItemId");
                _StockItemId = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("StockItemId");
                OnStockItemIdChanged();
            }
        }
        private int _StockItemId;
        partial void OnStockItemIdChanging(int value);
        partial void OnStockItemIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public double Quantity
        {
            get { return _Quantity; }
            set
            {
                OnQuantityChanging(value);
                ReportPropertyChanging("Quantity");
                _Quantity = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("Quantity");
                OnQuantityChanged();
            }
        }
        private double _Quantity;
        partial void OnQuantityChanging(double value);
        partial void OnQuantityChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<double> UnitPrice
        {
            get { return _UnitPrice; }
            set
            {
                OnUnitPriceChanging(value);
                ReportPropertyChanging("UnitPrice");
                _UnitPrice = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("UnitPrice");
                OnUnitPriceChanged();
            }
        }
        private Nullable<double> _UnitPrice;
        partial void OnUnitPriceChanging(Nullable<double> value);
        partial void OnUnitPriceChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<double> TotalAmount
        {
            get { return _TotalAmount; }
            set
            {
                OnTotalAmountChanging(value);
                ReportPropertyChanging("TotalAmount");
                _TotalAmount = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("TotalAmount");
                OnTotalAmountChanged();
            }
        }
        private Nullable<double> _TotalAmount;
        partial void OnTotalAmountChanging(Nullable<double> value);
        partial void OnTotalAmountChanged();
    }

    // =========================================================================
    // ENTITY: RoomStockTransaction  →  dbo.RoomStockTransactions
    // =========================================================================
    [EdmEntityTypeAttribute(NamespaceName = "SAMSModel", Name = "RoomStockTransaction")]
    [Serializable()]
    [DataContractAttribute(IsReference = true)]
    public partial class RoomStockTransaction : EntityObject
    {
        public static RoomStockTransaction CreateRoomStockTransaction(
            int roomStockId, int bookingDetailId, int bookingId, int roomId,
            int stockItemId, string stockType, double issuedQuantity)
        {
            var e = new RoomStockTransaction();
            e.RoomStockId       = roomStockId;
            e.BookingDetailId   = bookingDetailId;
            e.BookingId         = bookingId;
            e.RoomId            = roomId;
            e.StockItemId       = stockItemId;
            e.StockType         = stockType;
            e.IssuedQuantity    = issuedQuantity;
            e.ReturnedQuantity  = 0;
            return e;
        }

        [EdmScalarPropertyAttribute(EntityKeyProperty = true, IsNullable = false)]
        [DataMemberAttribute()]
        public int RoomStockId
        {
            get { return _RoomStockId; }
            set
            {
                if (_RoomStockId != value)
                {
                    OnRoomStockIdChanging(value);
                    ReportPropertyChanging("RoomStockId");
                    _RoomStockId = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("RoomStockId");
                    OnRoomStockIdChanged();
                }
            }
        }
        private int _RoomStockId;
        partial void OnRoomStockIdChanging(int value);
        partial void OnRoomStockIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public int BookingDetailId
        {
            get { return _BookingDetailId; }
            set
            {
                OnBookingDetailIdChanging(value);
                ReportPropertyChanging("BookingDetailId");
                _BookingDetailId = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("BookingDetailId");
                OnBookingDetailIdChanged();
            }
        }
        private int _BookingDetailId;
        partial void OnBookingDetailIdChanging(int value);
        partial void OnBookingDetailIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public int BookingId
        {
            get { return _BookingId; }
            set
            {
                OnBookingIdChanging(value);
                ReportPropertyChanging("BookingId");
                _BookingId = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("BookingId");
                OnBookingIdChanged();
            }
        }
        private int _BookingId;
        partial void OnBookingIdChanging(int value);
        partial void OnBookingIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public int RoomId
        {
            get { return _RoomId; }
            set
            {
                OnRoomIdChanging(value);
                ReportPropertyChanging("RoomId");
                _RoomId = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("RoomId");
                OnRoomIdChanged();
            }
        }
        private int _RoomId;
        partial void OnRoomIdChanging(int value);
        partial void OnRoomIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public int StockItemId
        {
            get { return _StockItemId; }
            set
            {
                OnStockItemIdChanging(value);
                ReportPropertyChanging("StockItemId");
                _StockItemId = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("StockItemId");
                OnStockItemIdChanged();
            }
        }
        private int _StockItemId;
        partial void OnStockItemIdChanging(int value);
        partial void OnStockItemIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public string StockType
        {
            get { return _StockType; }
            set
            {
                OnStockTypeChanging(value);
                ReportPropertyChanging("StockType");
                _StockType = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("StockType");
                OnStockTypeChanged();
            }
        }
        private string _StockType;
        partial void OnStockTypeChanging(string value);
        partial void OnStockTypeChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public double IssuedQuantity
        {
            get { return _IssuedQuantity; }
            set
            {
                OnIssuedQuantityChanging(value);
                ReportPropertyChanging("IssuedQuantity");
                _IssuedQuantity = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("IssuedQuantity");
                OnIssuedQuantityChanged();
            }
        }
        private double _IssuedQuantity;
        partial void OnIssuedQuantityChanging(double value);
        partial void OnIssuedQuantityChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public double ReturnedQuantity
        {
            get { return _ReturnedQuantity; }
            set
            {
                OnReturnedQuantityChanging(value);
                ReportPropertyChanging("ReturnedQuantity");
                _ReturnedQuantity = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("ReturnedQuantity");
                OnReturnedQuantityChanged();
            }
        }
        private double _ReturnedQuantity;
        partial void OnReturnedQuantityChanging(double value);
        partial void OnReturnedQuantityChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<DateTime> IssueDate
        {
            get { return _IssueDate; }
            set
            {
                OnIssueDateChanging(value);
                ReportPropertyChanging("IssueDate");
                _IssueDate = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("IssueDate");
                OnIssueDateChanged();
            }
        }
        private Nullable<DateTime> _IssueDate;
        partial void OnIssueDateChanging(Nullable<DateTime> value);
        partial void OnIssueDateChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public string Remarks
        {
            get { return _Remarks; }
            set
            {
                OnRemarksChanging(value);
                ReportPropertyChanging("Remarks");
                _Remarks = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Remarks");
                OnRemarksChanged();
            }
        }
        private string _Remarks;
        partial void OnRemarksChanging(string value);
        partial void OnRemarksChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<int> CreatedBy
        {
            get { return _CreatedBy; }
            set
            {
                OnCreatedByChanging(value);
                ReportPropertyChanging("CreatedBy");
                _CreatedBy = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("CreatedBy");
                OnCreatedByChanged();
            }
        }
        private Nullable<int> _CreatedBy;
        partial void OnCreatedByChanging(Nullable<int> value);
        partial void OnCreatedByChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<DateTime> CreatedOn
        {
            get { return _CreatedOn; }
            set
            {
                OnCreatedOnChanging(value);
                ReportPropertyChanging("CreatedOn");
                _CreatedOn = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("CreatedOn");
                OnCreatedOnChanged();
            }
        }
        private Nullable<DateTime> _CreatedOn;
        partial void OnCreatedOnChanging(Nullable<DateTime> value);
        partial void OnCreatedOnChanged();
    }

    // =========================================================================
    // ENTITY: RoomStockReturn  →  dbo.RoomStockReturns
    // =========================================================================
    [EdmEntityTypeAttribute(NamespaceName = "SAMSModel", Name = "RoomStockReturn")]
    [Serializable()]
    [DataContractAttribute(IsReference = true)]
    public partial class RoomStockReturn : EntityObject
    {
        public static RoomStockReturn CreateRoomStockReturn(
            int returnId, int roomStockId, int bookingDetailId, int stockItemId, double returnQuantity)
        {
            var e = new RoomStockReturn();
            e.ReturnId        = returnId;
            e.RoomStockId     = roomStockId;
            e.BookingDetailId = bookingDetailId;
            e.StockItemId     = stockItemId;
            e.ReturnQuantity  = returnQuantity;
            return e;
        }

        [EdmScalarPropertyAttribute(EntityKeyProperty = true, IsNullable = false)]
        [DataMemberAttribute()]
        public int ReturnId
        {
            get { return _ReturnId; }
            set
            {
                if (_ReturnId != value)
                {
                    OnReturnIdChanging(value);
                    ReportPropertyChanging("ReturnId");
                    _ReturnId = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("ReturnId");
                    OnReturnIdChanged();
                }
            }
        }
        private int _ReturnId;
        partial void OnReturnIdChanging(int value);
        partial void OnReturnIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public int RoomStockId
        {
            get { return _RoomStockId; }
            set
            {
                OnRoomStockIdChanging(value);
                ReportPropertyChanging("RoomStockId");
                _RoomStockId = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("RoomStockId");
                OnRoomStockIdChanged();
            }
        }
        private int _RoomStockId;
        partial void OnRoomStockIdChanging(int value);
        partial void OnRoomStockIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public int BookingDetailId
        {
            get { return _BookingDetailId; }
            set
            {
                OnBookingDetailIdChanging(value);
                ReportPropertyChanging("BookingDetailId");
                _BookingDetailId = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("BookingDetailId");
                OnBookingDetailIdChanged();
            }
        }
        private int _BookingDetailId;
        partial void OnBookingDetailIdChanging(int value);
        partial void OnBookingDetailIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public int StockItemId
        {
            get { return _StockItemId; }
            set
            {
                OnStockItemIdChanging(value);
                ReportPropertyChanging("StockItemId");
                _StockItemId = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("StockItemId");
                OnStockItemIdChanged();
            }
        }
        private int _StockItemId;
        partial void OnStockItemIdChanging(int value);
        partial void OnStockItemIdChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public double ReturnQuantity
        {
            get { return _ReturnQuantity; }
            set
            {
                OnReturnQuantityChanging(value);
                ReportPropertyChanging("ReturnQuantity");
                _ReturnQuantity = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("ReturnQuantity");
                OnReturnQuantityChanged();
            }
        }
        private double _ReturnQuantity;
        partial void OnReturnQuantityChanging(double value);
        partial void OnReturnQuantityChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<DateTime> ReturnDate
        {
            get { return _ReturnDate; }
            set
            {
                OnReturnDateChanging(value);
                ReportPropertyChanging("ReturnDate");
                _ReturnDate = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("ReturnDate");
                OnReturnDateChanged();
            }
        }
        private Nullable<DateTime> _ReturnDate;
        partial void OnReturnDateChanging(Nullable<DateTime> value);
        partial void OnReturnDateChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public string Reason
        {
            get { return _Reason; }
            set
            {
                OnReasonChanging(value);
                ReportPropertyChanging("Reason");
                _Reason = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Reason");
                OnReasonChanged();
            }
        }
        private string _Reason;
        partial void OnReasonChanging(string value);
        partial void OnReasonChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<int> CreatedBy
        {
            get { return _CreatedBy; }
            set
            {
                OnCreatedByChanging(value);
                ReportPropertyChanging("CreatedBy");
                _CreatedBy = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("CreatedBy");
                OnCreatedByChanged();
            }
        }
        private Nullable<int> _CreatedBy;
        partial void OnCreatedByChanging(Nullable<int> value);
        partial void OnCreatedByChanged();

        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<DateTime> CreatedOn
        {
            get { return _CreatedOn; }
            set
            {
                OnCreatedOnChanging(value);
                ReportPropertyChanging("CreatedOn");
                _CreatedOn = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("CreatedOn");
                OnCreatedOnChanged();
            }
        }
        private Nullable<DateTime> _CreatedOn;
        partial void OnCreatedOnChanging(Nullable<DateTime> value);
        partial void OnCreatedOnChanged();
    }
}
