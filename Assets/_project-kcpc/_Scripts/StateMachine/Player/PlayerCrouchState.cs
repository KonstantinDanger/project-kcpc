using ProjectKCPC.Scripts.Config;
using ProjectKCPC.Scripts.Entity;
using ProjectKCPC.Scripts.Extensions;
using UnityEngine;

namespace ProjectKCPC.Scripts.StateMachine 
{
    public class PlayerCrouchState : PlayerGroundedState
    {
        private enum CrouchState { None = 0, CrouchingDown = 1, StandingUp = 2}

        private const float CrouchStateChangeProgressThreshold = 1f;

        private readonly ICrouchable _crouch;

        private readonly SurfaceChecker _surfaceChecker;
        private readonly PlayerInput _input;
        private readonly Transform _cameraHolderTransform;

        private readonly MovementConfig _movementConfig;
        private readonly CrouchConfig _crouchConfig;

        private readonly Vector3 _cameraInitialPosition;

        private CrouchState _crouchState = CrouchState.None;

        public PlayerCrouchState(Player player, IStateMachine stateMachine) : base(player, stateMachine)
        {
            _crouch = Get<ICrouchable>(player);
            _input = Get<PlayerInput>(player);
            _surfaceChecker = Get<SurfaceChecker>(player);
            _cameraHolderTransform = Get<PlayerCamera>(player).CameraHolder;

            _crouchConfig = player.CrouchConfig;
            _movementConfig = player.MovementConfig;

            _cameraInitialPosition = _cameraHolderTransform.localPosition;
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            SetCameraLocalPosition(_cameraInitialPosition);
        }

        protected override void OnExit()
        {
            base.OnExit();

            SetCameraLocalPosition(_cameraInitialPosition);
        }

        protected override void Update(float deltaTime)
        {
            float speed = _input.MovementVector == Vector2.zero ? 0f : _crouchConfig.CrouchSpeed;
            base.SetSpeed(_crouchState == CrouchState.StandingUp ? _movementConfig.WalkSpeed : speed, _movementConfig.SpeedChangeAcceleration, deltaTime); 
            base.Move(_movementConfig.MovementSmoothness);

            if (_surfaceChecker.IsUnderCeiling)
            {
                _crouchState = SetCrouch(true);
                _input.IsCrouching = true;
                return;
            }

            _crouchState = SetCrouch(_input.IsCrouching);

            if (!_input.IsCrouching && _crouchState == CrouchState.None)
            {
                Transit<PlayerMovementState>();
            }
        }

        private CrouchState SetCrouch(bool isActive)
        {
            float currentHeight = _crouch.SetCrouch(isActive, _crouchConfig.CrouchHeight, _crouchConfig.CrouchAcceleration);
            float heightToShrink = _crouch.InitialHeight - _crouchConfig.CrouchHeight;
            float shrinkedHeight = _crouch.InitialHeight - currentHeight;
            float progress = Mathf.Clamp01(shrinkedHeight / heightToShrink);
            progress = isActive ? progress : 1 - progress;

            HandleCameraPosition(isActive, progress, heightToShrink);

            if (!isActive && progress < CrouchStateChangeProgressThreshold)
                return CrouchState.StandingUp;
            else if (isActive && progress < CrouchStateChangeProgressThreshold)
                return CrouchState.CrouchingDown;

            return CrouchState.None;
        }

        private void HandleCameraPosition(bool isActive, float progress, float heightToShrink)
        {
            Vector3 startingPosition = isActive
                ? _cameraInitialPosition
                : _cameraInitialPosition.WithY(_cameraInitialPosition.y - heightToShrink);

            Vector3 targetPosition = isActive
                ? _cameraInitialPosition.WithY(_cameraInitialPosition.y - heightToShrink)
                : _cameraInitialPosition;

            Vector3 deltaPosition = Vector3.Lerp(startingPosition, targetPosition, progress);

            SetCameraLocalPosition(deltaPosition);
        }

        private void SetCameraLocalPosition(Vector3 position)
        {
            _cameraHolderTransform.localPosition = position;
        }
    }
}
