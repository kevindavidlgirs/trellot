
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using prid_2021_g06.Models;
using System.Collections.Generic;


namespace prid_2021_g06.Service
{
    public interface IGeneralHubService
    {
        Task Notify(DTO e);
    }

    public class GeneralHub : Hub<IGeneralHubService>
    {
    }
}