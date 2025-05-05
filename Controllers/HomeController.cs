using System.Diagnostics;
using Dj_listens.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dj_listens.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChooseRole(string role)
        {
            if (role == "dj")
                return RedirectToAction("LogReg", "DJ");
            else if (role == "party")
                return RedirectToAction("EnterParty", "PartyMan"); 

            return RedirectToAction("Index");
        }
    }
}
