namespace StateMachine
{
    public interface IStatus<TStatus, in TOperation, out T>
    {
        TStatus Value { get; }

        T Transition(TOperation operation);
    }
}