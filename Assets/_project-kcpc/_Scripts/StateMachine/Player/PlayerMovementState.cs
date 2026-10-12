using ProjectKCPC.Scripts.Config;
using ProjectKCPC.Scripts.Entity;
using UnityEngine;

namespace ProjectKCPC.Scripts.StateMachine 
{ 
    public class PlayerMovementState : PlayerGroundedState
    {
        private MovementConfig MovementConfig { get; }
        private IMovable Movable { get; }
        private PlayerInput InputBrain { get; }

        private float _currentSpeed;
        private Vector3 _movementDirection;

        private Vector2 MovementInput => InputBrain.MovementVector;

        public PlayerMovementState(Player player, IStateMachine fsm) : base(player, fsm)
        {
            MovementConfig = player.MovementConfig;
            Movable = Get<IMovable>(player);
            InputBrain = Get<PlayerInput>(player);
        }

        protected override void OnUpdate(float deltaTime)
        {
            _currentSpeed =
                (InputBrain.IsSprinting && MovementInput != Vector2.zero) ?
                MovementConfig.SprintSpeed : MovementConfig.WalkSpeed;

            _movementDirection = GetMovementDirection(MovementInput);
            Movable.Move(_movementDirection, _currentSpeed, MovementConfig.MovementSmoothness);
            Movable.ApplyGravity(MovementConfig.Gravity, MovementConfig.MaxFallSpeed);
            Movable.UpdateExternalForce(MovementConfig.ExternalForceDrag);

            base.OnUpdate(deltaTime);
        }

        public override string ToString()
            => $"Movement state of player {player.name}" +
            $"\nCurrent speed: {_currentSpeed}" +
            $"\nMovement input: {MovementInput}" +
            $"\nMovement direction: {_movementDirection}";
    }
}