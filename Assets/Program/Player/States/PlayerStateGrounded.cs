using UnityEngine;

public class PlayerStateGrounded : BaseState<PlayerState>
{
    /// <summary>State Name</summary>
    public override string Name => "Grounded";

    public override void OnEnter()
    {
        // Lock mouse cursor for first-person control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        base.OnEnter();
    }

    /// <summary>
    /// State Tick.<br/>
    /// </summary>
    public override void OnTick()
    {
        // Grab the player controller from the state machine context
        PlayerController controller = (StateMachine as PlayerStateMachine).Context;

        /**
         * Grounded-Airborne Check
         * Disabled for now because no real movement code is implemented, and I want most of it in grounded...
        **/
        /**
        if (!controller.IsGrounded)
        {
            StateMachine.SwitchState(PlayerState.Airborne);
            return;
        }
        **/

        /**
         * Looking
        **/
        controller.Turn(controller.ReadLookInput());

        /**
         * Moving
        **/
        Vector2 input = controller.ReadMoveInput();
        float speed   = controller.ReadDashInput() ? controller.DashSpeed : controller.WalkSpeed;

        controller.Move(input, speed);

        base.OnTick();
    }
}
