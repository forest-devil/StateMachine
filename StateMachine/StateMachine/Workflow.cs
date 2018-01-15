using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StateMachine
{
    /// <summary>
    /// 工作流单例，由于类型参数包含TStatus，故每个TStatus都有一个单例
    /// 封装了工作流(状态机)的逻辑，内部使用两层嵌套的Dictionary实现
    /// </summary>
    public sealed class Workflow<TStatusEnum, TOperationEnum, TStatus> : IWorkflow<TStatusEnum, TOperationEnum>
        where TStatusEnum : struct          // 实际上要求是Enum，但是语法不支持直接写Enum
        where TOperationEnum : struct       // 实际上要求是Enum，但是语法不支持直接写Enum
        where TStatus : IStatus
    {
        private static readonly Lazy<Workflow<TStatusEnum, TOperationEnum, TStatus>> _workflow
            = new Lazy<Workflow<TStatusEnum, TOperationEnum, TStatus>>(
                () => new Workflow<TStatusEnum, TOperationEnum, TStatus>());

        private readonly Dictionary<TStatusEnum, Dictionary<TOperationEnum, TStatusEnum>> _dictionary
             = new Dictionary<TStatusEnum, Dictionary<TOperationEnum, TStatusEnum>>();

        private readonly Lazy<IEnumerable<TOperationEnum>> _validOperations = new Lazy<IEnumerable<TOperationEnum>>(
            () => Instance._dictionary.SelectMany(kvp => kvp.Value).Select(kvp => kvp.Key).Distinct());

        private readonly Lazy<IEnumerable<TStatusEnum>> _validStatuses = new Lazy<IEnumerable<TStatusEnum>>(
            () => Instance._dictionary.Where(kvp => kvp.Value.Count > 0).Select(kvp => kvp.Key)
                .Concat(Instance._dictionary.SelectMany(kvp => kvp.Value.Values)).Distinct());

        private bool _sealed = false;

        private Workflow()
        {
            foreach (TStatusEnum status in Enum.GetValues(typeof(TStatusEnum)))
            {
                _dictionary.Add(status, new Dictionary<TOperationEnum, TStatusEnum>());
            }
        }

        public static Workflow<TStatusEnum, TOperationEnum, TStatus> Instance => _workflow.Value;

        public bool Sealed
        {
            get => _sealed;
            private set
            {
                if (!_sealed && value == true)
                {
                    var emptyKeys = _dictionary.Where(kvp => kvp.Value.Count == 0).Select(kvp => kvp.Key).ToList();
                    foreach (var key in emptyKeys)
                    {
                        _dictionary.Remove(key);
                    }
                    _sealed = value;
                }
            }
        }

        public IEnumerable<TOperationEnum> ValidOperations => _sealed ? _validOperations.Value : new List<TOperationEnum>();
        IEnumerable IWorkflow.ValidOperations => ValidOperations;
        public IEnumerable<TStatusEnum> ValidStatuses => _sealed ? _validStatuses.Value : new List<TStatusEnum>();
        IEnumerable IWorkflow.ValidStatuses => ValidStatuses;

        /// <summary>
        /// 设置转换规则，可以一次设置多个分支。建议在Status具体类的static构造函数中调用，并适当格式化
        /// </summary>
        /// <param name="status">原状态</param>
        /// <param name="rhs">转换规则，每一条为一个tuple，格式为(operation, result)</param>
        /// <example>
        ///     AddRule(ArticleStatus.已修改,
        ///         (ArticleOperation.提交, ArticleStatus.已提交),
        ///         (ArticleOperation.发布, ArticleStatus.已发布));
        /// </example>
        public IWorkflow<TStatusEnum, TOperationEnum> AddRule(TStatusEnum status, params (TOperationEnum operation, TStatusEnum result)[] rhs)
        {
            if (Sealed)
                throw new InvalidOperationException();

            foreach (var (operation, result) in rhs)
                AddRule(status, operation, result);

            return this;
        }

        /// <summary>
        /// 设置转换规则。只能在Seal()之前调用，否则抛出异常
        /// </summary>
        /// <param name="status">原状态</param>
        /// <param name="operation">操作</param>
        /// <param name="result">结果状态</param>
        /// <example>
        ///     AddRule(ArticleStatus.已提交,
        ///         ArticleOperation.发布, ArticleStatus.已发布);
        /// </example>
        public IWorkflow<TStatusEnum, TOperationEnum> AddRule(TStatusEnum status, TOperationEnum operation, TStatusEnum result)
        {
            if (Sealed)
                throw new InvalidOperationException();

            var oprations = _dictionary[status];
            if (!oprations.ContainsKey(operation))
                oprations.Add(operation, result);

            return this;
        }

        /// <summary>
        /// 密封工作流状态机
        /// 密封之前可以用AddRule()定义新的状态转换，但Transition()、ValidStatuses、ValidOperations都不可用
        /// 密封之后不能再定义状态转换，上述几个成员变为可用
        /// 密封操作会清理状态机中空的节点
        /// </summary>
        public IWorkflow<TStatusEnum, TOperationEnum> Seal()
        {
            Sealed = true;
            return this;
        }

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <param name="operation">操作</param>
        /// <returns>this</returns>
        public TStatusEnum Transition(TStatusEnum status, TOperationEnum operation)
        {
            if (!Sealed)
                throw new InvalidOperationException();

            try
            {
                return _dictionary[status][operation];
            }
            catch (NullReferenceException) { }
            catch (KeyNotFoundException) { }
            return status;
        }
    }
}