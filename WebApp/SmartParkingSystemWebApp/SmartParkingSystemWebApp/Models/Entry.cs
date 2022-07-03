using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace SmartParkingSystemWebApp.Models
{
    public partial class Entry
    {
        public Guid Id { get; set; }
        [Required]
        public string Plate { get; set; }
        [Required]
        public double? Price { get; set; }
        public DateTime? CheckoutDate { get; set; }
        public DateTime? CheckinDate { get; set; }
    }
}
