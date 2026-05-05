using UnityEngine;

public abstract class Station : MonoBehaviour
{
    [Header("Station Info")]
    public string stationName;
    public Color highlightColor = Color.yellow;

    protected MeshRenderer meshRenderer;
    protected Color originalColor;

    protected virtual void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            originalColor = meshRenderer.sharedMaterial.color;
    }

    /// <summary>
    /// Called when the player interacts with this station.
    /// </summary>
    public abstract void OnInteract(PlayerController player);

    /// <summary>
    /// Visual feedback for when the player is looking at the station.
    /// </summary>
    public virtual void SetHighlight(bool highlight)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = highlight ? highlightColor : originalColor;
        }
    }
}
