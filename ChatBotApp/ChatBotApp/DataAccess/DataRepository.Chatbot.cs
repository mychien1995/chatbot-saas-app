using ChatBotApp.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System;

namespace ChatBotApp.DataAccess
{
    public partial class DataRepository
    {
        public List<Chatbot> GetChatbots(long userIdentifier, int pageNumber, int pageSize, string sortColumn, string sortDirection)
        {
            List<Chatbot> chatbots = new List<Chatbot>();
            string[] validSortColumns = { "Id", "BotId", "BotTitle", "Opening", "Closing" };
            if (!Array.Exists(validSortColumns, column => column.Equals(sortColumn, StringComparison.OrdinalIgnoreCase)))
                sortColumn = "Id";
            if (sortDirection.ToUpper() != "ASC" && sortDirection.ToUpper() != "DESC")
                sortDirection = "ASC";

            string query = $@"
            SELECT Id, Identifier, BotId, BotTitle
            FROM [dbo].[Chatbots]
            WHERE Identifier = @Identifier
            ORDER BY {sortColumn} {sortDirection}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Identifier", SqlDbType.BigInt).Value = userIdentifier;
                    cmd.Parameters.Add("@Offset", SqlDbType.Int).Value = (pageNumber - 1) * pageSize;
                    cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            chatbots.Add(new Chatbot
                            {
                                Id = reader.GetInt64(reader.GetOrdinal("Id")),
                                Identifier = reader.IsDBNull(reader.GetOrdinal("Identifier")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("Identifier")),
                                BotId = reader.IsDBNull(reader.GetOrdinal("BotId")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("BotId")),
                                BotTitle = reader.IsDBNull(reader.GetOrdinal("BotTitle")) ? null : reader.GetString(reader.GetOrdinal("BotTitle"))
                            });
                        }
                    }
                }
            }
            return chatbots;
        }

        public void InsertChatbot(long? identifier, long? botId, string botTitle, string opening, string closing)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO [dbo].[Chatbots] (Identifier, BotId, BotTitle, Opening, Closing) " +
                               "VALUES (@Identifier, @BotId, @BotTitle, @Opening, @Closing)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Identifier", SqlDbType.BigInt).Value = (object)identifier ?? DBNull.Value;
                    cmd.Parameters.Add("@BotId", SqlDbType.BigInt).Value = (object)botId ?? DBNull.Value;
                    cmd.Parameters.Add("@BotTitle", SqlDbType.NVarChar, 255).Value = (object)botTitle ?? DBNull.Value;
                    cmd.Parameters.Add("@Opening", SqlDbType.NVarChar).Value = (object)opening ?? DBNull.Value;
                    cmd.Parameters.Add("@Closing", SqlDbType.NVarChar).Value = (object)closing ?? DBNull.Value;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Chatbot GetChatbotById(long id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, Identifier, BotId, BotTitle, Opening, Closing FROM [dbo].[Chatbots] WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = id;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Chatbot
                            {
                                Id = reader.GetInt64(reader.GetOrdinal("Id")),
                                Identifier = reader.IsDBNull(reader.GetOrdinal("Identifier")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("Identifier")),
                                BotId = reader.IsDBNull(reader.GetOrdinal("BotId")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("BotId")),
                                BotTitle = reader.IsDBNull(reader.GetOrdinal("BotTitle")) ? null : reader.GetString(reader.GetOrdinal("BotTitle")),
                                Opening = reader.IsDBNull(reader.GetOrdinal("Opening")) ? null : reader.GetString(reader.GetOrdinal("Opening")),
                                Closing = reader.IsDBNull(reader.GetOrdinal("Closing")) ? null : reader.GetString(reader.GetOrdinal("Closing"))
                            };
                        }
                    }
                }
            }
            return null;
        }

        public void UpdateChatbot(long id, string botTitle, string opening, string closing)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE [dbo].[Chatbots] SET BotTitle = @BotTitle, Opening = @Opening, Closing = @Closing WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = id;
                    cmd.Parameters.Add("@BotTitle", SqlDbType.NVarChar, 255).Value = (object)botTitle ?? DBNull.Value;
                    cmd.Parameters.Add("@Opening", SqlDbType.NVarChar).Value = (object)opening ?? DBNull.Value;
                    cmd.Parameters.Add("@Closing", SqlDbType.NVarChar).Value = (object)closing ?? DBNull.Value;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool IsBotIdExists(long botId, long identifier)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(1) FROM [dbo].[Chatbots] WHERE BotId = @BotId and Identifier = @Identifier";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@BotId", SqlDbType.BigInt).Value = botId;
                    cmd.Parameters.Add("@Identifier", SqlDbType.BigInt).Value = identifier;
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public void InsertChatbotContent(long? identifier, long? botId, int? msgType, string msgContent, string msgChoices, string msgAction, string displayCondition)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO [dbo].[ChatbotContents] (Identifier, BotId, MsgType, MsgContent, MsgChoices, MsgAction, DisplayCondition) " +
                               "VALUES (@Identifier, @BotId, @MsgType, @MsgContent, @MsgChoices, @MsgAction, @DisplayCondition)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Identifier", SqlDbType.BigInt).Value = (object)identifier ?? DBNull.Value;
                    cmd.Parameters.Add("@BotId", SqlDbType.BigInt).Value = (object)botId ?? DBNull.Value;
                    cmd.Parameters.Add("@MsgType", SqlDbType.Int).Value = (object)msgType ?? DBNull.Value;
                    cmd.Parameters.Add("@MsgContent", SqlDbType.NVarChar).Value = (object)msgContent ?? DBNull.Value;
                    cmd.Parameters.Add("@MsgChoices", SqlDbType.NVarChar).Value = (object)msgChoices ?? DBNull.Value;
                    cmd.Parameters.Add("@MsgAction", SqlDbType.NVarChar).Value = (object)msgAction ?? DBNull.Value;
                    cmd.Parameters.Add("@DisplayCondition", SqlDbType.NVarChar).Value = (object)displayCondition ?? DBNull.Value;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<ChatbotContent> GetChatbotContents(long botId)
        {
            List<ChatbotContent> contents = new List<ChatbotContent>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, Identifier, BotId, MsgType, MsgContent, MsgChoices, MsgAction, DisplayCondition " +
                               "FROM [dbo].[ChatbotContents] WHERE BotId = @BotId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@BotId", SqlDbType.BigInt).Value = botId;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            contents.Add(new ChatbotContent
                            {
                                Id = reader.GetInt64(reader.GetOrdinal("Id")),
                                Identifier = reader.IsDBNull(reader.GetOrdinal("Identifier")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("Identifier")),
                                BotId = reader.GetInt64(reader.GetOrdinal("BotId")),
                                MsgType = reader.IsDBNull(reader.GetOrdinal("MsgType")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("MsgType")),
                                MsgContent = reader.IsDBNull(reader.GetOrdinal("MsgContent")) ? null : reader.GetString(reader.GetOrdinal("MsgContent")),
                                MsgChoices = reader.IsDBNull(reader.GetOrdinal("MsgChoices")) ? null : reader.GetString(reader.GetOrdinal("MsgChoices")),
                                MsgAction = reader.IsDBNull(reader.GetOrdinal("MsgAction")) ? null : reader.GetString(reader.GetOrdinal("MsgAction")),
                                DisplayCondition = reader.IsDBNull(reader.GetOrdinal("DisplayCondition")) ? null : reader.GetString(reader.GetOrdinal("DisplayCondition"))
                            });
                        }
                    }
                }
            }
            return contents;
        }

        public ChatbotContent GetChatbotContentById(long id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Id, Identifier, BotId, MsgType, MsgContent, MsgChoices, MsgAction, DisplayCondition " +
                               "FROM [dbo].[ChatbotContents] WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = id;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ChatbotContent
                            {
                                Id = reader.GetInt64(reader.GetOrdinal("Id")),
                                Identifier = reader.IsDBNull(reader.GetOrdinal("Identifier")) ? (long?)null : reader.GetInt64(reader.GetOrdinal("Identifier")),
                                BotId = reader.GetInt64(reader.GetOrdinal("BotId")),
                                MsgType = reader.IsDBNull(reader.GetOrdinal("MsgType")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("MsgType")),
                                MsgContent = reader.IsDBNull(reader.GetOrdinal("MsgContent")) ? null : reader.GetString(reader.GetOrdinal("MsgContent")),
                                MsgChoices = reader.IsDBNull(reader.GetOrdinal("MsgChoices")) ? null : reader.GetString(reader.GetOrdinal("MsgChoices")),
                                MsgAction = reader.IsDBNull(reader.GetOrdinal("MsgAction")) ? null : reader.GetString(reader.GetOrdinal("MsgAction")),
                                DisplayCondition = reader.IsDBNull(reader.GetOrdinal("DisplayCondition")) ? null : reader.GetString(reader.GetOrdinal("DisplayCondition"))
                            };
                        }
                    }
                }
            }
            return null;
        }

        public void UpdateChatbotContent(long id, int? msgType, string msgContent, string msgChoices, string msgAction, string displayCondition)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE [dbo].[ChatbotContents] SET MsgType = @MsgType, MsgContent = @MsgContent, MsgChoices = @MsgChoices, MsgAction = @MsgAction, DisplayCondition = @DisplayCondition WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = id;
                    cmd.Parameters.Add("@MsgType", SqlDbType.Int).Value = (object)msgType ?? DBNull.Value;
                    cmd.Parameters.Add("@MsgContent", SqlDbType.NVarChar).Value = (object)msgContent ?? DBNull.Value;
                    cmd.Parameters.Add("@MsgChoices", SqlDbType.NVarChar).Value = (object)msgChoices ?? DBNull.Value;
                    cmd.Parameters.Add("@MsgAction", SqlDbType.NVarChar).Value = (object)msgAction ?? DBNull.Value;
                    cmd.Parameters.Add("@DisplayCondition", SqlDbType.NVarChar).Value = (object)displayCondition ?? DBNull.Value;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}