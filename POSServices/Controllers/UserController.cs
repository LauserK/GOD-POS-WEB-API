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
    public class UserController : ApiController
    {
        // GET: api/User
        public BasicResponse Get(String token = "", String idcompany="")
        {
            BasicResponse response = new BasicResponse { description = "User list", error = false };

            List<User> userList = new List<User>();
            Connection connection = new Connection();

            if(token == "")
            {
                response.error = true;
                response.description = "token empty";
                return response;
            }

            if (!Tools.isUserLogged(token, idcompany))
            {
                response.error = false;
                response.description = "bad user";
                return response;
            }

            string query = "SELECT UserCompany.IdUser, UserPaseo.FirstName, UserPaseo.IdentificationNumber FROM UserCompany INNER JOIN UserPaseo ON UserPaseo.IdUser = UserCompany.IdUser WHERE UserCompany.IdCompany = @IdCompany";
            if (connection.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection.connection);
                cmd.Parameters.AddWithValue("@IdCompany", idcompany);
                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        userList.Add(new User
                        {
                            id = dataReader["IdUser"].ToString(),
                            name = dataReader["FirstName"].ToString(),
                            identificationNumber = dataReader["IdentificationNumber"].ToString(),                            
                        });
                    }
                    //close Data Reader
                    dataReader.Close();
                }

                foreach (User user in userList)
                {
                    cmd.CommandText = "SELECT Template FROM UserFingerprint WHERE IdUser = '" + user.id + "'";                   
                    dataReader = cmd.ExecuteReader();

                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            user.fingerprint.Add(new Fingerprint
                            {
                                Template = dataReader["Template"].ToString()
                            });
                        }
                    }
                    dataReader.Close();
                }
                connection.CloseConnection();
            } else
            {
                response.error = true;
                response.description = "Error connecting to database";
            }

            response.data.AddRange(userList);
            return response;
        }        

        // GET: api/User/5
        public string Get(int id)
        {
            return "";
        }

        // POST: api/User
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/User/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/User/5
        public void Delete(int id)
        {
        }
    }
}
