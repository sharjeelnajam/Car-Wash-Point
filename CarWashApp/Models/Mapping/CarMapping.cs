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
                Name = car.Name,
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
                Name = dto.Name,
                Color = dto.Color,
                LoyaltyPoints = dto.LoyaltyPoints
            };
        }
    }
}
