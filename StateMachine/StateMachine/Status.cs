using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace StateMachine
{
    [JsonConverter(typeof(StringStatusJsonConverter))]
    public abstract class Status<TStatus, TOperation, T> : IStatus<TStatus, TOperation, T>
        where TStatus : struct, IComparable, IConvertible, IFormattable         // 实际上要求是Enum，但是语法不支持直接写Enum
        where TOperation : struct, IComparable, IConvertible, IFormattable       // 实际上要求是Enum，但是语法不支持直接写Enum
        where T : Status<TStatus, TOperation, T>
    {
        private static readonly Lazy<IEnumerable<TOperation>> _usedOperations = new Lazy<IEnumerable<TOperation>>(
            () => Workflow.Instance.SelectMany(kvp => kvp.Value).Select(kvp => kvp.Key).Distinct());

        private static readonly Lazy<IEnumerable<TStatus>> _usedStatuses = new Lazy<IEnumerable<TStatus>>(
                () => Workflow.Instance.Where(kvp => kvp.Value.Count > 0).Select(kvp => kvp.Key));

        public Status(object status)
        {
            switch (status)
            {
                case TStatus statusValue:
                    Value = statusValue;
                    break;

                case string statusStr when Enum.TryParse(statusStr, out TStatus statusValue):
                    Value = statusValue;
                    break;
            }
        }

        public static IEnumerable<TOperation> UsedOperations { get { return _usedOperations.Value; } }

        public static IEnumerable<TStatus> UsedStatuses { get { return _usedStatuses.Value; } }

        public TStatus Value { get; private set; }

        public static Func<TObject, bool> GetFilter<TObject>(Func<TObject, T> memberFunc, params TStatus[] statuses)
        {
            var casted = statuses.Select(s => s.ToString()).ToArray();
            return obj => casted.Contains(memberFunc(obj).ToString());
        }

        public static Expression<Func<TObject, bool>> GetFilter<TObject>(Expression<Func<TObject, TStatus>> predicate, params TStatus[] statuses)
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

        public static Expression<Func<TObject, bool>> GetFilter<TObject>(Expression<Func<TObject, string>> predicate, params TStatus[] statuses)
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

        public static T operator +(Status<TStatus, TOperation, T> start, TOperation operation)
        {
            var tmp = (T)start.MemberwiseClone();
            return tmp.Transition(operation);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <param name="operation">操作</param>
        /// <returns>this</returns>
        public T Transition(TOperation operation)
        {
            try
            {
                Value = Workflow.Instance[Value][operation];
            }
            catch (NullReferenceException) { }
            catch (KeyNotFoundException) { }
            return (T)this;
        }

        protected static void Set(TStatus status, params (TOperation operation, TStatus result)[] rhs)
        {
            foreach (var (operation, result) in rhs)
                Set(status, operation, result);
        }

        protected static void Set(TStatus status, TOperation operation, TStatus result)
        {
            var oprations = Workflow.Instance[status];
            if (!oprations.ContainsKey(operation))
                oprations.Add(operation, result);
        }

        private sealed class Workflow : Dictionary<TStatus, Dictionary<TOperation, TStatus>>
        {
            private static readonly Lazy<Workflow> lazy = new Lazy<Workflow>(() => new Workflow());

            private Workflow()
            {
                foreach (TStatus status in Enum.GetValues(typeof(TStatus)))
                {
                    Add(status, new Dictionary<TOperation, TStatus>());
                }
            }

            public static Workflow Instance { get { return lazy.Value; } }
        }
    }
}