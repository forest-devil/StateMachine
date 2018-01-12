using System;
using System.Collections;
using System.Collections.Generic;

namespace StateMachine
{
    /// <summary>
    /// 状态接口
    /// </summary>
    public interface IStatus
    {
        /// <summary>
        /// [只读] Enum类型的内部状态值
        /// </summary>
        object Value { get; }

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <param name="operation">要执行的操作</param>
        /// <returns>自身，用于链式调用写法</returns>
        object Transition(object operation);
        /// <summary>
        /// 本类型能转换到的状态列表。如果某个状态通过任何转换规则都不可达，将不会列出
        /// </summary>
        IEnumerable ValidOperations { get; }

        /// <summary>
        /// 本类型所涉及的操作列表，未使用的操作将不会列出
        /// </summary>
        IEnumerable ValidStatuses { get; }
    }

    /// <summary>
    /// 状态接口（模板化）
    /// </summary>
    /// <typeparam name="TStatusEnum">状态Enum类型</typeparam>
    /// <typeparam name="TOperationEnum">操作Enum类型</typeparam>
    /// <typeparam name="TStatus">自引用，用于接口方法的返回类型</typeparam>
    public interface IStatus<TStatusEnum, TOperationEnum, out TStatus> : IStatus
    {
        /// <summary>
        /// [只读] Enum类型的内部状态值
        /// </summary>
        new TStatusEnum Value { get; }

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <param name="operation">要执行的操作</param>
        /// <returns>自身，用于链式调用写法</returns>
        TStatus Transition(TOperationEnum operation);

        /// <summary>
        /// 本类型能转换到的状态列表。如果某个状态通过任何转换规则都不可达，将不会列出
        /// </summary>
        new IEnumerable<TOperationEnum> ValidOperations { get; }

        /// <summary>
        /// 本类型所涉及的操作列表，未使用的操作将不会列出
        /// </summary>
        new IEnumerable<TStatusEnum> ValidStatuses { get; }
    }
}