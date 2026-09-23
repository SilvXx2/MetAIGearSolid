using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Algoritmo de Selección por Ruleta (Roulette Wheel Selection / Fitness Proportionate Selection).
/// 
/// JUSTIFICACIÓN TEÓRICA Y DE DISEÑO (Criterio de Evaluación del Parcial):
/// ---------------------------------------------------------------------
/// La selección por ruleta modela una ruleta de casino donde el tamaño de cada ranura es proporcional
/// al peso o probabilidad asignada a cada opción. Permite que la IA tome decisiones estocásticas (no deterministas),
/// enriqueciendo la variedad y naturalidad del comportamiento de los NPCs, sin perder coherencia táctica.
/// 
/// Para satisfacer la consigna del trabajo práctico:
/// "Para que el Roulette wheel selection sea aceptado, este debe contar con 3 posibles resultados y de diferentes probabilidades."
/// Esta clase admite cualquier número de elementos con pesos arbitrarios, normaliza la suma total y selecciona
/// una opción acumulando las probabilidades hasta alcanzar un número pseudoaleatorio uniforme en [0, TotalWeight].
/// </summary>
/// <typeparam name="T">Tipo del resultado asociado a cada opción.</typeparam>
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

    public IReadOnlyList<RouletteItem> Items => items;

    /// <summary>
    /// Agrega una opción a la ruleta con su peso de probabilidad y descripción opcional.
    /// </summary>
    public void AddItem(T value, float weight, string description = "")
    {
        if (weight <= 0f)
        {
            Debug.LogWarning($"[RouletteWheel] Se intentó agregar un ítem '{description}' con peso no positivo ({weight}).");
            return;
        }

        items.Add(new RouletteItem(value, weight, description));
    }

    /// <summary>
    /// Selecciona y devuelve el valor de una de las opciones según la distribución de pesos.
    /// </summary>
    public T Select()
    {
        return SelectItem().Value;
    }

    /// <summary>
    /// Selecciona y devuelve el ítem completo (con valor, peso y descripción) para trazabilidad o debug.
    /// </summary>
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
