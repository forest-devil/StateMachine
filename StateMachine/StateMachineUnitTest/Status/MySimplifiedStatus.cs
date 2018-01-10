using StateMachine;

namespace StateMachineTest
{
    public class MySimplifiedStatus : Status<ArticleStatus, ArticleOperation, MySimplifiedStatus>
    {
        static MySimplifiedStatus()
        {
            Set(ArticleStatus.已修改,
                ArticleOperation.发布, ArticleStatus.已发布);
            Set(ArticleStatus.已发布,
                ArticleOperation.撤回, ArticleStatus.已修改);
        }

        public MySimplifiedStatus(object status) : base(status)
        {
        }
    }
}