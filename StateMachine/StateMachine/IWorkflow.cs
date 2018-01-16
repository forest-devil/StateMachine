using System.Collections;
using System.Collections.Generic;

namespace StateMachine
{
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

        IWorkflow<TStatusEnum, TOperationEnum> AddRule(TStatusEnum status, params (TOperationEnum operation, TStatusEnum result)[] rhs);

        IWorkflow<TStatusEnum, TOperationEnum> AddRule(TStatusEnum status, TOperationEnum operation, TStatusEnum result);

        TStatusEnum Transition(TStatusEnum status, TOperationEnum operation);
    }
}