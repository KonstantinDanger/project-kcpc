using UnityEngine;

namespace ProjectKCPC.Scripts.Entity
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private float _movementSpeed;
        //[SerializeField] private IMovable _movement;

        [SerializeField] private PlayerMovement _movement;

        private PlayerInput _input;

        private void Awake()
        {
            _input = new PlayerInput();
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
            Vector2 moveInput = _input.Player.Move.ReadValue<Vector2>();

            Vector3 inputDir = transform.forward * moveInput.y + transform.right * moveInput.x;
            inputDir.Normalize();

            _movement.Move(inputDir, _movementSpeed);
        }
    }
}
