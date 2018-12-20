using POSServices.Models;
using POSServices.Models.Data;
using POSServices.Models.Request;
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
    public class DeviceController : ApiController
    {
        // GET: api/Device
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Device/5
        [Route("api/Device/verify")]
        [HttpPost]
        public BasicResponse Get([FromBody] RequestGetDevice request)
        {
            BasicResponse response = new BasicResponse { description = "Device found", error = true };
            Connection connection = new Connection();

            string query = "SELECT Name, MAC, IdDevice, IdArea FROM Device WHERE MAC = @mac";
            if (connection.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection.connection);
                cmd.Parameters.AddWithValue("@mac", request.mac);

                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        response.data.Add(new Device {
                            IdDevice = dataReader["IdDevice"].ToString(),
                            Name     = dataReader["Name"].ToString(),
                            MAC      = dataReader["MAC"].ToString(),
                            IdArea   = dataReader["IdArea"].ToString()
                        });                        
                    }
                }
                else
                {
                    response.description = "Device not found";
                    response.error = true;
                    return response;
                }
            } else
            {
                response.description = "Connection not found";
                response.error = true;
                return response;
            }

            return response;
        }

        // POST: api/Device
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Device/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Device/5
        public void Delete(int id)
        {
        }
    }
}
