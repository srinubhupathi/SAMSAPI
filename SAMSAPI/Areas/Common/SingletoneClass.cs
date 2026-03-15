using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAMSAPI.Areas.Common
{
    public class SingletoneClass
    {
        public static string authorizationToken = String.Empty;


        public static void setHeaders(System.Net.Http.Headers.HttpRequestHeaders headers)
        {
            if (headers.Contains("Authorization"))
            {
                authorizationToken = headers.GetValues("Authorization").First();
            }
        }
    }
}