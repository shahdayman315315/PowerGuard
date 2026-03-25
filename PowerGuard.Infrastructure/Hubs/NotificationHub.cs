using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub:Hub
    {
       
        public override async Task OnConnectedAsync()
        {
            var factoryId= Context.User?.FindFirst("FactoryId")?.Value;
            var departmentId= Context.User?.FindFirst("DepartmentId")?.Value;
            var userRole= Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(factoryId))
            {
                if (userRole == "FactoryManager")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"FactoryManagers-{factoryId}");
                }

                else if(!string.IsNullOrEmpty(departmentId) && userRole=="DepartmentManager")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Department-{departmentId}");
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
