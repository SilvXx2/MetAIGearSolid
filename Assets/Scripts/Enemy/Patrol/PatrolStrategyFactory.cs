public static class PatrolStrategyFactory
{
    public static IPatrolRouteStrategy Create(PatrolRouteMode mode, int pauseEveryNWaypoints = 2)
    {
        switch (mode)
        {
            case PatrolRouteMode.Loop:
                return new LoopPatrolStrategy(pauseEveryNWaypoints);

            case PatrolRouteMode.RandomWeighted:
                return new RandomWeightedPatrolStrategy(pauseEveryNWaypoints);

            case PatrolRouteMode.PingPong:
            default:
                return new PingPongPatrolStrategy();
        }
    }
}
