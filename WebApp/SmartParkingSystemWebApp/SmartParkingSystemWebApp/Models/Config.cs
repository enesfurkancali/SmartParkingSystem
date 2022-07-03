using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace SmartParkingSystemWebApp.Models
{
    public partial class Config
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        [Required]
        public string Value { get; set; }
    }
}
