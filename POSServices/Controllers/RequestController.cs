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
    public class RequestController : ApiController
    {
        // GET: api/Request
        public BasicResponse Get()
        {
            BasicResponse response = new BasicResponse { };
            Connection connection = new Connection();
            List<RequestType> requestTypeList = new List<RequestType>();
            if (connection.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("", connection.connection);
                cmd.CommandText = "SELECT IdRequestType, Name FROM RequestType WHERE Enabled = 1";
                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        requestTypeList.Add(new RequestType
                        {
                            IdRequestType = dataReader["IdRequestType"].ToString(),
                            Name = dataReader["Name"].ToString()
                        });
                    }
                }
            }

            response.data.AddRange(requestTypeList);
            return response;
        }

        // GET: api/Request/5
        public BasicResponse Get(int UserId)
        {
            BasicResponse response = new BasicResponse { description = "", error = false };           
            return response;
        }

        [Route("api/Request/{UserId}/list")]
        public BasicResponse GetUserList(int UserId)
        {
            BasicResponse response = new BasicResponse { description = "", error = false };
            Connection connection = new Connection();

            if (connection.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("", connection.connection);
                cmd.CommandText = "SELECT Request.IdRequest, Request.IdRequestType, RequestType.Name, FORMAT(Request.RequestDate, 'dd/MM/yyyy') as RequestDate, FORMAT(Request.DeliverDate, 'dd/MM/yyyy') as DeliverDate," +
                    "FORMAT(Request.ApproximateDeliverDate, 'dd/MM/yyyy') AS ApproximateDeliverDate, Request.IdRequestStatus, RequestStatus.Name" +
                    " FROM Request" + 
                    " INNER JOIN RequestType ON Request.IdRequestType = RequestType.IdRequestType"+
                    " INNER JOIN RequestStatus ON Request.IdRequestStatus = RequestStatus.IdRequestStatus"+
                    " WHERE Request.IdUser = @IdUser";
                cmd.Parameters.AddWithValue("@IdUser", UserId);

                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        RequestType rqt = new RequestType {
                            IdRequestType = dataReader["IdRequestType"].ToString(),
                            Name = dataReader["Name"].ToString()
                        };

                        RequestStatus rqs = new RequestStatus
                        {
                            IdRequestStatus = dataReader["IdRequestStatus"].ToString(),
                            Name = dataReader["Name"].ToString()
                        };

                        response.data.Add(new Request
                        {
                            RequestStatus = rqs,
                            RequestType = rqt,
                            IdUser = UserId.ToString(),
                            ApproximateDeliverDate = dataReader["ApproximateDeliverDate"].ToString(),
                            DeliverDate = dataReader["DeliverDate"].ToString(),
                            IdRequest = dataReader["IdRequest"].ToString(),
                            RequestDate = dataReader["RequestDate"].ToString()
                        });
                    }
                }                  
            }

            return response;
        }

        // POST: api/Request
        public BasicResponse Post([FromBody]RequestSaveRequest request)
        {
            BasicResponse response = new BasicResponse { description = "", error = false };
            Connection connection = new Connection();
            DateTime date = DateTime.Now; //.ToString("yyyy/MM/dd");

            if (connection.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("", connection.connection);

                // get days 
                cmd.CommandText = "SELECT Days FROM RequestType WHERE IdRequestType = '" + request.IdRequestType + "'";
                SqlDataReader dataReader = cmd.ExecuteReader();                
                int baseDays = 0;
                int days = 0;
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        baseDays = (int) dataReader["Days"];
                    }

                    dataReader.Close();                    
                    //date = date.AddDays(days);                     

                    // Verificar si los dias son feriados (Sabado y Domingo) pronto demas dias
                    for (var i = 0; i < baseDays; i++)
                    {
                        DateTime d = DateTime.Now;
                        days += 1;
                        date = d.AddDays(days);
                        DayOfWeek dateDayOfWeek = date.DayOfWeek;
                        if (dateDayOfWeek == DayOfWeek.Saturday || dateDayOfWeek == DayOfWeek.Sunday)
                        {
                            i--;
                        }

                    }     

                    cmd.CommandText = "INSERT INTO Request (IdRequestType, IdUser, RequestDate, ApproximateDeliverDate, IdRequestStatus) VALUES (@IdRequestType, @IdUser, @RequestDate, @ApproximateDeliverDate, @IdRequestStatus)";
                    cmd.Parameters.AddWithValue("@IdRequestType", request.IdRequestType);
                    cmd.Parameters.AddWithValue("@IdUser", request.IdUser);
                    cmd.Parameters.AddWithValue("@RequestDate", DateTime.Now.ToString("yyyy/MM/dd"));
                    cmd.Parameters.AddWithValue("@ApproximateDeliverDate", date.ToString("yyyy/MM/dd"));
                    cmd.Parameters.AddWithValue("@IdRequestStatus", request.IdRequestStatus);

                    Request req = new Request
                    {
                        ApproximateDeliverDate = date.ToString("yyyy/MM/dd"),
                        IdRequestType = request.IdRequestType,
                        IdRequestStatus = request.IdRequestStatus,
                        IdUser = request.IdUser,
                        RequestDate = DateTime.Now.ToString("yyyy/MM/dd")
                    };
                    
                    try
                    {
                        dataReader = cmd.ExecuteReader();
                    } catch (Exception)
                    {
                        response.error = true;
                        response.description = "Error inserting to db";
                        return response;
                    }

                    if (dataReader.RecordsAffected == 0) {
                        response.error = true;
                        response.description = "Record not saved";
                        return response;
                    }

                    response.data.Add(req);

                } else
                {
                    response.error = true;
                    response.description = "IdRequestType doesnt exists!";
                    return response;
                }
            }

            response.description = "";
            return response;
        }

        // PUT: api/Request/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Request/5
        public void Delete(int id)
        {
        }
    }
}
