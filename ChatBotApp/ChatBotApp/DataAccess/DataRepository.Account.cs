using System;
using System.Data.SqlClient;
using System.Data;
using ChatBotApp.Models;

namespace ChatBotApp.DataAccess
{
    public partial class DataRepository
    {
        private readonly string _connectionString;

        public DataRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Account Login(string userId, int userCode)
        {
            Account account = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, UserId, UserCode, Sitekey, Domain, Expiration, Identifier " +
                               "FROM [dbo].[Accounts] WHERE UserId = @UserId AND UserCode = @UserCode";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@UserId", SqlDbType.NVarChar, 255).Value = userId;
                    cmd.Parameters.Add("@UserCode", SqlDbType.Int).Value = userCode;
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if(!reader.Read()) return null;
                        account = new Account
                        {
                            Id = reader.GetInt64(reader.GetOrdinal("Id")),
                            UserId = reader.GetString(reader.GetOrdinal("UserId")),
                            UserCode = reader.IsDBNull(reader.GetOrdinal("UserCode")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("UserCode")),
                            Sitekey = reader.IsDBNull(reader.GetOrdinal("Sitekey")) ? null : reader.GetString(reader.GetOrdinal("Sitekey")),
                            Domain = reader.IsDBNull(reader.GetOrdinal("Domain")) ? null : reader.GetString(reader.GetOrdinal("Domain")),
                            Expiration = reader.IsDBNull(reader.GetOrdinal("Expiration")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("Expiration")),
                            Identifier = reader.IsDBNull(reader.GetOrdinal("Identifier")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("Identifier")),
                        };
                    }
                }
            }
            return account;
        }
    }
}