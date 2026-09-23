using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class KeyHudCanvasView : MonoBehaviour, IKeyHudView
{
    private readonly List<Image> slots = new List<Image>();

    public void Render(IReadOnlyCollection<KeyDefinition> keys)
    {
        int used = 0;

        foreach (KeyDefinition key in keys)
        {
            if (key == null) continue;

            Image slot = GetOrCreateSlot(used);
            slot.sprite = key.Icon;
            slot.color = key.Color;
            slot.gameObject.SetActive(true);
            used++;
        }

        for (int i = used; i < slots.Count; i++)
        {
            slots[i].gameObject.SetActive(false);
        }
    }

    private Image GetOrCreateSlot(int index)
    {
        if (index < slots.Count) return slots[index];

        var slotObject = new GameObject("KeySlot", typeof(RectTransform), typeof(Image));
        slotObject.layer = gameObject.layer;
        slotObject.transform.SetParent(transform, false);

        var image = slotObject.GetComponent<Image>();
        image.raycastTarget = false;

        slots.Add(image);
        return image;
    }
}
