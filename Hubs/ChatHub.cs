using eShop.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShop
{
    [Authorize]
    public class ChatHub : Hub
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private class UserList
        {
            public string FullName { get; set; }
            public string UserName { get; set; }
        }

        public void Send(string message)
        {
            DateTime timestamp = DateTime.Now;
            string name = Context.User.Identity.Name;
            ApplicationUser user = db.Users.Where(x => x.UserName == name).First();

            if (user != null)
            {
                Clients.All.addNewMessageToPage(user.FullName, message, timestamp.ToString("dd/MM/yyyy hh:mm:ss"), user.UserName);

                db.Chats.Add(new Chat { Message = message, UserId = user.Id, Created = timestamp });
                db.SaveChanges();
            }

        }

        public void SendUserList()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();

            List<UserList> users = db.ChatUserOnlines.Select(x => new UserList
            {
                UserName = x.User.UserName,
                FullName = x.User.FullName
            }).Distinct().ToList();

            context.Clients.All.updateUserList(users);
        }

        public override Task OnConnected()
        {
            string username = Context.User.Identity.Name;

            ApplicationUser user = db.Users.Where(x => x.UserName == username).First();

            ChatUserOnline userOnline = new ChatUserOnline { UserId = user.Id };

            db.ChatUserOnlines.Add(userOnline);
            db.SaveChanges();

            SendUserList();

            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            string username = Context.User.Identity.Name;

            ApplicationUser user = db.Users.Where(x => x.UserName == username).First();

            ChatUserOnline userOnline = new ChatUserOnline { UserId = user.Id };

            db.ChatUserOnlines.Add(userOnline);
            db.SaveChanges();

            SendUserList();

            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string username = Context.User.Identity.Name;

            ApplicationUser user = db.Users.Where(x => x.UserName == username).First();

            db.ChatUserOnlines.RemoveRange(db.ChatUserOnlines.Where(x => x.UserId == user.Id));
            db.SaveChanges();

            SendUserList();

            return base.OnDisconnected(stopCalled);
        }
    }
}