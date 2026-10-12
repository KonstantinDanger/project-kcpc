using ProjectKCPC.Scripts.Config;
using ProjectKCPC.Scripts.Entity;
using UnityEngine;

namespace ProjectKCPC.Scripts.StateMachine 
{ 
    public class PlayerAirborneState : PlayerState
    {
        private readonly MovementConfig _cfg;
        private readonly IMovable _movable;

        private Vector3 _acceleration;
        private Vector3 CachedVelocity { get; set; }

        private float _maxSpeed;

        public PlayerAirborneState(Player player, IStateMachine stateMachine) : base(player, stateMachine)
        {
            _cfg = player.MovementConfig;
            _movable = Get<IMovable>(player);
        }

        protected override void OnEnter()
        {
            CachedVelocity = _movable.Velocity;
            Vector3 modified = CachedVelocity;
            modified.y = 0f;
            CachedVelocity = modified;

            _acceleration = Vector3.zero;

            _maxSpeed = CachedVelocity.magnitude;
            _maxSpeed = Mathf.Clamp(_maxSpeed, _cfg.WalkSpeed, _cfg.SprintSpeed);
        }

        protected override void OnUpdate(float deltaTime)
        {
            _movable.ApplyGravity(_cfg.Gravity, _cfg.MaxFallSpeed);

            if (_movable.IsGrounded)
            {
                Transit<PlayerMovementState>();
                return;
            }

            _movable.UpdateExternalForce(_cfg.ExternalForceDrag);
            MaintainAirborneVelocity(deltaTime);
        }

        private void MaintainAirborneVelocity(float deltaTime)
        {
            Vector3 movementDirection = GetMovementDirection(_input.MovementVector);
            _acceleration = deltaTime * _cfg.AirMovementAcceleration * movementDirection;
            CachedVelocity += _acceleration;

            CachedVelocity = Vector3.ClampMagnitude(CachedVelocity, _maxSpeed);
            _movable.Move(CachedVelocity, CachedVelocity.magnitude);
        }
    }
}
