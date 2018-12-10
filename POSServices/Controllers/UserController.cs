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
        public BasicResponse Get()
        {
            BasicResponse response = new BasicResponse { description = "User list", error = false };

            List<User> userList = new List<User>();
            Connection connection = new Connection();

            string query = "SELECT * FROM UserPaseo";
            if (connection.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection.connection);

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
                            fingerprint = dataReader["PersonalAccount"].ToString()
                        });
                    }
                    //close Data Reader
                    dataReader.Close();
                    connection.CloseConnection();
                }
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
            return "value";
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
