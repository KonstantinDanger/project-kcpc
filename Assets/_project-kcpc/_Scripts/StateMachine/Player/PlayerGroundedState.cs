using ProjectKCPC.Scripts.Config;
using ProjectKCPC.Scripts.Entity;
using UnityEngine;

namespace ProjectKCPC.Scripts.StateMachine 
{ 
    public abstract class PlayerGroundedState : PlayerState
    {
        private readonly IMovable _movement;

        private readonly PlayerInput _input;
        private readonly SurfaceChecker _surfaceChecker;

        private Vector3 _movementDirection;
        private Vector2 MovementInput => _input.MovementVector;

        private float _currentSpeed;

        public PlayerGroundedState(Player player, IStateMachine stateMachine) : base(player, stateMachine) 
        { 
            _input = Get<PlayerInput>(player);
            _surfaceChecker = Get<SurfaceChecker>(player);
            _movement = Get<IMovable>(player);
        }

        protected new virtual void Update(float deltaTime) { }

        protected sealed override void OnUpdate(float deltaTime)
        {
            if (!_surfaceChecker.IsGrounded)
            {
                Transit<PlayerAirborneState>();
                return;
            }

            Update(deltaTime);
        }

        protected void SetSpeed(float desiredSpeed, float speedAcceleration, float deltaTime)
        {
            if (_currentSpeed == desiredSpeed)
                return;

            _currentSpeed = Mathf.Lerp(_currentSpeed, desiredSpeed, speedAcceleration * deltaTime);
        }

        protected void Move(float movementSmoothness)
        {
            _movementDirection = GetMovementDirection(MovementInput);
            _movement.Move(_movementDirection, _currentSpeed, movementSmoothness);
        }

        public override string ToString()
            => $"Movement state of player {player.name}" +
            $"\nCurrent speed: {_currentSpeed}" +
            $"\nMovement input: {MovementInput}" +
            $"\nMovement direction: {_movementDirection}";
    }
}