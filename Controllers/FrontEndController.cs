using eShop.Models;
using System.IO;
using System.Web.Mvc;

namespace eShop.Controllers
{
    public class FrontEndController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public class FrontEndCustomer
        {
            public int Id { get; set; }

            public string Token { get; set; }

            public string Name { get; set; }

            public string Key { get; set; }

            public string PIN { get; set; }

            public string ConfirmPIN { get; set; }

            public string Emails { get; set; }
        }

        private static byte[] StreamToBytes(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}