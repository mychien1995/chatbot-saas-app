using ChatBotApp.DataAccess;
using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ChatBotApp.Controllers
{
    public class AccountController : BaseController
    {
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(string userId, int userCode)
        {
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "UserId is required.");
                return View();
            }

            var user = DataRepo.Login(userId, userCode);

            if (user != null)
            {
                var ticket = new FormsAuthenticationTicket(
                        1,
                        user.UserId,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(120),
                        false,
                        user.Identifier.ToString(),
                        FormsAuthentication.FormsCookiePath
                    );

                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(authCookie);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }
        }
    }
}