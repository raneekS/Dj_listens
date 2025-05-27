using Microsoft.AspNetCore.Mvc;
using Dj_listens.Models;
using Microsoft.Data.Sqlite;
using System.Text.RegularExpressions;
using Newtonsoft.Json;


namespace Dj_listens.Controllers
{
    public class PartyManController : Controller
    {
        public static List<string> VotedNicknames = new List<string>();

        // Ovo je privremena lista prijavljenih korisnika po party kodu
        public static Dictionary<string, List<string>> PartyNicknames = new Dictionary<string, List<string>>();

        [HttpGet]
        public IActionResult EnterParty()
        {
            return View();
        }
        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("dj_username"));
        }

        [HttpPost]
        public IActionResult EnterParty(string partyCode, string nickname)
        {
            if (string.IsNullOrWhiteSpace(partyCode) || string.IsNullOrWhiteSpace(nickname))
            {
                TempData["Error"] = "Kod i nadimak moraju biti ispunjeni.";
                return RedirectToAction("EnterParty");
            }

            using (var connection = new SqliteConnection("Data Source=C:\\party_data\\party_app.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
            SELECT COUNT(*) 
            FROM Parties 
            WHERE PartyCode = @code AND EndTime IS NULL;";
                command.Parameters.AddWithValue("@code", partyCode);

                long exists = (long)command.ExecuteScalar();

                if (exists == 0)
                {
                    TempData["Error"] = $"Nije pronađen nijedan aktivan party s kodom \"{partyCode}\".";
                    return RedirectToAction("EnterParty");
                }
            }

            // Provjera da isti nickname nije već u upotrebi za taj party
            if (!PartyNicknames.ContainsKey(partyCode))
                PartyNicknames[partyCode] = new List<string>();

            if (PartyNicknames[partyCode].Contains(nickname))
            {
                TempData["Error"] = $"Nadimak \"{nickname}\" je već zauzet u ovom partyju.";
                return RedirectToAction("EnterParty");
            }

            // Spremi kod i nickname u session
            HttpContext.Session.SetString("CurrentPartyCode", partyCode);
            HttpContext.Session.SetString("Nickname", nickname);
            PartyNicknames[partyCode].Add(nickname);

            return RedirectToAction("SendSong");
        }



        [HttpGet]
        public IActionResult SendSong()
        {
            var nickname = HttpContext.Session.GetString("Nickname");
            var partyCode = HttpContext.Session.GetString("CurrentPartyCode");

            if (DJController.CurrentPollSongs != null && DJController.CurrentPollSongs.Count > 0)
            {
                if (!VotedNicknames.Contains(nickname))
                {
                    return RedirectToAction("VoteInPoll");
                }
            }

            var mySongs = SongRequests
                .Where(s => s.PartyCode == partyCode && s.Nickname == nickname)
                .OrderByDescending(s => s.Id)
                .ToList();

            ViewBag.MySongs = mySongs;
            ViewBag.PartyCode = partyCode;  

            return View();
        }






        [HttpPost]
        public IActionResult SendSong(string YoutubeUrl, string Description)
        {
            var partyCode = HttpContext.Session.GetString("CurrentPartyCode");
            var nickname = HttpContext.Session.GetString("Nickname");

            string songName = GetYouTubeTitle(YoutubeUrl); // ⬅️ automatski dohvat

            var existingSong = SongRequests
                .FirstOrDefault(s => s.PartyCode == partyCode && s.YoutubeUrl == YoutubeUrl);

            if (existingSong != null)
            {
                existingSong.VoteCount++;
            }
            else
            {
                SongRequests.Add(new SongRequest
                {
                    Id = SongRequests.Count + 1,
                    SongName = songName, // ⬅️ koristi dohvaćeni naziv
                    YoutubeUrl = YoutubeUrl,
                    Description = Description,
                    PartyCode = partyCode,
                    Nickname = nickname,
                    VoteCount = 1
                });
            }

            return RedirectToAction("SendSong");
        }




        // Dodaj privremenu listu SongRequest-a ako je nemaš:
        public static List<SongRequest> SongRequests = new List<SongRequest>();


        [HttpGet]
        public IActionResult VoteInPoll()
        {
            var nickname = HttpContext.Session.GetString("Nickname");

            // Ako je već glasao, preusmjeri ga na SendSong bez prikaza ankete
            if (VotedNicknames.Contains(nickname))
            {
                return RedirectToAction("SendSong");
            }

            var songs = DJController.CurrentPollSongs;

            if (songs == null || songs.Count == 0)
            {
                return RedirectToAction("SendSong");
            }


            return View(songs);
        }



        [HttpPost]
        public IActionResult SubmitVote(int songId)
        {
            var nickname = HttpContext.Session.GetString("Nickname");

            if (VotedNicknames.Contains(nickname))
            {
                // Ako je već glasao, samo ga vratimo na SendSong
                return RedirectToAction("SendSong");
            }

            var song = DJController.CurrentPollSongs.FirstOrDefault(s => s.Id == songId);
            if (song != null)
            {
                song.PollVotes++;
            }

            VotedNicknames.Add(nickname); // Spremimo da je glasao

            return RedirectToAction("SendSong");
        }
        private string GetYouTubeTitle(string url)
        {
            string videoId = ExtractVideoId(url);
            if (string.IsNullOrEmpty(videoId)) return "Nepoznata pjesma";

            string apiKey = "AIzaSyDHD8vsRqz8ZyKCqOPUuEKmBYnkqarZ9Tk";
            string apiUrl = $"https://www.googleapis.com/youtube/v3/videos?part=snippet&id={videoId}&key={apiKey}";

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(apiUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    dynamic data = JsonConvert.DeserializeObject(json);
                    return data.items.Count > 0 ? data.items[0].snippet.title.ToString() : "Nepoznata pjesma";
                }
            }
            return "Greška u dohvaćanju naslova";
        }
        private string ExtractVideoId(string url)
        {
            var regex = new Regex(@"(?:youtube\.com.*(?:\\?|&)v=|youtu\.be/)([^&\n?#]+)");
            var match = regex.Match(url);
            return match.Success ? match.Groups[1].Value : null;
        }


        [HttpGet, HttpPost]
        public IActionResult LeaveParty()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }







    }
}
