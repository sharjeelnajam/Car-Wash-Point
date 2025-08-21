using System.ComponentModel.DataAnnotations;

namespace CarWashApp.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string LicensePlate { get; set; } // Unique identifier

        [Required]
        [StringLength(50)]
        public string Name { get; set; } // e.g., Corolla

        public string Color { get; set; }

        // Loyalty points accumulated by washes
        public int LoyaltyPoints { get; set; }
    }
}
