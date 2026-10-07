using UnityEngine;

namespace ProjectKCPC.Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour, IMovable
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private ForceMode _moveForceMode;
        [SerializeField] private ForceMode _jumpForceMode;

        public Vector3 Velocity { get; private set; }

        public void Move(Vector3 direction, float speed, float smoothness = 0.03F)
        {
            Velocity = speed * Time.fixedDeltaTime * direction;

            _rigidbody.AddForce(Velocity, _moveForceMode);

            LimitVelocity(speed);
        }

        public void Jump(float height)
        {
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
