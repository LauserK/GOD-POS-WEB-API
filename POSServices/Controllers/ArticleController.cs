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
    public class ArticleController : ApiController
    {
        // GET: api/Article
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Article/{ClientId}/list
        [Route("api/Article/{ClientId}/list")]
        public BasicResponse GetArticlesFromAccount(string ClientId)
        {
            /*
             Get all the articles from account             
             */
            List<Article> articles = new List<Article>();
            BasicResponse response = new BasicResponse { };
            Connection connection = new Connection();

            string query = "SELECT IdClient FROM Client WHERE IdentificationNumber = '" + ClientId + "'";
            if (connection.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection.connection);

                SqlDataReader dataReader = cmd.ExecuteReader();
                response.error = false;

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {                        
                        response.description = "Articles from client: " + ClientId;

                        
                        query = "SELECT * FROM Sales WHERE IdClient = '" + dataReader["IdClient"].ToString() + "' AND IdSaleStatus = 1";
                        dataReader.Close();
                        cmd.CommandText = query;
                        dataReader = cmd.ExecuteReader();

                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                query = "SELECT Product.Name, Product.IVA, Product.barcode, LineSale.Unity, LineSale.Price FROM LineSale INNER JOIN Product ON LineSale.IdProduct = Product.IdProduct WHERE IdSale = '" + dataReader["IdSale"].ToString() + "'";
                                dataReader.Close();
                                cmd.CommandText = query;
                                dataReader = cmd.ExecuteReader();

                                if (dataReader.HasRows)
                                {
                                    while (dataReader.Read())
                                    {
                                        decimal IVA = decimal.Parse(dataReader["IVA"].ToString());
                                        // Calculate the netprice
                                        decimal netPrice = Math.Round(decimal.Parse(dataReader["price"].ToString()) / ((IVA / 100) + 1), 2);

                                        articles.Add(new Article
                                        {
                                            name = dataReader["Name"].ToString(),
                                            price = dataReader["Price"].ToString(),
                                            unity = decimal.Parse(dataReader["Unity"].ToString()),
                                            IVA = IVA,
                                            netPrice = netPrice,
                                            barcode = dataReader["barcode"].ToString()

                                        });
                                    }
                                    response.data.AddRange(articles);
                                    return response;
                                }
                            }

                            
                        } else
                        {
                            response.error = true;
                            response.description = "The client: " + ClientId + " does not have a sale open!";
                        }

                        response.data.AddRange(articles);
                        
                    }
                    //close Data Reader
                    dataReader.Close();
                    connection.CloseConnection();
                } else
                {                    
                    response.error = true;
                    response.description = "The client: " + ClientId + " does not exists!";                    
                }
            }

            return response;
        }

        // GET: api/Article/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Article
        [Route("api/Article/{ClientId}/addArticle")]
        [HttpPost]
        public BasicResponse Post(string ClientId, [FromBody]Sale sale)
        {
            /*
             Insert article into the shoppingcart of client
             */
            BasicResponse response = new BasicResponse { };
            Connection connection = new Connection();
            String IdSale = "";
            DateTime timeNow = DateTime.Now;

            // -- Verify first if the client already exists!
            string query = "SELECT IdClient, IdentificationNumber FROM Client WHERE IdentificationNumber = @ClientIDN";
            if (connection.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection.connection);
                cmd.Parameters.AddWithValue("@ClientIDN", ClientId);

                SqlDataReader dataReader = cmd.ExecuteReader();
                String fecha = DateTime.Now.ToString("yyyy/MM/dd");

                if (dataReader.HasRows == false)
                {
                    // Response with errors
                    response.description = "The specified client doesnt exists.";
                    response.error = true;                    

                    return response;
                } else
                {
                    while (dataReader.Read())
                    {
                        ClientId = dataReader["IdClient"].ToString();
                    }
                }
                dataReader.Close();

                System.Diagnostics.Debug.WriteLine("ClientID-->" + ClientId);

                // Verify if the Sale exists firsts
                query = "SELECT * FROM Sales WHERE IdSaleStatus = 1 AND IdClient = @IdClient";
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@IdClient", ClientId);
                dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows == false)
                {
                    dataReader.Close();
                    // Insert into Sale table
                    query = "INSERT INTO Sales (IdUser, StartDate, EndDate, Total, IdClient, IdSaleStatus, IdDevice, Hour, IdFiscalClient) VALUES (@IdUser1, @StartDate, @EndDate, 0.00, @IdClient, @IdSaleStatus, @IdDevice, @Hour, @IdFiscalClient);";
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@IdUser1", sale.UserId);
                    cmd.Parameters.AddWithValue("@StartDate", fecha);
                    cmd.Parameters.AddWithValue("@EndDate", fecha);                                       
                    cmd.Parameters.AddWithValue("@IdSaleStatus", "1");
                    cmd.Parameters.AddWithValue("@IdDevice", sale.DeviceId);
                    cmd.Parameters.AddWithValue("@Hour", timeNow);
                    cmd.Parameters.AddWithValue("@IdFiscalClient", sale.FiscalClientId);

                    System.Diagnostics.Debug.WriteLine("IdUser-->" + sale.UserId);
                    System.Diagnostics.Debug.WriteLine("StartDate-->" + fecha);
                    System.Diagnostics.Debug.WriteLine("EndDate-->" + fecha);
                    System.Diagnostics.Debug.WriteLine("Total-->" + 0);
                    System.Diagnostics.Debug.WriteLine("IdClient-->" + ClientId);
                    System.Diagnostics.Debug.WriteLine("IdSaleStatus-->" + "1");
                    System.Diagnostics.Debug.WriteLine("IdDevice-->" + sale.DeviceId);


                    dataReader = cmd.ExecuteReader();
                    
                    // Get the IdSale for the current user
                    if (dataReader.RecordsAffected <= 0)
                    {
                        response.error = true;
                        response.description = "Error trying to create the sale!";
                        return response;
                    }                 
                }

                // Get the IdSale from the active sale of user
                dataReader.Close();
                query = "SELECT IdSale FROM Sales WHERE IdUser = @IdUser AND IdSaleStatus = 1";
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@IdUser", sale.UserId);
                dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        IdSale = dataReader["IdSale"].ToString();
                    }
                }

                dataReader.Close();          
                // Insert the article to LineSale                
                query = "INSERT INTO LineSale (Price, NetPrice, IVA, Unity, IdSale, IdProduct, Added, Dispatched, IdSaleStatus) VALUES (@Price, @NetPrice, @IVA, @Unity, @IdSale, @IdProduct, @Added, @Dispatched, @IdSaleStatus)";
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@Price", sale.article.price);
                cmd.Parameters.AddWithValue("@NetPrice", sale.article.netPrice);
                cmd.Parameters.AddWithValue("@IVA", sale.article.IVA);
                cmd.Parameters.AddWithValue("@Unity", sale.article.unity);
                cmd.Parameters.AddWithValue("@IdSale", IdSale);
                cmd.Parameters.AddWithValue("@IdProduct", sale.article.id);
                cmd.Parameters.AddWithValue("@Added", timeNow);
                cmd.Parameters.AddWithValue("@Dispatched", timeNow);
                cmd.Parameters.AddWithValue("@IdSaleStatus", 1);

                System.Diagnostics.Debug.WriteLine("IdSale-->" + IdSale);

                dataReader = cmd.ExecuteReader();

                if (dataReader.RecordsAffected <= 0)
                {                    
                    response.description = "Error trying to save the article!";
                    response.error = true;
                    return response;
                }

                // Calculate total
                dataReader.Close();
                query = "SELECT * FROM LineSale WHERE IdSale = @IdSale";
                cmd.CommandText = query;
                dataReader = cmd.ExecuteReader();
                Decimal total = 0;

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        total = total + ( Decimal.Parse(dataReader["Price"].ToString()) * Decimal.Parse(dataReader["Unity"].ToString()) );
                    }
                    dataReader.Close();
                }

                // Set total
                query = "UPDATE Sales SET Total = @Total WHERE IdSale = @IdSale";
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@Total", total);

                dataReader = cmd.ExecuteReader();
                if (dataReader.RecordsAffected > 0)
                {
                    response.data.Add(sale.article);
                    response.description = "Article added to client";
                    return response;
                }
            }

            return response;
        }

        // PUT: api/Article/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Article/5
        public void Delete(int id)
        {
        }
    }
}
