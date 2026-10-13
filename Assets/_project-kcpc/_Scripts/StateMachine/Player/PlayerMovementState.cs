using ProjectKCPC.Scripts.Config;
using ProjectKCPC.Scripts.Entity;
using UnityEngine;

namespace ProjectKCPC.Scripts.StateMachine 
{ 
    public class PlayerMovementState : PlayerGroundedState
    {
        private readonly MovementConfig _movementConfig;
        private readonly PlayerInput _input;

        private float MovementSpeed 
            => (_input.IsSprinting && _input.MovementVector != Vector2.zero) 
            ? _movementConfig.SprintSpeed 
            : _movementConfig.WalkSpeed;

        public PlayerMovementState(Player player, IStateMachine fsm) : base(player, fsm)
        {
            _movementConfig = player.MovementConfig;
            _input = Get<PlayerInput>(player);
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_input.IsCrouching)
            {
                Transit<PlayerCrouchState>();
                return;
            }

            base.Move(_movementConfig.MovementSmoothness);
            base.SetSpeed(MovementSpeed, _movementConfig.SpeedChangeAcceleration, deltaTime);
            base.OnUpdate(deltaTime);
        }
    }
}