namespace StateMachine
{
    /// <summary>
    /// 状态基类型
    /// </summary>
    /// <typeparam name="TStatusEnum">状态Enum类型</typeparam>
    /// <typeparam name="TOperationEnum">操作Enum类型</typeparam>
    /// <typeparam name="TStatus">自引用，用于接口方法的返回类型</typeparam>
    public abstract class Status : IStatus
    {
        /// <summary>
        /// [只读] Enum类型的内部状态值
        /// </summary>
        public virtual object Value { get; protected set; }

        public virtual IWorkflow Workflow { get; }

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
        public virtual object Transition(object operation)
        {
            Value = Workflow.Transition(Value, operation);
            return this;
        }
    }
}