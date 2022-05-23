using Microsoft.EntityFrameworkCore;
using SmartParkingSystemAPI.TokenOperations.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartParkingSystemAPI.Context
{
    public class SecurityContext : DbContext
    {
        public SecurityContext(DbContextOptions<SecurityContext> options) : base(options)
        {

        }

        public DbSet<LoggedInUser> LoggedInUsers { get; set; }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }
    }
}
