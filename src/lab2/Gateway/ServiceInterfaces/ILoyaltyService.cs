using System.Net.Http;
using System;
using Gateway.Models;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Gateway.Controllers;
using Microsoft.Extensions.Logging;
using Gateway.Utils;
using System.Net;

namespace Gateway.ServiceInterfaces
{
    public interface ILoyaltyService
    {

        public Task<bool> HealthCheckAsync();

        public Task<Loyalty?> GetLoyaltyByUsernameAsync(string username);

        public Task<Loyalty?> PutLoyaltyByUsernameAsync(string username);

        public Task<Loyalty?> DeleteLoyaltyByUsernameAsync(string username);
    }
}
