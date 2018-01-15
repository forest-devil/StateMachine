using AutoMapper;
using System;
using System.Linq;

namespace StateMachine
{
    /// <summary>
    /// 用于配置AutoMapper的Profile类。
    /// 使用方法见 http://automapper.readthedocs.io/en/latest/Configuration.html#profile-instances
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// Mapper.Initialize(cfg =>
    /// {
    ///    cfg.AddProfile<StateMachineAutoMapperProfile>();
    /// });
    /// ]]>
    /// </code>
    /// </example>
    public class StateMachineAutoMapperProfile : Profile
    {
        /// <summary>
        /// 构造函数
        /// </summary>
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
                    .ConstructUsing(status => statusType.GetProperty("Value", typeOfEnum).GetValue(status));
            }
        }
    }
}