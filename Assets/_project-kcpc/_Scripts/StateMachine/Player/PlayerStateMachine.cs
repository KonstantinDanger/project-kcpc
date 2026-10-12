using ProjectKCPC.Scripts.Entity;

namespace ProjectKCPC.Scripts.StateMachine 
{ 
    public class PlayerStateMachine : StateMachine
    {
        public PlayerStateMachine(Player player)
        {
            AddState(new PlayerMovementState(player, this))
            .AddState(new PlayerAirborneState(player, this))
            .StartWith<PlayerMovementState>();
        }
    }
}
