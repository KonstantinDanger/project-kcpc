using ProjectKCPC.Scripts.Config;
using ProjectKCPC.Scripts.Entity;
using UnityEngine;

namespace ProjectKCPC.Scripts.StateMachine 
{ 
    public class PlayerMovementState : PlayerGroundedState
    {
        private readonly MovementConfig _movementConfig;
        private readonly GravityConfig _gravityConfig;
        private readonly ExternalForceConfig _externalForceConfig;

        private readonly IMovable _movable;
        private readonly IApplyGravity _gravityApplier;
        private readonly PlayerInput _inputBrain;
        private readonly IExternalForceHandler _externalForceHandler;

        private float _currentSpeed;
        private Vector3 _movementDirection;
        private Vector2 MovementInput => _inputBrain.MovementVector;

        public PlayerMovementState(Player player, IStateMachine fsm) : base(player, fsm)
        {
            _movementConfig = player.MovementConfig;
            _gravityConfig = player.GravityConfig;
            _externalForceConfig = player.ExternalForceConfig;

            _movable = Get<IMovable>(player);
            _inputBrain = Get<PlayerInput>(player);
            _gravityApplier = Get<IApplyGravity>(player);
            _externalForceHandler = Get<IExternalForceHandler>(player);
        }

        protected override void OnUpdate(float deltaTime)
        {
            _currentSpeed =
                (_inputBrain.IsSprinting && MovementInput != Vector2.zero) ?
                _movementConfig.SprintSpeed : _movementConfig.WalkSpeed;

            _movementDirection = GetMovementDirection(MovementInput);
            _movable.Move(_movementDirection, _currentSpeed, _movementConfig.MovementSmoothness);
            _gravityApplier.ApplyGravity(_gravityConfig.Gravity, _gravityConfig.MaxFallSpeed);
            _externalForceHandler.UpdateExternalForce(_externalForceConfig.Drag);

            base.OnUpdate(deltaTime);
        }

        public override string ToString()
            => $"Movement state of player {player.name}" +
            $"\nCurrent speed: {_currentSpeed}" +
            $"\nMovement input: {MovementInput}" +
            $"\nMovement direction: {_movementDirection}";
    }
}