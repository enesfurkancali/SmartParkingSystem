using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartParkingSystemAPI.Hubs
{
    public class ParksisHub:Hub
    {
        public async Task SendMessageAsync(string plate)
        {
            await Clients.All.SendAsync("recievePlate", plate);
        }
    }
}
