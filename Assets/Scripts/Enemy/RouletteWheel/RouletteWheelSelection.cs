using System;
using System.Collections.Generic;
using UnityEngine;

public class RouletteWheelSelection<T>
{
    public class RouletteItem
    {
        public T Value { get; }
        public float Weight { get; }
        public string Description { get; }

        public RouletteItem(T value, float weight, string description = "")
        {
            Value = value;
            Weight = Mathf.Max(0f, weight);
            Description = description;
        }
    }

    private readonly List<RouletteItem> items = new List<RouletteItem>();

    public void AddItem(T value, float weight, string description = "")
    {
        if (weight <= 0f)
        {
            Debug.LogWarning($"[RouletteWheel] Se intentó agregar un ítem '{description}' con peso no positivo ({weight}).");
            return;
        }

        items.Add(new RouletteItem(value, weight, description));
    }

    public RouletteItem SelectItem()
    {
        if (items.Count == 0)
        {
            throw new InvalidOperationException("[RouletteWheel] No hay opciones configuradas en la ruleta.");
        }

        float totalWeight = 0f;
        for (int i = 0; i < items.Count; i++)
        {
            totalWeight += items[i].Weight;
        }

        if (totalWeight <= 0.0001f)
        {
            return items[0];
        }

        float randomPoint = UnityEngine.Random.value * totalWeight;
        float cumulative = 0f;

        for (int i = 0; i < items.Count; i++)
        {
            cumulative += items[i].Weight;
            if (randomPoint <= cumulative)
            {
                return items[i];
            }
        }

        return items[items.Count - 1];
    }
}
