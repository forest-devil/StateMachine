using StateMachine;

namespace StateMachineTest
{
    public class MyWorkflow : Workflow<ArticleStatus, ArticleOperation>
    {
        public MyWorkflow()
        {
            AddRule(ArticleStatus.已修改,
                ArticleOperation.提交, ArticleStatus.已提交);
            AddRule(ArticleStatus.已提交,
                ArticleOperation.发布, ArticleStatus.已发布);
            AddRule(ArticleStatus.已发布,
                (ArticleOperation.撤回, ArticleStatus.已修改),
                (ArticleOperation.存档, ArticleStatus.已存档));
        }
    }
}