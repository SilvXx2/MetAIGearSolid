using System;
using System.Collections.Generic;
using UnityEngine;

public class RouletteWheelSelection<T>
{
    public class RouletteItem
    {
        //LOS ITEMS NO SE PUEDEN MODIFICAR UNA VEZ EN LA LISTA, POR ESO SON SOLO GETTERS
        public T Value { get; }
        public float Weight { get; }
        public string Description { get; }

        public RouletteItem(T value, float weight, string description = "")
        {
            Value = value;
            //RECORTAR LOS PESOS NEGATIVOS Y NO DEJA QUE SEA 0
            Weight = Mathf.Max(0f, weight);
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
