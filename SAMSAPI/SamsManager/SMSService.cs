using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace SAMSAPI.Manager
{
    public class SMSService
    {
        public SMSService()
        {
        }


    //single message    https://smslogin.co/spanelv2/api.php?username=xxxx&password=xxxx&to=xxxxxxxxx&from=xxxxxxxx&message=xxxxxxxxxxxx

//multi message https://smslogin.co/spanelv2/api.php?username=xxxx&password=xxxx&to=xxxxxxxxx,xxxxxxxxx,xxxxxxxxxx&from=xxxxxxxx&message=xxxxxxxxxxxx

        public int SendSMS(string number, string message )
        {
            number = "9246668725";
            string url = "https://smslogin.co/spanelv2/api.php?username=cosmopolitanclub&password=daikin@sms";
            url += "&to=" + number;
            url += "&from=COSMOB";
            url += "&message=" + message;
                //https://smslogin.co/spanelv2/api.php?username=cosmopolitanclub&password=daikin@sms&to=9246668725&from=COSMOB&message=test from cosmo
                //to=xxxxxxxxx&from=xxxxxxxx&message=xxxxxxxxxxxx"
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@url);
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string responseText = reader.ReadToEnd(); // it takes the response from your url. now you can use as your need  
            response.Close();

            if (responseText == "Invalid user credentials" || responseText == "Invalid Senderid..!")
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

    }
}