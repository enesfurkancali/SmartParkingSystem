using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartParkingSystemAPI.Context;
using SmartParkingSystemAPI.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableDependency.SqlClient;

namespace SmartParkingSystemAPI.Subscription
{
    public interface IDatabaseSubscription
    {
        void Configure(string tableName); //takip edilecek tablonun ismini alacak
    }
    public class DatabaseSubscription<T> : IDatabaseSubscription where T : class, new()
    {
        IConfiguration _configuration;
        IHubContext<ParksisHub> _hubContext;

        public DatabaseSubscription(IConfiguration configuration, IHubContext<ParksisHub> hubContext)
        {
            _configuration = configuration;
            _hubContext = hubContext;
        }

        SqlTableDependency<T> _tableDependency;
        public void Configure(string tableName)
        {
            _tableDependency = new SqlTableDependency<T>(_configuration.GetConnectionString("Default"), tableName);
            _tableDependency.OnChanged += async (o, e) =>
            {

                SmartParkingSystemContext context = new SmartParkingSystemContext();

                if (tableName == "Entry")
                {
                    var veriList = context.Entries.OrderBy(x => x.CheckinDate).ToList();
                    await _hubContext.Clients.All.SendAsync("recieveMessageEntry", veriList);
                }
                else if (tableName=="User")
                {
                    var veriList = context.Users.OrderBy(x => x.Username).ToList();
                    await _hubContext.Clients.All.SendAsync("recieveMessageUser", veriList);
                }
                else
                {
                    var veriList = context.Configs.OrderBy(x => x.Name).ToList();
                    await _hubContext.Clients.All.SendAsync("recieveMessageConfig", veriList);
                }
               
            };
            _tableDependency.OnError += (o, e) =>
            {

            };

            _tableDependency.Start();
        }

        ~DatabaseSubscription()
        {
            _tableDependency.Stop();
        }
    }
}
