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

        [Route("api/groups/list")]
        public BasicResponse GetAllGroupsFromCompany(string token = "", string idcompany = "")
        {
            List<Group> groups = new List<Group>();
            Connection connection = new Connection();
            BasicResponse response = new BasicResponse { description = "group list", error = false };
            AES aes = new AES(idcompany);

            if (!Tools.isUserLogged(token, idcompany))
            {
                response.error = false;
                response.description = "bad user";
                return response;
            }

            string query = "SELECT SubCategory.IdSubCategory, SubCategory.Name FROM SubCategory INNER JOIN Category ON Category.IdCategory = SubCategory.IdCategory WHERE Category.IdCompany = @IdCompany ORDER BY Name";
            if (connection.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection.connection);                
                cmd.Parameters.AddWithValue("@IdCompany", idcompany);

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

            response.data.AddRange(groups);

            return response;
        }

        [Route("api/groups/{AreaId}/groups")]
        public BasicResponse GetGroupsFromArea(int AreaId, string token = "", string idcompany = "", string user = "")
        {
            /*
             Get all the groups             
             */
            List<Group> groups = new List<Group>();
            Connection connection = new Connection();
            BasicResponse response = new BasicResponse { description = "group list", error = false };
            AES aes = new AES(idcompany);

            if (!Tools.isUserLogged(token, idcompany))
            {
                response.error = false;
                response.description = "bad user";
                return response;
            }
            /*
            if (!Tools.isUserLogged(user, idcompany))
            {
                response.error = false;
                response.description = "bad user token";
                return response;
            }*/


            string query = "SELECT SubCategory.IdSubCategory, SubCategory.Name FROM SubCategoryArea INNER JOIN SubCategory ON SubCategory.IdSubCategory = SubCategoryArea.IdSubCategory INNER JOIN Category ON Category.IdCategory = SubCategory.IdCategory WHERE IdArea = @IdArea AND IdCompany = @IdCompany ORDER BY Name";
            if (connection.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection.connection);
                cmd.Parameters.AddWithValue("@IdArea", AreaId);
                cmd.Parameters.AddWithValue("@IdCompany", idcompany);

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

            response.data.AddRange(groups);

            return response;
        }

        [Route("api/groups/{GroupId}/articles")]
        public BasicResponse GetArticlesFromGroup(int GroupId, string token = "", string idcompany = "", string user = "")
        {
            List<Article> articles = new List<Article>();

            Connection connection = new Connection();
            BasicResponse response = new BasicResponse { error = false, description = "Article list" };
            AES aes = new AES(idcompany);

            if (!Tools.isUserLogged(token, idcompany))
            {
                response.error = false;
                response.description = "bad user";
                return response;
            }

            if (!Tools.isUserLogged(user, idcompany))
            {
                response.error = false;
                response.description = "bad user token";
                return response;
            }

            string query = "SELECT Product.IdProduct, Product.Name, Product.Barcode, Product.IVA, Product.price, Product.NetPrice, Tax.Percentage as Tax, Product.IdTax, Product.IsSoldByWeight FROM Product INNER JOIN Tax ON Product.IdTax = Tax.IdTax WHERE IdSubCategory = @GroupID AND Product.IdCompany = @IdCompany ORDER BY Name";
            if (connection.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection.connection);
                cmd.Parameters.AddWithValue("@GroupId", GroupId);
                cmd.Parameters.AddWithValue("@IdCompany", idcompany);
                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {                                 
                        articles.Add(new Article
                        {
                            id = int.Parse(dataReader["IdProduct"].ToString()),
                            name = dataReader["Name"].ToString(),
                            barcode = dataReader["Barcode"].ToString(),
                            IVA = decimal.Parse(dataReader["IVA"].ToString()),
                            netPrice = decimal.Parse(dataReader["NetPrice"].ToString()),
                            price = decimal.Parse(dataReader["price"].ToString()),
                            photo = "test.jpg",
                            tax = decimal.Parse(dataReader["Tax"].ToString()),
                            Idtax = dataReader["IdTax"].ToString(),
                            IsSoldByWeight = (bool)dataReader["IsSoldByWeight"]
                        });
                    }
                    //close Data Reader
                    dataReader.Close();
                    connection.CloseConnection();
                }
            } else
            {
                response.error = true;
                response.description = "Error conecting to database";
            }

            response.data.AddRange(articles);
            return response;
        }

        // GET: api/Groups/5
        public IEnumerable<Group> Get(int AreaId, string token = "", string idcompany = "")
        {
            /*
            Get all the groups from the area

            */
            List<Group> groups = new List<Group>();
            Connection connection = new Connection();
            AES aes = new AES(idcompany);

            if (!Tools.isUserLogged(token, idcompany))
            {                
                return groups;
            }

            string query = "SELECT * FROM Category WHERE IdCompany = @IdCompany ORDER BY Name";
            if (connection.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection.connection);
                cmd.Parameters.AddWithValue("@IdCompany", idcompany);
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
