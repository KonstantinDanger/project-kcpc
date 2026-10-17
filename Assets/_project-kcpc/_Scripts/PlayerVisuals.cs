using FishNet.Object;
using ProjectKCPC.Scripts.Entity;
using UnityEngine;

namespace ProjectKCPC.Scripts
{
    public class PlayerVisuals : NetworkBehaviour
    {
        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private string _horizontalVelocityParamName = "HorizontalVelocity";
        [SerializeField] private string _verticalVelocityParamName = "VerticalVelocity";

        [Header("Misc")]
        [SerializeField] private Player _player;
        [SerializeField] private Transform _parentTransform;
        [SerializeField] private SimpleRotatable _modelRotation;
        [SerializeField, Min(0f)] private float _rotationSpeed;

        private IMovable _movement;

        private void Start()
        {
            _movement = _parentTransform.GetComponent<IMovable>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!IsOwner)
                enabled = false;
        }

        private void Update()
        {
            Vector3 velocity = transform.InverseTransformDirection(_movement.Velocity);

            if (velocity.sqrMagnitude > 1f)
            {
                _modelRotation.Rotate(_player.ProjectedForward, _rotationSpeed);
            }

            UpdateMovementAnimations(velocity);
        }

        private void UpdateMovementAnimations(Vector3 velocity)
        {
            _animator.SetFloat(_horizontalVelocityParamName, velocity.x);
            _animator.SetFloat(_verticalVelocityParamName, velocity.z);
        }
    }
}
