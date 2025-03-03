﻿using POSServices.Models;
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
            return new string[] { };
        }

        [Route("api/Device/setup")]
        [HttpPost]
        public BasicResponse GetDeviceSetup([FromBody] RequestGetDeviceSetup request, string token = "", string idcompany = "")
        {
            BasicResponse response = new BasicResponse { description = "Device setup", error = false };
            Connection connection = new Connection();

            if (!Tools.isUserLogged(token, idcompany))
            {
                response.error = false;
                response.description = "bad user";
                return response;
            }

            if (connection.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("", connection.connection);
                cmd.CommandText = "SELECT IdTax1, IdTax2, IdTax3, IdPrinterType FROM DeviceSetup WHERE IdDevice = @IdDevice";
                cmd.Parameters.AddWithValue("@IdDevice", request.IdDevice);
                SqlDataReader dataReader = cmd.ExecuteReader();
                
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        response.data.Add(new TaxDevice
                        {
                            IdTax1 = dataReader["IdTax1"].ToString(),
                            IdTax2 = dataReader["IdTax2"].ToString(),
                            IdTax3 = dataReader["IdTax3"].ToString(),
                            IdPrinterType = dataReader["IdPrinterType"].ToString()
                        });
                    }
                }
            }

            return response;
        }

        // GET: api/Device/5
        [Route("api/Device/verify")]
        [HttpPost]
        public BasicResponse Get([FromBody] RequestGetDevice request, string token = "", string idcompany = "")
        {
            BasicResponse response = new BasicResponse { description = "Device found", error = true };
            Connection connection = new Connection();

            AES aes = new AES(idcompany);            

            if (!Tools.isUserLogged(token, idcompany))
            {
                response.error = false;
                response.description = "bad user";
                return response;
            }

            string query = "SELECT Name, MAC, IdDevice, IdArea, Enabled FROM Device WHERE MAC = @mac AND IdCompany = @IdCompany";
            if (connection.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection.connection);
                cmd.Parameters.AddWithValue("@mac", request.mac);
                cmd.Parameters.AddWithValue("@IdCompany", idcompany);

                SqlDataReader dataReader = cmd.ExecuteReader();
                String IdDevice = "";
                String IdUser = "";

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        IdDevice = dataReader["IdDevice"].ToString();
                        response.data.Add(new Device {
                            IdDevice = IdDevice,
                            Name     = dataReader["Name"].ToString(),
                            MAC      = dataReader["MAC"].ToString(),
                            IdArea   = dataReader["IdArea"].ToString(),
                            Enabled  = (bool) dataReader["Enabled"]
                        });                        
                    }
                    dataReader.Close();                   
                }
                else
                {
                    dataReader.Close();
                    query = "INSERT INTO Device (Name, MAC, Enabled, IdCompany) VALUES (@Name, @mac, @Enable, @IdCompany); SELECT SCOPE_IDENTITY() AS IdDevice";
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Name", "");                    
                    cmd.Parameters.AddWithValue("@Enable","1");

                    dataReader = cmd.ExecuteReader();

                    if (dataReader.HasRows) {
                        while (dataReader.Read())
                        {
                            IdDevice = dataReader["IdDevice"].ToString();
                            response.data.Add(new Device
                            {
                                IdDevice = IdDevice,
                                Name = "",
                                MAC = request.mac,                                
                                Enabled = true
                            });
                        }
                    }

                    if (dataReader.RecordsAffected > 0)
                    {                        
                        dataReader.Close();
                    }
                    else
                    {
                        dataReader.Close();
                        response.error = true;
                        response.description = "Device not found";
                        return response;
                    }
                }

                cmd.CommandText = "UPDATE DeviceUser SET IdDevice=@IdDevice WHERE Token=@Token";
                cmd.Parameters.AddWithValue("@IdDevice", IdDevice);
                cmd.Parameters.AddWithValue("@Token", token);

                dataReader = cmd.ExecuteReader();

                if (dataReader.RecordsAffected > 0)
                {
                    dataReader.Close();
                    return response;
                } else
                {
                    dataReader.Close();
                    response.error = true;
                    response.description = "Error updating Device";
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
