using UnityEngine;

namespace ProjectKCPC.Scripts
{
    public class PlayerVisuals : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Transform _parentTransform;
        [SerializeField] private SimpleRotatable _modelRotation;
        [SerializeField, Min(0f)] private float _rotationSpeed;
        private IMovable _movement;

        private void Start()
        {
            _movement = _parentTransform.GetComponent<IMovable>();
        }

        private void Update()
        {
            Vector3 velocity = _movement.Velocity;
            velocity.y = 0f;

            if (velocity.sqrMagnitude > 1f)
                _modelRotation.Rotate(velocity, _rotationSpeed);
        }
    }
}
