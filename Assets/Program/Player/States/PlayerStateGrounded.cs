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
        **/
        if (!controller.IsGrounded)
        {
            StateMachine.SwitchState(PlayerState.Airborne);
            return;
        }

        /**
         * Looking
        **/
        controller.Turn(controller.ReadLookInput());

        /**
         * Moving
        **/
        bool isDashing = controller.ReadDashInput() && GameManager.Instance.ProjectData.enableDash;
        Vector2 input = controller.ReadMoveInput();
        float speed = 0F;

        if (input.sqrMagnitude > 0.0001f)
            speed = isDashing ? controller.DashSpeed : controller.WalkSpeed;

        controller.Move(input, speed);

        base.OnTick();
    }
}
