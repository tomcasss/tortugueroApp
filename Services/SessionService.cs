using tortugueroApp.Models;

namespace tortugueroApp.Services;

public class SessionService
{
    private static SessionService? _instance;
    private Usuario? _currentUser;
    private bool _isGuestMode;

    public static SessionService Instance => _instance ??= new SessionService();

    private SessionService() { }

   
    public Usuario? CurrentUser
    {
        get => _currentUser;
        private set
        {
            _currentUser = value;
            OnUserChanged?.Invoke(value);
        }
    }

    public bool IsLoggedIn => CurrentUser != null;
    public bool IsGuestMode => _isGuestMode;
    public long CurrentUserId => CurrentUser?.IdUsuario ?? 0;

   
    public event Action<Usuario?>? OnUserChanged;
    public event Action? OnLogout;

   
    public void SetUser(Usuario usuario)
    {
        CurrentUser = usuario;
        _isGuestMode = false;
        
       
        Preferences.Set("user_id", usuario.IdUsuario);
        Preferences.Set("user_name", usuario.NombreCompleto ?? "");
        Preferences.Set("user_email", usuario.Correo ?? "");
        Preferences.Set("is_logged_in", true);
    }

    public void SetGuestMode()
    {
        CurrentUser = null;
        _isGuestMode = true;
        Preferences.Set("is_guest_mode", true);
        Preferences.Set("is_logged_in", false);
    }

    public async Task<bool> RestoreSessionAsync()
    {
        var isLoggedIn = Preferences.Get("is_logged_in", false);
        
        if (isLoggedIn)
        {
            var userId = Preferences.Get("user_id", 0);
            if (userId > 0)
            {
                try
                {
                 
                    var client = await SupabaseService.GetClientAsync();
                    var response = await client
                        .From<Usuario>()
                        .Where(x => x.IdUsuario == userId)
                        .Get();

                    if (response.Models.Count > 0)
                    {
                        CurrentUser = response.Models[0];
                        _isGuestMode = false;
                        return true;
                    }
                }
                catch
                {
                   
                    ClearSession();
                }
            }
        }
        else
        {
            _isGuestMode = Preferences.Get("is_guest_mode", false);
        }

        return false;
    }

    public void Logout()
    {
        ClearSession();
        OnLogout?.Invoke();
    }

    private void ClearSession()
    {
        CurrentUser = null;
        _isGuestMode = false;
        
        Preferences.Remove("user_id");
        Preferences.Remove("user_name");
        Preferences.Remove("user_email");
        Preferences.Remove("is_logged_in");
        Preferences.Remove("is_guest_mode");
    }

    public bool CanAccessFeature(string featureName)
    {
       
        var restrictedFeatures = new[] { "booking", "reservations", "profile" };
        
        if (restrictedFeatures.Contains(featureName.ToLower()))
        {
            return IsLoggedIn;
        }
        
        return true; 
    }
}
