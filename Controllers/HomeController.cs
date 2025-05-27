using System.Diagnostics;
using Dj_listens.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Dj_listens.Controllers
{
    public class HomeController : Controller
    {
      
        
            // Ovo je po�etna stranica aplikacije (https://localhost:7211/)
            public IActionResult Index()
            {
                return View(); // vra�a Index.cshtml
            }

            // GET metoda za prikaz stranice gdje korisnik bira ulogu (DJ ili PartyMan)
            [HttpGet]
            public IActionResult ChooseRole()
            {
                return View("Choose"); // Choose.cshtml je stranica za biranje DJ ili PartyMan
            }

            // POST metoda koja reagira na odabir korisnika
            [HttpPost]
            public IActionResult ChooseRole(string role)
            {
                if (role == "dj")
                    return RedirectToAction("LogReg", "DJ"); // vodi na DJ registraciju/prijavu

                if (role == "party")
                    return RedirectToAction("EnterParty", "PartyMan"); // vodi na PartyMan stranicu

                return RedirectToAction("Index"); // fallback ako ni�ta nije odabrano
            }
        [Route("Home/ErrorRedirect")]
        public IActionResult ErrorRedirect()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature != null)
            {
                // Ovdje mo�e� logirati gre�ku ako �eli�
                var exception = exceptionHandlerPathFeature.Error;
                // npr. log u datoteku ili bazu
                Console.WriteLine($"Gre�ka: {exception.Message}");
            }

            // Preusmjeri korisnika na po�etnu stranicu za odabir uloge
            return RedirectToAction("ChooseRole");
        }
    }
    
}
