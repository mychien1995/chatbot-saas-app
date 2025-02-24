namespace ChatBotApp.Helpers
{
    public static class ChatbotHelper
    {
        public static string GetMessageTypeName(int? msgType)
        {
            switch (msgType)
            {
                case 1:
                    return "Text";
                case 2:
                    return "Multiple Choice";
                case 3:
                    return "Email";
                case 4:
                    return "Link";
                default:
                    return "Unknown";
            }
        }
    }
}