using UnityEngine;

public class PlayerWalkingState : IState
{
    private readonly IPlayerContext context;

    public PlayerWalkingState(IPlayerContext context)
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

        if (context.Input.IsRunPressed())
        {
            context.StateMachine.ChangeState(context.RunningState);
            return;
        }

        context.Mover.Move(input, context.WalkSpeed);
        context.Mover.Rotate(input);
    }

    public void Exit() { }
}
