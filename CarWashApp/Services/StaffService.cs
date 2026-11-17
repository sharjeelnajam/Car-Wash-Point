using CarWashApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarWashApp.Services
{
    public class StaffService(CarWashAppDbContext db)
    {
        private readonly PasswordHasher<Staff> _hasher = new();

        public Task<List<Staff>> GetAllAsync() =>
            db.Staff.AsNoTracking().OrderBy(s => s.Username).ToListAsync();

        /// <summary>Returns an error message, or null when the user was created.</summary>
        public async Task<string?> CreateAsync(string username, string password, string role)
        {
            var name = (username ?? "").Trim();
            if (name.Length < 3) return "Username must be at least 3 characters.";
            if (string.IsNullOrEmpty(password) || password.Length < 6) return "Password must be at least 6 characters.";
            if (await db.Staff.AnyAsync(s => s.Username == name)) return "That username already exists.";

            var staff = new Staff { Username = name, Role = NormalizeRole(role), IsActive = true };
            staff.PasswordHash = _hasher.HashPassword(staff, password);
            db.Staff.Add(staff);
            await db.SaveChangesAsync();
            return null;
        }

        /// <summary>Updates a user. A blank newPassword keeps the old password. Returns an error message, or null.</summary>
        public async Task<string?> UpdateAsync(int id, string username, string role, bool isActive, string? newPassword)
        {
            var staff = await db.Staff.FirstOrDefaultAsync(s => s.Id == id);
            if (staff == null) return "User not found.";

            var name = (username ?? "").Trim();
            if (name.Length < 3) return "Username must be at least 3 characters.";
            if (await db.Staff.AnyAsync(s => s.Username == name && s.Id != id)) return "That username already exists.";
            if (!string.IsNullOrEmpty(newPassword) && newPassword.Length < 6) return "Password must be at least 6 characters.";

            var newRole = NormalizeRole(role);
            var losesAdmin = staff.Role == StaffRoles.Admin && staff.IsActive && (newRole != StaffRoles.Admin || !isActive);
            if (losesAdmin && !await db.Staff.AnyAsync(s => s.Id != id && s.Role == StaffRoles.Admin && s.IsActive))
                return "There must always be at least one active admin.";

            staff.Username = name;
            staff.Role = newRole;
            staff.IsActive = isActive;
            if (!string.IsNullOrEmpty(newPassword))
                staff.PasswordHash = _hasher.HashPassword(staff, newPassword);

            await db.SaveChangesAsync();
            return null;
        }

        /// <summary>Creates the default logins the first time the app runs.</summary>
        public static async Task SeedAsync(CarWashAppDbContext db, IConfiguration configuration)
        {
            if (await db.Staff.AnyAsync()) return;

            var hasher = new PasswordHasher<Staff>();
            var admin = new Staff { Username = "admin", Role = StaffRoles.Admin };
            admin.PasswordHash = hasher.HashPassword(admin, configuration["Seed:AdminPassword"] ?? "Admin@123");
            var staff = new Staff { Username = "staff", Role = StaffRoles.Staff };
            staff.PasswordHash = hasher.HashPassword(staff, configuration["Seed:StaffPassword"] ?? "Staff@123");

            db.Staff.AddRange(admin, staff);
            await db.SaveChangesAsync();
        }

        private static string NormalizeRole(string role) =>
            role == StaffRoles.Admin ? StaffRoles.Admin : StaffRoles.Staff;
    }
}
