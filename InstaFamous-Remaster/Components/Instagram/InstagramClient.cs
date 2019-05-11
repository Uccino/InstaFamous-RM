using System.Threading.Tasks;
using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Classes.Models;
using InstaSharper.Logger;

namespace InstaFamous.Components.Instagram
{
    class InstagramClient
    {
        private IInstaApi InstaClient;

        public InstagramClient(string username, string password, string tags)
        {
            // Get the new usersession
            var userSession = new UserSessionData()
            {
                UserName = username,
                Password = password
            };

            // Build the instagramclient
            var delay = RequestDelay.FromSeconds(1, 3);
            InstaClient = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.Exceptions))
                .SetRequestDelay(delay)
                .Build();

        }

        public bool Login()
        {
            var loginResult = Task.Run(() => InstaClient.LoginAsync().Result);
            if (loginResult.Result.Succeeded && InstaClient.IsUserAuthenticated)
            {
                return true;
            }

            return false;
        }

        public bool Logout()
        {
            if (!InstaClient.IsUserAuthenticated)
            {
                var logoutResult = Task.Run(() => InstaClient.LogoutAsync().Result);
                if (logoutResult.Result.Succeeded && !InstaClient.IsUserAuthenticated)
                {
                    return true;
                }
            }

            return false;
        }

        public void PostImage()
        {
            
        }
    }
}
