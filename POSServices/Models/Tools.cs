using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace POSServices.Models
{
    public class Tools
    {
        public static String sha256_hash(String value)
        {
            using (SHA256 hash = SHA256Managed.Create())
            {
                return String.Concat(hash
                  .ComputeHash(Encoding.UTF8.GetBytes(value))
                  .Select(item => item.ToString("x2")));
            }
        }
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static bool isUserLogged(string token, string IdCompany)
        {

            Connection connection = new Connection();

            if (connection.OpenConnection())
            {

                SqlCommand cmd = new SqlCommand("", connection.connection);
                cmd.CommandText = "SELECT IdUser FROM UserPaseo WHERE Token = @Token";
                cmd.Parameters.AddWithValue("@Token", token);
                SqlDataReader dataReader = cmd.ExecuteReader();

                if (!dataReader.HasRows)
                {
                    return false;
                }
                else
                {
                    while (dataReader.Read())
                    {
                        String IdUser = dataReader["IdUser"].ToString();
                        dataReader.Close();

                        cmd.CommandText = "SELECT * FROM UserCompany WHERE IdCompany = @IdCompany AND IdUser = @IdUser";
                        cmd.Parameters.AddWithValue("@IdCompany", IdCompany);
                        cmd.Parameters.AddWithValue("@IdUser", IdUser);

                        dataReader = cmd.ExecuteReader();

                        if (dataReader.HasRows)
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
    }
}