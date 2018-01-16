using StateMachine;

namespace StateMachineTest
{
    public class MySimplifiedStatus : Status<ArticleStatus, ArticleOperation, MySimplifiedStatus>
    {
        static MySimplifiedStatus()
        {
            WorkflowSingleton<ArticleStatus, ArticleOperation, MySimplifiedStatus>.Instance
                .AddRule(ArticleStatus.已修改,
                    ArticleOperation.发布, ArticleStatus.已发布)
                .AddRule(ArticleStatus.已发布,
                    ArticleOperation.撤回, ArticleStatus.已修改)
                .Seal();
        }

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