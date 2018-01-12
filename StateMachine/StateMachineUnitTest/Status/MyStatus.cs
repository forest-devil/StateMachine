using StateMachine;

namespace StateMachineTest
{
    public class MyStatus : Status<ArticleStatus, ArticleOperation, MyStatus>
    {
        static MyStatus()
        {
            Set(ArticleStatus.已修改,
                ArticleOperation.提交, ArticleStatus.已提交);
            Set(ArticleStatus.已提交,
                ArticleOperation.发布, ArticleStatus.已发布);
            Set(ArticleStatus.已发布,
                (ArticleOperation.撤回, ArticleStatus.已修改),
                (ArticleOperation.存档, ArticleStatus.已存档));
        }

        public MyStatus(object status) : base(status)
        {
        }
    }
}