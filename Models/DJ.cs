using Dj_listens.Models;

namespace Dj_listens.Models


{

    public class DJ
    {

        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // plain text za sada
        // u budućnosti možeš dodati Email, Slika, itd.
    }
}
