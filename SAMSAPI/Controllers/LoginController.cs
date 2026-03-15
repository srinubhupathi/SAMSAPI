using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using SAMSAPI.Manager;
namespace SAMSAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class LoginController : ApiController
    {

        [HttpPost]
        [ActionName("UserLogin")]
        public User UserLogin(User user)
        {
            var userDetails = new SAMSManager().ValidateUser(user);

            if (userDetails !=null)
            {
                userDetails.token = ""; //Generate token
            }

            return userDetails;
        }
            // GET: api/Login
            public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Login/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Login
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Login/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Login/5
        public void Delete(int id)
        {
        }
    }
}
