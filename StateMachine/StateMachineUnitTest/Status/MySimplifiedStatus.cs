using StateMachine;

namespace StateMachineTest
{
    public class MySimplifiedStatus : Status<ArticleStatus, ArticleOperation, MySimplifiedStatus>
    {
        public MySimplifiedStatus(ArticleStatus status) : base(status)
        {
        }

        public MySimplifiedStatus(string status) : base(status)
        {
        }

        public override IWorkflow<ArticleStatus, ArticleOperation> Workflow
            => WorkflowSingleton<ArticleStatus, ArticleOperation, MySimplifiedStatus>.Instance;
    }
}