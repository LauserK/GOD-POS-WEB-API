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

                if (request.Email != "" && request.Password != "" && request.MAC_Device != "")
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
                                name = dataReader["FirstName"].ToString() + " " + dataReader["LastName"].ToString(),
                                identificationNumber = dataReader["IdentificationNumber"].ToString(),
                                id = dataReader["IdUser"].ToString(),
                                token = token
                            };                            
                            dataReader.Close();


                            cmd.CommandText = "SELECT Company.IdCompany, Company.Name FROM UserCompany INNER JOIN Company ON Company.IdCompany = UserCompany.IdCompany WHERE IdUser = @IdUser";
                            cmd.Parameters.AddWithValue("@IdUser", user.id);

                            dataReader = cmd.ExecuteReader();

                            if (dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    user.companies.Add(new Company {
                                        IdCompany = dataReader["IdCompany"].ToString(),
                                        Name = dataReader["Name"].ToString()
                                    });
                                }
                            }

                            dataReader.Close();                            

                            cmd.CommandText = "SELECT IsCompanyLogin FROM DeviceUser WHERE IdUser = @IdUser AND IsCompanyLogin = 1 AND IdDevice IS NULL";                            
                            dataReader = cmd.ExecuteReader();
                            if (dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    dataReader.Close();
                                    cmd.CommandText = "UPDATE DeviceUser SET Token = '" + token + "' WHERE IdUser=@IdUser AND IdDevice IS NULL AND IsCompanyLogin=1";
                                    dataReader = cmd.ExecuteReader();

                                    if (dataReader.RecordsAffected > 0)
                                    {
                                        response.data.Add(user);
                                    }
                                }
                            } else
                            {
                                dataReader.Close();
                                cmd.CommandText = "INSERT INTO DeviceUser (IdUser, IsCompanyLogin, Token) VALUES (@IdUser,1,@Token)";
                                cmd.Parameters.AddWithValue("@Token", token);

                                dataReader = cmd.ExecuteReader();

                                if (dataReader.RecordsAffected > 0)
                                {
                                    response.data.Add(user);
                                }
                            }


                            return response;
                            /*
                            //Verify device 
                            String Id_Device = "";
                            Boolean createDevice = false;
                            cmd.CommandText = "SELECT IdDevice FROM Device WHERE MAC = @MAC";
                            cmd.Parameters.AddWithValue("@MAC", request.MAC_Device);
                            dataReader = cmd.ExecuteReader();

                            if (dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    Id_Device = dataReader["IdDevice"].ToString();
                                }
                                dataReader.Close();
                            } else
                            {
                                // device not found
                                // create device
                                createDevice = true;
                                dataReader.Close();
                                cmd.CommandText = "INSERT INTO Device (Name, MAC, Enabled) VALUES (@Name, @MAC, @Enable);SELECT SCOPE_IDENTITY() AS IdDevice";                                
                                cmd.Parameters.AddWithValue("@Name", "");
                                cmd.Parameters.AddWithValue("@Enable", "1");

                                dataReader = cmd.ExecuteReader();

                                if (dataReader.RecordsAffected == 0)
                                {
                                    response.error = true;
                                    response.description = "Error creating device";
                                    return response;
                                } else
                                {
                                    while (dataReader.Read())
                                    {
                                        Id_Device = dataReader["IdDevice"].ToString();
                                    }
                                    dataReader.Close();
                                }
                            }

                            //cmd.CommandText = "UPDATE UserPaseo SET Token = @Token WHERE IdUser = @IdUser";
                            cmd.CommandText = "SELECT IsCompanyLogin FROM DeviceUser WHERE IdUser = @IdUser AND IdDevice = @IdDevice AND IsCompanyLogin = 1";
                            cmd.Parameters.AddWithValue("@IdDevice", Id_Device);
                            dataReader = cmd.ExecuteReader();
                            if (dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    dataReader.Close();
                                    cmd.CommandText = "UPDATE DeviceUser SET Token = '"+ token +"' WHERE IdUser=@IdUser AND IdDevice=@IdDevice AND IsCompanyLogin=1";
                                    dataReader = cmd.ExecuteReader();

                                    if (dataReader.RecordsAffected > 0)
                                    {
                                        response.data.Add(user);
                                    }
                                }
                            }
                            else
                            {
                                dataReader.Close();
                                cmd.CommandText = "INSERT INTO DeviceUser (IdDevice, IdUser, IsCompanyLogin, Token) VALUES (@IdDevice, @IdUser,1,@Token)";                            
                                cmd.Parameters.AddWithValue("@Token", token);

                                dataReader = cmd.ExecuteReader();

                                if (dataReader.RecordsAffected > 0)
                                {
                                    response.data.Add(user);
                                }
                            }
                            */

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

        [Route("api/Login/user")]
        [HttpPost]
        public BasicResponse updateUserLogin([FromBody]RequestUpdateLoginUser request, String token, String idcompany)
        {
            BasicResponse response = new BasicResponse { description = "", error = false };
            Connection connection = new Connection();

            if (!Tools.isUserLogged(token, idcompany))
            {
                response.error = false;
                response.description = "bad user";
                return response;
            }

            String token2 = "";

            if (connection.OpenConnection())
            {
                if (request.Fingerprint != "" && request.IdUser != "")
                {
                    SqlCommand cmd = new SqlCommand("", connection.connection);
                    cmd.CommandText = "SELECT UserCompany.IdUser, UserPaseo.FirstName, UserPaseo.LastName, UserPaseo.IdentificationNumber FROM UserCompany INNER JOIN UserPaseo ON UserCompany.IdUser = UserPaseo.IdUser WHERE UserCompany.IdUser = @IdUser AND IdCompany = @IdCompany";
                    cmd.Parameters.AddWithValue("@IdUser", request.IdUser);
                    cmd.Parameters.AddWithValue("@IdCompany", idcompany);
                    SqlDataReader dataReader = cmd.ExecuteReader();

                    if (dataReader.HasRows)
                    {
                        User user = null;
                        while (dataReader.Read())
                        {
                            token2 = Tools.sha256_hash(dataReader["IdentificationNumber"].ToString() + "." + Tools.RandomString(2));
                            user = new User
                            {
                                name = dataReader["FirstName"].ToString() + " " + dataReader["LastName"].ToString(),
                                identificationNumber = dataReader["IdentificationNumber"].ToString(),
                                id = dataReader["IdUser"].ToString(),
                                token = token2
                            };
                        }
                        dataReader.Close();
                        cmd.CommandText = "SELECT IdDeviceUser FROM DeviceUser INNER JOIN Device ON Device.IdDevice = DeviceUser.IdDevice WHERE DeviceUser.IdUser = @IdUser AND Device.IdCompany = @IdCompany AND IsCompanyLogin = 0";
                        dataReader = cmd.ExecuteReader();                        
                        cmd.Parameters.AddWithValue("@Token", token2);

                        String IdDeviceUser = "";

                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                IdDeviceUser = dataReader["IdDeviceUser"].ToString();
                            }
                            dataReader.Close();
                            cmd.CommandText = "UPDATE DeviceUser SET Token = @Token, IdDevice = @Device WHERE IdDeviceUser = @IdDeviceUser";
                            cmd.Parameters.AddWithValue("@Device", request.IdDevice);
                            cmd.Parameters.AddWithValue("@IdDeviceUser", IdDeviceUser);                            
                            dataReader = cmd.ExecuteReader();

                            if (dataReader.RecordsAffected > 0)
                            {
                                response.data.Add(user);
                                response.description = "updated";
                                return response;
                            }
                            else
                            {
                                dataReader.Close();
                                response.error = true;
                                response.description = "error";
                                return response;
                            }
                        }
                        else
                        {
                            dataReader.Close();
                            cmd.CommandText = "INSERT INTO DeviceUser (IdDevice, IdUser, IsCompanyLogin, Token) VALUES (@IdDevice, @IdUser, 0, @Token)";
                            cmd.Parameters.AddWithValue("@IdDevice", request.IdDevice);
                            cmd.Parameters.AddWithValue("@Token", token2);
                            dataReader = cmd.ExecuteReader();

                            if (dataReader.RecordsAffected == 0)
                            {
                                response.error = true;
                                response.description = "Error creating DeviceUser";
                                return response;
                            }
                            else
                            {
                                response.data.Add(user);
                                response.description = "updated";
                                return response;
                            }
                        }
                    }
                    else
                    {
                        dataReader.Close();
                        response.error = true;
                        response.description = "user not found";
                        return response;
                    }
                } else if (request.Password != "" && idcompany != "")
                {
                    
                }

                
            } else
            {
                response.error = true;
                response.description = "Connection not found";
                return response;
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
