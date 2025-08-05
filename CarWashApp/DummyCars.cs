using CarWashApp.Models;

namespace CarWashApp
{
    public static class DummyCars
    {
        public static List<Car> GetCars()
        {
            return new List<Car>
            {
                new Car { Id = 1, LicensePlate="ABC-123",Name = "Sedan"},
                new Car { Id = 2, LicensePlate="ABC-124",Name = "SUV" },
                new Car { Id = 3, LicensePlate = "ABC-125", Name = "Hatchback" }
            };
        }

    }
}
