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
    public class WorkingDayController : ApiController
    {
        // GET: api/WorkingDay
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/WorkingDay/5
        [Route("api/WorkingDay/{IdDevice}/")]
        public BasicResponse Get(int IdDevice)
        {
            Connection connection = new Connection();
            BasicResponse response = new BasicResponse { description = "" };

            if (connection.OpenConnection())
            {
                WorkingDay workingDay = new WorkingDay { };
                DateTime today = DateTime.Now;
                DateTime yesterday = today.AddDays(-1);

                SqlCommand cmd = new SqlCommand("", connection.connection);
                cmd.CommandText = "SELECT WorkingDaySession.IdWorkingDaySession, CAST(DateBegin AS DATE) as DateBegin, UserPaseo.IdUser, UserPaseo.FirstName, UserPaseo.LastName, UserPaseo.IdentificationNumber FROM WorkingDaySession INNER JOIN UserPaseo ON UserPaseo.IdUser = WorkingDaySession.IdUser INNER JOIN Device ON Device.IdDevice = WorkingDaySession.IdDevice WHERE WorkingDaySession.IdDevice = @IdDevice AND Active = 1";
                cmd.Parameters.AddWithValue("@IdDevice", IdDevice);                

                SqlDataReader dataReader = cmd.ExecuteReader();
                
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        User userObject = new User
                        {
                            id = dataReader["IdUser"].ToString(),
                            name = dataReader["FirstName"].ToString() + dataReader["LastName"].ToString(),
                            identificationNumber = dataReader["IdentificationNumber"].ToString()
                        };

                        workingDay.Date = DateTime.Parse(dataReader["DateBegin"].ToString()).ToString("yyyy/MM/dd");
                        workingDay.IdWorkingDaySession = dataReader["IdWorkingDaySession"].ToString();
                        workingDay.User = userObject;

                        // if exists an Active session of day before
                        if (yesterday.ToString("yyyy/MM/dd") == workingDay.Date)
                        {
                            workingDay.mustClose = true;
                        }
                    }
                    response.data.Add(workingDay);
                }
            }

            return response;
        }

        // POST: api/WorkingDay
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/WorkingDay/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/WorkingDay/5
        public void Delete(int id)
        {
        }
    }
}
