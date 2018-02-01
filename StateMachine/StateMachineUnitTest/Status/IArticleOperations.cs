﻿namespace StateMachineTest
{
    /// <summary>
    /// 示例业务操作接口，无论业务是否引起状态改变，都可以写在这里
    /// 实际上，这里的业务方法只要与实际的业务一一对应即可，跟是否采用了Status类无关
    /// </summary>
    public interface IArticleBusinessOperations
    {
        void AddComment(params object[] arguments);

        void AddRemark(params object[] arguments);

        void Edit(params object[] arguments);
    }
}