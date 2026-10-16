using FishNet.Object;
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
            Vector3 velocity = _movement.Velocity;
            velocity.y = 0f;

            if (velocity.sqrMagnitude > 1f)
                _modelRotation.Rotate(velocity, _rotationSpeed);

            UpdateMovementAnimations();
        }

        private void UpdateMovementAnimations()
        {
            _animator.SetFloat(_horizontalVelocityParamName, _movement.Velocity.x);
            _animator.SetFloat(_verticalVelocityParamName, _movement.Velocity.z);
        }
    }
}
