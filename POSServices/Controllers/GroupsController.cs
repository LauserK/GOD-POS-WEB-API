using POSServices.Models;
using POSServices.Models.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace POSServices.Controllers
{
    public class GroupsController : ApiController
    {
        // GET: api/Groups
        public IEnumerable<Group> Get()
        {
            /*
             Get all the groups             
             */     
            List<Group> groups = new List<Group>();
            Connection connection = new Connection();

            string query = "SELECT * FROM SubCategory ORDER BY Name";
            if (connection.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection.connection);

                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        groups.Add(new Group {
                            auto = dataReader["IdSubCategory"].ToString(),
                            name = dataReader["Name"].ToString().ToUpper()
                        });
                    }
                    //close Data Reader
                    dataReader.Close();
                    connection.CloseConnection();                
                }
            }       

            return groups;
        }

        [Route("api/groups/{AreaId}/groups")]
        public IEnumerable<Group> GetGroupsFromArea(int AreaId)
        {
            /*
             Get all the groups             
             */
            List<Group> groups = new List<Group>();
            Connection connection = new Connection();

            string query = "SELECT * FROM SubCategory ORDER BY Name";
            if (connection.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection.connection);

                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        groups.Add(new Group
                        {
                            auto = dataReader["IdSubCategory"].ToString(),
                            name = dataReader["Name"].ToString().ToUpper()
                        });
                    }
                    //close Data Reader
                    dataReader.Close();
                    connection.CloseConnection();
                }
            }

            return groups;
        }

        [Route("api/groups/{GroupId}/articles")]
        public IEnumerable<Article> GetArticlesFromGroup(int GroupId)
        {
            List<Article> articles = new List<Article>();

            Connection connection = new Connection();

            string query = "SELECT * FROM Product WHERE IdSubCategory = " + GroupId + " ORDER BY Name";
            if (connection.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection.connection);

                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        decimal IVA = decimal.Parse(dataReader["IVA"].ToString());
                        // Calculate the netprice
                        decimal netPrice = decimal.Parse(dataReader["price"].ToString()) / ((IVA / 100) + 1);
                        //decimal netPrice = decimal.Parse(dataReader["NetPrice"].ToString());

                        articles.Add(new Article
                        {
                            name = dataReader["Name"].ToString(),
                            barcode = dataReader["Barcode"].ToString(),
                            IVA = IVA,
                            netPrice = netPrice,
                            price = dataReader["price"].ToString(),
                            photo = "test.jpg"
                        });
                    }
                    //close Data Reader
                    dataReader.Close();
                    connection.CloseConnection();
                }
            }

            return articles;
        }

        // GET: api/Groups/5
        public IEnumerable<Group> Get(int AreaId)
        {
            /*
            Get all the groups from the area

            */
            List<Group> groups = new List<Group>();
            Connection connection = new Connection();

            string query = "SELECT * FROM Category ORDER BY Name";
            if (connection.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection.connection);

                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        groups.Add(new Group
                        {
                            auto = dataReader["IdCategory"].ToString(),
                            name = dataReader["Name"].ToString().ToUpper()
                        });
                    }
                    //close Data Reader
                    dataReader.Close();
                    connection.CloseConnection();
                }
            }

            return groups;
        }

        // POST: api/Groups
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Groups/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Groups/5
        public void Delete(int id)
        {
        }
    }
}
