using POSServices.Models;
using POSServices.Models.Request;
using POSServices.Models.Response;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace POSServices.Controllers
{
    public class MovementController : ApiController
    {
        // GET: api/Movement
        public IEnumerable<string> Get()
        {
            return new string[] {"HOLA"};
        }

        [Route("api/Movement/{userId}/")]
        public BasicResponse Get(int userId)
        {
            BasicResponse response = new BasicResponse { error = false, description = "movements from user " + userId };
            Connection connection = new Connection();
            List<string> availableOptions = new List<string>();
            List<string> enabledOptions = new List<string>();
            List<string> selectedOptions = new List<string>();
            DateTime timeNow = DateTime.Now;
            String fecha = DateTime.Now.ToString("yyyy/MM/dd");

            if (connection.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("", connection.connection);
                cmd.CommandText = "SELECT IdUserMovementType, Name FROM UserMovementType WHERE Enabled = 1";

                SqlDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        availableOptions.Add(dataReader["IdUserMovementType"].ToString());
                    }
                    dataReader.Close();

                    cmd.CommandText = "SELECT IdUserMovementLog, IdUserMovementType FROM UserMovementLog WHERE IdUser = @IdUser AND CAST(DateTime AS DATE) = CAST(@date AS DATE)";
                    cmd.Parameters.AddWithValue("@IdUser", userId);
                    cmd.Parameters.AddWithValue("@date", fecha);

                    dataReader = cmd.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            try
                            {
                                selectedOptions.Add(dataReader["IdUserMovementType"].ToString());
                            } catch (Exception e) { }
                        }
                    }

                    enabledOptions.AddRange(availableOptions);
                    enabledOptions.Remove("2");
                    enabledOptions.Remove("3");
                    enabledOptions.Remove("4");

                    foreach (var option in selectedOptions)
                    {
                        if (option == "1")
                        {
                            enabledOptions.Clear();
                            enabledOptions.AddRange(availableOptions);
                            enabledOptions.Remove("1");
                            enabledOptions.Remove("4");
                        } else if (option == "2")
                        {
                            enabledOptions.Clear();
                            enabledOptions.AddRange(availableOptions);
                            enabledOptions.Remove("1");
                            enabledOptions.Remove("3");
                            enabledOptions.Remove("4");
                        } else if (option == "3")
                        {
                            enabledOptions.Clear();
                            enabledOptions.AddRange(availableOptions);
                            enabledOptions.Remove("1");
                            enabledOptions.Remove("2");
                            enabledOptions.Remove("3");
                        } else if (option == "4")
                        {
                            enabledOptions.Clear();
                            enabledOptions.AddRange(availableOptions);
                            enabledOptions.Remove("1");
                            enabledOptions.Remove("4");                            
                        }
                    }

                    response.data.AddRange(enabledOptions);
                }
            } else
            {
                response.error = true;
                response.description = "error triying to connect to DB";
            }

            return response;
        }

        // POST: api/Movement
        [Route("api/Movement")]
        [HttpPost]
        public BasicResponse Post([FromBody]RequestSaveMovement request)
        {
            BasicResponse response = new BasicResponse { error = false, description = "" };
            Connection connection = new Connection();
            DateTime timeNow = DateTime.Now;

            if (connection.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("", connection.connection);
                cmd.CommandText = "INSERT INTO UserMovementLog (IdUserMovementType, IdUser, DateTime) VALUES (@IdUserMovementType, @IdUser, @DateTime)";
                cmd.Parameters.AddWithValue("@IdUserMovementType", request.movementTypeId);
                cmd.Parameters.AddWithValue("@IdUser", request.userId);
                cmd.Parameters.AddWithValue("@DateTime", timeNow);
                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.RecordsAffected > 0)
                {
                    response.description = "saved";
                } else
                {
                    response.description = "error saving";
                    response.error = false;
                }
            }

            return response;
        }

        // PUT: api/Movement/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Movement/5
        public void Delete(int id)
        {
        }
    }
}
