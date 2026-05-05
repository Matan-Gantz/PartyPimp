using UnityEngine;

public enum ItemType
{
    None,
    Drink,
    Food
}

public enum ItemState
{
    Held,
    Thrown,
    Idle
}

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
    [Header("Item Settings")]
    public ItemType itemType;
    public string itemName;

    private Rigidbody rb;
    private Collider itemCollider;
    private ItemState currentState = ItemState.Idle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        // Only auto-initialize if we aren't already being held by someone
        if (currentState == ItemState.Idle && transform.parent == null)
        {
            InitializeItem();
        }
    }

    public void InitializeItem()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // Don't force Idle if we were initialized into another state (like Held)
        if (currentState == ItemState.Idle)
        {
            SetState(ItemState.Idle);
        }
    }

    public void SetState(ItemState newState)
    {
        currentState = newState;
        // Debug.Log($"[Item] {itemName} state changed to: {newState}");

        switch (currentState)
        {
            case ItemState.Held:
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.detectCollisions = false; 
                rb.interpolation = RigidbodyInterpolation.None; 
                if (itemCollider != null) itemCollider.enabled = false;
                break;

            case ItemState.Thrown:
                if (transform.parent != null) transform.SetParent(null);
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.detectCollisions = true;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                if (itemCollider != null) itemCollider.enabled = true;
                break;

            case ItemState.Idle:
                if (transform.parent != null) transform.SetParent(null);
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.detectCollisions = true;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                if (itemCollider != null) itemCollider.enabled = true;
                break;
}
    }

    public void ApplyThrowForce(Vector3 force)
    {
        if (currentState != ItemState.Thrown) return;
        rb.AddForce(force, ForceMode.Impulse);
    }
}
