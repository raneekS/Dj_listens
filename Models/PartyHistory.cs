namespace Dj_listens.Models
{
    public class PartyHistory
    {
        public int Id { get; set; }             // ID partya
        public string PartyName { get; set; }   // Naziv partya
        public string PartyCode { get; set; }   // Kod partya (jedinstveni)
        public string Location { get; set; }    // Lokacija partya
        public string Description { get; set; } // Opis partya
        public DateTime StartTime { get; set; } // Vrijeme početka partya
        public DateTime? EndTime { get; set; }  // Vrijeme završetka partya (može biti null)
    }
}
