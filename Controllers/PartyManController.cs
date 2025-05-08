using Microsoft.AspNetCore.Mvc;
using Dj_listens.Models;

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

        [HttpPost]
        public IActionResult EnterParty(string partyCode, string nickname)
        {
            if (!PartyNicknames.ContainsKey(partyCode))
            {
                PartyNicknames[partyCode] = new List<string>();
            }

            if (PartyNicknames[partyCode].Contains(nickname))
            {
                ViewBag.Error = "Taj nickname već postoji na ovom partiju!";
                return View();
            }

            // Dodaj nickname
            PartyNicknames[partyCode].Add(nickname);

            // Spremi kod i nick u session
            HttpContext.Session.SetString("PartyCode", partyCode);
            HttpContext.Session.SetString("Nickname", nickname);

            return RedirectToAction("SendSong");
        }

        [HttpGet]
        public IActionResult SendSong()
        {
            var nickname = HttpContext.Session.GetString("Nickname");

            if (DJController.CurrentPollSongs != null && DJController.CurrentPollSongs.Count > 0)
            {
                if (!VotedNicknames.Contains(nickname))
                {
                    return RedirectToAction("VoteInPoll");
                }
            }

            return View();
        }



        [HttpPost]
        public IActionResult SendSong(string SongName, string YoutubeUrl, string Description)
        {
            var partyCode = HttpContext.Session.GetString("PartyCode");
            var nickname = HttpContext.Session.GetString("Nickname");

            // Prvo provjeri postoji li pjesma s istim URL-om i kodom
            var existingSong = SongRequests
                .FirstOrDefault(s => s.PartyCode == partyCode && s.YoutubeUrl == YoutubeUrl);

            if (existingSong != null)
            {
                // Ako postoji, samo povećaj broj glasova
                existingSong.VoteCount++;
            }
            else
            {
                // Ako ne postoji, dodaj novu pjesmu
                SongRequests.Add(new SongRequest
                {
                    Id = SongRequests.Count + 1,
                    SongName = SongName,
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
            var songs = DJController.CurrentPollSongs;

            if (songs == null || songs.Count == 0)
            {
                return Content("Trenutno nema aktivne ankete.");
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



    }
}
