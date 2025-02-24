using ChatBotApp.DataAccess;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace ChatBotApp.Controllers
{
    public class BaseController : Controller
    {
        protected DataRepository DataRepo { get;}
        public BaseController()
        {
            DataRepo = new DataRepository(ConfigurationManager.ConnectionStrings["Default"].ConnectionString);
        }

        protected long GetCurrentUserIdentifier()
        {
            if (!User.Identity.IsAuthenticated) return -1;
            var authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    if (long.TryParse(ticket.UserData, out long identifier))
                    {
                        return identifier;
                    }
                }
            }

            return -1;
        }
    }
}