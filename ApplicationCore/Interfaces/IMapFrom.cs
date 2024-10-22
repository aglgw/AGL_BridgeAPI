using AutoMapper;

namespace AGL.Api.ApplicationCore.Interfaces
{
    public interface IMapFrom<T>
    {
        void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
    }
}
