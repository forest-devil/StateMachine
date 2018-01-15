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

        IWorkflow Workflow { get; }
    }

    /// <summary>
    /// 状态接口（模板化）
    /// </summary>
    /// <typeparam name="TStatusEnum">状态Enum类型</typeparam>
    /// <typeparam name="TOperationEnum">操作Enum类型</typeparam>
    /// <typeparam name="TStatus">自引用，用于接口方法的返回类型</typeparam>
    public interface IStatus<TStatusEnum, TOperationEnum, out TStatus> : IStatus
        where TStatusEnum : struct          // 实际上要求是Enum，但是语法不支持直接写Enum
        where TOperationEnum : struct       // 实际上要求是Enum，但是语法不支持直接写Enum
        where TStatus : Status<TStatusEnum, TOperationEnum, TStatus>
    {
        /// <summary>
        /// [只读] Enum类型的内部状态值
        /// </summary>
        new TStatusEnum Value { get; }

        new IWorkflow<TStatusEnum, TOperationEnum> Workflow { get; }

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <param name="operation">要执行的操作</param>
        /// <returns>自身，用于链式调用写法</returns>
        TStatus Transition(TOperationEnum operation);
    }
}