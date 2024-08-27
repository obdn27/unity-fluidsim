using System.Collections.Generic;
using UnityEngine;

public class FluidSimVisualiser2 : MonoBehaviour
{
    public Mesh arrowMesh; // The arrow mesh
    public Material arrowMaterial; // The material for the arrows
    public MeshRenderer densityRenderer;
    public Material densityMaterial;
    public float arrowScale = 1f; // Scale for the arrows
    public float arrowSpacing = 1f;
    public float minScale = .1f;
    public float maxScale = .5f;
    public bool arrows = false;

    private Texture2D xVelReadTexture;
    private Texture2D yVelReadTexture;

    public void Initialize(int gridWidth, int gridHeight)
    {
        xVelReadTexture = new Texture2D(gridWidth, gridHeight, TextureFormat.RFloat, false);
        yVelReadTexture = new Texture2D(gridWidth, gridHeight, TextureFormat.RFloat, false);
    }

    public void VisualizeField (RenderTexture xVelTexture, RenderTexture yVelTexture, RenderTexture densityTexture, int gridWidth, int gridHeight)
    {
        if (arrows)
        {
            RenderTexture.active = xVelTexture;
            xVelReadTexture.ReadPixels(new Rect(0, 0, gridWidth, gridHeight), 0, 0);
            xVelReadTexture.Apply();

            RenderTexture.active = yVelTexture;
            yVelReadTexture.ReadPixels(new Rect(0, 0, gridWidth, gridHeight), 0, 0);
            yVelReadTexture.Apply();
            RenderTexture.active = null;

            List<Matrix4x4> batches = new List<Matrix4x4>();

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    float xVel = xVelReadTexture.GetPixel(x, y).r;
                    float yVel = yVelReadTexture.GetPixel(x, y).r;

                    Vector2 velocity = new Vector2(xVel, yVel);
                    if (velocity.magnitude < 1e-4f) continue;

                    Matrix4x4 currentMatrix = Matrix4x4.TRS(
                        new Vector3(x * arrowSpacing, y * arrowSpacing, 0),
                        Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg),
                        Vector3.one * Mathf.Clamp(arrowScale * velocity.magnitude, minScale, maxScale)
                    );

                    batches.Add(currentMatrix);
                }
            }

            RenderBatches(batches);
        }        

        VisualizeDensityField(densityTexture);
    }

    public void VisualizeDensityField(RenderTexture densityTexture)
    {
        // Ensure the density material is correctly set up with the density texture
        if (densityMaterial != null)
        {
            densityMaterial.SetTexture("_DensityTex", densityTexture);
        }
    }

    private void RenderBatches(List<Matrix4x4> batches)
    {
        Graphics.DrawMeshInstanced(arrowMesh, 0, arrowMaterial, batches.ToArray());
    }
}

