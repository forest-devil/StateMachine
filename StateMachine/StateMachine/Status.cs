﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace StateMachine
{
    /// <summary>
    /// 状态基类型
    /// </summary>
    /// <typeparam name="TStatusEnum">状态Enum类型</typeparam>
    /// <typeparam name="TOperationEnum">操作Enum类型</typeparam>
    /// <typeparam name="TStatus">自引用，用于接口方法的返回类型</typeparam>
    [JsonConverter(typeof(StringStatusJsonConverter))]
    public abstract class Status<TStatusEnum, TOperationEnum, TStatus> : IStatus<TStatusEnum, TOperationEnum, TStatus>
        where TStatusEnum : struct          // 实际上要求是Enum，但是语法不支持直接写Enum
        where TOperationEnum : struct       // 实际上要求是Enum，但是语法不支持直接写Enum
        where TStatus : Status<TStatusEnum, TOperationEnum, TStatus>
    {
        private static readonly Lazy<IEnumerable<TOperationEnum>> _usedOperations = new Lazy<IEnumerable<TOperationEnum>>(
            () => Workflow.Instance.SelectMany(kvp => kvp.Value).Select(kvp => kvp.Key).Distinct());

        private static readonly Lazy<IEnumerable<TStatusEnum>> _usedStatuses = new Lazy<IEnumerable<TStatusEnum>>(
                () => Workflow.Instance.Where(kvp => kvp.Value.Count > 0).Select(kvp => kvp.Key));

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="status">string类型的状态字符串，或TStatus类型的状态Enum</param>
        public Status(object status)
        {
            switch (status)
            {
                case TStatusEnum statusValue:
                    Value = statusValue;
                    break;

                case string statusStr when Enum.TryParse(statusStr, out TStatusEnum statusValue):
                    Value = statusValue;
                    break;
            }
        }

        /// <summary>
        /// 本类型能转换到的状态列表。如果某个状态通过任何转换规则都不可达，将不会列出
        /// </summary>
        public static IEnumerable<TOperationEnum> UsedOperations { get { return _usedOperations.Value; } }

        /// <summary>
        /// 本类型所涉及的操作列表，未使用的操作将不会列出
        /// </summary>
        public static IEnumerable<TStatusEnum> UsedStatuses { get { return _usedStatuses.Value; } }

        /// <summary>
        /// [只读] Enum类型的内部状态值
        /// </summary>
        public TStatusEnum Value { get; private set; }

        object IStatus.Value => Value;

        /// <summary>
        /// 生成筛选函数，用于IEnumerable类型，并且状态字段类型为<typeparamref name="TStatus"/>
        /// </summary>
        /// <typeparam name="TObject">包含字段类型为<typeparamref name="TStatus"/>的对象类型</typeparam>
        /// <param name="predicate">属性表达式，例如<example><code>obj => obj.Status</code></example></param>
        /// <param name="statuses">要筛选的状态</param>
        /// <returns></returns>
        public static Func<TObject, bool> GetFilter<TObject>(Func<TObject, TStatus> predicate, params TStatusEnum[] statuses)
        {
            var casted = statuses.Select(s => s.ToString()).ToArray();
            return obj => casted.Contains(predicate(obj).ToString());
        }

        /// <summary>
        /// 生成筛选表达式，用于IQueryable类型，并且状态字段类型为<typeparamref name="TStatusEnum"/>
        /// </summary>
        /// <typeparam name="TObject">包含字段类型为<typeparamref name="TStatusEnum"/>的对象类型</typeparam>
        /// <param name="predicate">属性表达式，例如<example><code>obj => obj.Status</code></example></param>
        /// <param name="statuses">要筛选的状态</param>
        /// <returns></returns>
        public static Expression<Func<TObject, bool>> GetFilter<TObject>(Expression<Func<TObject, TStatusEnum>> predicate, params TStatusEnum[] statuses)
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
            return Expression.Lambda<Func<TObject, bool>>(bodyExpr, paramList);
        }

        /// <summary>
        /// 生成筛选表达式，用于IQueryable类型，并且状态字段类型为string
        /// </summary>
        /// <typeparam name="TObject">包含字段类型为string的对象类型</typeparam>
        /// <param name="predicate">属性表达式，例如<example><code>obj => obj.Status</code></example></param>
        /// <param name="statuses">要筛选的状态</param>
        /// <returns></returns>
        public static Expression<Func<TObject, bool>> GetFilter<TObject>(Expression<Func<TObject, string>> predicate, params TStatusEnum[] statuses)
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
            return Expression.Lambda<Func<TObject, bool>>(bodyExpr, paramList);
        }

        /// <summary>
        /// 加号操作，一般只用于"+="操作。原状态+操作=结果状态
        /// </summary>
        /// <param name="start">原状态</param>
        /// <param name="operation">操作</param>
        /// <returns>结果状态</returns>
        public static TStatus operator +(Status<TStatusEnum, TOperationEnum, TStatus> start, TOperationEnum operation)
        {
            var tmp = (TStatus)start.MemberwiseClone();
            return tmp.Transition(operation);
        }

        /// <summary>
        /// 转化为字符串，输出状态字符串，已重载
        /// </summary>
        /// <returns>状态字符串</returns>
        public override string ToString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <param name="operation">操作</param>
        /// <returns>this</returns>
        public TStatus Transition(TOperationEnum operation)
        {
            try
            {
                Value = Workflow.Instance[Value][operation];
            }
            catch (NullReferenceException) { }
            catch (KeyNotFoundException) { }
            return (TStatus)this;
        }

        public object Transition(object operation)
        {
            return Transition((TOperationEnum)operation);
        }

        /// <summary>
        /// 设置转换规则，可以一次设置多个分支。建议在Status具体类的static构造函数中调用，并适当格式化
        /// </summary>
        /// <param name="status">原状态</param>
        /// <param name="rhs">转换规则，每一条为一个tuple，格式为(operation, result)</param>
        /// <example>
        /// Set(ArticleStatus.已修改,
        ///        (ArticleOperation.提交, ArticleStatus.已提交),
        ///        (ArticleOperation.发布, ArticleStatus.已发布));
        /// </example>
        protected static void Set(TStatusEnum status, params (TOperationEnum operation, TStatusEnum result)[] rhs)
        {
            foreach (var (operation, result) in rhs)
                Set(status, operation, result);
        }

        /// <summary>
        /// 设置转换规则
        /// </summary>
        /// <param name="status">原状态</param>
        /// <param name="operation">操作</param>
        /// <param name="result">结果状态</param>
        /// <example>
        ///    Set(ArticleStatus.已提交,
        ///        ArticleOperation.发布, ArticleStatus.已发布);
        /// </example>
        protected static void Set(TStatusEnum status, TOperationEnum operation, TStatusEnum result)
        {
            var oprations = Workflow.Instance[status];
            if (!oprations.ContainsKey(operation))
                oprations.Add(operation, result);
        }

        private sealed class Workflow : Dictionary<TStatusEnum, Dictionary<TOperationEnum, TStatusEnum>>
        {
            private static readonly Lazy<Workflow> lazy = new Lazy<Workflow>(() => new Workflow());

            private Workflow()
            {
                foreach (TStatusEnum status in Enum.GetValues(typeof(TStatusEnum)))
                {
                    Add(status, new Dictionary<TOperationEnum, TStatusEnum>());
                }
            }

            public static Workflow Instance { get { return lazy.Value; } }
        }
    }
}