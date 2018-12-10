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
    public class ClientController : ApiController
    {
        // GET: api/Client
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Client/5
        public BasicResponse Get(string id)
        {
            BasicResponse response = new BasicResponse { error = false, description = "Client" };
            List<Client> clients = new List<Client>();
            Connection connection = new Connection();

            if (connection.OpenConnection()) {
                SqlCommand cmd = new SqlCommand("", connection.connection);
                cmd.CommandText = "SELECT IdClient, FirstName, LastName, IdentificationNumber, Address, PhoneNumber, BirthDay FROM Client WHERE IdentificationNumber = @ID";
                cmd.Parameters.AddWithValue("@ID", id);
                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read()) {

                        clients.Add(new Client
                        {
                            IdClient = dataReader["IdClient"].ToString(),
                            Name = dataReader["FirstName"].ToString() + " " + dataReader["LastName"].ToString(),
                            IdentificationNumber = dataReader["IdentificationNumber"].ToString(),
                            Address = dataReader["Address"].ToString(),
                            PhoneNumber = dataReader["PhoneNumber"].ToString(),
                            BirthDay = dataReader["BirthDay"].ToString()
                        });
                    }

                    response.data.AddRange(clients);
                } else
                {
                    response.description = "Client do not exists!";
                    response.error = true;
                }                
            } else
            {
                response.error = true;
                response.description = "Error connecting to Database";
            }


            return response;
        }

        // POST: api/Client
        public BasicResponse Post([FromBody]Client client)
        {
            BasicResponse response = new BasicResponse { error = false, description = "" };
            String fecha = DateTime.Now.ToString("yyyy/MM/dd");

            if (client != null)
            {
                Connection connection = new Connection();

                if (connection.OpenConnection())
                {
                    SqlCommand cmd = connection.connection.CreateCommand();
                    SqlTransaction transaction;

                    // Start a local transaction.
                    transaction = connection.connection.BeginTransaction("CreateClient");
                    cmd.Connection = connection.connection;
                    cmd.Transaction = transaction;

                    try
                    {
                        cmd.CommandText = "INSERT INTO Client (FirstName, IdentificationNumber, Address, IdPriority, RegistrationDate) VALUES (@Name, @IdentificationNumber, @Address, 1, @Date)";
                        cmd.Parameters.AddWithValue("@Name", client.Name);
                        cmd.Parameters.AddWithValue("@IdentificationNumber", client.IdentificationNumber);
                        cmd.Parameters.AddWithValue("@Address", client.Address);
                        cmd.Parameters.AddWithValue("@Date", fecha);
                        cmd.ExecuteNonQuery();
                        transaction.Commit();

                        cmd.CommandText = "SELECT * FROM Client WHERE IdentificationNumber = @IdNumber";
                        cmd.Parameters.AddWithValue("@IdNumber", client.IdentificationNumber);
                        SqlDataReader dataReader = cmd.ExecuteReader();

                        if (dataReader.HasRows) {
                            while (dataReader.Read())
                            {
                                response.data.Add(new Client
                                {
                                    IdClient = dataReader["IdClient"].ToString(),
                                    Name = dataReader["FirstName"].ToString() + " " + dataReader["LastName"].ToString(),
                                    IdentificationNumber = dataReader["IdentificationNumber"].ToString(),
                                    Address = dataReader["Address"].ToString(),
                                    PhoneNumber = dataReader["PhoneNumber"].ToString(),
                                    BirthDay = dataReader["BirthDay"].ToString()
                                });
                            }
                            response.description = "created";
                        } else
                        {
                            response.description = "not created";
                            response.error = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Commit Exception Type: {0}", ex.GetType());
                        System.Diagnostics.Debug.WriteLine("  Message: {0}", ex.Message);
                        try
                        {
                            transaction.Rollback();
                        }
                        catch (Exception ex2)
                        {
                            System.Diagnostics.Debug.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                            System.Diagnostics.Debug.WriteLine("  Message: {0}", ex2.Message);
                        }

                        response.description = "Error trying to update the article!";
                        response.error = true;
                    }
                }
                else
                {
                    response.description = "Error trying to connect to DB.";
                    response.error = true;
                }

            }
            else
            {
                response.description = "Empty JSON!";
                response.error = true;
            }

            return response;
        }

        // PUT: api/Client/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Client/5
        public void Delete(int id)
        {
        }
    }
}
