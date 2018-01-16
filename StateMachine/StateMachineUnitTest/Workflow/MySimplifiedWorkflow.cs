using StateMachine;

namespace StateMachineTest
{
    public class MySimplifiedWorkflow : Workflow<ArticleStatus, ArticleOperation>
    {
        public MySimplifiedWorkflow()
        {
            AddRule(ArticleStatus.已修改,
                ArticleOperation.发布, ArticleStatus.已发布);
            AddRule(ArticleStatus.已发布,
                ArticleOperation.撤回, ArticleStatus.已修改);
        }
    }
}