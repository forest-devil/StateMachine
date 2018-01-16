using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StateMachine
{
    /// <summary>
    /// 工作流单例，额外增加一个类型参数T，故每个T都可以绑定一个单例
    /// </summary>
    public sealed class WorkflowSingleton<TStatusEnum, TOperationEnum, T> : Workflow<TStatusEnum, TOperationEnum>
        where TStatusEnum : struct          // 实际上要求是Enum，但是语法不支持直接写Enum
        where TOperationEnum : struct       // 实际上要求是Enum，但是语法不支持直接写Enum
    {
        private static readonly Lazy<WorkflowSingleton<TStatusEnum, TOperationEnum, T>> _workflow
            = new Lazy<WorkflowSingleton<TStatusEnum, TOperationEnum, T>>(
                () => new WorkflowSingleton<TStatusEnum, TOperationEnum, T>());

        private WorkflowSingleton() : base()
        {
        }

        public static WorkflowSingleton<TStatusEnum, TOperationEnum, T> Instance => _workflow.Value;
    }
}