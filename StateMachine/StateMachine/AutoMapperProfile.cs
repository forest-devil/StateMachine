using AutoMapper;
using System;
using System.Linq;

namespace StateMachine
{
    public class StateMachineAutoMapperProfile : Profile
    {
        public StateMachineAutoMapperProfile()
        {
            var statusTypes = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                               from type in assembly.GetTypes()
                               where type.BaseType != null
                                    && type.BaseType.IsConstructedGenericType
                                    && type.BaseType.GetGenericTypeDefinition() == typeof(Status<,,>)
                               select type);

            foreach (var statusType in statusTypes)
            {
                var typeOfEnum = statusType.BaseType.GetGenericArguments()[0];

                CreateMap(typeof(object), statusType)
                    .ConstructUsing(status => Activator.CreateInstance(statusType, status));
                CreateMap(statusType, typeof(string))
                    .ConstructUsing(status => status.ToString());
                CreateMap(statusType, typeOfEnum)
                    .ConstructUsing(status => statusType.GetProperty("Value").GetValue(status));
            }
        }
    }
}