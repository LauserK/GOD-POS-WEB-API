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
    public class SaleController : ApiController
    {
        // GET: api/Sale
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Sale/5
        public string Get(int id)
        {
            return "value";
        }

        // DELETE: api/Sale/5
        public void Delete(int id)
        {
        }        

        // PUT: api/Sale/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // GET: api/Sale/2323232323/articles
        [Route("api/Sale/{DocumentNumber}/articles")]
        [HttpGet]
        public BasicResponse getArticleFromSale(string DocumentNumber)
        {
            BasicResponse response = new BasicResponse { error = false, description = "" };
            Connection connection = new Connection();

            if (connection.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("SELECT ", connection.connection);
                cmd.CommandText = "SELECT LineSale.IdLineSale, LineSale.Unity, LineSale.IVA, LineSale.Price, LineSale.NetPrice, Sales.IdDevice, Sales.IdClient, Sales.Document, Product.Name, Product.Barcode, Tax.Percentage AS Tax, LineSale.Idtax, Client.FirstName, Client.LastName, Client.IdentificationNumber, Client.Address FROM LineSale INNER JOIN Sales ON Sales.IdSale = LineSale.IdSale INNER JOIN Product ON Product.IdProduct = LineSale.IdProduct INNER JOIN Tax ON Tax.IdTax = LineSale.IdTax INNER JOIN Client ON Client.IdClient = Sales.IdFiscalClient WHERE Sales.Document = @DocumentNum;";
                cmd.Parameters.AddWithValue("@DocumentNum", DocumentNumber);

                SqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows) {
                    while (dataReader.Read())
                    {
                        Client client = new Client
                        {
                            IdClient = dataReader["IdClient"].ToString(),
                            Name = dataReader["FirstName"].ToString() + " " + dataReader["LastName"].ToString(),
                            IdentificationNumber = dataReader["IdentificationNumber"].ToString(),
                            Address = dataReader["Address"].ToString()
                        };
                        ArticleSale model = new ArticleSale
                        {
                            client = client,
                            name = dataReader["Name"].ToString(),
                            price = decimal.Parse(dataReader["Price"].ToString()),
                            unity = decimal.Parse(dataReader["Unity"].ToString()),
                            IVA = decimal.Parse(dataReader["IVA"].ToString()),
                            netPrice = decimal.Parse(dataReader["NetPrice"].ToString()),
                            barcode = dataReader["barcode"].ToString(),
                            lineSaleId = dataReader["IdLineSale"].ToString(),
                            tax = decimal.Parse(dataReader["Tax"].ToString()),
                            Idtax = dataReader["IdTax"].ToString()
                        };

                        response.data.Add(model);
                    }
                }
                else {
                    response.description = "Document not exists!";
                    response.error = true;
                    return response;
                }
            }

            return response;
        }

        // POST: api/Sale
        [Route("api/Sale/payment/")]
        [HttpPost]
        public BasicResponse CreatePayment([FromBody] RequestSaveSale request)
        {
            /*
             * - Create SalePayment (IdSale, Received, Change)
             * - Create SalePaymentCurrencyLine (IdSalePayment, IdCurrencyList, Quantity, Sign)
             * - SalePaymentElectronicLine (IdSalePayment, Code, Amount, Reference, IdElectronicPaymentType)
             */
            BasicResponse response = new BasicResponse {error = false, description="" };

            if (request != null) {
                Connection connection = new Connection();
                if (connection.OpenConnection())
                {
                    SqlCommand cmd = new SqlCommand("", connection.connection);
                    cmd.CommandText = "INSERT INTO SalePayment (IdSale, Received, Change) VALUES (@IdSale, @Received, @Change)";
                    cmd.Parameters.AddWithValue("@IdSale", request.IdSale);
                    cmd.Parameters.AddWithValue("@Received", request.Received);
                    cmd.Parameters.AddWithValue("@Change", request.Change);

                    SqlDataReader dataReader = cmd.ExecuteReader();

                    if(dataReader.RecordsAffected > 0)
                    {
                        dataReader.Close();
                        cmd.CommandText = "SELECT IdSalePayment FROM SalePayment WHERE IdSale = @IdSale";
                        dataReader = cmd.ExecuteReader();
                        String IdSalePayment = "";

                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                IdSalePayment = dataReader["IdSalePayment"].ToString();
                            }
                            dataReader.Close();

                            foreach (Currency currency in request.Currencies)
                            {
                                SqlCommand cmd2 = new SqlCommand("", connection.connection);
                                cmd2.CommandText = "INSERT INTO SalePaymentCurrencyLine (IdSalePayment, IdCurrencyList, Quantity, Signal) VALUES (@IdSalePayment, @IdCurrencyList, @Quantity, @Sign)";
                                cmd2.Parameters.AddWithValue("@IdSalePayment", IdSalePayment);
                                cmd2.Parameters.AddWithValue("@IdCurrencyList", currency.IdCurrency);
                                cmd2.Parameters.AddWithValue("@Quantity", currency.Quantity);
                                cmd2.Parameters.AddWithValue("@Sign", "+");
                                SqlDataReader dt = cmd2.ExecuteReader();

                                if (dt.RecordsAffected == 0)
                                {
                                    response.description = response.description + " - " + currency.IdCurrency;
                                }
                                dt.Close();
                            }

                            foreach (ElectronicPayment electronic in request.Electronic) {
                                // - SalePaymentElectronicLine(IdSalePayment, Code, Amount, Reference, IdElectronicPaymentType)
                                SqlCommand cmd2 = new SqlCommand("", connection.connection);
                                cmd2.CommandText = "INSERT INTO SalePaymentElectronicLine (IdSalePayment, Code, Amount, Reference, IdElectronicPaymentType) VALUES (@IdSalePayment, @Code, @Amount, @Reference, @IdElectronicPaymentType)";
                                cmd2.Parameters.AddWithValue("@IdSalePayment", IdSalePayment);
                                cmd2.Parameters.AddWithValue("@Code", electronic.Code);
                                cmd2.Parameters.AddWithValue("@Amount", electronic.Amount);
                                cmd2.Parameters.AddWithValue("@Reference", electronic.Reference);
                                cmd2.Parameters.AddWithValue("@IdElectronicPaymentType", electronic.Type.Id);

                                SqlDataReader dt = cmd2.ExecuteReader();

                                if (dt.RecordsAffected == 0)
                                {
                                    response.description = response.description + " - " + electronic.Code;
                                }
                                dt.Close();
                            }
                        }                     
                    }

                    cmd.CommandText = "SELECT COUNT(*) AS rows, CompanyDetails.IdSubsidiary FROM Sales INNER JOIN CompanyDetails ON CompanyDetails.IdCompanyDetails = 1  WHERE StartDate = @Date";
                    //cmd.CommandText = "SELECT COUNT(*) AS rows FROM Sales WHERE StartDate = @Date";
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy/MM/dd").ToString());
                    dataReader = cmd.ExecuteReader();

                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            String rows = (int.Parse(dataReader["rows"].ToString()) + 1).ToString("D4");
                            String date = DateTime.Now.ToString("ddMMyy").ToString();
                            String subsidiary = int.Parse(dataReader["IdSubsidiary"].ToString()).ToString("D2");
                            //String subsidiary = int.Parse("1").ToString("D2");

                            String barcode = date + subsidiary + rows;
                            response.description = barcode;
                            return response;
                        }
                    }

                } else
                {
                    response.error = true;
                    response.description = "Cannot connect to DB?";
                    return response;
                }
            } else
            {
                response.error = true;
                response.description = "JSON empty!";
                return response;
            }

            return response;
        }

        [Route("api/Sale/update/")]
        [HttpPost]
        public BasicResponse Update([FromBody] RequestUpdateSaleStatus request)
        {
            BasicResponse response = new BasicResponse { error = false, description = "" };

            if (request != null)
            {
                Connection connection = new Connection();

                if (connection.OpenConnection())
                {
                    SqlCommand cmd = connection.connection.CreateCommand();
                    SqlTransaction transaction;

                    // Start a local transaction.
                    transaction = connection.connection.BeginTransaction("UpdateSalesStatus");
                    cmd.Connection = connection.connection;
                    cmd.Transaction = transaction;
                    
                    try
                    {
                        // Verify if the Sale exists firsts                        
                        cmd.CommandText = "SELECT * FROM Sales WHERE IdSaleStatus = 1 AND IdClient = @IdClient";
                        cmd.Parameters.AddWithValue("@IdClient", request.IdClient);
                        SqlDataReader dataReader = cmd.ExecuteReader();

                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                String query = "UPDATE Sales SET IdSaleStatus = 2, Document = '" + request.Document + "'";

                                if (request.Barcode != "")
                                {
                                    query = query +  ", Barcode = '"+ request.Barcode + "'";                                    
                                }
                                query = query + " WHERE IdSale = '" + dataReader["IdSale"].ToString() + "'";

                                cmd.CommandText = query;

                             }

                            dataReader.Close();
                            cmd.ExecuteNonQuery();
                            
                            transaction.Commit();
                            response.description = "Sale Updated!";
                        }
                        else
                        {
                            response.description = "Sale not updated!";
                            response.error = true;
                        }
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
                response.description = "Request JSON empty!";
                response.error = true;
                return response;
            }

            return response;
        }
    }
}
