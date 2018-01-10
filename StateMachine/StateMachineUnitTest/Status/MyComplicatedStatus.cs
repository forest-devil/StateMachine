using StateMachine;

namespace StateMachineTest
{
    public class MyComplicatedStatus : Status<ArticleStatus, ArticleOperation, MyComplicatedStatus>
    {
        static MyComplicatedStatus()
        {
            Set(ArticleStatus.已修改,
                (ArticleOperation.提交, ArticleStatus.已提交),
                (ArticleOperation.发布, ArticleStatus.已发布));
            Set(ArticleStatus.已提交,
                ArticleOperation.发布, ArticleStatus.已发布);
            Set(ArticleStatus.已发布,
                ArticleOperation.撤回, ArticleStatus.已修改);
        }

        public MyComplicatedStatus(object status) : base(status)
        {
        }
    }
}