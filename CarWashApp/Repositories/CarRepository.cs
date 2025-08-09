using CarWashApp.Models;

namespace CarWashApp.Repositories
{
    public interface ICarRepository
    {
        void AddCar(Car car);
        Car GetCarById(int id);
        IEnumerable<Car> GetAllCars();
        void UpdateCar(Car car);
        void DeleteCar(int id);
    }
    public class CarRepository(CarWashAppDbContext context) : ICarRepository
    {
        public void AddCar(Car car)
        {
            context.Add(car);
            context.SaveChanges();
        }

        public void DeleteCar(int id)
        {
            var car = context.Cars.FirstOrDefault(c => c.Id == id);
            if (car != null)
            {
                context.Cars.Remove(car);
                context.SaveChanges();
            }
        }

        public IEnumerable<Car> GetAllCars()
        {
            return context.Cars.ToList();
        }

        public Car GetCarById(int id)
        {
            return context.Cars.FirstOrDefault(c => c.Id == id);
        }

        public void UpdateCar(Car car)
        {
            context.Update(car);
            context.SaveChanges();
        }
    }
}
