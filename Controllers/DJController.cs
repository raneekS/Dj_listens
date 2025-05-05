using Microsoft.AspNetCore.Mvc;
using Dj_listens.Models;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;


namespace Dj_listens.Controllers
{
    public class DJController : Controller
    {
        private static List<Dj_listens.Models.SongRequest> SongRequests = Dj_listens.Controllers.PartyManController.SongRequests;



        private static List<DJ> DJs = new List<DJ>(); //lista za register(ne sprema se)

        [HttpPost]
        public IActionResult Register(string username, string email, string password, string confirmPassword)
        {
            var usernamePattern = @"^[a-zA-Z0-9_-]+$";

            // 1. Provjera korisničkog imena (format)
            if (!Regex.IsMatch(username, usernamePattern))
            {
                TempData["Error"] = "Korisničko ime smije sadržavati samo slova, brojeve, - i _ , bez razmaka.";
                return RedirectToAction("Register");
            }

            // 2. Provjera lozinki (moraju biti iste)
            if (password != confirmPassword)
            {
                TempData["Error"] = "Lozinke se ne podudaraju.";
                return RedirectToAction("Register");
            }

            // 3. Tek sada idemo provjeravati Username i Email u bazi
            using (var connection = new SqliteConnection("Data Source=C:\\party_data\\party_app.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT COUNT(*) FROM DJ WHERE Username = @username OR Email = @email;";
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@email", email);

                long exists = (long)command.ExecuteScalar();
                if (exists > 0)
                {
                    TempData["Error"] = "Korisničko ime ili email već postoje.";
                    return RedirectToAction("Register");
                }

                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = @"INSERT INTO DJ (Username, Email, Password) VALUES (@username, @email, @password);";
                insertCommand.Parameters.AddWithValue("@username", username);
                insertCommand.Parameters.AddWithValue("@email", email);
                insertCommand.Parameters.AddWithValue("@password", password);
                insertCommand.ExecuteNonQuery();
            }

            TempData["Success"] = "Registracija uspješna! Možete se prijaviti.";
            return RedirectToAction("LogReg");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            using (var connection = new SqliteConnection("Data Source=C:\\party_data\\party_app.db")
)
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT Username FROM DJ WHERE (Username = @input OR Email = @input) AND Password = @password;";
                command.Parameters.AddWithValue("@input", username); // jer unos može biti username ili email
                command.Parameters.AddWithValue("@password", password);

                var result = command.ExecuteScalar();

                if (result != null)
                {
                    string realUsername = result.ToString();
                    HttpContext.Session.SetString("dj_username", realUsername);
                    return RedirectToAction("Profile");
                }
            }

            // Login failed
            TempData["LoginError"] = "Neispravno korisničko ime/email ili lozinka.";
            return RedirectToAction("LogReg");
        }



        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("dj_username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Register");

            ViewBag.Username = username;
            return View();
        }

        public IActionResult LogReg()
        {
            return View();
        }

       
        [HttpPost]
        public IActionResult StartParty()
        {
            var code = GeneratePartyCode();
            ViewBag.PartyCode = code;

            // Spremi kod u session
            HttpContext.Session.SetString("CurrentPartyCode", code);

            return View("PartyRoom");
        }


