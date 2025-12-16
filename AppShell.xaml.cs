using tortugueroApp.Services;
using tortugueroApp.Views;

namespace tortugueroApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
           
            Routing.RegisterRoute(nameof(MapPage), typeof(MapPage));
            
           
            CheckAndRestoreSession();
        }

        private async void CheckAndRestoreSession()
        {
            var sessionService = SessionService.Instance;
            var hasSession = await sessionService.RestoreSessionAsync();

           
            if (hasSession)
            {
                await GoToAsync("//MainMenu");
            }
            
            else if (sessionService.IsGuestMode)
            {
                await GoToAsync("//MainMenu");
            }
           
            else
            {
                await GoToAsync("//Welcome");
            }
        }
    }
}
