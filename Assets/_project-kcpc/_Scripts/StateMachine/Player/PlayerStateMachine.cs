using ProjectKCPC.Scripts.Entity;

namespace ProjectKCPC.Scripts.StateMachine 
{ 
    public class PlayerStateMachine : StateMachine
    {
        public PlayerStateMachine(Player player)
        {
            AddState<PlayerMovementState>(new(player, this))
            .AddState<PlayerAirborneState>(new(player, this))
            .StartWith<PlayerMovementState>();
        }
    }
}
