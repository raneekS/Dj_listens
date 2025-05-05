namespace Dj_listens.Models
{
    public class SongRequest
    {
        public int Id { get; set; }
        public string SongName { get; set; }
        public string YoutubeUrl { get; set; }
        public string Description { get; set; }
        public string PartyCode { get; set; }
        public int VoteCount { get; set; } = 1;
        public string Nickname { get; set; } = "";
        public int PollVotes { get; set; } = 0;

    }
}
