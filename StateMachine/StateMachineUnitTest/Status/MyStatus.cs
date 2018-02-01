using StateMachine;

namespace StateMachineTest
{
    public class MyStatus : Status<ArticleStatus, ArticleOperation, MyStatus>
    {
        private IWorkflow<ArticleStatus, ArticleOperation> _workflow = Singleton<MyWorkflow>.Instance;

        public MyStatus(ArticleStatus status) : base(status)
        {
        }

        public MyStatus(string status) : base(status)
        {
        }

        public MyStatus(IWorkflow<ArticleStatus, ArticleOperation> workflow, ArticleStatus status)
        {
            _workflow = workflow;
            Value = status;
        }

        public override IWorkflow<ArticleStatus, ArticleOperation> Workflow
            => _workflow;
    }
}