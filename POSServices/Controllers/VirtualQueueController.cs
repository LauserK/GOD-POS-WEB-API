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
    public class VirtualQueueController : ApiController
    {
        // GET: api/VirtualQueue
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/VirtualQueue/5
        [Route("api/VirtualQueue/{AreaId}")]
        public BasicResponse Get(int AreaId)
        {
            BasicResponse response = new BasicResponse { error = false };
            Connection connection = new Connection();
            String todaysDate = DateTime.Now.ToString("yyyy-MM-dd");
            List<Client> clients = new List<Client>();
            if (connection.OpenConnection())
            {
                String query = "SELECT VirtualQueue.IdVirtualQueue, Client.IdClient, Client.FirstName, Client.LastName, Client.IdentificationNumber, Client.Adress, Client.PhoneNumber, Client.BirthDay FROM VirtualQueue INNER JOIN Client ON Client.IdClient = VirtualQueue.IdClient WHERE IdArea = @IdArea AND Date >= @Date AND IdVirtualQueueStatus = 1 ORDER BY IdPriority DESC, Date ASC";
                SqlCommand cmd = new SqlCommand(query, connection.connection);
                cmd.Parameters.AddWithValue("@IdArea", AreaId);
                cmd.Parameters.AddWithValue("@Date", todaysDate);

                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        clients.Add(new Client
                        {
                            Address = dataReader["Adress"].ToString(),
                            BirthDay = Convert.ToDateTime(dataReader["BirthDay"]).ToString("yyyy-MM-dd"),
                            Name = dataReader["FirstName"].ToString() + " " + dataReader["LastName"].ToString(),
                            IdClient = dataReader["IdClient"].ToString(),
                            IdentificationNumber = dataReader["IdentificationNumber"].ToString(),
                            IdVirtualQueue = dataReader["IdVirtualQueue"].ToString(),
                            PhoneNumber = dataReader["PhoneNumber"].ToString()

                        });                        
                    }

                    response.data.AddRange(clients);
                    response.description = "List of clients of Area: " + AreaId;                    
                }


            } else
            {
                response.description = "Error trying to connect to DB.";
                response.error = true;
            }

            return response;
        }

        // POST: api/VirtualQueue
        [Route("api/virtualqueue/update/")]
        [HttpPost]
        public BasicResponse Post([FromBody] RequestUpdateVirtualQueueStatus request)
        {
            BasicResponse response = new BasicResponse { error = false, description = ""};
            Connection connection = new Connection();            

            if (request == null)
            {
                response.description = "Empty json!";
                response.error = true;
                return response;
            } else
            {
                try
                {
                    if (request.AreaId == "" || request.StatusId == "" || request.IdVirtualQueue == "")
                    {
                        response.description = "Empty fields!";
                        response.error = true;
                        return response;
                    }
                } catch { }

                if (connection.OpenConnection())
                {
                    SqlCommand cmd = connection.connection.CreateCommand();
                    SqlTransaction transaction;

                    // Start a local transaction.
                    transaction = connection.connection.BeginTransaction("UpdateClientVirtualQueueStatus");
                    cmd.Connection = connection.connection;
                    cmd.Transaction = transaction;
                                                          
                    try
                    {
                        cmd.CommandText = "UPDATE VirtualQueue SET IdVirtualQueueStatus = @IdStatus, IdArea = @IdArea WHERE IdVirtualQueue = @IdVirtualQueue";
                        cmd.Parameters.AddWithValue("@IdStatus", request.StatusId);
                        cmd.Parameters.AddWithValue("@IdArea", request.AreaId);
                        cmd.Parameters.AddWithValue("@IdVirtualQueue", request.IdVirtualQueue);
                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                        response.description = "VirtualQueue Client Updated!";
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Commit Exception Type: {0}", ex.GetType());
                        System.Diagnostics.Debug.WriteLine("  Message: {0}", ex.Message);                        
                        try
                        {
                            transaction.Rollback();
                        }
                        catch (Exception ex2) {
                            System.Diagnostics.Debug.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                            System.Diagnostics.Debug.WriteLine("  Message: {0}", ex2.Message);
                        }

                        response.description = "Error trying update the VirtualQueue client!";
                        response.error = true;
                    }
                }
                else
                {
                    response.description = "Error trying to connect to DB.";
                    response.error = true;
                }                
            }
            return response;
        }

        // PUT: api/VirtualQueue/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/VirtualQueue/5
        public void Delete(int id)
        {
        }
    }
}
