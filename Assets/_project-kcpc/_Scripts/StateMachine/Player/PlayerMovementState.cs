using ProjectKCPC.Scripts.Config;
using ProjectKCPC.Scripts.Entity;
using UnityEngine;

namespace ProjectKCPC.Scripts.StateMachine 
{ 
    public class PlayerMovementState : PlayerGroundedState
    {
        private readonly MovementConfig _movementConfig;
        private readonly PlayerInput _input;
        private readonly SurfaceChecker _surfaceChecker;

        private float MovementSpeed 
            => (_input.IsSprinting && _input.MovementVector != Vector2.zero) 
            ? _movementConfig.SprintSpeed 
            : _movementConfig.WalkSpeed;

        public PlayerMovementState(Player player, IStateMachine fsm) : base(player, fsm)
        {
            _movementConfig = player.MovementConfig;
            _input = Get<PlayerInput>(player);
            _surfaceChecker = Get<SurfaceChecker>(player);
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
    }
}