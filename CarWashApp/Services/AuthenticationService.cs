using System.Security.Cryptography;
using System.Text;

namespace CarWashApp.Services
{
    public interface IAuthenticationService
    {
        bool IsAuthenticated { get; }
        string? Username { get; }
        Task<bool> LoginAsync(string username, string password);
        Task LogoutAsync();
        event Action? AuthenticationStateChanged;
    }

    public class AuthenticationService : IAuthenticationService
    {
        private bool _isAuthenticated = false;
        private string? _username = null;

        public bool IsAuthenticated => _isAuthenticated;
        public string? Username => _username;

        public event Action? AuthenticationStateChanged;

        public async Task<bool> LoginAsync(string username, string password)
        {
            // Simple hardcoded credentials for demo purposes
            // In production, you would validate against a database or external service
            if (username == "admin" && password == "admin123")
            {
                _isAuthenticated = true;
                _username = username;
                AuthenticationStateChanged?.Invoke();
                return true;
            }

            return false;
        }

        public async Task LogoutAsync()
        {
            await Task.Delay(50);
            _isAuthenticated = false;
            _username = null;
            AuthenticationStateChanged?.Invoke();
        }

        // Helper method to hash passwords (for future use with database)
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
