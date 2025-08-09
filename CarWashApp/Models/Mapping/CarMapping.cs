using CarWashApp.Models.DTOs;

namespace CarWashApp.Models.Mapping
{
    public static class CarMapping
    {
        public static CarDto ToDto(this Car car)
        {
            if (car == null) return null;
            return new CarDto
            {
                Id = car.Id,
                LicensePlate = car.LicensePlate,
                Company = car.Company,
                Name = car.Name,
                Year = car.Year,
                Color = car.Color,
                LoyaltyPoints = car.LoyaltyPoints
            };
        }
        public static Car ToEntity(this CarDto dto)
        {
            if (dto == null) return null;
            return new Car
            {
                Id = dto.Id,
                LicensePlate = dto.LicensePlate,
                Company = dto.Company,
                Name = dto.Name,
                Year = dto.Year,
                Color = dto.Color,
                LoyaltyPoints = dto.LoyaltyPoints
            };
        }
    }
}
