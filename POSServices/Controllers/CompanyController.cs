using POSServices.Models;
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
    public class CompanyController : ApiController
    {
        // GET: api/Company
        public IEnumerable<string> Get()
        {
            return new string[] { };
        }

        // GET: api/Company/5
        public string Get(int id)
        {
            return "";
        }

        // POST: api/Company
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Company/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Company/5
        public void Delete(int id)
        {
        }

        [Route("api/Company/create")]
        [HttpPost]
        public BasicResponse createCompany([FromBody] RequestCreateCompany request)
        {
            BasicResponse response = new BasicResponse { error = true, description = "" };

            Connection connection = new Connection();
            AES aes;

            if (request.FiscalNumber == "" || request.Name == "" || request.user.id == "")
            {
                response.description = "Empty fields";
                response.error = true;
                return response;
            }

            if (connection.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("", connection.connection);

                /* Create Company */
                cmd.CommandText = "INSERT INTO Company (Name, FiscalNumber, [Key]) VALUES ('','', @Key); SELECT SCOPE_IDENTITY() AS IdCompany";
                cmd.Parameters.AddWithValue("@Key", Tools.RandomString(16, false));                                
                SqlDataReader dataReader = cmd.ExecuteReader();

                String IdCompany = "";

                if (dataReader.RecordsAffected > 0)
                {
                    while (dataReader.Read())
                    {
                        IdCompany = dataReader["IdCompany"].ToString();
                       
                        dataReader.Close();

                        aes = new AES(IdCompany);

                        cmd.CommandText = "UPDATE Company SET Name=@Name, FiscalNumber=@FiscalNumber WHERE IdCompany = @IdCompany";
                        cmd.Parameters.AddWithValue("@IdCompany", IdCompany);
                        cmd.Parameters.AddWithValue("@Name", aes.encrypt(request.Name));
                        cmd.Parameters.AddWithValue("@FiscalNumber", aes.encrypt(request.FiscalNumber));

                        dataReader = cmd.ExecuteReader();

                        if (dataReader.RecordsAffected == 0)
                        {
                            response.description = "Error updating company";
                            response.error = true;
                            return response;
                        }                        
                    }
                    dataReader.Close();

                    aes = new AES(IdCompany);

                    /* Associate User to company */
                    cmd.CommandText = "INSERT INTO UserCompany (IdUser, IdCompany) VALUES (@IdUser, @IdCompany)";
                    cmd.Parameters.AddWithValue("@IdUser", request.user.id);                    
                    dataReader = cmd.ExecuteReader();

                    if (dataReader.RecordsAffected == 0)
                    {
                        response.description = "Error associating user";
                        response.error = true;
                        return response;
                    }
                    dataReader.Close();

                    /* Create default client of company */
                    cmd.CommandText = "INSERT INTO Client (FirstName, IdentificationNumber, Address, IdPriority, IdCompany) VALUES (@FirstName, @IdentificationNumber, @Address, @IdPriority, @IdCompany)";
                    cmd.Parameters.AddWithValue("@FirstName", aes.encrypt("NO CONTRIBUYENTE"));
                    cmd.Parameters.AddWithValue("@IdentificationNumber", aes.encrypt("-"));
                    cmd.Parameters.AddWithValue("@Address", "NO APLICA");
                    cmd.Parameters.AddWithValue("@IdPriority", 1);                    

                    dataReader = cmd.ExecuteReader();

                    if (dataReader.RecordsAffected == 0)
                    {
                        response.description = "Error creating client";
                        response.error = true;
                        return response;
                    }
                    dataReader.Close();

                    /* Create default warehouse */
                    cmd.CommandText = "INSERT INTO WareHouse (Name, Description, Adress, IdCompany) VALUES (@WName, '', '', @IdCompany)";
                    cmd.Parameters.AddWithValue("@WName", aes.encrypt("ALMACEN POR DEFECTO"));                   

                    dataReader = cmd.ExecuteReader();

                    if (dataReader.RecordsAffected == 0)
                    {
                        response.description = "Error creating client";
                        response.error = true;
                        return response;
                    }
                    dataReader.Close();

                    /* Create taxes */
                    cmd.CommandText = "INSERT INTO Tax (Name, Percentage, IdCompany) VALUES (@TName, '0', @IdCompany)";
                    cmd.Parameters.AddWithValue("@TName", aes.encrypt("TASA POR DEFECTO 1"));

                    dataReader = cmd.ExecuteReader();

                    if (dataReader.RecordsAffected > 0)
                    {
                        dataReader.Close();

                        cmd.CommandText = "INSERT INTO Tax (Name, Percentage, IdCompany) VALUES (@TName0, '16', @IdCompany)";
                        cmd.Parameters.AddWithValue("@TName0", aes.encrypt("TASA POR DEFECTO 2"));

                        dataReader = cmd.ExecuteReader();

                        if (dataReader.RecordsAffected > 0)
                        {
                            dataReader.Close();

                            cmd.CommandText = "INSERT INTO Tax (Name, Percentage, IdCompany) VALUES (@TName1, '0', @IdCompany)";
                            cmd.Parameters.AddWithValue("@TName1", aes.encrypt("TASA POR DEFECTO 3"));

                            dataReader = cmd.ExecuteReader();
                            dataReader.Close();
                        }
                    }

                    /* Create a default area */

                    response.description = "Company created";
                    response.error = false;
                    return response;
                }
            } else
            {
                response.description = "Error connecting to database";
                response.error = true;
                return response;
            }

            return response;
        }
    }
}
