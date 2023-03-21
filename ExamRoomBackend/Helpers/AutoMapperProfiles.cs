using AutoMapper;
using ExamRoomBackend.Models;
using ExamRoomBackend.Models.DTO;

namespace ExamRoomBackend.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<City, CityReadDto>().ReverseMap();
            CreateMap<Country, CountryReadDto>().ReverseMap();
            CreateMap<State, StateReadDto>().ReverseMap();
            CreateMap<Gender, GenderReadDto>().ReverseMap();
            //CreateMap<User, RegisterReqDto>().ReverseMap();
        }
    }
}
