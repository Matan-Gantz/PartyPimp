using UnityEngine;

public class BathroomStation : Station
{
    public override void OnInteract(PlayerController player)
    {
        Debug.Log("Interacted with Bathroom. Player used it? (No effect currently)");
    }

    private void OnTriggerEnter(Collider other)
    {
        GuestAI guest = other.GetComponent<GuestAI>();
        if (guest != null)
        {
            Debug.Log("Guest arrived at Bathroom.");
            guest.FulfillToiletNeed();
        }
    }
}
