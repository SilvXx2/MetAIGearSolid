using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyInventory : MonoBehaviour, IKeyInventoryReader, IKeyInventoryWriter, IKeyInventoryObservable
{
    [Header("Llaves Iniciales (opcional)")]
    [SerializeField] private List<KeyDefinition> startingKeys = new List<KeyDefinition>();

    private readonly HashSet<KeyDefinition> keys = new HashSet<KeyDefinition>();

    public event Action<KeyDefinition> KeyAdded;

    public IReadOnlyCollection<KeyDefinition> Keys => keys;

    private void Awake()
    {
        foreach (KeyDefinition key in startingKeys)
        {
            AddKey(key);
        }
    }

    public bool HasKey(KeyDefinition key)
    {
        return key != null && keys.Contains(key);
    }

    public bool AddKey(KeyDefinition key)
    {
        if (key == null || !keys.Add(key)) return false;

        KeyAdded?.Invoke(key);
        return true;
    }
}
