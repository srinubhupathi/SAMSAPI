using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SAMSAPI.Manager;
using SAMSData;
using System.Web.Http.Cors;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Web;
using SAMSAPI.Models.Dashboard;
using System.Web.UI.WebControls;
namespace SAMSAPI.Controllers
{

    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SaleEntryController : ApiController
    {

        [HttpGet]
        [ActionName("SaleList")]
        public IEnumerable<Sale> Get(string status)
        {
            var list = new SAMSManager().GetSaleList();
            return list;
        }
        [HttpGet]
        [ActionName("GetSale")]
        public Sale GetSale(int id)
        {
            Sale sale = new SAMSManager().GetSale(id);
            return sale;
        }

        [HttpPost]
        [ActionName("SaveSale")]
        public Sale SaveSale(Sale sale)
        {
            var _sale = new SAMSManager().SaveSale(sale);
            return _sale;
        }
    }
}