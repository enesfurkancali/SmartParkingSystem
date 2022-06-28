using SmartParkingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartParkingSystemAPI
{
    public class CalculatePrice
    {
        public static double price { get; set; } = 0;
        public static List<Config> priceList { get; set; }
        public static double Calculate(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours< 1)
                price +=Convert.ToDouble(priceList.Where(x=>x.Name=="0-1 Saat").Select(x=>x.Value).FirstOrDefault());
            else if (timeSpan.TotalHours >= 1 && timeSpan.TotalHours < 2)
                price += Convert.ToDouble(priceList.Where(x => x.Name == "1-2 Saat").Select(x => x.Value).FirstOrDefault());
            else if (timeSpan.TotalHours >= 2 && timeSpan.TotalHours < 4)
                price += Convert.ToDouble(priceList.Where(x => x.Name == "2-4 Saat").Select(x => x.Value).FirstOrDefault());
            else if (timeSpan.TotalHours >= 4 && timeSpan.TotalHours < 8)
                price += Convert.ToDouble(priceList.Where(x => x.Name == "4-8 Saat").Select(x => x.Value).FirstOrDefault());
            else if (timeSpan.TotalHours >= 8 && timeSpan.TotalHours < 12)
                price += Convert.ToDouble(priceList.Where(x => x.Name == "8-12 Saat").Select(x => x.Value).FirstOrDefault());
            else if (timeSpan.TotalHours >= 12 && timeSpan.TotalHours < 24)
                price += Convert.ToDouble(priceList.Where(x => x.Name == "Tam Gün").Select(x => x.Value).FirstOrDefault());
            else
            {
                price += Convert.ToDouble(priceList.Where(x => x.Name == "Tam Gün").Select(x => x.Value).FirstOrDefault());
                timeSpan += TimeSpan.FromHours(-24);
                Calculate(timeSpan);
            }
            return price;
        }
        
    }
}
