using UnityEngine;

public class PlayerRunningState : IState
{
    private readonly IPlayerContext context;

    public PlayerRunningState(IPlayerContext context)
    {
        this.context = context;
    }

    public void Enter() { }

    public void Update()
    {
        Vector3 input = context.Input.GetMovementInput();

        if (input == Vector3.zero)
        {
            context.StateMachine.ChangeState(context.IdleState);
            return;
        }

        if (!context.Input.IsRunPressed())
        {
            context.StateMachine.ChangeState(context.WalkingState);
            return;
        }

        context.Mover.Move(input, context.RunSpeed);
        context.Mover.Rotate(input);
    }

    public void Exit() { }
}
