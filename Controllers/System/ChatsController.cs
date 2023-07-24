using eShop.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace eShop.Controllers
{
    [Authorize]
    public class ChatsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Chat()
        {
            return PartialView("../System/Chats/_Chat", db.Set<Chat>().OrderByDescending(x => x.Id).Take(20).OrderBy(y => y.Id).ToList());
        }

        // GET: SystemLogs
        [Authorize(Roles = "ChatsActive")]
        public ActionResult Index()
        {
            return View("../System/Chats/Index");
        }

        [HttpGet]
        [Authorize(Roles = "ChatsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../System/Chats/_IndexGrid", db.Set<Chat>().AsQueryable());
            else
                return PartialView("../System/Chats/_IndexGrid", db.Set<Chat>().AsQueryable()
                    .Where(x => x.Message.Contains(search)));
        }

        // GET: MasterUnits/Create
        [Authorize(Roles = "ChatsDelete")]
        public ActionResult Delete()
        {
            List<SelectListItem> deleteRange = new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "Lebih dari 7 hari", Value = "7"
                },
                new SelectListItem
                {
                    Text = "Lebih dari 30 hari", Value = "30"
                },
                new SelectListItem
                {
                    Text = "Lebih dari 90 hari", Value = "90"
                },
                new SelectListItem
                {
                    Text = "Lebih dari 1 tahun", Value = "365"
                },
                new SelectListItem
                {
                    Text = "Hapus semua log", Value = "0"
                }
            };

            ViewBag.DeleteRange = deleteRange;

            return PartialView("../System/Chats/_Delete");
        }


        [HttpPost]
        [Authorize(Roles = "ChatsDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string DeleteRange)
        {
            int range = 999999;
            try
            {
                range = int.Parse(DeleteRange);

                if (range >= 0)
                {
                    using (db)
                    {
                        db.Chats.RemoveRange(db.Chats.Where(x => x.Created <= (DbFunctions.AddDays(DateTime.Now, range * -1))));
                        db.SaveChanges();
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (InvalidCastException e)
            {
                return Json(e, JsonRequestBehavior.AllowGet);
            }

            return PartialView("../System/Chats/_Delete");
        }

        private class ChatMessage
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string FullName { get; set; }
            public string TimeStamp { get; set; }
            public string Message { get; set; }
        }

        public ActionResult GetData(int chatIndex)
        {
            List<Chat> chats = new List<Chat>();

            if (chatIndex > 0)
            {
                chats = db.Chats.Where(x => x.Id < chatIndex).OrderByDescending(y => y.Id).Take(20).ToList();
            }
            else
            {
                chats = db.Chats.OrderByDescending(y => y.Id).Take(20).ToList();
            }

            return Json(chats.Select(x => new ChatMessage
            {
                Id = x.Id,
                UserName = x.User.UserName,
                FullName = x.User.FullName,
                TimeStamp = x.Created.ToString("dd/MM/yyyy hh:mm:ss"),
                Message = x.Message
            }).ToList(), JsonRequestBehavior.AllowGet);
        }
    }
}