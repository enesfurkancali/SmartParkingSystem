using System;
using System.Collections.Generic;

#nullable disable

namespace SmartParkingSystem.Models
{
    public partial class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Password { get; set; }
    }
}
