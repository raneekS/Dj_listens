using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Dj_listens.Hubs
{
    public class PartyHub : Hub
    {
        public async Task JoinPartyGroup(string partyCode)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, partyCode);
        }

        public async Task NotifyPartyEnded(string partyCode)
        {
            await Clients.Group(partyCode).SendAsync("PartyEnded");
        }
    }
}
