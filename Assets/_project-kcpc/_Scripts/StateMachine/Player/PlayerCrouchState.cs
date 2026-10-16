using ProjectKCPC.Scripts.Config;
using ProjectKCPC.Scripts.Entity;
using ProjectKCPC.Scripts.Extensions;
using UnityEngine;

namespace ProjectKCPC.Scripts.StateMachine 
{
    public class PlayerCrouchState : PlayerGroundedState
    {
        private enum CrouchState { None = 0, CrouchingDown = 1, StandingUp = 2}

        private const float CrouchStateChangeProgressThreshold = 0.96f;
        private const float StateTransitionProgressThresholdToJump = 0.9f;

        private readonly ICrouchable _crouch;
        private readonly IJumpable _jumpable;

        private readonly SurfaceChecker _surfaceChecker;
        private readonly PlayerInput _input;
        private readonly Transform _cameraHolderTransform;

        private readonly MovementConfig _movementConfig;
        private readonly CrouchConfig _crouchConfig;
        private readonly JumpConfig _jumpConfig;
        private readonly GravityConfig _gravityConfig;

        private readonly Vector3 _cameraInitialPosition;

        private CrouchState _crouchState = CrouchState.None;

        private float _stateTransitionProgress;
        private bool _initiatedJump;

        public PlayerCrouchState(Player player, IStateMachine stateMachine) : base(player, stateMachine)
        {
            _crouch = Get<ICrouchable>(player);
            _input = Get<PlayerInput>(player);
            _surfaceChecker = Get<SurfaceChecker>(player);
            _cameraHolderTransform = Get<PlayerCamera>(player).CameraHolder;
            _jumpable = Get<IJumpable>(player);

            _gravityConfig = player.GravityConfig;
            _crouchConfig = player.CrouchConfig;
            _movementConfig = player.MovementConfig;
            _jumpConfig = player.JumpConfig;

            _cameraInitialPosition = _cameraHolderTransform.localPosition;
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            SetCameraLocalPosition(_cameraInitialPosition);
            _crouch.ResetHeightToDefault();
            _input.JumpAction += HandleInitiateJump;
            _stateTransitionProgress = 0f;
        }

        protected override void OnExit()
        {
            base.OnExit();

            SetCameraLocalPosition(_cameraInitialPosition);
            _crouch.ResetHeightToDefault();
            _input.JumpAction -= HandleInitiateJump;
            _stateTransitionProgress = 0f;
        }

        protected override void Update(float deltaTime)
        {
            float speed = _input.MovementVector == Vector2.zero ? 0f : _crouchConfig.CrouchSpeed;
            float targetTransitionSpeed = _initiatedJump ? _crouchConfig.CrouchRapidTransitionSpeed : _crouchConfig.CrouchTransitionSpeed;
            base.SetSpeed(_crouchState == CrouchState.StandingUp ? _movementConfig.WalkSpeed : speed, _movementConfig.SpeedChangeAcceleration, deltaTime); 
            base.Move(_movementConfig.MovementSmoothness);

            if (_surfaceChecker.IsUnderCeiling)
            {
                _crouchState = SetCrouch(true, targetTransitionSpeed);
                _input.IsCrouching = true;
                return;
            }

            if (_initiatedJump)
            {
                _crouchState = SetCrouch(false, targetTransitionSpeed);
                _input.IsCrouching = false;

                if (_crouchState == CrouchState.StandingUp && _stateTransitionProgress < StateTransitionProgressThresholdToJump)
                    return;

                _jumpable.Jump(_jumpConfig.JumpHeight, _gravityConfig.Gravity);
                _initiatedJump = false;
            }

            _crouchState = SetCrouch(_input.IsCrouching, targetTransitionSpeed);

            if (!_input.IsCrouching && _crouchState == CrouchState.None)
            {
                Transit<PlayerMovementState>();
            }
        }

        private CrouchState SetCrouch(bool isActive, float transitionSpeed)
        {
            float currentHeight = _crouch.SetCrouch(isActive, _crouchConfig.CrouchHeight, transitionSpeed);
            float heightToShrink = _crouch.InitialHeight - _crouchConfig.CrouchHeight;
            float shrinkedHeight = _crouch.InitialHeight - currentHeight;
            _stateTransitionProgress = Mathf.Clamp01(shrinkedHeight / heightToShrink);
            _stateTransitionProgress = isActive ? _stateTransitionProgress : 1 - _stateTransitionProgress;

            HandleCameraPosition(isActive, _stateTransitionProgress, heightToShrink);

            if (!isActive && _stateTransitionProgress < CrouchStateChangeProgressThreshold)
                return CrouchState.StandingUp;
            else if (isActive && _stateTransitionProgress < CrouchStateChangeProgressThreshold)
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

        private void HandleInitiateJump()
        {
            if (_surfaceChecker.IsUnderCeiling)
                return;

            _initiatedJump = true;
        }
    }
}
