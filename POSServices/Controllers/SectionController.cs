using POSServices.Models;
using POSServices.Models.Response;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using POSServices.Models.Data;

namespace POSServices.Controllers
{
    public class SectionController : ApiController
    {
        // GET: api/Section
        public BasicResponse Get()
        {
            BasicResponse response = new BasicResponse { error = false };
            Connection connection = new Connection();

            List<SaleArea> sections = new List<SaleArea>();
            
            if (connection.OpenConnection() == true)
            {
                string query = "SELECT SaleArea.IdSaleArea, Area.Name FROM SaleArea INNER JOIN Area ON SaleArea.IdArea = Area.IdArea";
                SqlCommand cmd = new SqlCommand(query, connection.connection);
                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows) {
                    while (dataReader.Read())
                    {
                        sections.Add(new SaleArea
                        {
                            name = dataReader["Name"].ToString(),
                            id   = dataReader["IdSaleArea"].ToString()
                        });
                    }                    
                }

                response.data.AddRange(sections);
                response.description = "List of SalesAreas";
            } else
            {
                response.description = "Error trying connect to DB";
                response.error = true;
            }
            
            return response;
        }

        // GET: api/Section/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Section
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Section/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Section/5
        public void Delete(int id)
        {
        }
    }
}
