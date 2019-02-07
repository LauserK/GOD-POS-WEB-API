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

            string query = "SELECT Name, MAC, IdDevice, IdArea, Enabled FROM Device WHERE MAC = @mac";
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
                            IdArea   = dataReader["IdArea"].ToString(),
                            Enabled  = (bool) dataReader["Enabled"]
                        });                        
                    }
                    dataReader.Close();
                    return response;
                }
                else
                {
                    dataReader.Close();
                    query = "INSERT INTO Device (Name, MAC, IdArea, Enabled) VALUES (@Name, @mac, @IdArea, @Enable)";
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Name", "");                    
                    cmd.Parameters.AddWithValue("@IdArea", "1");
                    cmd.Parameters.AddWithValue("@Enable","0");

                    dataReader = cmd.ExecuteReader();

                    if (dataReader.RecordsAffected > 0)
                    {

                        response.description = "Device created";
                    }
                    else
                    {
                        response.error = true;
                        response.description = "Device not found";
                    }
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
