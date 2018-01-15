using StateMachine;

namespace StateMachineTest
{
    public class MyComplicatedStatus : Status<ArticleStatus, ArticleOperation, MyComplicatedStatus>
    {
        static MyComplicatedStatus()
        {
            Workflow<ArticleStatus, ArticleOperation, MyComplicatedStatus>.Instance
                .AddRule(ArticleStatus.已修改,
                    (ArticleOperation.提交, ArticleStatus.已提交),
                    (ArticleOperation.发布, ArticleStatus.已发布))
                .AddRule(ArticleStatus.已提交,
                    ArticleOperation.发布, ArticleStatus.已发布)
                .AddRule(ArticleStatus.已发布,
                    ArticleOperation.撤回, ArticleStatus.已修改)
                .Seal();
        }

        public MyComplicatedStatus(ArticleStatus status) : base(status)
        {
        }

        public MyComplicatedStatus(string status) : base(status)
        {
        }

        public override IWorkflow<ArticleStatus, ArticleOperation> Workflow
            => Workflow<ArticleStatus, ArticleOperation, MyComplicatedStatus>.Instance;
    }
}