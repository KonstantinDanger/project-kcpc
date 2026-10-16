using ProjectKCPC.Scripts.Extensions;
using System;
using UnityEngine;

namespace ProjectKCPC.Scripts.Movement
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(SurfaceChecker))]
    public class CharacterControllerPlayerMovement : MonoBehaviour, IMovable, IApplyGravity, IExternalForceHandler, IJumpable, ICrouchable
    {
        private const float AirborneSlopeLimit = 90f;

        public event Action OnMove;

        [SerializeField] private CharacterController _controller;
        [SerializeField] private float _groundedSnapForce = 5f;
        [SerializeField] private SurfaceChecker _surfaceChecker;

        public Vector3 Velocity => new Vector3(_horizontalVelocity.x, _verticalVelocity, _horizontalVelocity.z) + _externalForce;
        public Vector3 MoveDirection { get; private set; }

        public bool IsGravityActive { get; set; } = true;
        public bool IsGrounded => _surfaceChecker.IsGrounded;
        public float InitialHeight => _initialControllerHeight;

        private Vector3 _externalForce;
        private Vector3 _movementDelta;
        private Vector3 _slopeSlideVelocity;

        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        private float _initialControllerHeight;
        private Vector3 _initialCenter;
        private float _initialSlopeLimit;

        private void Awake()
        {
            _initialControllerHeight = _controller.height;
            _initialCenter = _controller.center;
            _initialSlopeLimit = _controller.slopeLimit;
        }

        private void Update()
        {
            _controller.slopeLimit = _surfaceChecker.IsGrounded ? _initialSlopeLimit : AirborneSlopeLimit;
        }

        public void ApplyGravity(float gravity, float maxFallSpeed)
        {
            if (!IsGravityActive)
                return;

            _verticalVelocity += gravity * Time.deltaTime;

            if (Mathf.Abs(_verticalVelocity) >= maxFallSpeed)
                _verticalVelocity = -maxFallSpeed;

            if (IsGrounded && _verticalVelocity < 0f && _externalForce.y <= 0.01f)
                _verticalVelocity = -_groundedSnapForce;

            Vector3 verticalVelocity = new(0f, _verticalVelocity + _externalForce.y, 0f);

            Vector3 slopeSlideDirection = GetSlopeSlideDirection(_surfaceChecker.SlopeHitInfo.normal, out float angleToNormal);

            float normalAngleMultiplier = 1f + Mathf.Clamp(angleToNormal, 0f, _controller.slopeLimit) / _controller.slopeLimit;

            float slideSpeed = Mathf.Abs(gravity * _verticalVelocity * Velocity.magnitude * normalAngleMultiplier);

            _slopeSlideVelocity = slideSpeed * Time.deltaTime * slopeSlideDirection;

            _controller.Move((verticalVelocity + _slopeSlideVelocity) * Time.deltaTime);
        }

        private Vector3 GetSlopeSlideDirection(Vector3 surfaceNormal, out float angleToNormal)
        {
            angleToNormal = Vector3.Angle(transform.up, surfaceNormal);

            if (!IsGrounded || surfaceNormal == Vector3.zero)
                return Vector3.zero;

            if (angleToNormal < _controller.slopeLimit)
                return Vector3.zero;

            Vector3 slideSurface = Vector3.ProjectOnPlane(-transform.up, surfaceNormal);

            return slideSurface;
        }

        public void ResetVerticalVelocity()
            => _verticalVelocity = 0;

        public void Jump(float jumpHeight, float gravity)
        {
            if (!IsGrounded)
                return;

            if (_slopeSlideVelocity.magnitude > 0.01f)
            {
                Vector3 jumpForce = _surfaceChecker.SlopeHitInfo.normal * 0.1f;
                jumpForce.y = 0f;
                ApplyExternalForce(jumpForce);
            }

            _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        public void Move(Vector3 direction, float speed, float smoothness = 0.03F)
        {
            MoveDirection = direction;

            Vector3 targetVelocity = direction.normalized * speed;

            _horizontalVelocity = Vector3.SmoothDamp(
                Velocity,
                targetVelocity,
                ref _movementDelta,
                smoothness);

            _controller.Move(_horizontalVelocity * Time.deltaTime);

            //if (NetworkClient.active)
            //    RpcOnMove();
            //else
            //    OnMove?.Invoke();
        }

        public void ApplyExternalForce(Vector3 direction, float force)
            => ApplyExternalForce(direction.normalized * force);

        public void ApplyExternalForce(Vector3 force)
            => _externalForce += force;

        public void UpdateExternalForce(float drag)
        {
            _externalForce *= 1f / (1f + drag * Time.deltaTime);

            _controller.Move(_externalForce);
        }

        public void Warp(Vector3 position)
        {
            _controller.enabled = false;

            transform.position = position;

            Physics.SyncTransforms();

            _controller.enabled = true;
        }

        public float SetCrouch(bool isCrouching, float crouchHeight, float crouchSpeed)
        {
            float selectedCrouchHeight = isCrouching ? crouchHeight : _initialControllerHeight;
            float crouchDelta = crouchSpeed * Time.deltaTime;
            float currentHeight = Mathf.Lerp(_controller.height, selectedCrouchHeight, crouchDelta);

            if (Mathf.Abs(currentHeight - selectedCrouchHeight) < 0.001f)
                currentHeight = selectedCrouchHeight;

            _controller.height = currentHeight;
            _controller.center = _initialCenter.WithY(_initialCenter.y - (InitialHeight - currentHeight) / 2f);

            return currentHeight;
        }

        /*[ClientRpc]*/
        private void RpcOnMove() => OnMove?.Invoke();

        public void ResetHeightToDefault()
        {
            _controller.center = _initialCenter;
            _controller.height = _initialControllerHeight;
        }
    }
}
