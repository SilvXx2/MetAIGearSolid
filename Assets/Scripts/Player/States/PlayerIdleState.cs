using UnityEngine;

public class PlayerIdleState : IState
{
    private readonly IPlayerContext context;

    public PlayerIdleState(IPlayerContext context)
    {
        this.context = context;
    }

    public void Enter() { }

    public void Update()
    {
        Vector3 input = context.Input.GetMovementInput();

        if (input != Vector3.zero)
        {
            if (context.Input.IsRunPressed())
            {
                context.StateMachine.ChangeState(context.RunningState);
            }
            else
            {
                context.StateMachine.ChangeState(context.WalkingState);
            }
        }
    }

    public void Exit() { }
}
