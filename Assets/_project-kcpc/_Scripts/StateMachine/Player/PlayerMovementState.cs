using ProjectKCPC.Scripts.Config;
using ProjectKCPC.Scripts.Entity;
using UnityEngine;

namespace ProjectKCPC.Scripts.StateMachine 
{ 
    public class PlayerMovementState : PlayerGroundedState
    {
        private readonly PlayerInput _input;
        private readonly SurfaceChecker _surfaceChecker;
        private readonly IJumpable _jump;

        private readonly JumpConfig _jumpConfig;
        private readonly GravityConfig _gravityConfig;
        private readonly MovementConfig _movementConfig;

        private float MovementSpeed 
            => (_input.IsSprinting && _input.MovementVector != Vector2.zero) 
            ? _movementConfig.SprintSpeed 
            : _movementConfig.WalkSpeed;

        public PlayerMovementState(Player player, IStateMachine fsm) : base(player, fsm)
        {
            _input = Get<PlayerInput>(player);
            _surfaceChecker = Get<SurfaceChecker>(player);
            _jump = Get<IJumpable>(player);

            _movementConfig = player.MovementConfig;
            _jumpConfig = player.JumpConfig;
            _gravityConfig = player.GravityConfig;
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            _input.JumpAction += HandleJump;
        }

        protected override void OnExit()
        {
            base.OnExit();

            _input.JumpAction -= HandleJump;
        }

        protected override void Update(float deltaTime)
        {
            if (_input.IsCrouching || _surfaceChecker.IsUnderCeiling)
            {
                Transit<PlayerCrouchState>();
                return;
            }

            base.Move(_movementConfig.MovementSmoothness);
            base.SetSpeed(MovementSpeed, _movementConfig.SpeedChangeAcceleration, deltaTime);
        }

        private void HandleJump()
            => _jump.Jump(_jumpConfig.JumpHeight, _gravityConfig.Gravity);
    }
}