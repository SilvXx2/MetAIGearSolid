using System;
using System.Collections.Generic;
using UnityEngine;

public class RouletteWheelSelection<T>
{
    public class RouletteItem
    {
        private readonly float staticWeight;
        private readonly Func<float> dynamicWeightProvider;

        //LOS ITEMS NO SE PUEDEN MODIFICAR UNA VEZ EN LA LISTA, POR ESO SON SOLO GETTERS
        public T Value { get; }
        public float Weight => dynamicWeightProvider != null
            ? Mathf.Max(0f, dynamicWeightProvider.Invoke())
            : staticWeight;
        public string Description { get; }

        public RouletteItem(T value, float weight, string description = "")
        {
            Value = value;
            //RECORTAR LOS PESOS NEGATIVOS Y NO DEJA QUE SEA 0
            staticWeight = Mathf.Max(0f, weight);
            dynamicWeightProvider = null;
            Description = description;
        }

        public RouletteItem(T value, Func<float> weightProvider, string description = "")
        {
            Value = value;
            staticWeight = 0f;
            dynamicWeightProvider = weightProvider;
            Description = description;
        }
    }

    private readonly List<RouletteItem> items = new List<RouletteItem>();

    public void AddItem(T value, float weight, string description = "")
    {
        //SI ES MENOR O IGUAL A 0, NO SE AÑADE
        if (weight <= 0f)
        {
            return;
        }

        items.Add(new RouletteItem(value, weight, description));
    }

    public void AddItem(T value, Func<float> weightProvider, string description = "")
    {
        if (weightProvider == null)
        {
            return;
        }

        items.Add(new RouletteItem(value, weightProvider, description));
    }

    public RouletteItem SelectItem()
    {
        //EVITAR CRASH POR RULETA VACIA
        if (items.Count == 0)
        {
            throw new InvalidOperationException("[RouletteWheel] No hay opciones configuradas en la ruleta.");
        }

        //SUMAR TODOS LOS PESOS
        float totalWeight = 0f;
        for (int i = 0; i < items.Count; i++)
        {
            totalWeight += items[i].Weight;
        }

        //SI EL PESO TOTAL ES CERO, NO HACEMOS NADA
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
        //NOS ASEGURAMOS DE QUE NO HAYA UN ERROR DE REDONDEO
        return items[items.Count - 1];
    }
}
