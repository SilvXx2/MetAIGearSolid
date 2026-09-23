public interface IPauseService
{
    bool IsPaused { get; }

    void SetPaused(bool paused);
}
