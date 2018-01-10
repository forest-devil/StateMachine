namespace StateMachine
{
    /// <summary>
    /// 状态接口
    /// </summary>
    /// <typeparam name="TStatus">状态Enum类型</typeparam>
    /// <typeparam name="TOperation">操作Enum类型</typeparam>
    /// <typeparam name="T">自引用，用于接口方法的返回类型</typeparam>
    public interface IStatus<TStatus, in TOperation, out T>
    {
        /// <summary>
        /// [只读] Enum类型的内部状态值
        /// </summary>
        TStatus Value { get; }

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <param name="operation">要执行的操作</param>
        /// <returns>自身，用于链式调用写法</returns>
        T Transition(TOperation operation);
    }
}