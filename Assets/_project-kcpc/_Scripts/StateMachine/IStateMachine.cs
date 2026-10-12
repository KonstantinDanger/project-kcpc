namespace ProjectKCPC.Scripts.StateMachine
{
    public interface IStateMachine
    {
        public IState CurrentState { get; set; }
        public IStateMachine StartWith<T>()
            where T : IState;
        public IStateMachine AddState<T>(T newState)
            where T : IState;
        public IStateMachine RemoveState<T>(out T removedState)
            where T : IState;
        public IStateMachine ChangeState<T>()
            where T : IState;
        bool HasState<T>()
            where T : IState;
    }
}
