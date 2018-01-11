using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachine.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// 生成筛选函数，用于IEnumerable类型，并且状态字段类型为<typeparamref name="TStatus"/>
        /// </summary>
        /// <typeparam name="TObject">包含字段类型为<typeparamref name="TStatus"/>的对象类型</typeparam>
        /// <param name="predicate">属性表达式，例如<example><code>obj => obj.Status</code></example></param>
        /// <param name="statuses">要筛选的状态</param>
        /// <returns></returns>
        public static IEnumerable<TObject> FilterByStatus<TObject, TStatus, TStatusEnum>(this IEnumerable<TObject> source, Func<TObject, TStatus> predicate, params TStatusEnum[] statuses)
            where TStatusEnum : struct
            where TStatus : IStatus
        {
            var casted = statuses.Select(s => s.ToString()).ToArray();
            return source.Where(obj => casted.Contains(predicate(obj).ToString()));
        }
    }
}
