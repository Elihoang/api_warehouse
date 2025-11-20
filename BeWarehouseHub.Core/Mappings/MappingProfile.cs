// BeWarehouseHub.Core/Mappings/MappingProfile.cs
using AutoMapper;
using System.Reflection;

namespace BeWarehouseHub.Core.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Cách CHUẨN NHẤT – đơn giản, không lỗi, dùng mãi mãi
        ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        var mapFromType = typeof(IMapFrom<>);

        var mappingMethodName = nameof(IMapFrom<object>.Mapping);

        var types = assembly.GetExportedTypes()
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == mapFromType))
            .ToList();

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);
            var methodInfo = type.GetMethod(mappingMethodName);

            if (methodInfo != null)
            {
                methodInfo.Invoke(instance, new object[] { this });
            }
            else
            {
                // Fallback: nếu không tìm thấy method Mapping thì dùng interface
                var interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == mapFromType);

                foreach (var interfaceType in interfaces)
                {
                    var interfaceMethod = interfaceType.GetMethod(mappingMethodName);
                    interfaceMethod?.Invoke(instance, new object[] { this });
                }
            }
        }
    }
}