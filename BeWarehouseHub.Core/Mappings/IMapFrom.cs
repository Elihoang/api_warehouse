// BeWarehouseHub.Core/Mappings/IMapFrom.cs
using AutoMapper;

namespace BeWarehouseHub.Core.Mappings;

public interface IMapFrom<T>
{
    void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
}