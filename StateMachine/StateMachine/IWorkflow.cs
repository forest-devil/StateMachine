using System.Collections;
using System.Collections.Generic;

namespace StateMachine
{
    /// <summary>
    /// 工作流
    /// </summary>
    public interface IWorkflow
    {
        /// <summary>
        /// 能转换到的状态列表。如果某个状态通过任何转换规则都不可达，将不会列出
        /// </summary>
        IEnumerable ValidOperations { get; }

        /// <summary>
        /// 所涉及的操作列表，未使用的操作将不会列出
        /// </summary>
        IEnumerable ValidStatuses { get; }
    }

    /// <summary>
    /// 工作流
    /// </summary>
    /// <typeparam name="TStatusEnum">状态枚举类型</typeparam>
    /// <typeparam name="TOperationEnum">操作枚举类型</typeparam>
    public interface IWorkflow<TStatusEnum, TOperationEnum> : IWorkflow
        where TStatusEnum : struct
        where TOperationEnum : struct
    {
        /// <summary>
        /// 能转换到的状态列表。如果某个状态通过任何转换规则都不可达，将不会列出
        /// </summary>
        new IEnumerable<TOperationEnum> ValidOperations { get; }

        /// <summary>
        /// 所涉及的操作列表，未使用的操作将不会列出
        /// </summary>
        new IEnumerable<TStatusEnum> ValidStatuses { get; }

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
        IWorkflow<TStatusEnum, TOperationEnum> AddRule(TStatusEnum status, params (TOperationEnum operation, TStatusEnum result)[] rhs);

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
        IWorkflow<TStatusEnum, TOperationEnum> AddRule(TStatusEnum status, TOperationEnum operation, TStatusEnum result);

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <param name="status">原状态</param>
        /// <param name="operation">操作</param>
        /// <returns>转换后状态</returns>
        TStatusEnum Transition(TStatusEnum status, TOperationEnum operation);
    }
}