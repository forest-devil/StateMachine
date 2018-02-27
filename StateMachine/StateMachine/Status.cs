using System;
using System.Linq;

namespace StateMachine
{
    /// <summary>
    /// 状态基类型
    /// </summary>
    /// <typeparam name="TStatusEnum">状态Enum类型</typeparam>
    /// <typeparam name="TOperationEnum">操作Enum类型</typeparam>
    /// <typeparam name="TStatus">自引用，用于接口方法的返回类型</typeparam>
    public abstract class Status<TStatusEnum, TOperationEnum, TStatus> : IStatus<TStatusEnum, TOperationEnum, TStatus>
        where TStatusEnum : struct          // 实际上要求是Enum，但是语法不支持直接写Enum
        where TOperationEnum : struct       // 实际上要求是Enum，但是语法不支持直接写Enum
        where TStatus : Status<TStatusEnum, TOperationEnum, TStatus>
    {
        private TStatusEnum _value;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Status()
        {
            Value = Workflow.ValidStatuses.FirstOrDefault();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="status">状态Enum</param>
        public Status(TStatusEnum status) => Value = status;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="status">状态字符串</param>
        public Status(string status)
        {
            Enum.TryParse(status, out TStatusEnum statusValue);
            Value = statusValue;
        }

        /// <summary>
        /// 状态值
        /// </summary>
        public virtual TStatusEnum Value
        {
            get => _value;
            protected set
            {
                if (!Workflow.ValidStatuses.Contains(value))
                    value = Workflow.ValidStatuses.FirstOrDefault();
                _value = value;
            }
        }

        object IStatus.Value => Value;

        /// <summary>
        /// 工作流
        /// </summary>
        public virtual IWorkflow<TStatusEnum, TOperationEnum> Workflow { get; }

        IWorkflow IStatus.Workflow => Workflow;

        /// <summary>
        /// 加号操作，一般只用于"+="操作。原状态+操作=结果状态
        /// </summary>
        /// <param name="start">原状态</param>
        /// <param name="operation">操作</param>
        /// <returns>结果状态</returns>
        public static TStatus operator +(Status<TStatusEnum, TOperationEnum, TStatus> start, TOperationEnum operation)
            => ((TStatus)start.MemberwiseClone()).Transition(operation);

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
        public virtual TStatus Transition(TOperationEnum operation)
        {
            Value = Workflow.Transition(Value, operation);
            return (TStatus)this;
        }
    }
}