using System.Diagnostics;
using Dj_listens.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Dj_listens.Controllers
{
    public class HomeController : Controller
    {
      
        
            // Ovo je poèetna stranica aplikacije (https://localhost:7211/)
            public IActionResult Index()
            {
                return View(); // vraæa Index.cshtml
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

                return RedirectToAction("Index"); // fallback ako ništa nije odabrano
            }
        [Route("Home/ErrorRedirect")]
        public IActionResult ErrorRedirect()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature != null)
            {
                // Ovdje možeš logirati grešku ako želiš
                var exception = exceptionHandlerPathFeature.Error;
                // npr. log u datoteku ili bazu
                Console.WriteLine($"Greška: {exception.Message}");
            }

            // Preusmjeri korisnika na poèetnu stranicu za odabir uloge
            return RedirectToAction("ChooseRole");
        }
    }
    
}
