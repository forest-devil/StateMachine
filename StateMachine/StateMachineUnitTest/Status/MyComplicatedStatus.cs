using StateMachine;
using System;

namespace StateMachineTest
{
    public abstract class MyComplicatedStatus : Status<ArticleStatus, ArticleOperation, MyComplicatedStatus>, IArticleBusinessOperations
    {
        public MyComplicatedStatus() : base()
        {
        }

        public MyComplicatedStatus(ArticleStatus status) : base(status)
        {
        }

        public MyComplicatedStatus(string status) : base(status)
        {
        }

        public override IWorkflow<ArticleStatus, ArticleOperation> Workflow
                    => Singleton<MyComplicatedWorkflow>.Instance;

        public static MyComplicatedStatus CreateInstance(ArticleStatus status)
        {
            switch (status)
            {
                case ArticleStatus.已修改:
                    return new 已修改();

                case ArticleStatus.已提交:
                    return new 已提交();

                case ArticleStatus.已发布:
                    return new 已发布();

                case ArticleStatus.已存档:
                    return new 已存档();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual void AddComment(params object[] arguments) => throw new InvalidOperationException();

        public virtual void AddRemark(params object[] arguments) => throw new InvalidOperationException();

        public virtual void Edit(params object[] arguments) => throw new InvalidOperationException();

        public override MyComplicatedStatus Transition(ArticleOperation operation)
        {
            var result = Workflow.Transition(Value, operation);
            return CreateInstance(result);
        }
    }

    public class 已存档 : MyComplicatedStatus
    {
        public 已存档() : base(ArticleStatus.已存档)
        {
        }

        public override ArticleStatus Value => ArticleStatus.已存档;
    }

    public class 已发布 : MyComplicatedStatus
    {
        public 已发布() : base(ArticleStatus.已发布)
        {
        }

        public override ArticleStatus Value => ArticleStatus.已发布;

        public override void AddComment(params object[] arguments)
        {
            // blah blah
        }
    }

    public class 已提交 : MyComplicatedStatus
    {
        public 已提交() : base(ArticleStatus.已提交)
        {
        }

        public override ArticleStatus Value => ArticleStatus.已提交;

        public override void AddRemark(params object[] arguments)
        {
            // blah blah
        }
    }

    public class 已修改 : MyComplicatedStatus
    {
        public 已修改() : base(ArticleStatus.已修改)
        {
        }

        public override ArticleStatus Value => ArticleStatus.已修改;

        public override void Edit(params object[] arguments)
        {
            // blah blah
        }
    }
}