using System;
using UnityEngine;

namespace ProjectKCPC.Scripts
{
    public class PlayerInput : MonoBehaviour
    {
        private PlayerInputActions _actions;

        private Vector2 _movementInput;
        private Vector2 _rotationInput;
        private bool _isSprinting;

        public Vector2 MovementVector => _movementInput;
        public Vector2 Rotation => _rotationInput;
        public bool IsCrouching { get; set; }
        public bool IsSprinting => _isSprinting;

        public event Action JumpAction;
        public event Action OnMenuInvoked;
        public event Action AttackAction;
        public event Action OnInteraction;

        public void Initialize()
        {
            _actions = new();

            _actions.Player.Attack.performed += _ => AttackAction?.Invoke();
            _actions.Player.Jump.performed += _ => JumpAction?.Invoke();
            //_actions.UI.Menu.performed += _ => OnMenuInvoked?.Invoke();

            _actions.Player.Sprint.started += _ => _isSprinting = true;
            _actions.Player.Sprint.canceled += _ => _isSprinting = false;
            _actions.Player.Crouch.performed += _ => IsCrouching = !IsCrouching;

            _actions.Player.Interact.performed += _ => OnInteraction?.Invoke();
        }

        public void UpdateLogic()
        {
            _movementInput = _actions.Player.Move.ReadValue<Vector2>();
            _rotationInput = _actions.Player.Look.ReadValue<Vector2>();
        }

        public void SetInputState(Vector2 movement, Vector2 rotation, bool sprinting)
        {
            _movementInput = movement;
            _rotationInput = rotation;
            _isSprinting = sprinting;
        }

        public void Enable()
            => _actions.Enable();

        public void Disable()
            => _actions.Disable();

        public void SetUiInput(bool active)
        {
            if (!_actions.Player.enabled)
                (active ? (Action)_actions.UI.Enable : _actions.UI.Disable)();
        }

        public void SetPlayerInput(bool active)
            => (active ? (Action)_actions.Player.Enable : _actions.Player.Disable)();

        public override string ToString()
            => $"Movement input {_movementInput} | Rotation input {_rotationInput}";
    }
}
