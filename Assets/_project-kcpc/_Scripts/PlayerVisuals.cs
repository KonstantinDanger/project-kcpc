using UnityEngine;

namespace ProjectKCPC.Scripts
{
    public class PlayerVisuals : MonoBehaviour
    {
        [SerializeField] private SimpleRotatable _modelRotation;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField, Min(0f)] private float _rotationSpeed;

        private void Update()
        {
            Vector3 velocity = _movement.Velocity;

            _modelRotation.Rotate(velocity, _rotationSpeed);
        }
    }
}
