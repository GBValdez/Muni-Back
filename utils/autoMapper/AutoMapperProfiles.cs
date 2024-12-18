using AutoMapper;

using project.roles;
using project.roles.dto;
using project.users;
using project.users.dto;
using project.users.Models;
using project.utils.catalogue;
using project.utils.catalogues.dto;
using project.utils.Catalogues.dto;

namespace project.utils.autoMapper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<userEntity, userDto>()
            .ForMember(userDtoId => userDtoId.isActive, options => options.MapFrom(src => src.deleteAt == null));
            ;

            CreateMap<rolEntity, rolDto>();
            CreateMap<rolCreationDto, rolEntity>();
            CreateMap<Client, clientDto>();
            CreateMap<clientCreationDto, Client>();
            CreateMap<clientCreationDto, userCreationDto>();
            CreateMap<Catalogue, catalogueDto>();
            CreateMap<catalogueCreationDto, Catalogue>();
            CreateMap<catalogueTypeDtoCreation, catalogueType>();
            CreateMap<catalogueType, catalogueTypeDtoCreation>();

        }

    }
}