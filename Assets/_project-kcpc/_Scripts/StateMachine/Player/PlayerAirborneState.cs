using ProjectKCPC.Scripts.Config;
using ProjectKCPC.Scripts.Entity;
using UnityEngine;

namespace ProjectKCPC.Scripts.StateMachine 
{ 
    public class PlayerAirborneState : PlayerState
    {
        private readonly MovementConfig _movementConfig;
        private readonly GravityConfig _gravityConfig;
        private readonly ExternalForceConfig _externalForceConfig;

        private readonly IMovable _movable;
        private readonly IExternalForceHandler _externalForceHandler;
        private readonly IApplyGravity _gravityApplier;
        private readonly SurfaceChecker _surfaceChecker;
        private readonly PlayerInput _input;

        private Vector3 _acceleration;
        private Vector3 _cachedVelocity;

        private float _maxSpeed;

        public PlayerAirborneState(Player player, IStateMachine stateMachine) : base(player, stateMachine)
        {
            _movementConfig = player.MovementConfig;
            _gravityConfig = player.GravityConfig;
            _externalForceConfig = player.ExternalForceConfig;

            _movable = Get<IMovable>(player);
            _externalForceHandler = Get<IExternalForceHandler>(player);
            _gravityApplier = Get<IApplyGravity>(player);
            _surfaceChecker = Get<SurfaceChecker>(player);
            _input = Get<PlayerInput>(player);
        }

        protected override void OnEnter()
        {
            _cachedVelocity = _movable.Velocity;
            Vector3 modified = _cachedVelocity;
            modified.y = 0f;
            _cachedVelocity = modified;

            _acceleration = Vector3.zero;

            _maxSpeed = _cachedVelocity.magnitude;
            _maxSpeed = Mathf.Clamp(_maxSpeed, _movementConfig.WalkSpeed, _movementConfig.SprintSpeed);
        }

        protected override void OnUpdate(float deltaTime)
        {
            _gravityApplier.ApplyGravity(_gravityConfig.Gravity, _gravityConfig.MaxFallSpeed);

            if (_surfaceChecker.IsGrounded)
            {
                Transit<PlayerMovementState>();
                return;
            }

            _externalForceHandler.UpdateExternalForce(_externalForceConfig.Drag);
            MaintainAirborneVelocity(deltaTime);
        }

        private void MaintainAirborneVelocity(float deltaTime)
        {
            Vector3 movementDirection = GetMovementDirection(_input.MovementVector);
            _acceleration = deltaTime * _movementConfig.AirMovementAcceleration * movementDirection;
            _cachedVelocity += _acceleration;

            _cachedVelocity = Vector3.ClampMagnitude(_cachedVelocity, _maxSpeed);
            _movable.Move(_cachedVelocity, _cachedVelocity.magnitude);
        }
    }
}
