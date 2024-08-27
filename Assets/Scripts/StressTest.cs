using UnityEngine;

public class StressTest : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab; // The prefab to spawn
    [SerializeField]
    private int width = 100; // Number of objects in the width of the grid
    [SerializeField]
    private int height = 100; // Number of objects in the height of the grid
    [SerializeField]
    private float spacing = 1.0f; // Spacing between each object
    [SerializeField]
    private Material material; // Material to apply to each spawned prefab

    // Start is called before the first frame update
    void Start()
    {
        SpawnGrid();
    }

    void SpawnGrid()
    {
        // Ensure prefab and material are assigned
        if (prefab == null || material == null)
        {
            Debug.LogError("Prefab or Material not assigned.");
            return;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Calculate the position based on grid indices and spacing
                Vector3 position = new Vector3(x * spacing, 0, y * spacing);

                // Instantiate the prefab at the calculated position
                GameObject obj = Instantiate(prefab, position, Quaternion.identity);

                // Assign the material to the object (if it has a renderer)
                Renderer objRenderer = obj.GetComponent<Renderer>();
                if (objRenderer != null)
                {
                    objRenderer.material = material;
                }

                // Optional: Set the object as a child of this GameObject for better organization
                obj.transform.parent = this.transform;
            }
        }
    }
}