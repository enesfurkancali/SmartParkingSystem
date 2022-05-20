using System;
using System.Collections.Generic;

#nullable disable

namespace SmartParkingSystem.Models
{
    public partial class Entry
    {
        public Guid Id { get; set; }
        public string Plate { get; set; }
        public double? Price { get; set; }
        public DateTime? CheckoutDate { get; set; }
        public DateTime? CheckinDate { get; set; }
    }
}
