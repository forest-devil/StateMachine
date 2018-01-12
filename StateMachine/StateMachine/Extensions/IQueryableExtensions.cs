using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace StateMachine.Extensions
{
    /// <summary>
    /// 针对IQueryable的扩展
    /// </summary>
    public static class IQueryableExtensions
    {
        /// <summary>
        /// 生成筛选表达式，用于IQueryable类型，并且状态字段类型为<typeparamref name="TStatusEnum"/>
        /// </summary>
        /// <typeparam name="TObject">包含字段类型为<typeparamref name="TStatusEnum"/>的对象类型</typeparam>
        /// <typeparam name="TStatusEnum">状态枚举的类型</typeparam>
        /// <param name="source">筛选目标</param>
        /// <param name="predicate">属性表达式，例如<example><code>obj => obj.Status</code></example></param>
        /// <param name="statuses">要筛选的状态</param>
        /// <returns>筛选结果</returns>
        public static IQueryable<TObject> FilterByStatus<TObject, TStatusEnum>(this IQueryable<TObject> source, Expression<Func<TObject, TStatusEnum>> predicate, params TStatusEnum[] statuses)
            where TStatusEnum : struct
        {
            var memberExpr = (MemberExpression)predicate.Body;
            var paramExpr = (ParameterExpression)memberExpr.Expression;
            var paramList = new List<ParameterExpression>() { paramExpr };

            Expression bodyExpr = null;
            foreach (var status in statuses)
            {
                var condition = Expression.Equal(memberExpr, Expression.Constant(status));
                if (bodyExpr == null)
                {
                    bodyExpr = condition;
                }
                else
                {
                    bodyExpr = Expression.Or(bodyExpr, condition);
                }
            }
            return source.Where(Expression.Lambda<Func<TObject, bool>>(bodyExpr, paramList));
        }

        /// <summary>
        /// 生成筛选表达式，用于IQueryable类型，并且状态字段类型为string
        /// </summary>
        /// <typeparam name="TObject">包含字段类型为string的对象类型</typeparam>
        /// <typeparam name="TStatusEnum">状态枚举的类型</typeparam>
        /// <param name="source">筛选目标</param>
        /// <param name="predicate">属性表达式，例如<example><code>obj => obj.Status</code></example></param>
        /// <param name="statuses">要筛选的状态</param>
        /// <returns>筛选结果</returns>
        public static IQueryable<TObject> FilterByStatus<TObject, TStatusEnum>(this IQueryable<TObject> source, Expression<Func<TObject, string>> predicate, params TStatusEnum[] statuses)
            where TStatusEnum : struct
        {
            var memberExpr = (MemberExpression)predicate.Body;
            var paramExpr = (ParameterExpression)memberExpr.Expression;
            var paramList = new List<ParameterExpression>() { paramExpr };

            Expression bodyExpr = null;
            foreach (var status in statuses)
            {
                var condition = Expression.Equal(memberExpr, Expression.Constant(status.ToString()));
                if (bodyExpr == null)
                {
                    bodyExpr = condition;
                }
                else
                {
                    bodyExpr = Expression.Or(bodyExpr, condition);
                }
            }
            return source.Where(Expression.Lambda<Func<TObject, bool>>(bodyExpr, paramList));
        }
    }
}