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
    public class CurrencyController : ApiController
    {
        // GET: api/Currency
        public IEnumerable<string> Get()
        {
            return new string[] {  };
        }

        // GET: api/Currency/5
        public BasicResponse Get(int id)
        {
            BasicResponse response = new BasicResponse { error = false, description = "Currency list" };
            List<Currency> currencyList = new List<Currency>();
            Connection connection = new Connection();

            if (connection.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("", connection.connection);
                cmd.CommandText = "SELECT IdCurrencyList, Denomination FROM CurrencyList WHERE IdCurrency = @IdCurrency AND Enabled = 1";
                cmd.Parameters.AddWithValue("@IdCurrency", id);
                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        currencyList.Add(new Currency
                        {
                            IdCurrency = dataReader["IdCurrencyList"].ToString(),
                            Denomination = float.Parse(dataReader["Denomination"].ToString())
                        });
                    }
                    response.data.AddRange(currencyList);
                }
                else
                {
                    response.description = "Currency not exists!";
                    response.error = true;
                }
            }
            else
            {
                response.error = true;
                response.description = "Error connecting to Database";
            }

            return response;
        }

        // POST: api/Currency
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Currency/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Currency/5
        public void Delete(int id)
        {
        }
    }
}
