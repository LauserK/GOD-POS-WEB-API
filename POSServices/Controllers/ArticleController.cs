﻿using Newtonsoft.Json;
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
    public class ArticleController : ApiController
    {
        // GET: api/Article
        public IEnumerable<string> Get()
        {
            return new string[] {};
        }

        // GET: api/Article/{ClientId}/list
        [Route("api/Article/{ClientId}/list")]
        public BasicResponse GetArticlesFromAccount(string ClientId, string token = "", string idcompany="", string user="", string iddevice ="")
        {
            /*
             Get all the articles from account
             */
            List<Article> articles = new List<Article>();
            BasicResponse response = new BasicResponse {error = false };
            Connection connection = new Connection();
            AES aes = new AES(idcompany);

            string query = "";
            if (connection.OpenConnection() == true)
            {
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

                SqlCommand cmd = new SqlCommand("", connection.connection);
                cmd.CommandText = "SELECT IdClient FROM Client WHERE IdentificationNumber = @ClientId AND IdCompany = @IdCompany";
                cmd.Parameters.AddWithValue("@ClientId", ClientId);
                cmd.Parameters.AddWithValue("@IdCompany", idcompany);
                SqlDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        response.description = "Articles from client: " + ClientId;
                        query = "SELECT * FROM Sales WHERE IdClient = '" + dataReader["IdClient"].ToString() + "' AND IdSaleStatus = 1";
                        dataReader.Close();
                        if (ClientId == "-")
                        {
                            query = query + " AND IdDevice = @Device";
                            cmd.Parameters.AddWithValue("@Device", iddevice);
                        }
                        cmd.CommandText = query;
                        dataReader = cmd.ExecuteReader();

                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                query = "SELECT LineSale.IdLineSale, LineSale.IdProduct, Product.Name, Product.IsSoldByWeight, LineSale.IVA, Product.barcode, LineSale.Unity, LineSale.Price, LineSale.NetPrice, Tax.Percentage AS Tax, LineSale.Idtax FROM LineSale INNER JOIN Product ON LineSale.IdProduct = Product.IdProduct INNER JOIN Tax ON Product.IdTax = Tax.IdTax WHERE IdSale = '" + dataReader["IdSale"].ToString() + "'";
                                response.description = dataReader["IdSale"].ToString();
                                dataReader.Close();
                                cmd.CommandText = query;
                                dataReader = cmd.ExecuteReader();

                                if (dataReader.HasRows)
                                {
                                    while (dataReader.Read())
                                    {
                                        articles.Add(new Article
                                        {
                                            id = (int)dataReader["IdProduct"],
                                            name =  dataReader["Name"].ToString(),
                                            price = decimal.Parse(dataReader["Price"].ToString()),
                                            unity = decimal.Parse(dataReader["Unity"].ToString()),
                                            IVA = decimal.Parse(dataReader["IVA"].ToString()),
                                            netPrice = decimal.Parse(dataReader["NetPrice"].ToString()),
                                            barcode = dataReader["barcode"].ToString(),
                                            lineSaleId = dataReader["IdLineSale"].ToString(),
                                            tax = decimal.Parse(dataReader["Tax"].ToString()),
                                            Idtax = dataReader["IdTax"].ToString(),
                                            IsSoldByWeight = (bool)dataReader["IsSoldByWeight"]
                                        });
                                    }
                                    response.data.AddRange(articles);
                                    return response;
                                }
                            }
                        }
                        else
                        {
                            response.error = true;
                            response.description = "The client: " + ClientId + " does not have a sale open!";
                        }

                        response.data.AddRange(articles);

                    }
                    //close Data Reader
                    dataReader.Close();
                    connection.CloseConnection();
                }
                else
                {
                    response.error = true;
                    response.description = "The client: " + ClientId + " does not exists!";
                }
            }

            return response;
        }

        [Route("api/Article/{barcode}")]
        [HttpGet]
        public BasicResponse Get(string barcode, string token = "", string idcompany = "", string user = "")
        {
            /*
             Get all the groups
             */
            List<Article> articles = new List<Article>();
            Connection connection = new Connection();
            BasicResponse response = new BasicResponse { };
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

            if (connection.OpenConnection() == true)
            {
                if (!Tools.isUserLogged(token, idcompany))
                {
                    response.error = false;
                    response.description = "bad user";
                    return response;
                }

                SqlCommand cmd = new SqlCommand("", connection.connection);
                string query = "";
                query = "SELECT Product.IdProduct, Product.Name, Product.Barcode, Product.IVA, Product.price, Product.NetPrice, Tax.Percentage as Tax, Product.IdTax, Product.IsSoldByWeight FROM Product INNER JOIN Tax ON Product.IdTax = Tax.IdTax WHERE Barcode = @Barcode AND Product.IdCompany = @IdCompany ORDER BY Name";
                if (barcode.Length > 1)
                {
                    if (barcode.Substring(0, 2) == "31" && barcode.Length == 13)
                    {
                        // its homemade barcode
                        query = "SELECT Product.IdProduct, Product.Name, Product.Barcode, Product.IVA, Product.price, Product.NetPrice, Tax.Percentage as Tax, Product.IdTax, Product.IsSoldByWeight FROM Product INNER JOIN Tax ON Product.IdTax = Tax.IdTax WHERE Product.IdProduct = @Barcode AND Product.IdCompany = @IdCompany ORDER BY Name";
                        barcode = long.Parse(barcode.Substring(2, 5)).ToString();
                    }
                }
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@Barcode", barcode);
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
                            IsSoldByWeight = (bool) dataReader["IsSoldByWeight"]
                        });
                    }
                    //close Data Reader
                    dataReader.Close();

                } else
                {
                    //close Data Reader
                    dataReader.Close();
                    query = "SELECT Product.IdProduct, Product.Name, Product.Barcode, Product.IVA, Product.price, Product.NetPrice, Tax.Percentage as Tax, Product.IdTax, Product.IsSoldByWeight FROM ProductAlternativeCodes INNER JOIN Product ON Product.IdProduct = ProductAlternativeCodes.IdProduct INNER JOIN Tax ON Product.IdTax = Tax.IdTax WHERE ProductAlternativeCodes.AlternativeCode = @Barcode AND Product.IdCompany = @IdCompany ORDER BY Name";
                    cmd.CommandText = query;
                    dataReader = cmd.ExecuteReader();

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
                    }
                    //close Data Reader
                    dataReader.Close();
                }

                connection.CloseConnection();
            }



            response.data.AddRange(articles);

            return response;
        }

        // POST: api/Article
        [Route("api/Article/{ClientId}/addArticle")]
        [HttpPost]
        public BasicResponse Post(string ClientId, [FromBody]Sale sale, string token = "", string idcompany = "", string user = "")
        {
            /*
             Insert article into the shoppingcart of client
             */
            BasicResponse response = new BasicResponse { };
            Connection connection = new Connection();
            String IdSale = "";
            DateTime timeNow = DateTime.Now;
            AES aes = new AES(idcompany);
            string IdentificationNumber = ClientId;

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

            // -- Verify first if the client already exists!
            string query = "SELECT IdClient, IdentificationNumber FROM Client WHERE IdentificationNumber = @ClientIDN AND IdCompany = @IdCompany";
            if (connection.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection.connection);
                cmd.Parameters.AddWithValue("@ClientIDN", ClientId);
                cmd.Parameters.AddWithValue("@IdCompany", idcompany);

                SqlDataReader dataReader = cmd.ExecuteReader();
                String fecha = DateTime.Now.ToString("yyyy/MM/dd");

                if (dataReader.HasRows == false)
                {
                    // Response with errors
                    response.description = "The specified client doesnt exists.";
                    response.error = true;

                    return response;
                }
                else
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
                    query = "INSERT INTO Sales (IdUser, StartDate, EndDate, Total, IdClient, IdSaleStatus, IdDevice, Hour, IdFiscalClient, IdCompany) VALUES (@IdUser1, @StartDate, @EndDate, 0.00, @IdClient, @IdSaleStatus, @IdDevice, @Hour, @IdFiscalClient, @IdCompany);";
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
                query = "SELECT IdSale FROM Sales WHERE IdUser = @IdUser AND IdSaleStatus = 1 AND IdCompany = @IdCompany";
                if (IdentificationNumber == "-")
                {
                    query = query + " AND IdDevice = @Device";
                    cmd.Parameters.AddWithValue("@Device", sale.DeviceId);
                }

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
                query = "INSERT INTO LineSale (Price, NetPrice, IVA, Unity, IdSale, IdProduct, Added, IdSaleStatus, IdTax) VALUES (@Price, @NetPrice, @IVA, @Unity, @IdSale, @IdProduct, @Added, @IdSaleStatus2, @IdTax)";
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@Price", sale.article.price);
                cmd.Parameters.AddWithValue("@NetPrice", sale.article.netPrice);
                cmd.Parameters.AddWithValue("@IVA", sale.article.IVA);
                cmd.Parameters.AddWithValue("@Unity", sale.article.unity);
                cmd.Parameters.AddWithValue("@IdSale", IdSale);
                cmd.Parameters.AddWithValue("@IdProduct", sale.article.id);
                cmd.Parameters.AddWithValue("@Added", timeNow);
                //cmd.Parameters.AddWithValue("@Dispatched", timeNow);
                cmd.Parameters.AddWithValue("@IdSaleStatus2", 1);
                cmd.Parameters.AddWithValue("@IdTax", sale.article.Idtax);

                System.Diagnostics.Debug.WriteLine("IdSale-->" + IdSale);

                dataReader = cmd.ExecuteReader();

                if (dataReader.RecordsAffected <= 0)
                {
                    response.description = "Error trying to save the article!";
                    response.error = true;
                    return response;
                }
                dataReader.Close();


                cmd.CommandText = "SELECT TOP 1 IdLineSale FROM LineSale WHERE IdSale = @IdSale AND IdProduct = @IdProduct AND Unity = @Unity ORDER BY IdLineSale DESC";
                dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        sale.article.lineSaleId = dataReader["IdLineSale"].ToString();
                    }
                }
                dataReader.Close();

                /*
                 Verify if exists WAREHOUSE ROWS
                 - Verify first if the product has a row in ProductWareHouseMovement with IdMovementType = 1 (SALES) if not exists create with default warehouse of the company
                 - Verify second if the product has a row in ProductWareHouse with the IdWareHouse got before, if no exists create with all mounts on "0"
                 - Update ProductWareHouse fields Reserved and Available with data got before
                 */
                cmd.CommandText = "SELECT IdWareHouse FROM ProductWareHouseMovement WHERE IdProduct = @IdProduct AND IdMovementType = 1"; // IdMovementType = 1 (SALE)
                dataReader = cmd.ExecuteReader();
                string IdWareHouse = "";
                if (dataReader.HasRows)
                {
                    while(dataReader.Read())
                    {
                        IdWareHouse = dataReader["IdWareHouse"].ToString();
                    }
                }
                else
                {
                    dataReader.Close();
                    cmd.CommandText = "SELECT TOP 1 IdWareHouse FROM WareHouse WHERE IdCompany = @IdCompany ";
                    dataReader = cmd.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while(dataReader.Read())
                        {
                            IdWareHouse = dataReader["IdWareHouse"].ToString();
                        }
                    }
                    dataReader.Close();

                    cmd.CommandText = "INSERT INTO ProductWareHouseMovement (IdMovementType, IdWareHouse, IdProduct) VALUES (1, "+ IdWareHouse +", @IdProduct)";                    
                    dataReader = cmd.ExecuteReader();                    
                }
                dataReader.Close();

                // Verify if exists ProductWareHouse
                if (IdWareHouse != ""){
                    cmd.CommandText = "SELECT IdProductWareHouse FROM ProductWareHouse WHERE IdWareHouse = @IdWareHouse AND IdProduct = @IdProduct";
                    cmd.Parameters.AddWithValue("@IdWareHouse", IdWareHouse);
                    dataReader = cmd.ExecuteReader();

                    if (!dataReader.HasRows)
                    {
                        dataReader.Close();
                        cmd.CommandText = "INSERT INTO ProductWareHouse (IdProduct, IdWareHouse, Reserved, Available, Minimum, Maximum) VALUES (@IdProduct, @IdWareHouse, 0, 0, 0 ,0)";
                        dataReader = cmd.ExecuteReader();
                    }
                    dataReader.Close();
                }

                /* ** UPDATE WAREHOUSE INVENTORY ** */
                decimal Reserved = sale.article.unity;
                query = "UPDATE ProductWareHouse SET Reserved = Reserved + @Reserved, Available = Available - @Reserved WHERE IdProduct = @IdProduct AND IdWareHouse = @IdWareHouse";
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@Reserved", Reserved);
                dataReader = cmd.ExecuteReader();
                dataReader.Close();

                // Calculate total
                query = "SELECT * FROM LineSale WHERE IdSale = @IdSale";
                cmd.CommandText = query;
                dataReader = cmd.ExecuteReader();
                Decimal total = 0;

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        total = total + (Decimal.Parse(dataReader["Price"].ToString()) * Decimal.Parse(dataReader["Unity"].ToString()));
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
            else
            {
                response.error = true;
                response.description = "Error connecting to database";
            }

            return response;
        }

        [Route("api/Article/update/")]
        [HttpPost]
        // PUT: api/Article/update
        public BasicResponse Put([FromBody]Article article, string token = "", string idcompany = "")
        {
            BasicResponse response = new BasicResponse { error = false, description = "" };

            if (!Tools.isUserLogged(token, idcompany))
            {
                response.error = false;
                response.description = "bad user";
                return response;
            }

            if (article != null)
            {
                Connection connection = new Connection();

                if (connection.OpenConnection())
                {
                    SqlCommand cmd = connection.connection.CreateCommand();
                    SqlTransaction transaction;

                    cmd.CommandText = "SELECT IdWareHouse FROM ProductWareHouseMovement WHERE IdProduct = @IdProduct AND IdMovementType = 1"; // IdMovementType = 1 (SALE)
                    cmd.Parameters.AddWithValue("@IdProduct", article.id);
                    cmd.Parameters.AddWithValue("@IdCompany", idcompany);
                    SqlDataReader dataReader = cmd.ExecuteReader();
                    string IdWareHouse = "";
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            IdWareHouse = dataReader["IdWareHouse"].ToString();
                        }
                    }
                    else
                    {
                        dataReader.Close();
                        cmd.CommandText = "SELECT TOP 1 IdWareHouse FROM WareHouse WHERE IdCompany = @IdCompany ";
                        dataReader = cmd.ExecuteReader();
                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                IdWareHouse = dataReader["IdWareHouse"].ToString();
                            }
                        }
                        dataReader.Close();

                        cmd.CommandText = "INSERT INTO ProductWareHouseMovement (IdMovementType, IdWareHouse, IdProduct) VALUES (1, " + IdWareHouse + ", @IdProduct)";
                        dataReader = cmd.ExecuteReader();
                    }
                    dataReader.Close();

                    // Verify if exists ProductWareHouse
                    if (IdWareHouse != "")
                    {
                        cmd.CommandText = "SELECT IdProductWareHouse FROM ProductWareHouse WHERE IdWareHouse = @IdWareHouse AND IdProduct = @IdProduct";
                        cmd.Parameters.AddWithValue("@IdWareHouse", IdWareHouse);
                        dataReader = cmd.ExecuteReader();

                        if (!dataReader.HasRows)
                        {
                            dataReader.Close();
                            cmd.CommandText = "INSERT INTO ProductWareHouse (IdProduct, IdWareHouse, Reserved, Available, Minimum, Maximum) VALUES (@IdProduct, @IdWareHouse, 0, 0, 0 ,0)";
                            dataReader = cmd.ExecuteReader();
                        }
                        dataReader.Close();
                    }

                    /* GET Actual unity*/
                    decimal oldUnity = 0;
                    cmd.CommandText = "SELECT Unity FROM LineSale WHERE IdLineSale = @IdLineSale";
                    cmd.Parameters.AddWithValue("@IdLineSale", article.lineSaleId);
                    dataReader = cmd.ExecuteReader();

                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            oldUnity = decimal.Parse(dataReader["Unity"].ToString());
                        }
                    }
                    dataReader.Close();
                    // Start a local transaction.
                    transaction = connection.connection.BeginTransaction("UpdateArticlesOfClient");
                    cmd.Connection = connection.connection;
                    cmd.Transaction = transaction;
                    
                    try
                    {
                        /* ** UPDATE WAREHOUSE INVENTORY ** */
                        decimal Reserved = article.unity;
                        cmd.CommandText = "UPDATE ProductWareHouse SET Reserved = Reserved - @Old + @New, Available = Available + @Old - @New WHERE IdProduct = @IdProduct AND IdWareHouse = @IdWareHouse";
                        cmd.Parameters.AddWithValue("@Old", oldUnity);
                        cmd.Parameters.AddWithValue("@New", Reserved);
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "UPDATE LineSale SET Unity = @Unity WHERE IdLineSale = @IdLineSale";
                        cmd.Parameters.AddWithValue("@Unity", article.unity);
                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        response.description = "Article updated!";
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Commit Exception Type: {0}", ex.GetType());
                        System.Diagnostics.Debug.WriteLine("  Message: {0}", ex.Message);
                        try
                        {
                            transaction.Rollback();
                        }
                        catch (Exception ex2)
                        {
                            System.Diagnostics.Debug.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                            System.Diagnostics.Debug.WriteLine("  Message: {0}", ex2.Message);
                        }

                        response.description = "Error trying to update the article!";
                        response.error = true;
                    }
                }
                else
                {
                    response.description = "Error trying to connect to DB.";
                    response.error = true;
                }
            }
            else
            {
                response.description = "Empty JSON!";
                response.error = true;
            }

            return response;
        }

        [Route("api/Article/delete/")]
        [HttpPost]
        // DELETE: api/Article/delete
        public BasicResponse Delete([FromBody] RequestDeleteVirtualQueueClientProducts request, string token = "", string idcompany = "")
        {
            BasicResponse response = new BasicResponse { error = false, description = "" };

            if (!Tools.isUserLogged(token, idcompany))
            {
                response.error = false;
                response.description = "bad user";
                return response;
            }

            if (request != null)
            {
                Connection connection = new Connection();

                if (connection.OpenConnection())
                {
                    SqlCommand cmd = connection.connection.CreateCommand();
                    SqlTransaction transaction;

                    // Start a local transaction.
                    transaction = connection.connection.BeginTransaction("DeleteArticlesOfClient");
                    cmd.Connection = connection.connection;
                    cmd.Transaction = transaction;

                    try
                    {
                        if (!request.deleteAll)
                        {
                            cmd.CommandText = "SELECT IdProduct, Unity FROM LineSale WHERE IdLineSale = @IdLineSale";
                            cmd.Parameters.AddWithValue("@IdLineSale", request.article.lineSaleId);
                            SqlDataReader dataReader = cmd.ExecuteReader();

                            String IdProduct = "";
                            String Unity = "";
                            if (dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    Unity = dataReader["Unity"].ToString();
                                    IdProduct = dataReader["IdProduct"].ToString();
                                }                                
                            }
                            dataReader.Close();

                            cmd.CommandText = "SELECT IdWareHouse FROM ProductWareHouseMovement WHERE IdProduct = @IdProduct AND IdMovementType = 1"; // IdMovementType = 1 (SALE)
                            cmd.Parameters.AddWithValue("@IdProduct", IdProduct);
                            dataReader = cmd.ExecuteReader();
                            string IdWareHouse = "";
                            if (dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    IdWareHouse = dataReader["IdWareHouse"].ToString();
                                }
                            }
                            dataReader.Close();

                            SqlCommand cmd3 = connection.connection.CreateCommand();
                            cmd3.Connection = connection.connection;
                            cmd3.Transaction = transaction;
                            cmd3.CommandText = "UPDATE ProductWareHouse SET Reserved = Reserved - @Reserved, Available = Available + @Reserved WHERE IdProduct = @IdProduct AND IdWareHouse = " + IdWareHouse;
                            cmd3.Parameters.AddWithValue("@Reserved", Unity);
                            cmd3.Parameters.AddWithValue("@IdProduct", IdProduct);
                            cmd3.ExecuteNonQuery();

                            cmd.CommandText = "DELETE FROM LineSale WHERE IdLineSale = @IdLineSale";

                        }
                        else
                        {
                            cmd.CommandText = "SELECT IdSale FROM Sales WHERE IdClient = @IdClient AND IdSaleStatus = 1 AND IdCompany = @IdCompany";
                            cmd.Parameters.AddWithValue("@IdClient", request.client.IdClient);
                            cmd.Parameters.AddWithValue("@IdCompany", idcompany);
                            SqlDataReader dataReader = cmd.ExecuteReader();

                            if (dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    SqlCommand cmd2 = connection.connection.CreateCommand();
                                    cmd2.Connection = connection.connection;
                                    cmd2.Transaction = transaction;
                                    cmd2.CommandText = "SELECT IdProduct, Unity FROM LineSale WHERE IdSale = @IdSale";
                                    cmd2.Parameters.AddWithValue("@IdSale", dataReader["IdSale"]);
                                    SqlDataReader dataReader2 = cmd2.ExecuteReader();

                                    if (dataReader2.HasRows)
                                    {
                                        while (dataReader2.Read())
                                        {
                                            SqlCommand cmd3 = connection.connection.CreateCommand();
                                            cmd3.Connection = connection.connection;
                                            cmd3.Transaction = transaction;
                                            cmd3.CommandText = "SELECT IdWareHouse FROM ProductWareHouseMovement WHERE IdProduct = @IdProduct AND IdMovementType = 1"; // IdMovementType = 1 (SALE)
                                            cmd3.Parameters.AddWithValue("@IdProduct", dataReader2["IdProduct"].ToString());
                                            SqlDataReader dataReader3 = cmd3.ExecuteReader();
                                            string IdWareHouse = "";
                                            if (dataReader3.HasRows)
                                            {
                                                while (dataReader3.Read())
                                                {
                                                    IdWareHouse = dataReader3["IdWareHouse"].ToString();
                                                }
                                            }
                                            dataReader3.Close();

                                            SqlCommand cmd4 = connection.connection.CreateCommand();
                                            cmd4.Connection = connection.connection;
                                            cmd4.Transaction = transaction;
                                            cmd4.CommandText = "UPDATE ProductWareHouse SET Reserved = Reserved - @Reserved, Available = Available + @Reserved WHERE IdProduct = @IdProduct AND IdWareHouse = " + IdWareHouse;
                                            cmd4.Parameters.AddWithValue("@Reserved", dataReader2["Unity"].ToString());
                                            cmd4.Parameters.AddWithValue("@IdProduct", dataReader2["IdProduct"].ToString());
                                            cmd4.ExecuteNonQuery();
                                        }
                                        dataReader2.Close();
                                    }

                                    // Delete LineSale
                                    cmd.CommandText = "DELETE FROM LineSale WHERE IdSale = @IdSale";
                                    cmd.Parameters.AddWithValue("@IdSale", dataReader["IdSale"]);
                                }
                            }
                            dataReader.Close();
                        }

                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                        response.description = "Article deleted!";
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Commit Exception Type: {0}", ex.GetType());
                        System.Diagnostics.Debug.WriteLine("  Message: {0}", ex.Message);
                        try
                        {
                            transaction.Rollback();
                        }
                        catch (Exception ex2)
                        {
                            System.Diagnostics.Debug.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                            System.Diagnostics.Debug.WriteLine("  Message: {0}", ex2.Message);
                        }

                        response.description = "Error trying to delete the article!";
                        response.error = true;
                    }
                }
                else
                {
                    response.description = "Error trying to connect to DB.";
                    response.error = true;
                }
            }
            else
            {
                response.description = "Request JSON empty!";
                response.error = true;
                return response;
            }

            return response;
        }

        [Route("api/Article/migration")]
        [HttpPost]
        public BasicResponse MigrateArticle([FromBody] RequestSaveMigrationArticle request, string token = "", string idcompany = "")
        {
            BasicResponse response = new BasicResponse { error = false, description = "migrated" };

            if (!Tools.isUserLogged(token, idcompany))
            {
                response.error = false;
                response.description = "bad user";
                return response;
            }

            AES aes = new AES(idcompany);

            if (response != null) {

                Connection connection = new Connection();
                //AES aes = new AES();
                if (connection.OpenConnection())
                {
                    for (int i = 0; i < request.articles.Count(); i++)
                    {
                        SqlCommand cmd = new SqlCommand("", connection.connection);
                        ArticleMigration article = request.articles[i];
                        cmd.CommandText = "SELECT IdProduct FROM Product WHERE Barcode = @Barcode AND IdCompany ="+idcompany;
                        cmd.Parameters.AddWithValue("@Barcode", article.barcode);

                        SqlDataReader dataReader = cmd.ExecuteReader();

                        if (dataReader.HasRows)
                        {
                            // if exists the article we update the data
                            dataReader.Close();

                            cmd.CommandText = "SELECT Percentage FROM Tax WHERE IdTax = @Tax";
                            cmd.Parameters.AddWithValue("@Tax", article.tax);

                            SqlDataReader dataReader2 = cmd.ExecuteReader();
                            if (dataReader2.HasRows)
                            {
                                while (dataReader2.Read())
                                {
                                    decimal tax = (decimal.Parse(dataReader2["Percentage"].ToString()) + 100) / 100;
                                    decimal netPrice = Math.Round(article.price / tax, 2);
                                    decimal iva = article.price - netPrice;

                                    SqlCommand cmd2 = new SqlCommand("", connection.connection);
                                    cmd2.CommandText = "UPDATE Product SET Name=@Name, price=@Price, IVA=@iva, NetPrice=@NetPrice, IdSubCategory= @IdSubCategory, isSoldByWeight = @isSoldByWeight, IdTax = @IdTax WHERE Barcode = @Barcode";
                                    cmd2.Parameters.AddWithValue("@Name", article.name);
                                    cmd2.Parameters.AddWithValue("@price", article.price);
                                    cmd2.Parameters.AddWithValue("@IdSubCategory", article.groupId);
                                    cmd2.Parameters.AddWithValue("@IdTax", article.tax);
                                    cmd2.Parameters.AddWithValue("@isSoldByWeight", article.isSoldByWeight);
                                    cmd2.Parameters.AddWithValue("@iva", iva);
                                    cmd2.Parameters.AddWithValue("@NetPrice", netPrice);
                                    cmd2.Parameters.AddWithValue("@Barcode", article.barcode);

                                    dataReader2.Close();
                                    dataReader2 = cmd2.ExecuteReader();

                                    if (dataReader2.RecordsAffected == 0)
                                    {
                                        response.description = "Some articles was not migrated";
                                    }
                                }
                            }
                            dataReader2.Close();
                        } else
                        {
                            // if not exists we create the article
                            dataReader.Close();

                            cmd.CommandText = "SELECT Percentage FROM Tax WHERE IdTax = @Tax";
                            cmd.Parameters.AddWithValue("@Tax", article.tax);

                            SqlDataReader dataReader2 = cmd.ExecuteReader();
                            SqlCommand cmd2 = new SqlCommand("", connection.connection);
                            if (dataReader2.HasRows)
                            {
                                while (dataReader2.Read())
                                {
                                    cmd2.CommandText = "INSERT INTO Product (Name, price, IdWareHouse, IdSubCategory, Barcode, IVA, NetPrice, IdTax, isSoldByWeight, IdCompany) VALUES (@Name, @price, 1, @IdSubCategory, @Barcode, @IVA, @NetPrice, @IdTax, @isSoldByWeight, @IdCompany)";
                                    cmd2.Parameters.AddWithValue("@Name", article.name);
                                    cmd2.Parameters.AddWithValue("@price", article.price);
                                    cmd2.Parameters.AddWithValue("@IdSubCategory", article.groupId);
                                    cmd2.Parameters.AddWithValue("@Barcode", article.barcode);
                                    cmd2.Parameters.AddWithValue("@IdTax", article.tax);
                                    cmd2.Parameters.AddWithValue("@isSoldByWeight", article.isSoldByWeight);
                                    cmd2.Parameters.AddWithValue("@IdCompany", idcompany);

                                    decimal tax = (decimal.Parse(dataReader2["Percentage"].ToString()) + 100) / 100;
                                    decimal netPrice =  Math.Round( article.price / tax, 2);
                                    decimal iva = article.price - netPrice;
                                    cmd2.Parameters.AddWithValue("@IVA", iva);
                                    cmd2.Parameters.AddWithValue("@NetPrice", netPrice);
                                    dataReader2.Close();
                                    dataReader2 = cmd2.ExecuteReader();

                                    if (dataReader2.RecordsAffected == 0)
                                    {
                                        response.description = "Some articles was not migrated";
                                    }
                                }
                            }
                            dataReader2.Close();
                        }

                        dataReader.Close();
                    }
                }
                else
                {
                    response.description = "Error connecting to DB";
                    response.error = true;
                    return response;
                }
            }

            return response;
        }
    }
}
