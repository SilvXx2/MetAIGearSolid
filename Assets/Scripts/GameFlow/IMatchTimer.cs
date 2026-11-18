// Lectura del tiempo de partida para las vistas (DIP: el HUD no conoce al MatchTimer concreto).
public interface IMatchTimer
{
    float Duration { get; }
    float RemainingSeconds { get; }
    bool IsRunning { get; }
}
