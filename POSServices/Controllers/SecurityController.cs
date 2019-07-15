using POSServices.Models;
using POSServices.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace POSServices.Controllers
{
    public class SecurityController : ApiController
    {
        // GET: api/Security
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Security/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Security
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Security/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Security/5
        public void Delete(int id)
        {
        }


        // POST: api/Security/aes
        [Route("api/Security/aes")]
        [HttpPost]
        public string EncryptDecryptInfo([FromBody] RequestDecryptAndEncrypt request, String type = "encrypt")
        {
            Connection connection = new Connection();
            AES aes = new AES(request.IdCompany);

            if (type == "encrypt")
            {
                return aes.encrypt(request.Text);
            } else
            {
                return aes.decrypt(request.Text);
            }           
        }
    }
}
