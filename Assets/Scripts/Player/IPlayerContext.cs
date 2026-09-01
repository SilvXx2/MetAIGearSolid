public interface IPlayerContext
{
    StateMachine StateMachine { get; }
    IMover Mover { get; }
    IPlayerInput Input { get; }
    float WalkSpeed { get; }
    float RunSpeed { get; }
    IState IdleState { get; }
    IState WalkingState { get; }
    IState RunningState { get; }
}
