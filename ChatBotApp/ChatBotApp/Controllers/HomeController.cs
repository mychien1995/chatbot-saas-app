using ChatBotApp.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace ChatBotApp.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public ActionResult Index(int page = 1, int pageSize = 10, string sortColumn = "Id", string sortDirection = "DESC")
        {
            long userIdentifier = GetCurrentUserIdentifier();

            if (userIdentifier == -1)
                return new HttpUnauthorizedResult("User not found or not authenticated.");
            var chatbots = DataRepo.GetChatbots(userIdentifier, page, pageSize, sortColumn, sortDirection);
            ViewBag.PageNumber = page;
            ViewBag.PageSize = pageSize;
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;
            return View(chatbots);
        }

        public ActionResult CreateChatbot()
        {
            return View(new Chatbot());
        }

        [HttpPost]
        public ActionResult CreateChatbot(Chatbot model)
        {
            if (ModelState.IsValid)
            {
                if (DataRepo.IsBotIdExists(model.BotId.Value, GetCurrentUserIdentifier()))
                    ModelState.AddModelError("", "Bot Id already in used");
                else
                {
                    DataRepo.InsertChatbot(GetCurrentUserIdentifier(), model.BotId, model.BotTitle, model.Opening, model.Closing);
                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }

        [Authorize]
        public ActionResult ChatBotDetail(long id)
        {
            var chatbot = DataRepo.GetChatbotById(id);
            if (chatbot == null || chatbot.Identifier != GetCurrentUserIdentifier())
                return HttpNotFound("Chatbot not found.");
            var contents = DataRepo.GetChatbotContents(chatbot.Id);
            FormatChatbotContentList(contents);
            var viewModel = new ChatbotDetailViewModel
            {
                Chatbot = chatbot,
                ChatbotContents = contents
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult ChatBotDetail(ChatbotDetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                DataRepo.UpdateChatbot(model.Chatbot.Id, model.Chatbot.BotTitle, model.Chatbot.Opening, model.Chatbot.Closing);
                TempData["SuccessMessage"] = "Chatbot details updated successfully!";
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        public ActionResult CreateChatbotContent(long botId)
        {
            ViewBag.ChatbotContents = GetReferencesChatbotContents(botId);
            return View(new ChatbotContent() { BotId = botId });
        }

        [HttpPost]
        public ActionResult CreateChatbotContent(ChatbotContent model)
        {
            if (ModelState.IsValid)
            {
                if (model.MsgType == 2 && string.IsNullOrEmpty(model.MsgChoices.Trim()))
                {
                    ViewBag.ChatbotContents = DataRepo.GetChatbotContents(model.BotId.Value);
                    ModelState.AddModelError("", "Choices are required");
                    return View(model);
                }
                if (model.MsgType == 2)
                    model.MsgChoices = model.MsgChoices.Trim();
                DataRepo.InsertChatbotContent(GetCurrentUserIdentifier(), model.BotId, model.MsgType, model.MsgContent, model.MsgChoices, model.MsgAction, model.DisplayCondition);
                TempData["SuccessMessage"] = "Chatbot message created successfully!";
                return RedirectToAction("ChatBotDetail", "Home", new { id = model.BotId });
            }
            ViewBag.ChatbotContents = GetReferencesChatbotContents(model.BotId.Value);
            return View(model);
        }

        public ActionResult DetailsContent(long id)
        {
            ChatbotContent content = DataRepo.GetChatbotContentById(id);
            if (content == null)
                return HttpNotFound("Chatbot content not found.");
            ViewBag.ChatbotContents = GetReferencesChatbotContents(content.BotId.Value, id);
            return View(content);
        }

        [HttpPost]
        public ActionResult DetailsContent(ChatbotContent model)
        {
            if (ModelState.IsValid)
            {
                if (model.MsgType == 2 && !string.IsNullOrEmpty(model.MsgChoices))
                    model.MsgChoices = model.MsgChoices.Trim();
                if (model.MsgType == 2 && string.IsNullOrEmpty(model.MsgChoices.Trim()))
                {
                    ViewBag.ChatbotContents = DataRepo.GetChatbotContents(model.BotId.Value);
                    ModelState.AddModelError("", "Choices are required");
                    return View(model);
                }
                DataRepo.UpdateChatbotContent(model.Id, model.MsgType, model.MsgContent, model.MsgChoices, model.MsgAction, model.DisplayCondition);
                TempData["SuccessMessage"] = "Chatbot content updated successfully!";
                return RedirectToAction("ChatBotDetail", "Home", new { id = model.BotId });
            }

            ViewBag.ChatbotContents = GetReferencesChatbotContents(model.BotId.Value, model.Id);
            return View(model);
        }

        private List<ChatbotContent> GetReferencesChatbotContents(long botId, long? currentId = null)
        {
            var data = DataRepo.GetChatbotContents(botId).Where(i => i.Id != currentId && i.MsgType == 2).ToList();
            return data;
        }

        private void FormatChatbotContentList(List<ChatbotContent> chatbotContents)
        {
            var serializer = new JsonSerializer();
            for (var i = 0; i < chatbotContents.Count; i++)
            {
                var content = chatbotContents[i];
                if (!string.IsNullOrEmpty(content.DisplayCondition))
                {
                    var condition = JsonConvert.DeserializeObject<DisplayCondition>(content.DisplayCondition);
                    var referencedId = condition.ReferenceContentId;
                    var referenceContent = chatbotContents.FirstOrDefault(c => c.Id == referencedId);
                    if (referenceContent == null || string.IsNullOrWhiteSpace(referenceContent.MsgChoices) || referenceContent.MsgType != 2) continue;
                    var referenceChoices = JsonConvert.DeserializeObject<ChatbotContentChoice[]>(referenceContent.MsgChoices);
                    var selectedChoice = referenceChoices.FirstOrDefault(c => c.Id == condition.Value);
                    if (selectedChoice == null) continue;
                    content.DependsOnText = $"Question {chatbotContents.IndexOf(referenceContent) + 1}. Choice: {selectedChoice.Value}";
                }
            }
        }
    }
}