using System.Collections.Generic;

public interface IKeyHudView
{
    void Render(IReadOnlyCollection<KeyDefinition> keys);
}
