using CarWashApp.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarWashApp.Services
{
    public interface IAuthenticationService
    {
        bool IsAuthenticated { get; }
        bool IsAdmin { get; }
        int? StaffId { get; }
        string? Username { get; }
        string? Role { get; }
        Task<bool> LoginAsync(string username, string password);
        Task LogoutAsync();
        /// <summary>Restores the login after a page refresh (needs the browser, so call after first render).</summary>
        Task RestoreAsync();
        /// <summary>Re-reads the signed-in user, e.g. after the admin edits their own account.</summary>
        Task RefreshAsync();
        event Action? AuthenticationStateChanged;
    }

    public class AuthenticationService(CarWashAppDbContext db, ProtectedSessionStorage sessionStorage) : IAuthenticationService
    {
        private const string StorageKey = "staffId";
        private readonly PasswordHasher<Staff> _hasher = new();

        public bool IsAuthenticated => StaffId.HasValue;
        public bool IsAdmin => Role == StaffRoles.Admin;
        public int? StaffId { get; private set; }
        public string? Username { get; private set; }
        public string? Role { get; private set; }

        public event Action? AuthenticationStateChanged;

        public async Task<bool> LoginAsync(string username, string password)
        {
            var name = username.Trim();
            var staff = await db.Staff.AsNoTracking().FirstOrDefaultAsync(s => s.Username == name && s.IsActive);
            if (staff == null)
            {
                // Same work as a real check, so the response time doesn't reveal whether the user exists.
                _hasher.VerifyHashedPassword(new Staff(), _hasher.HashPassword(new Staff(), "x"), password);
                return false;
            }

            var result = _hasher.VerifyHashedPassword(staff, staff.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
                return false;

            SignIn(staff);
            await sessionStorage.SetAsync(StorageKey, staff.Id);
            AuthenticationStateChanged?.Invoke();
            return true;
        }

        public async Task LogoutAsync()
        {
            StaffId = null;
            Username = null;
            Role = null;
            try { await sessionStorage.DeleteAsync(StorageKey); } catch { /* browser not available */ }
            AuthenticationStateChanged?.Invoke();
        }

        public async Task RestoreAsync()
        {
            if (IsAuthenticated) return;

            try
            {
                var stored = await sessionStorage.GetAsync<int>(StorageKey);
                if (!stored.Success) return;

                var staff = await db.Staff.AsNoTracking().FirstOrDefaultAsync(s => s.Id == stored.Value && s.IsActive);
                if (staff != null)
                {
                    SignIn(staff);
                    AuthenticationStateChanged?.Invoke();
                }
            }
            catch
            {
                // Stored value can't be read (e.g. keys changed): treat as signed out.
            }
        }

        public async Task RefreshAsync()
        {
            if (StaffId is not int id) return;

            var staff = await db.Staff.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
            if (staff == null)
            {
                await LogoutAsync();
                return;
            }

            SignIn(staff);
            AuthenticationStateChanged?.Invoke();
        }

        private void SignIn(Staff staff)
        {
            StaffId = staff.Id;
            Username = staff.Username;
            Role = staff.Role;
        }
    }
}
