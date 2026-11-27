public readonly struct PatrolStepResult
{
    public readonly int NextIndex;
    public readonly bool ShouldPauseForIdle;

    public PatrolStepResult(int nextIndex, bool shouldPauseForIdle)
    {
        NextIndex = nextIndex;
        ShouldPauseForIdle = shouldPauseForIdle;
    }
}
