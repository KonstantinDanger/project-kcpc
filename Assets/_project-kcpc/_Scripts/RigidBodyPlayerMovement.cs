using UnityEngine;

namespace ProjectKCPC.Scripts.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidBodyPlayerMovement : MonoBehaviour, IMovable
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private ForceMode _moveForceMode;
        [SerializeField] private ForceMode _jumpForceMode;

        public Vector3 Velocity { get; private set; }

        public void Move(Vector3 direction, float speed, float smoothness = 0.03F)
        {
            Velocity = new Vector3(direction.x, 0f, direction.z).normalized;
            _rigidbody.AddForce(speed * Velocity, _moveForceMode);

            LimitVelocity(speed);
        }

        public void Jump(float height)
        {
            Vector3 velocity = _rigidbody.linearVelocity;
            velocity.y = 0f;
            _rigidbody.linearVelocity = velocity;

            _rigidbody.AddForce(transform.up * height, _jumpForceMode);
        }

        private void LimitVelocity(float speed)
        {
            Vector3 horizontalVelocity = _rigidbody.linearVelocity;
            horizontalVelocity.y = 0f;
            horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, speed);
            _rigidbody.linearVelocity = new Vector3(horizontalVelocity.x, _rigidbody.linearVelocity.y, horizontalVelocity.z);
        }
    }
}
