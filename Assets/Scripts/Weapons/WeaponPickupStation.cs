using System.Collections.Generic;
using UnityEngine;

public class WeaponPickupStation : MonoBehaviour
{
    private static readonly List<WeaponPickupStation> ActiveStations = new List<WeaponPickupStation>();

    [Header("Configuración de Armero / Pickup")]
    [SerializeField] private float pickupRadius = 1.6f;
    [SerializeField] private bool unlimitedSupply = true;
    [SerializeField] private float respawnCooldown = 8f;
    [SerializeField] private GameObject stationWeaponVisual;
    [SerializeField] private bool rotateVisual = true;
    [SerializeField] private float rotationSpeed = 65f;

    private bool isAvailable = true;
    private float respawnTimer;

    public bool IsAvailable => isAvailable;
    public float PickupRadius => Mathf.Max(0.5f, pickupRadius);
    public Vector3 Position => transform.position;

    private void OnEnable()
    {
        if (!ActiveStations.Contains(this))
        {
            ActiveStations.Add(this);
        }
        UpdateVisual();
    }

    private void OnDisable()
    {
        ActiveStations.Remove(this);
    }

    private void Update()
    {
        if (!isAvailable && !unlimitedSupply)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f)
            {
                isAvailable = true;
                UpdateVisual();
            }
        }

        if (isAvailable && rotateVisual && stationWeaponVisual != null)
        {
            stationWeaponVisual.transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
        }
    }

    public bool TryPickup(IWeaponHolder holder)
    {
        if (holder == null || !isAvailable) return false;

        holder.EquipWeapon();

        if (!unlimitedSupply)
        {
            isAvailable = false;
            respawnTimer = Mathf.Max(0.5f, respawnCooldown);
            UpdateVisual();
        }

        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isAvailable || other == null) return;

        var holder = other.GetComponentInParent<IWeaponHolder>();
        if (holder != null && !holder.HasWeapon)
        {
            TryPickup(holder);
        }
    }

    private void UpdateVisual()
    {
        if (stationWeaponVisual != null)
        {
            stationWeaponVisual.SetActive(isAvailable);
        }
    }

    public static WeaponPickupStation FindClosestAvailable(Vector3 fromPosition)
    {
        WeaponPickupStation closest = null;
        float minDistSqr = float.MaxValue;

        for (int i = 0; i < ActiveStations.Count; i++)
        {
            WeaponPickupStation station = ActiveStations[i];
            if (station == null || !station.IsAvailable) continue;

            Vector3 diff = station.transform.position - fromPosition;
            diff.y = 0f;
            float distSqr = diff.sqrMagnitude;

            if (distSqr < minDistSqr)
            {
                minDistSqr = distSqr;
                closest = station;
            }
        }

        return closest;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isAvailable ? new Color(0.2f, 0.75f, 1f, 0.7f) : new Color(0.5f, 0.5f, 0.5f, 0.4f);
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.4f, new Vector3(0.9f, 0.8f, 0.9f));
        Gizmos.DrawWireSphere(transform.position, PickupRadius);
    }
}
