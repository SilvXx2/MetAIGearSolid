using System;
using System.Collections.Generic;

public interface IKeyInventoryObservable
{
    IReadOnlyCollection<KeyDefinition> Keys { get; }

    event Action<KeyDefinition> KeyAdded;
}
