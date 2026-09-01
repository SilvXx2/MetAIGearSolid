using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour, IPlayerContext, IMover, IPlayerInput
{
    [Header("Velocidades")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 9f;

    [Header("Tecla para Correr")]
    [SerializeField] private Key runKey = Key.LeftShift;

    public StateMachine StateMachine { get; private set; }
    public IMover Mover => this;
    public IPlayerInput Input => this;
    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public IState IdleState { get; private set; }
    public IState WalkingState { get; private set; }
    public IState RunningState { get; private set; }

    private void Awake()
    {
        StateMachine = new StateMachine();
        IdleState = new PlayerIdleState(this);
        WalkingState = new PlayerWalkingState(this);
        RunningState = new PlayerRunningState(this);
    }

    private void Start()
    {
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        StateMachine.Update();
    }

    public void Move(Vector3 direction, float speed)
    {
        transform.position += direction * (speed * Time.deltaTime);
    }

    public void Rotate(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }
    }

    public Vector3 GetMovementInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return Vector3.zero;

        float x = 0f;
        float z = 0f;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) z += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) z -= 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;

        return new Vector3(x, 0f, z).normalized;
    }

    public bool IsRunPressed()
    {
        var keyboard = Keyboard.current;
        return keyboard != null && keyboard[runKey].isPressed;
    }
}
