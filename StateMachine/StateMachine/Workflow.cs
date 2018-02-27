using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StateMachine
{
    /// <summary>
    /// 工作流(状态机)逻辑，内部使用两层嵌套的Dictionary实现
    /// 使用Seal()密封之前可以用AddRule()定义新的状态转换，但Transition()、ValidStatuses、ValidOperations都不可用
    /// 密封之后不能再定义状态转换，上述几个成员变为可用
    /// 密封操作会清理状态机中空的节点
    /// </summary>
    public class Workflow<TStatusEnum, TOperationEnum> : IWorkflow<TStatusEnum, TOperationEnum>
        where TStatusEnum : struct          // 实际上要求是Enum，但是语法不支持直接写Enum
        where TOperationEnum : struct       // 实际上要求是Enum，但是语法不支持直接写Enum
    {
        /// <summary>
        /// 状态转换字典
        /// </summary>
        protected readonly Dictionary<TStatusEnum, Dictionary<TOperationEnum, TStatusEnum>> _dictionary
             = new Dictionary<TStatusEnum, Dictionary<TOperationEnum, TStatusEnum>>();

        /// <summary>
        /// 有效操作
        /// </summary>
        protected readonly List<TOperationEnum> _validOperations = new List<TOperationEnum>();

        /// <summary>
        /// 有效状态
        /// </summary>
        protected readonly List<TStatusEnum> _validStatuses = new List<TStatusEnum>();

        /// <summary>
        /// 工作流
        /// </summary>
        public Workflow()
        {
            foreach (TStatusEnum status in Enum.GetValues(typeof(TStatusEnum)))
            {
                _dictionary.Add(status, new Dictionary<TOperationEnum, TStatusEnum>());
            }
        }

        /// <summary>
        /// 有效状态
        /// </summary>
        public virtual IEnumerable<TOperationEnum> ValidOperations => _validOperations;

        IEnumerable IWorkflow.ValidOperations => ValidOperations;

        /// <summary>
        /// 有效状态
        /// </summary>
        public virtual IEnumerable<TStatusEnum> ValidStatuses => _validStatuses;

        IEnumerable IWorkflow.ValidStatuses => ValidStatuses;

        /// <summary>
        /// 设置转换规则，可以一次设置多个分支。
        /// </summary>
        /// <param name="status">原状态</param>
        /// <param name="rhs">转换规则，每一条为一个tuple，格式为(operation, result)</param>
        /// <example>
        ///     AddRule(ArticleStatus.已修改,
        ///         (ArticleOperation.提交, ArticleStatus.已提交),
        ///         (ArticleOperation.发布, ArticleStatus.已发布));
        /// </example>
        public virtual IWorkflow<TStatusEnum, TOperationEnum> AddRule(TStatusEnum status, params (TOperationEnum operation, TStatusEnum result)[] rhs)
        {
            foreach (var (operation, result) in rhs)
                AddRule(status, operation, result);

            return this;
        }

        /// <summary>
        /// 设置转换规则
        /// </summary>
        /// <param name="status">原状态</param>
        /// <param name="operation">操作</param>
        /// <param name="result">结果状态</param>
        /// <example>
        ///     AddRule(ArticleStatus.已提交,
        ///         ArticleOperation.发布, ArticleStatus.已发布);
        /// </example>
        public virtual IWorkflow<TStatusEnum, TOperationEnum> AddRule(TStatusEnum status, TOperationEnum operation, TStatusEnum result)
        {
            var oprations = _dictionary[status];
            if (!oprations.ContainsKey(operation))
            {
                oprations.Add(operation, result);

                if (!_validOperations.Contains(operation))
                    _validOperations.Add(operation);
                if (!_validStatuses.Contains(status))
                    _validStatuses.Add(status);
                if (!_validStatuses.Contains(result))
                    _validStatuses.Add(result);
            }

            return this;
        }

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <param name="status">原状态</param>
        /// <param name="operation">操作</param>
        /// <returns>转换后状态</returns>
        public virtual TStatusEnum Transition(TStatusEnum status, TOperationEnum operation)
        {
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