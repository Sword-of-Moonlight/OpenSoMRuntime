public enum PlayerState : uint
{
    Grounded = 0x00000001,
    Airborne = 0x00000002
}

public class PlayerStateMachine : StateMachine<PlayerState>
{
    public PlayerController Context { get; private set; } = null;

    public PlayerStateMachine(PlayerController playerController) : base(false)
    {
        // State Registration
        Register(PlayerState.Grounded, new PlayerStateGrounded(), true);
        Register(PlayerState.Airborne, new PlayerStateAirborne());

        // Store player controller as context
        Context = playerController;
    }
}
