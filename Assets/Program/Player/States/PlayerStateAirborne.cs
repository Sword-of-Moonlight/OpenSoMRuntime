public class PlayerStateAirborne : BaseState<PlayerState>
{
    /// <summary>State Name</summary>
    public override string Name => "Airborne";

    /// <summary>
    /// State Tick.<br/>
    /// </summary>
    public override void OnTick()
    {
        // Grab the player controller from the state machine context
        PlayerController controller = (StateMachine as PlayerStateMachine).Context;

        /**
         * Airborne->Grounded Check
        **/
        if (controller.IsGrounded)
        {
            StateMachine.SwitchState(PlayerState.Grounded);
            return;
        }

        /**
         * Looking
        **/
        controller.Turn(controller.ReadLookInput());

        /**
         * Moving
        **/
        controller.Move(controller.ReadMoveInput(), controller.WalkSpeed);

        base.OnTick();
    }
}
