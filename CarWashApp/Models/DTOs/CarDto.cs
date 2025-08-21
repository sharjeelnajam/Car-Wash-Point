using System.ComponentModel.DataAnnotations;

namespace CarWashApp.Models.DTOs
{
    public class CarDto
    {
        public int Id { get; set; }

        public string LicensePlate { get; set; } // Unique identifier

        public string Name { get; set; } // e.g., Corolla

        public string Color { get; set; }

        public int LoyaltyPoints { get; set; }
    }
}
