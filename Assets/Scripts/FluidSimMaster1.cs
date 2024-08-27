using UnityEngine;

public class FluidSimMaster1 : MonoBehaviour
{
    public int gridWidth = 100;
    public int gridHeight = 100;
    public int iters = 4;
    public float zOffset = 10f;
    public float radius = 5f;
    public float timescale = 1f;
    public float mouseScaling = 2f;
    public Vector2 testVector = Vector2.one;
    public FluidSimVisualiser1 simVisualiser;
    public ComputeShader fluidComputeShader;
    private RenderTexture velocityTexture;

    void OnEnable()
    {
        transform.position = new Vector3(gridWidth / 2f, gridHeight / 2f, -zOffset) + new Vector3(0.5f, 0.5f, 0f);

        Debug.Log("Setting velocity field...");

        // Initialize a 4-channel 2D texture (RGBA)
        velocityTexture = new RenderTexture(gridWidth, gridHeight, 0, RenderTextureFormat.ARGBFloat);
        velocityTexture.enableRandomWrite = true;
        velocityTexture.Create();

        // Set the texture on the compute shader
        fluidComputeShader.SetTexture(0, "velocityTexture", velocityTexture);
        fluidComputeShader.SetTexture(1, "velocityTexture", velocityTexture);
    }

    void Update()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Vector2 mouseAcceleration = GetMouseAcceleration();

        for (int i = 0; i < iters; i++)
        {
            // Dispatch the compute shader for the red-black Gauss-Seidel update
            DispatchRedBlack(mouseWorldPosition, mouseAcceleration);
        }        

        // Call the visualizer with the updated velocityTexture
        simVisualiser.VisualizeVelocityField(velocityTexture, gridWidth, gridHeight);
    }

    void DispatchRedBlack(Vector3 mouseWorldPosition, Vector2 mouseAcceleration)
    {
        // First, dispatch the red phase (kernel 0)
        fluidComputeShader.SetVector("mousePosition", new Vector4(mouseWorldPosition.x, mouseWorldPosition.y, 0, 0));
        fluidComputeShader.SetVector("mouseAcceleration", mouseAcceleration * mouseScaling);
        fluidComputeShader.SetFloat("radius", radius);
        fluidComputeShader.SetFloat("dt", Time.deltaTime * timescale);
        fluidComputeShader.Dispatch(0, Mathf.CeilToInt(gridWidth / 8f), Mathf.CeilToInt(gridHeight / 8f), 1);

        // Then, dispatch the black phase (kernel 1)
        fluidComputeShader.Dispatch(1, Mathf.CeilToInt(gridWidth / 8f), Mathf.CeilToInt(gridHeight / 8f), 1);
    }

    void OnDisable()
    {
        if (velocityTexture != null)
        {
            velocityTexture.Release();
        }
    }

    // Helper method to get mouse world position
    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = zOffset;  // Offset to match the plane where the fluid is
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }

    // Helper method to get mouse acceleration based on Input.GetAxis for "Mouse X" and "Mouse Y"
    Vector2 GetMouseAcceleration()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        return new Vector2(mouseX, mouseY);
    }
}
