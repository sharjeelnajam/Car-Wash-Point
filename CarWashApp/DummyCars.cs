using CarWashApp.Models;

namespace CarWashApp
{
    public static class DummyCars
    {
        private static List<Car> _cars = new List<Car>
            {
                new Car { Id = 1, LicensePlate="ABC-123", Name = "Sedan", Color = "Blue", LoyaltyPoints = 15},
                new Car { Id = 2, LicensePlate="ABC-124", Name = "SUV", Color = "Red", LoyaltyPoints = 8 },
                new Car { Id = 3, LicensePlate = "ABC-125", Name = "Hatchback", Color = "Green", LoyaltyPoints = 3 }
            };

        public static List<Car> GetCars()
        {
            return _cars;
        }

        public static void AddCar(Car car)
        {
            if (car != null && !_cars.Any(c => c.LicensePlate == car.LicensePlate))
            {
                car.Id = _cars.Count + 1;
                _cars.Add(car);
            }
        }

    }
}
