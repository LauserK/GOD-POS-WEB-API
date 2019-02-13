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
    public class LoginController : ApiController
    {
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
        public BasicResponse Post([FromBody]RequestLogin request)
        {
            BasicResponse response = new BasicResponse { };
            Connection connection = new Connection();

            if (connection.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("", connection.connection);

                if (request.Email != "" && request.Password != "")
                {
                    cmd.CommandText = "SELECT * FROM UserPaseo WHERE Email = @email AND Password = @password";
                    cmd.Parameters.AddWithValue("@email", request.Email.ToLower());
                    cmd.Parameters.AddWithValue("@password", Tools.sha256_hash(request.Password));

                    SqlDataReader dataReader = cmd.ExecuteReader();

                    if (dataReader.HasRows)
                    {
                        while(dataReader.Read())
                        {
                            String token = Tools.sha256_hash(dataReader["IdentificationNumber"].ToString() + "." + Tools.RandomString(2));
                            User user = new User
                            {
                                name = dataReader["FirstName"].ToString() + dataReader["LastName"].ToString(),
                                identificationNumber = dataReader["IdentificationNumber"].ToString(),
                                id = dataReader["IdUser"].ToString(),
                                token = token
                            };                            
                            dataReader.Close();
                            cmd.CommandText = "UPDATE UserPaseo SET Token = @Token WHERE IdUser = @IdUser";
                            cmd.Parameters.AddWithValue("@Token", token);
                            cmd.Parameters.AddWithValue("@IdUser", user.id);

                            dataReader = cmd.ExecuteReader();

                            if (dataReader.RecordsAffected > 0)
                            {
                                response.data.Add(user);
                            }
                        }
                    }
                    else
                    {
                        response.description = "user not found";
                        response.error = true;
                        return response;
                    }
                }
                else
                {
                    return response;
                }
            }

            return response;
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
