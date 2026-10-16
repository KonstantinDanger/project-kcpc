using ProjectKCPC.Scripts.Config;
using ProjectKCPC.Scripts.StateMachine;
using UnityEngine;

namespace ProjectKCPC.Scripts.Entity
{
    public class Player : Entity
    {
        //private readonly SyncVar<ulong> _steamID;
        //private readonly SyncVar<string> _steamName;
        //public ulong SteamID => _steamID.Value;
        //public string SteamName => _steamName.Value;

        [Header("Components")]
        [SerializeField] private PlayerCamera _playerCamera;
        [SerializeField] private Transform _directionPivot;
        [SerializeField] private PlayerInput _input;

        private IStateMachine _stateMachine;
        private Vector3 _movementInputDirection;

        public Vector3 CachedMovementDirection => _movementInputDirection;

        private void Awake()
        {
            _input.Initialize();
        }

        private void OnEnable()
        {
            _input.Enable();
        }

        private void OnDisable()
        {
            _input.Disable();
        }

        public override void OnStartClient()
        {
            _playerCamera.Initialize(HasActionAuthority(), CameraConfig);
            _stateMachine = new PlayerStateMachine(this);

            base.OnStartClient();
        }

        protected override void OnUpdate()
        {
            if (!HasActionAuthority())
                return;

            _stateMachine.CurrentState.Update(Time.deltaTime);
            _input.UpdateLogic();

            Rotate();
            CacheMovementDirection();
        }

        private void CacheMovementDirection()
        {
            Vector2 moveInput = _input.MovementVector;

            Vector3 projectedRight = Vector3.ProjectOnPlane(_directionPivot.right, transform.up).normalized;

            Vector3 projectedForward = Vector3.Cross(projectedRight, transform.up).normalized;

            _movementInputDirection = projectedForward * moveInput.y + projectedRight * moveInput.x;
            _movementInputDirection.Normalize();
        }

        private void Rotate()
            => _playerCamera.Rotate(_input.Rotation, CameraConfig.RotationSpeed);
        
        private bool HasActionAuthority()
        {
            return IsOwner;
        }
    }
}
