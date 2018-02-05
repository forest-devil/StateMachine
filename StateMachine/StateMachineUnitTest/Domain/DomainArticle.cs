namespace StateMachineTest
{
    public class DomainArticle : IArticleBusinessOperations
    {
        public string Content { get; set; }

        //public MyStatus Status { get; set; }
        public MyComplicatedStatus Status { get; set; }

        public string Title { get; set; }

        public void AddComment(params object[] arguments) => Status.AddComment(arguments);

        public void AddRemark(params object[] arguments) => Status.AddRemark(arguments);

        public void Edit(params object[] arguments) => Status.Edit(arguments);
    }
}