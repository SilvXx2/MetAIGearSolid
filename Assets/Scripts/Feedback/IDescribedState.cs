// ISP: un estado puede describirse para el feedback sin que la FSM ni las vistas conozcan su tipo concreto.
public interface IDescribedState
{
    string StatusLabel { get; }
    NpcAlertLevel AlertLevel { get; }
}
