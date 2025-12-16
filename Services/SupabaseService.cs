using Supabase;

namespace tortugueroApp.Services;

public class SupabaseService
{
    private static Supabase.Client? _client;

    public static async Task<Supabase.Client> GetClientAsync()
    {
        if (_client == null)
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            _client = new Supabase.Client(AppConfig.SupabaseUrl, AppConfig.SupabaseKey, options);
            await _client.InitializeAsync();
        }

        return _client;
    }
}
