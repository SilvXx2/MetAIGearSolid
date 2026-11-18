// Agrega al nombre del estado el modo de navegación activo (Steering directo vs. Theta* sin línea de visión).
public static class StateLabelFormatter
{
    public static string WithNavigation(string label, IPathNavigator navigator)
    {
        return navigator != null && navigator.IsFollowingPath ? label + " · THETA*" : label;
    }
}
