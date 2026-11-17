using System.ComponentModel.DataAnnotations;

namespace CarWashApp.Models
{
    public class Staff
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        // "Admin" can manage staff and see reports; "Staff" can only take payments.
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = StaffRoles.Staff;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public static class StaffRoles
    {
        public const string Admin = "Admin";
        public const string Staff = "Staff";
    }
}
