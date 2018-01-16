using StateMachine;

namespace StateMachineTest
{
    public class MyComplicatedStatus : Status<ArticleStatus, ArticleOperation, MyComplicatedStatus>
    {
        public MyComplicatedStatus(ArticleStatus status) : base(status)
        {
        }

        public MyComplicatedStatus(string status) : base(status)
        {
        }

        public override IWorkflow<ArticleStatus, ArticleOperation> Workflow
            => Singleton<MyComplicatedWorkflow>.Instance;
    }
}