using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public float cellSize = 1f;
    public Vector2Int gridSize = new Vector2Int(20, 20);
    public Color gridColor = Color.white;

    private void OnDrawGizmos()
    {
        Gizmos.color = gridColor;
        
        // Calculate start position to center the grid on the object's position
        Vector3 origin = transform.position;
        float width = gridSize.x * cellSize;
        float height = gridSize.y * cellSize;

        // Draw horizontal lines
        for (int i = 0; i <= gridSize.y; i++)
        {
            float z = origin.z + (i * cellSize);
            Gizmos.DrawLine(new Vector3(origin.x, origin.y, z), new Vector3(origin.x + width, origin.y, z));
        }

        // Draw vertical lines
        for (int i = 0; i <= gridSize.x; i++)
        {
            float x = origin.x + (i * cellSize);
            Gizmos.DrawLine(new Vector3(x, origin.y, origin.z), new Vector3(x, origin.y, origin.z + height));
        }
    }

    /// <summary>
    /// Converts a world position to grid coordinates.
    /// </summary>
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - transform.position.x) / cellSize);
        int z = Mathf.FloorToInt((worldPos.z - transform.position.z) / cellSize);
        return new Vector2Int(x, z);
    }

    /// <summary>
    /// Converts grid coordinates to the center of that cell in world space.
    /// </summary>
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float x = transform.position.x + (gridPos.x * cellSize) + (cellSize * 0.5f);
        float z = transform.position.z + (gridPos.y * cellSize) + (cellSize * 0.5f);
        return new Vector3(x, transform.position.y, z);
    }

    /// <summary>
    /// Snaps a world position to the center of the nearest grid cell.
    /// </summary>
    public Vector3 GetSnappedPosition(Vector3 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);
        return GridToWorld(gridPos);
    }
}
