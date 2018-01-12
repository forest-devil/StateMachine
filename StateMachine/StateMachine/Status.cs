using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="status">string类型的状态字符串，或TStatus类型的状态Enum</param>
        public Status(object status)
        {
            switch (status)
            {
                case TStatusEnum statusValue when typeof(TStatusEnum).IsEnum:
                    Value = statusValue;
                    break;

                case string statusStr when Enum.TryParse(statusStr, out TStatusEnum statusValue):
                    Value = statusValue;
                    break;

                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// 本类型能转换到的状态列表。如果某个状态通过任何转换规则都不可达，将不会列出
        /// </summary>
        public IEnumerable<TOperationEnum> ValidOperations => Workflow.Instance.ValidOperations;

        IEnumerable IStatus.ValidOperations => ValidOperations;

        /// <summary>
        /// 本类型所涉及的操作列表，未使用的操作将不会列出
        /// </summary>
        public IEnumerable<TStatusEnum> ValidStatuses => Workflow.Instance.ValidStatuses;

        IEnumerable IStatus.ValidStatuses => ValidStatuses;

        /// <summary>
        /// [只读] Enum类型的内部状态值
        /// </summary>
        public TStatusEnum Value { get; private set; }

        object IStatus.Value => Value;

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
        public override string ToString() => Value.ToString();

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <param name="operation">操作</param>
        /// <returns>this</returns>
        public TStatus Transition(TOperationEnum operation)
        {
            if (!Workflow.Instance.Sealed)
                throw new InvalidOperationException();

            try
            {
                Value = Workflow.Instance[Value][operation];
            }
            catch (NullReferenceException) { }
            catch (KeyNotFoundException) { }
            return (TStatus)this;
        }

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <param name="operation">操作</param>
        /// <returns>this</returns>
        public object Transition(object operation) => Transition((TOperationEnum)operation);

        /// <summary>
        /// 密封工作流状态机
        /// 密封之前可以用Set()定义新的状态转换，但Transition()、ValidStatuses、ValidOperations都不可用
        /// 密封之后不能再定义状态转换，上述几个成员变为可用
        /// 密封操作会清理状态机中空的节点
        /// </summary>
        protected static void Seal()
        {
            var emptyKeys = Workflow.Instance.Where(kvp => kvp.Value.Count == 0).Select(kvp => kvp.Key).ToList();
            foreach (var key in emptyKeys)
            {
                Workflow.Instance.Remove(key);
            }
            Workflow.Instance.Sealed = true;
        }

        /// <summary>
        /// 设置转换规则，可以一次设置多个分支。建议在Status具体类的static构造函数中调用，并适当格式化
        /// </summary>
        /// <param name="status">原状态</param>
        /// <param name="rhs">转换规则，每一条为一个tuple，格式为(operation, result)</param>
        /// <example>
        ///     Set(ArticleStatus.已修改,
        ///         (ArticleOperation.提交, ArticleStatus.已提交),
        ///         (ArticleOperation.发布, ArticleStatus.已发布));
        /// </example>
        protected static void Set(TStatusEnum status, params (TOperationEnum operation, TStatusEnum result)[] rhs)
        {
            if (Workflow.Instance.Sealed)
                throw new InvalidOperationException();

            foreach (var (operation, result) in rhs)
                Set(status, operation, result);
        }

        /// <summary>
        /// 设置转换规则。只能在Seal()之前调用，否则抛出异常
        /// </summary>
        /// <param name="status">原状态</param>
        /// <param name="operation">操作</param>
        /// <param name="result">结果状态</param>
        /// <example>
        ///     Set(ArticleStatus.已提交,
        ///         ArticleOperation.发布, ArticleStatus.已发布);
        /// </example>
        protected static void Set(TStatusEnum status, TOperationEnum operation, TStatusEnum result)
        {
            if (Workflow.Instance.Sealed)
                throw new InvalidOperationException();

            var oprations = Workflow.Instance[status];
            if (!oprations.ContainsKey(operation))
                oprations.Add(operation, result);
        }

        /// <summary>
        /// 本类嵌套单例，封装了工作流(状态机)的逻辑，内部使用两层嵌套的Dictionary实现
        /// </summary>
        private sealed class Workflow : Dictionary<TStatusEnum, Dictionary<TOperationEnum, TStatusEnum>>
        {
            private static readonly Lazy<Workflow> _workflow = new Lazy<Workflow>(() => new Workflow());

            private readonly Lazy<IEnumerable<TOperationEnum>> _validOperations = new Lazy<IEnumerable<TOperationEnum>>(
                () => Instance.SelectMany(kvp => kvp.Value).Select(kvp => kvp.Key).Distinct());

            private readonly Lazy<IEnumerable<TStatusEnum>> _validStatuses = new Lazy<IEnumerable<TStatusEnum>>(
                () => Instance.Where(kvp => kvp.Value.Count > 0).Select(kvp => kvp.Key)
                    .Concat(Instance.SelectMany(kvp => kvp.Value.Values)).Distinct());

            private Workflow()
            {
                foreach (TStatusEnum status in Enum.GetValues(typeof(TStatusEnum)))
                {
                    Add(status, new Dictionary<TOperationEnum, TStatusEnum>());
                }
            }

            public static Workflow Instance => _workflow.Value;
            public bool Sealed { get; set; } = false;
            public IEnumerable<TOperationEnum> ValidOperations => Sealed ? _validOperations.Value : null;
            public IEnumerable<TStatusEnum> ValidStatuses => Sealed ? _validStatuses.Value : null;
        }
    }
}