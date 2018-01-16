using StateMachine;

namespace StateMachineTest
{
    public class MyStatus : Status<ArticleStatus, ArticleOperation, MyStatus>
    {
        static MyStatus()
        {
            WorkflowSingleton<ArticleStatus, ArticleOperation, MyStatus>.Instance
                .AddRule(ArticleStatus.已修改,
                    ArticleOperation.提交, ArticleStatus.已提交)
                .AddRule(ArticleStatus.已提交,
                    ArticleOperation.发布, ArticleStatus.已发布)
                .AddRule(ArticleStatus.已发布,
                    (ArticleOperation.撤回, ArticleStatus.已修改),
                    (ArticleOperation.存档, ArticleStatus.已存档))
                .Seal();
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