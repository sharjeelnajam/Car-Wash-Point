using System.ComponentModel.DataAnnotations;

namespace CarWashApp.Models.DTOs
{
    public class CarDto
    {
        public int Id { get; set; }

        public string LicensePlate { get; set; } // Unique identifier

        public string Company { get; set; } // e.g., Toyota

        public string Name { get; set; } // e.g., Corolla

        public int Year { get; set; }

        public string Color { get; set; }

        public int LoyaltyPoints { get; set; }
    }
}
