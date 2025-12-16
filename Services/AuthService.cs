using System.Security.Cryptography;
using System.Text;
using tortugueroApp.Models;

namespace tortugueroApp.Services;

public class AuthService
{
    private readonly SessionService _sessionService;

    public AuthService()
    {
        _sessionService = SessionService.Instance;
    }

    public async Task<(bool success, string message, Usuario? usuario)> LoginAsync(string usuarioOCorreo, string contrasenia)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(usuarioOCorreo) || string.IsNullOrWhiteSpace(contrasenia))
            {
                return (false, "Please enter username/email and password", null);
            }

            
            string identificadorNormalizado = usuarioOCorreo.ToLower().Trim();
            string contraseniaHash = HashPassword(contrasenia);

            var client = await SupabaseService.GetClientAsync();
            
        
            var response = await client
                .From<Usuario>()
                .Where(x => x.ContraseniaHash == contraseniaHash)
                .Get();

            // Filtrar en memoria por correo o nombre de usuario
            var usuario = response.Models.FirstOrDefault(u => 
                u.Correo?.ToLower() == identificadorNormalizado || 
                u.NombreUsuario?.ToLower() == identificadorNormalizado);

            if (usuario == null)
            {
                return (false, "Incorrect username or password", null);
            }

            _sessionService.SetUser(usuario);
            return (true, "Login successful", usuario);
        }
        catch (Exception ex)
        {
            return (false, $"Login error: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message, Usuario? usuario)> RegisterAsync(
        string nombreCompleto,
        string correo,
        string nombreUsuario,
        string contrasenia,
        string? telefono = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto) || 
                string.IsNullOrWhiteSpace(correo) || 
                string.IsNullOrWhiteSpace(nombreUsuario) ||
                string.IsNullOrWhiteSpace(contrasenia))
            {
                return (false, "Please complete all required fields", null);
            }

            if (contrasenia.Length < 6)
            {
                return (false, "Password must be at least 6 characters", null);
            }

          
            string correoNormalizado = correo.ToLower().Trim();
            string usuarioNormalizado = nombreUsuario.ToLower().Trim();

            var client = await SupabaseService.GetClientAsync();
            
          
            var existingEmail = await client
                .From<Usuario>()
                .Where(x => x.Correo == correoNormalizado)
                .Get();

            if (existingEmail.Models.Count > 0)
            {
                return (false, "This email is already registered", null);
            }

          
            var existingUsername = await client
                .From<Usuario>()
                .Where(x => x.NombreUsuario == usuarioNormalizado)
                .Get();

            if (existingUsername.Models.Count > 0)
            {
                return (false, "This username is already taken", null);
            }

          
            var nuevoUsuario = new Usuario
            {
                NombreCompleto = nombreCompleto.Trim(),
                Correo = correoNormalizado,
                NombreUsuario = usuarioNormalizado,
                ContraseniaHash = HashPassword(contrasenia),
                NumeroTelefono = telefono?.Trim(),
                FechaCreacion = DateTime.UtcNow
            };

            var response = await client
                .From<Usuario>()
                .Insert(nuevoUsuario);

            var usuarioCreado = response.Models[0];
            _sessionService.SetUser(usuarioCreado);
            
            return (true, "Account created successfully", usuarioCreado);
        }
        catch (Exception ex)
        {
            return (false, $"Registration error: {ex.Message}", null);
        }
    }

    public void Logout()
    {
        _sessionService.Logout();
    }

    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
