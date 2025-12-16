namespace tortugueroApp.Services;

public static class AppConfig
{
    public static string SupabaseUrl { get; private set; } = string.Empty;
    public static string SupabaseKey { get; private set; } = string.Empty;

    public static void Initialize()
    {
        LoadFromEnvironmentOrDefaults();
    }

    private static void LoadFromEnvironmentOrDefaults()
    {
        SupabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") 
            ?? "https://vhqycikuscmrnkejswyu.supabase.co";
        
        SupabaseKey = Environment.GetEnvironmentVariable("SUPABASE_KEY") 
            ?? "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InZocXljaWt1c2Ntcm5rZWpzd3l1Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjQxMTY1MDAsImV4cCI6MjA3OTY5MjUwMH0.dthzHoySfo082ruEugVMkTPUe7totdHFLGg5IUkRk8Y";
    }
}
