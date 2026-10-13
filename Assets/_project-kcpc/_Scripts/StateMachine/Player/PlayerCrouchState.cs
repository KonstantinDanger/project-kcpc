using ProjectKCPC.Scripts.Config;
using ProjectKCPC.Scripts.Entity;
using UnityEngine;

namespace ProjectKCPC.Scripts.StateMachine 
{
    public class PlayerCrouchState : PlayerGroundedState
    {
        private readonly ICrouchable _crouch;

        private readonly SurfaceChecker _surfaceChecker;
        private readonly PlayerInput _input;

        private readonly MovementConfig _movementConfig;
        private readonly CrouchConfig _crouchConfig;

        private readonly Vector3 _groundCheckInitialPosition;

        public PlayerCrouchState(Player player, IStateMachine stateMachine) : base(player, stateMachine)
        {
            _crouch = Get<ICrouchable>(player);
            _input = Get<PlayerInput>(player);
            _surfaceChecker = Get<SurfaceChecker>(player);

            _crouchConfig = player.CrouchConfig;
            _movementConfig = player.MovementConfig;

            _groundCheckInitialPosition = _surfaceChecker.GroundCheck.localPosition;
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            UnityEngine.Debug.Log("crouch state enter ");
        }

        protected override void OnExit()
        {
            base.OnExit();
            SetCrouch(false);

            AddGroundCheckerPositionOffsetY(-_crouchConfig.CrouchHeight);
        }

        protected override void OnUpdate(float deltaTime)
        {
            //base.OnUpdate(deltaTime);

            float speed = _input.MovementVector == Vector2.zero ? 0f : _crouchConfig.CrouchSpeed;
            base.SetSpeed(speed, _movementConfig.SpeedChangeAcceleration, deltaTime); 
            base.Move(_movementConfig.MovementSmoothness);

            float currentHeight = SetCrouch(true);
            AddGroundCheckerPositionOffsetY(currentHeight);

            if (!_input.IsCrouching && !_surfaceChecker.IsUnderCeiling)
                Transit<PlayerMovementState>();
        }

        private void AddGroundCheckerPositionOffsetY(float yOffset)
        {
            Vector3 position = _groundCheckInitialPosition;
            position.y = _groundCheckInitialPosition.y + yOffset;
            _surfaceChecker.GroundCheck.localPosition = position;
        }

        private float SetCrouch(bool isActive)
            => _crouch.SetCrouch(isActive, _crouchConfig.CrouchHeight, _crouchConfig.CrouchAcceleration);
    }
}
