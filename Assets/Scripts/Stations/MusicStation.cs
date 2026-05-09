using UnityEngine;
using System.Collections.Generic;

public class MusicStation : Station
{
    [Header("Music Station Settings")]
    public float attractionRadius = 5f;
    public int maxGuests = 15;
    public float attractionStrength = 2f;

    private List<GuestAI> attractedGuests = new List<GuestAI>();
    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake();
        // If meshRenderer was not found on root, look in children
        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null)
                originalColor = meshRenderer.sharedMaterial.color;
        }

        audioSource = GetComponent<AudioSource>();
    }

    public override void OnInteract(PlayerController player)
{
        Debug.Log("Interacted with Music Station.");
    }

    private void Update()
    {
        UpdateAttraction();
    }

    private void UpdateAttraction()
    {
        // 1. Check existing guests: remove if too far or urgent need
        for (int i = attractedGuests.Count - 1; i >= 0; i--)
        {
            GuestAI guest = attractedGuests[i];
            if (guest == null || Vector3.Distance(transform.position, guest.transform.position) > attractionRadius * 1.2f || guest.IsInUrgentNeed())
            {
                guest?.AssignToMusicStation(null);
                attractedGuests.RemoveAt(i);
            }
        }

        // 2. Look for new guests
        if (attractedGuests.Count < maxGuests)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, attractionRadius);
            foreach (var hit in hits)
            {
                if (attractedGuests.Count >= maxGuests) break;

                GuestAI guest = hit.GetComponent<GuestAI>();
                if (guest != null && !attractedGuests.Contains(guest) && !guest.IsInUrgentNeed())
                {
                    attractedGuests.Add(guest);
                    guest.AssignToMusicStation(this);
                    Debug.Log($"[MusicStation] {guest.name} attracted to the music!");
                }
}
        }
    }

    public void UnregisterGuest(GuestAI guest)
    {
        if (attractedGuests.Contains(guest))
        {
            attractedGuests.Remove(guest);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}
