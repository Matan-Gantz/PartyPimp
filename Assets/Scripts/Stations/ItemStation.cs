using UnityEngine;

public class ItemStation : Station
{
    [Header("Item Production")]
    public GameObject itemPrefab;
    public Transform spawnPoint;

    public override void OnInteract(PlayerController player)
    {
        if (itemPrefab != null)
        {
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position + Vector3.up;
            GameObject spawnedObj = Instantiate(itemPrefab, pos, Quaternion.identity);
            Item item = spawnedObj.GetComponent<Item>();
            
            if (item != null)
            {
                item.InitializeItem(); // Step 3: Explicit reset on spawn
                player.PickupItem(item);
                Debug.Log($"Gave {item.itemName} to player.");
            }
        }
    }
}
