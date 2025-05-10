using AutoMapper;
using Cdv.Domain.Entities;
using Cdv.Functions.Dto;

namespace Cdv.Functions.Profiles;

public class PersonDtoProfile :Profile
{
    public PersonDtoProfile()
    {
        CreateMap<PersonDto, PersonEntity>();
    }
}