using CarWashApp.Models;
using CarWashApp.Models.DTOs;
using CarWashApp.Models.Mapping;
using CarWashApp.Repositories;

namespace CarWashApp.Services
{
    public interface ICarService
    {
        CarDto GetCarById(int id);
        CarDto GetCarByLicensePlate(string licensePlate);
        List<CarDto> GetAllCars();
        void AddCar(CarDto car);
        void UpdateCar(CarDto car);
    }
    public class CarService(ICarRepository carRepository) : ICarService
    {
        public void AddCar(CarDto car)
        {
            carRepository.AddCar(car.ToEntity());
        }

        public List<CarDto> GetAllCars()
        {
            var cars = carRepository.GetAllCars();
            return cars.Select(c => c.ToDto()).ToList();
        }

        public CarDto GetCarById(int id)
        {
            var carDto = carRepository.GetCarById(id);
            return carDto?.ToDto();
        }

        public CarDto GetCarByLicensePlate(string licensePlate)
        {
            var car = carRepository.GetCarByLicensePlate(licensePlate);
            return car?.ToDto();
        }

        public void UpdateCar(CarDto car)
        {
            var existingCar = carRepository.GetCarById(car.Id);
            if (existingCar != null)
            {
                existingCar.LicensePlate = car.LicensePlate;
                existingCar.Name = car.Name;
                existingCar.Color = car.Color;
                existingCar.LoyaltyPoints = car.LoyaltyPoints;
                carRepository.UpdateCar(existingCar);
            }
        }
    }
}
