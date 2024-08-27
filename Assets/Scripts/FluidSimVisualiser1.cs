using System.Collections.Generic;
using UnityEngine;

public class FluidSimVisualiser1 : MonoBehaviour
{
    public Mesh arrowMesh; // The arrow mesh
    public Material arrowMaterial; // The material for the arrows
    public float arrowScale = 1f; // Scale for the arrows
    public float arrowSpacing = 1f;
    public float minScale = .1f;
    public float maxScale = .5f;

    private const int maxBatchSize = 1023; // Maximum number of instances per batch for Unity's instancing

    List<List<Matrix4x4>> batches = new List<List<Matrix4x4>>();
    private Texture2D velocityReadTexture;

    // Initialize the texture we use to read velocity values from the GPU
    void Start()
    {
        velocityReadTexture = new Texture2D(100, 100, TextureFormat.RGBAFloat, false);
    }

    // Call this function from FluidSimMaster1 after the compute shader has finished
    public void VisualizeVelocityField(RenderTexture velocityTexture, int gridWidth, int gridHeight)
    {
        // Copy the data from the GPU render texture to a readable Texture2D
        RenderTexture.active = velocityTexture;
        velocityReadTexture.ReadPixels(new Rect(0, 0, gridWidth, gridHeight), 0, 0);
        velocityReadTexture.Apply();
        RenderTexture.active = null;

        List<Matrix4x4> batch = new List<Matrix4x4>();

        for (int i = 0; i < gridWidth * gridHeight; i++)
        {
            // Calculate the x and y positions based on the index
            int x = i % gridWidth;
            int y = i / gridWidth;

            // Get the velocity from the texture
            Color velocityColor = velocityReadTexture.GetPixel(x, y);
            Vector2 velocity = new Vector2(velocityColor.r, velocityColor.g); // R and G channels store velocity

            if (velocity.magnitude < 1e-4f) continue;

            // Build the transformation matrix for each arrow
            Matrix4x4 currentMatrix = Matrix4x4.TRS(
                new Vector3(x * arrowSpacing, y * arrowSpacing, 0),
                Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg),
                Vector3.one * Mathf.Clamp(arrowScale * velocity.magnitude, minScale, maxScale)
            );

            batch.Add(currentMatrix);

            // If batch limit is reached, start a new batch
            if (batch.Count == maxBatchSize)
            {
                batches.Add(batch);
                batch = new List<Matrix4x4>();
            }
        }

        // Add any remaining batch
        if (batch.Count > 0)
        {
            batches.Add(batch);
        }

        RenderBatches();
    }

    private void RenderBatches()
    {
        foreach (List<Matrix4x4> batch in batches)
        {
            Graphics.DrawMeshInstanced(arrowMesh, 0, arrowMaterial, batch);
        }
        batches.Clear();
    }
}
