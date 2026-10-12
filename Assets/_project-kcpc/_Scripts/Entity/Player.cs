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

        [field: Header("Configs")]
        [field: SerializeField] public CameraConfig CameraConfig {get; private set;}
        [field: SerializeField] public MovementConfig MovementConfig { get; private set; }

        [Header("Components")]
        [SerializeField] private PlayerCamera _playerCamera;
        [SerializeField] private IMovable _movement;
        [SerializeField] private Transform _directionPivot;
        [SerializeField] private PlayerInput _input;

        private Vector3 _movementInputDirection;

        public override void OnStartClient()
        {
            _playerCamera.Initialize(HasActionAuthority(), CameraConfig);
            
            base.OnStartClient();
        }

        private void OnEnable()
        {
            _input.Enable();

            _input.JumpAction += HandleJump;
        }

        private void OnDisable()
        {
            _input.Disable();
         
            _input.JumpAction -= HandleJump;
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
            Vector2 moveInput = _input.MovementVector;

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

        private void HandleJump()
        {
            if (!HasActionAuthority())
                return;

            _movement.Jump(MovementConfig.JumpHeight);
        }

        private void Rotate()
        {
            Vector2 input = _input.Rotation;

            _playerCamera.Rotate(input, CameraConfig.RotationSpeed);
        }

        private void Move()
        {
            _movement.Move(_movementInputDirection, MovementConfig.WalkSpeed);
        }

        private bool HasActionAuthority()
        {
            return IsOwner;
        }
    }
}
