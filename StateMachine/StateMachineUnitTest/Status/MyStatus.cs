using StateMachine;

namespace StateMachineTest
{
    public class MyStatus : Status<ArticleStatus, ArticleOperation, MyStatus>
    {
        public MyStatus() : base()
        {
        }

        public MyStatus(ArticleStatus status) : base(status)
        {
        }

        public MyStatus(string status) : base(status)
        {
        }

        public override IWorkflow<ArticleStatus, ArticleOperation> Workflow
            => WorkflowSingleton<ArticleStatus, ArticleOperation, MyStatus>.Instance;
    }
}