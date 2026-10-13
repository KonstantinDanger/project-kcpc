using ProjectKCPC.Scripts.Entity;
using UnityEngine;

namespace ProjectKCPC.Scripts.StateMachine 
{
    public class PlayerState : IState
    {
        private readonly IStateMachine _stateMachine;
        protected readonly Player player;

        public PlayerState(Player player, IStateMachine stateMachine)
        {
            this.player = player;
            _stateMachine = stateMachine;
        }

        public void Enter() => OnEnter();
        public void Exit() => OnExit();
        public void FixedUpdate(float fixedDeltaTime) => OnFixedUpdate(fixedDeltaTime);
        public void Update(float deltaTime) => OnUpdate(deltaTime);

        protected virtual void OnEnter() { }
        protected virtual void OnExit() { }
        protected virtual void OnFixedUpdate(float fixedDeltaTime) { }
        protected virtual void OnUpdate(float deltaTime) { }

        protected void Transit<T>() where T : IState
            => _stateMachine.ChangeState<T>();

        protected Vector3 GetMovementDirection(Vector2 movementVector)
            => player.CachedMovementDirection;

        protected T Get<T>(Player player)
        {
            if (!player.TryGetComponent(out T component))
                throw new System.Exception($"No \"{component}\" found on player");

            return component;
        }
    }
}
