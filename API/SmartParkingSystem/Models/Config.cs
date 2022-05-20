using System;
using System.Collections.Generic;

#nullable disable

namespace SmartParkingSystem.Models
{
    public partial class Config
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