        [HttpPost]
        public IActionResult ConfirmParty(string PartyName, string PartyCode, string Location, string Description)
        {
            var username = HttpContext.Session.GetString("dj_username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("LogReg");

            int djId = -1;

            using (var connection = new SqliteConnection("Data Source=C:\\party_data\\party_app.db"))
            {
                connection.Open();

                // Prvo dohvatimo DJ-ev ID iz baze
                var getDjCommand = connection.CreateCommand();
                getDjCommand.CommandText = @"SELECT Id FROM DJ WHERE Username = @username;";
                getDjCommand.Parameters.AddWithValue("@username", username);

                var result = getDjCommand.ExecuteScalar();
                if (result != null)
                {
                    djId = Convert.ToInt32(result);
                }
                else
                {
                    TempData["Error"] = "Greška: DJ nije pronađen.";
                    return RedirectToAction("Profile");
                }

                // Sad unosimo Party
                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = @"
            INSERT INTO Parties (PartyName, PartyCode, Location, Description, DjId, StartTime)
            VALUES (@name, @code, @location, @description, @djid, @starttime);";

                insertCommand.Parameters.AddWithValue("@name", PartyName);
                insertCommand.Parameters.AddWithValue("@code", PartyCode);
                insertCommand.Parameters.AddWithValue("@location", Location);
                insertCommand.Parameters.AddWithValue("@description", Description);
                insertCommand.Parameters.AddWithValue("@djid", djId);
                insertCommand.Parameters.AddWithValue("@starttime", DateTime.Now);

                insertCommand.ExecuteNonQuery();
            }

            // Spremimo kod partya u Session
            HttpContext.Session.SetString("CurrentPartyCode", PartyCode);

            return RedirectToAction("PartyLive");
        }


        [HttpPost]
        public IActionResult StopParty()
        {
            var partyCode = HttpContext.Session.GetString("CurrentPartyCode");
            if (string.IsNullOrEmpty(partyCode))
                return RedirectToAction("Profile");

            using (var connection = new SqliteConnection("Data Source=C:\\party_data\\party_app.db"))
            {
                connection.Open();

                var updateCommand = connection.CreateCommand();
                updateCommand.CommandText = @"
            UPDATE Parties
            SET EndTime = @endtime
            WHERE PartyCode = @code;";

                updateCommand.Parameters.AddWithValue("@endtime", DateTime.Now);
                updateCommand.Parameters.AddWithValue("@code", partyCode);

                updateCommand.ExecuteNonQuery();
            }

            // Očistimo Session jer party je završen
            HttpContext.Session.Remove("CurrentPartyCode");

            TempData["PollClosed"] = "Party je uspješno završen!";
            return RedirectToAction("Profile");
        }





        private string GeneratePartyCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public IActionResult PartyHistory()
        {
            return Content("Ovdje će biti prikaz povijesti partija.");
        }

        public IActionResult Settings()
        {
            return Content("Postavke će biti dostupne uskoro.");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult ChangePicture()
        {
            return Content("Promjena profilne slike još nije implementirana.");
        }
        [HttpPost]
        public IActionResult PickRandomSong()
        {
            string currentCode = HttpContext.Session.GetString("CurrentPartyCode");

            var songs = PartyManController.SongRequests
                .Where(s => s.PartyCode == currentCode)
                .ToList();

            if (songs.Count == 0)
            {
                TempData["RandomSong"] = "Nema dostupnih pjesama.";
            }
            else
            {
                var random = new Random();
                var randomSong = songs[random.Next(songs.Count)];
                TempData["RandomSong"] = $"🎵 Random pjesma je: {randomSong.SongName}";
            }

            return RedirectToAction("PartyLive");
        }

        [HttpPost]
        public IActionResult RejectSong(int id)
        {
            var song = PartyManController.SongRequests.FirstOrDefault(s => s.Id == id);
            if (song != null)
            {
                PartyManController.SongRequests.Remove(song);
            }

            return RedirectToAction("PartyLive");
        }
        public static List<SongRequest> CurrentPollSongs = new List<SongRequest>();

        [HttpPost]
        public IActionResult StartPoll()
        {
            string currentCode = HttpContext.Session.GetString("CurrentPartyCode");

            var songs = PartyManController.SongRequests
                .Where(s => s.PartyCode == currentCode)
                .OrderByDescending(s => s.VoteCount)
                .Take(4)
                .ToList();

            CurrentPollSongs = songs;

            // Sve PartyMan korisnike preusmjeravamo na VoteInPoll
            return RedirectToAction("PartyLive");
        }

        [HttpPost]
        public IActionResult ClosePoll()
        {
            DJController.CurrentPollSongs.Clear();
            PartyManController.VotedNicknames.Clear();

            TempData["PollClosed"] = "Anketa je uspješno zatvorena.";

            return RedirectToAction("PartyLive");
        }


       





    }
}
