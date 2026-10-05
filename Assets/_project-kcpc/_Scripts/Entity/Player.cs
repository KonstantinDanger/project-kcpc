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
        [SerializeField] private PlayerCamera _playerCamera;
        [SerializeField] private float _movementSpeed;
        [SerializeField] private Transform _directionPivot;
        [SerializeField] private PlayerMovement _movement;

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
        }

        private void OnDisable()
        {
            _input.Disable();
        }

        private void Update()
        {
            if (!HasActionAuthority())
                return;

            Rotate();

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

        private void Rotate()
        {
            Vector2 input = _input.Player.Look.ReadValue<Vector2>();

            _playerCamera.Rotate(input, _cameraConfig.RotationSpeed);
        }

        private void Move()
        {
            _movement.Move(_movementInputDirection, _movementSpeed);
        }

        private bool HasActionAuthority()
        {
            return IsOwner;
        }
    }
}
