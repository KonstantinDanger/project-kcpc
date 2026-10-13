using ProjectKCPC.Scripts.Config;
using ProjectKCPC.Scripts.Entity;

namespace ProjectKCPC.Scripts.StateMachine 
{
    public class PlayerCrouchState : PlayerGroundedState
    {
        private readonly SurfaceChecker _surfaceChecker;
        private readonly ICrouchable _crouch;
        private readonly IMovable _movable;
        private readonly PlayerInput _input;

        private readonly CrouchConfig _crouchConfig; 

        public PlayerCrouchState(Player player, IStateMachine stateMachine) : base(player, stateMachine)
        {
            _crouch = Get<ICrouchable>(player);
            _movable = Get<IMovable>(player);
            _input = Get<PlayerInput>(player);

            _crouchConfig = player.CrouchConfig;
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            SetCrouch(true);
        }

        protected override void OnExit()
        {
            base.OnExit();

            SetCrouch(false);
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_surfaceChecker.IsUnderCeiling)
                return;

            if (!_input.IsCrouching)
                Transit<PlayerMovementState>();
        }

        private void SetCrouch(bool isActive)
            => _crouch.SetCrouch(isActive, _crouchConfig.CrouchHeight, _crouchConfig.CrouchAcceleration);
    }
}
