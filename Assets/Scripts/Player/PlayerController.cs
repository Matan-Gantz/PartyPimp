using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Interaction Settings")]
    public Transform holdPoint;
    public float interactionRange = 2.5f;
    public float throwForce = 12f;
    public LayerMask interactableLayer;

    private Rigidbody rb;
    private Animator animator;
    private Vector2 moveInput;
    private Item heldItem;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        
        // LOCK physics rotation to prevent "orbiting"
rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Create a default hold point if not assigned
        if (holdPoint == null)
        {
            GameObject hp = new GameObject("HoldPoint");
            hp.transform.parent = transform;
            hp.transform.localPosition = new Vector3(0, 1.8f, 1.2f);
            holdPoint = hp.transform;
}
    }

    private void Update()
    {
        HandleInput();
    }

    public bool HasItem() => heldItem != null;
    public ItemType GetHeldItemType() => heldItem != null ? heldItem.itemType : ItemType.None;

    public void NotifyItemCollected()
    {
        if (heldItem != null)
        {
            // The item will be destroyed by the guest, we just clear the reference
            heldItem = null;
            Debug.Log("[Player] Item auto-collected by guest!");
        }
    }

    private void HandleInput()
{
        if (Keyboard.current == null) return;

        // Movement Input
        float moveX = 0;
        float moveZ = 0;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ = 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ = -1;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1;
        moveInput = new Vector2(moveX, moveZ).normalized;

        // Interact Input (Space)
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (heldItem != null)
                ThrowItem();
            else
                Interact();
        }

        // Throw Input (Left Click)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (heldItem != null)
                ThrowItem();
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);

        if (animator != null)
        {
            animator.SetFloat("Speed", moveInput.magnitude);
        }

        if (moveDirection.sqrMagnitude > 0.1f)
{
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void Interact()
    {
        // Check for interactables in front, lowered center to catch items on floor
        Vector3 checkCenter = transform.position + (transform.forward * 0.7f);
        checkCenter.y = 0.5f;
        
        Collider[] hitColliders = Physics.OverlapSphere(checkCenter, interactionRange);
        
        // Find closest interactable for better feel
        float closestDist = float.MaxValue;
        GameObject closestObj = null;

        foreach (var hit in hitColliders)
        {
            float dist = Vector3.Distance(checkCenter, hit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestObj = hit.gameObject;
            }
        }

        if (closestObj != null)
        {
            // Try Station
            Station station = closestObj.GetComponentInParent<Station>();
            if (station != null)
            {
                station.OnInteract(this);
                return;
            }

            // Try Item on floor
            Item item = closestObj.GetComponent<Item>();
            if (item != null && heldItem == null)
            {
                PickupItem(item);
                return;
            }
        }
    }

    public void PickupItem(Item item)
    {
        if (heldItem != null) return;

        heldItem = item;
        heldItem.SetState(ItemState.Held);
        
        // Parent item to player hold point
        heldItem.transform.SetParent(holdPoint);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;
        
        Debug.Log($"[Player] Picked up: {item.itemName}");
    }

    [Header("Aim Assist")]
    public float aimAssistAngle = 30f;
    public float aimAssistRange = 10f;

    private void ThrowItem()
    {
        if (heldItem == null) return;

        Item itemToThrow = heldItem;
        heldItem = null;
        
        itemToThrow.SetState(ItemState.Thrown);
        
        // Aim Assist Logic: Find the best guest to target
        Vector3 throwDir = transform.forward;
        GuestAI target = FindAimAssistTarget();
        
        if (target != null)
        {
            Vector3 toTarget = (target.transform.position - transform.position).normalized;
            throwDir = Vector3.Lerp(transform.forward, toTarget, 0.6f); // Nudge towards target
            Debug.Log($"[Aim Assist] Nudging throw towards {target.name}");
        }

        throwDir += Vector3.up * 0.2f; // Add slight arc
        itemToThrow.ApplyThrowForce(throwDir.normalized * throwForce);
        
        Debug.Log($"[Player] Threw item");
    }

    private GuestAI FindAimAssistTarget()
    {
        GuestAI[] guests = GameObject.FindObjectsByType<GuestAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        GuestAI bestTarget = null;
        float bestScore = float.MinValue;

        foreach (var guest in guests)
        {
            Vector3 toGuest = (guest.transform.position - transform.position);
            float distance = toGuest.magnitude;
            
            if (distance > aimAssistRange) continue;

            float angle = Vector3.Angle(transform.forward, toGuest.normalized);
            if (angle > aimAssistAngle) continue;

            // Score based on angle and distance (closer/centered is better)
            float score = (1f - (angle / aimAssistAngle)) + (1f - (distance / aimAssistRange));
            
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = guest;
            }
        }

        return bestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 checkCenter = transform.position + (transform.forward * 0.7f);
        checkCenter.y = 0.5f;
        Gizmos.DrawWireSphere(checkCenter, interactionRange);
    }
}
