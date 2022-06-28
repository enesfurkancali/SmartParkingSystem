using Microsoft.AspNetCore.SignalR;
using SmartParkingSystem.Models;
using SmartParkingSystemAPI.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartParkingSystemAPI.Hubs
{
    public class ParksisHub:Hub
    {
        private readonly SmartParkingSystemContext _context;

        public ParksisHub(SmartParkingSystemContext context)
        {
            _context = context;
        }

        public async Task SendMessageAsync(string plate)
        {
            var entry = _context.Entries.FirstOrDefault(x => x.Plate == plate && x.CheckoutDate == null);
            if (entry is not null)
            {
                entry.CheckoutDate = DateTime.Now;
                TimeSpan diff = (TimeSpan)(entry.CheckoutDate - entry.CheckinDate);
                CalculatePrice.price = 0;
                CalculatePrice.priceList = _context.Configs.ToList();
                entry.Price = CalculatePrice.Calculate(diff);
            }
            else
            {
                entry = new Entry();
                entry.Id = Guid.NewGuid();
                entry.CheckinDate = DateTime.Now;
                entry.CheckoutDate = null;
                entry.Plate = plate.Trim();
                entry.Price = 0;
            }
            await Clients.All.SendAsync("recievePlate", entry);
        }
    }
}
