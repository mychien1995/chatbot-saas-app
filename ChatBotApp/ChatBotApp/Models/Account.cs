using System;

namespace ChatBotApp.Models
{
    public class Account
    {
        public long Id { get; set; }
        public long? Identifier { get; set; }
        public string UserId { get; set; }
        public int? UserCode { get; set; }
        public string Sitekey { get; set; }
        public string Domain { get; set; }
        public DateTime? Expiration { get; set; }
    }
}