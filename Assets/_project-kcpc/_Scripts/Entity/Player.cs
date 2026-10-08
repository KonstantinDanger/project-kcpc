using FishNet.Object;
using ProjectKCPC.Scripts.Config;
using UnityEngine;

namespace ProjectKCPC.Scripts.Entity
{
    public class Player : NetworkBehaviour
    {
        //private readonly SyncVar<ulong> _steamID;
        //private readonly SyncVar<string> _steamName;
        //public ulong SteamID => _steamID.Value;
        //public string SteamName => _steamName.Value;

        [SerializeField] private CameraConfig _cameraConfig;
        [SerializeField] private MovementConfig _movementConfig;
        [SerializeField] private PlayerCamera _playerCamera;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private Transform _directionPivot;

        private PlayerInput _input;
        private Vector3 _movementInputDirection;

        private void Awake()
        {
            _input = new PlayerInput();
        }

        public override void OnStartClient()
        {
            _playerCamera.Initialize(HasActionAuthority(), _cameraConfig);
            
            base.OnStartClient();
        }

        private void OnEnable()
        {
            _input.Enable();

            _input.Player.Jump.performed += HandleJump;
        }

        private void OnDisable()
        {
            _input.Disable();
         
            _input.Player.Jump.performed -= HandleJump;
        }

        private void Update()
        {
            if (!HasActionAuthority())
                return;

            Rotate();
            CacheMovementDirection();
        }

        private void CacheMovementDirection()
        {
            Vector2 moveInput = _input.Player.Move.ReadValue<Vector2>();

            Vector3 projectedForward = Vector3.ProjectOnPlane(_directionPivot.forward, transform.up).normalized;
            Vector3 projectedRight = Vector3.ProjectOnPlane(_directionPivot.right, transform.up).normalized;

            _movementInputDirection = projectedForward * moveInput.y + projectedRight * moveInput.x;
            _movementInputDirection.Normalize();
        }

        private void FixedUpdate()
        {
            if (!HasActionAuthority())
                return;

            Move();
        }

        private void HandleJump(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (!HasActionAuthority())
                return;

            _movement.Jump(_movementConfig.JumpHeight);
        }

        private void Rotate()
        {
            Vector2 input = _input.Player.Look.ReadValue<Vector2>();

            _playerCamera.Rotate(input, _cameraConfig.RotationSpeed);
        }

        private void Move()
        {
            _movement.Move(_movementInputDirection, _movementConfig.MovementSpeed);
        }

        private bool HasActionAuthority()
        {
            return IsOwner;
        }
    }
}
