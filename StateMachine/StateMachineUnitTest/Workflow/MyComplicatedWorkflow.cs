using StateMachine;

namespace StateMachineTest
{
    public class MyComplicatedWorkflow : Workflow<ArticleStatus, ArticleOperation>
    {
        public MyComplicatedWorkflow()
        {
            AddRule(ArticleStatus.已修改,
                (ArticleOperation.提交, ArticleStatus.已提交),
                (ArticleOperation.发布, ArticleStatus.已发布));
            AddRule(ArticleStatus.已提交,
                ArticleOperation.发布, ArticleStatus.已发布);
            AddRule(ArticleStatus.已发布,
                ArticleOperation.撤回, ArticleStatus.已修改);
        }
    }
}