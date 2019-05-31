using POSServices.Models;
using POSServices.Models.Data;
using POSServices.Models.Response;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace POSServices.Controllers
{
    public class TaxController : ApiController
    {
        // GET: api/Tax
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Tax/5
        public string Get(int id)
        {
            return "value";
        }

        [Route("api/tax/company/")]
        [HttpGet]
        public BasicResponse GetTaxFromCompany(String token = "", String idcompany = "")
        {
            Connection connection = new Connection();
            BasicResponse response = new BasicResponse { };
            List<Tax> taxes = new List<Tax>();

            if (!Tools.isUserLogged(token, idcompany))
            {
                response.error = false;
                response.description = "bad user";
                return response;
            }

            AES aes = new AES(idcompany);

            if (connection.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("", connection.connection);
                cmd.CommandText = "SELECT IdTax, Name, Percentage From Tax WHERE IdCompany = @IdCompany";
                cmd.Parameters.AddWithValue("@IdCompany", idcompany);

                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        response.data.Add(new Tax
                        {
                            IdTax = dataReader["IdTax"].ToString(),
                            Name = aes.decrypt(dataReader["Name"].ToString()),
                            Percentage = dataReader["Percentage"].ToString()
                        });
                    }
                }
            }

            return response;
        }

        // POST: api/Tax
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Tax/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Tax/5
        public void Delete(int id)
        {
        }
    }
}
