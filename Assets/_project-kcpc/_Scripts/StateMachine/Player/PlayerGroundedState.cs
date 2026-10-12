using ProjectKCPC.Scripts.Entity;

namespace ProjectKCPC.Scripts.StateMachine 
{ 
    public class PlayerGroundedState : PlayerState
    {
        private SurfaceChecker _surfaceChecker;

        public PlayerGroundedState(Player player, IStateMachine stateMachine) : base(player, stateMachine) { }

        protected override void OnEnter()
        {
            base.OnEnter();

            _surfaceChecker = Get<SurfaceChecker>(player);
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (!_surfaceChecker.IsGrounded)
                Transit<PlayerAirborneState>();
        }
    }
}