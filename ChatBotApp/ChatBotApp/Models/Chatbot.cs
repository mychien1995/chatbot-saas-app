using System.Collections.Generic;

namespace ChatBotApp.Models
{
    public class Chatbot
    {
        public long Id { get; set; }
        public long? Identifier { get; set; }
        public long? BotId { get; set; }
        public string BotTitle { get; set; }
        public string Opening { get; set; }
        public string Closing { get; set; }
    }

    public class ChatbotContent
    {
        public long Id { get; set; }
        public long? Identifier { get; set; }
        public long? BotId { get; set; }
        public int? MsgType { get; set; }
        public string MsgContent { get; set; }
        public string MsgChoices { get; set; }
        public string MsgAction { get; set; }
        public string DisplayCondition { get; set; }
        public string DependsOnText { get; set; }
    }

    public class ChatbotDetailViewModel
    {
        public Chatbot Chatbot { get; set; }
        public List<ChatbotContent> ChatbotContents { get; set; }
    }

    public class DisplayCondition
    {
        public long ReferenceContentId { get; set; }
        public long Value { get; set; }
    }

    public class ChatbotContentChoice
    {
        public long Id { get; set; }
        public string Value { get; set; }
    }
}