using UnityEngine;

public class FluidSimMaster2 : MonoBehaviour
{
    public int gridWidth = 100;
    public int gridHeight = 100;
    public int iters = 4;
    public float zOffset = 10f;
    public float radius = 5f;
    public float timescale = 1f;
    public float mouseScaling = 2f;
    public float viscosity = 1f;
    public float diffusionCoeff = 1f;
    public Vector2 testVector = Vector2.one;
    public FluidSimVisualiser2 simVisualiser;
    public ComputeShader fluidComputeShader;

    private RenderTexture xVelTexture, xVelNextTexture;
    private RenderTexture yVelTexture, yVelNextTexture;
    private RenderTexture divergenceTexture, divergenceNextTexture;
    private RenderTexture pressureTexture, pressureNextTexture;
    private RenderTexture densityTexture, densityNextTexture;

    void OnEnable()
    {
        transform.position = new Vector3(gridWidth / 2f, gridHeight / 2f, -zOffset) + new Vector3(0.5f, 0.5f, 0f);

        Debug.Log("Setting up textures for fluid simulation...");

        // Initialize all the required textures
        xVelTexture = CreateTexture();
        xVelNextTexture = CreateTexture();
        yVelTexture = CreateTexture();
        yVelNextTexture = CreateTexture();
        divergenceTexture = CreateTexture();
        pressureTexture = CreateTexture();
        pressureNextTexture = CreateTexture();
        densityTexture = CreateTexture();
        densityNextTexture = CreateTexture();

        // Set textures on the compute shader
        SetTexturesOnShader();

        simVisualiser.Initialize(gridWidth, gridHeight);
    }

    void Update()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Vector2 mouseAcceleration = GetMouseAcceleration();

        for (int i = 0; i < iters; i++)
        {
            // Dispatch the compute shader for the red-black Gauss-Seidel update
            DispatchRedBlack(mouseWorldPosition, mouseAcceleration);

            // Swap the textures at the end of each iteration
            SwapTextures(ref xVelTexture, ref xVelNextTexture);
            SwapTextures(ref yVelTexture, ref yVelNextTexture);
            SwapTextures(ref pressureTexture, ref pressureNextTexture);
            SwapTextures(ref divergenceTexture, ref divergenceNextTexture);
            SwapTextures(ref densityTexture, ref densityNextTexture);
        }

        // Call the visualizer with one of the textures, e.g., x-velocity
        simVisualiser.VisualizeField(xVelTexture, yVelTexture, densityTexture, gridWidth, gridHeight);
    }

    void DispatchRedBlack(Vector3 mouseWorldPosition, Vector2 mouseAcceleration)
    {
        // First, dispatch the red phase (kernel 0)
        fluidComputeShader.SetVector("mousePosition", new Vector4(mouseWorldPosition.x, mouseWorldPosition.y, 0, 0));
        fluidComputeShader.SetVector("mouseAcceleration", mouseAcceleration * mouseScaling);
        fluidComputeShader.SetFloat("radius", radius);
        fluidComputeShader.SetFloat("dt", Time.deltaTime * timescale);
        fluidComputeShader.SetFloat("diffusionCoeff", diffusionCoeff);
        fluidComputeShader.SetFloat("viscosity", viscosity);

        // Dispatch both red and black phases with appropriate textures bound
        fluidComputeShader.Dispatch(0, Mathf.CeilToInt(gridWidth / 8f), Mathf.CeilToInt(gridHeight / 8f), 1);
        fluidComputeShader.Dispatch(1, Mathf.CeilToInt(gridWidth / 8f), Mathf.CeilToInt(gridHeight / 8f), 1);
    }

    // Helper to create textures
    RenderTexture CreateTexture()
    {
        RenderTexture texture = new RenderTexture(gridWidth, gridHeight, 0, RenderTextureFormat.RFloat);
        texture.enableRandomWrite = true;
        texture.Create();
        return texture;
    }

    // Helper to set textures on the shader
    void SetTexturesOnShader()
    {
        // Kernel 0 is the red phase
        fluidComputeShader.SetTexture(0, "xVelTexture", xVelTexture);
        fluidComputeShader.SetTexture(0, "xVelNextTexture", xVelNextTexture);
        fluidComputeShader.SetTexture(0, "yVelTexture", yVelTexture);
        fluidComputeShader.SetTexture(0, "yVelNextTexture", yVelNextTexture);
        fluidComputeShader.SetTexture(0, "divergenceTexture", divergenceTexture);
        fluidComputeShader.SetTexture(0, "pressureTexture", pressureTexture);
        fluidComputeShader.SetTexture(0, "pressureNextTexture", pressureNextTexture);
        fluidComputeShader.SetTexture(0, "densityTexture", densityTexture);
        fluidComputeShader.SetTexture(0, "densityNextTexture", densityNextTexture);

        // Kernel 1 is the black phase
        fluidComputeShader.SetTexture(1, "xVelTexture", xVelTexture);
        fluidComputeShader.SetTexture(1, "xVelNextTexture", xVelNextTexture);
        fluidComputeShader.SetTexture(1, "yVelTexture", yVelTexture);
        fluidComputeShader.SetTexture(1, "yVelNextTexture", yVelNextTexture);
        fluidComputeShader.SetTexture(1, "divergenceTexture", divergenceTexture);
        fluidComputeShader.SetTexture(1, "pressureTexture", pressureTexture);
        fluidComputeShader.SetTexture(1, "pressureNextTexture", pressureNextTexture);
        fluidComputeShader.SetTexture(1, "densityTexture", densityTexture);
        fluidComputeShader.SetTexture(1, "densityNextTexture", densityNextTexture);
    }

    // Swapping textures (ping-pong technique)
    void SwapTextures(ref RenderTexture tex1, ref RenderTexture tex2)
    {
        RenderTexture temp = tex1;
        tex1 = tex2;
        tex2 = temp;
    }

    void OnDisable()
    {
        // Release all textures
        ReleaseTexture(xVelTexture);
        ReleaseTexture(xVelNextTexture);
        ReleaseTexture(yVelTexture);
        ReleaseTexture(yVelNextTexture);
        ReleaseTexture(divergenceTexture);
        ReleaseTexture(pressureTexture);
        ReleaseTexture(pressureNextTexture);
        ReleaseTexture(densityTexture);
        ReleaseTexture(densityNextTexture);
    }

    // Helper method to release textures
    void ReleaseTexture(RenderTexture texture)
    {
        if (texture != null)
        {
            texture.Release();
        }
    }

    // Helper method to get mouse world position
    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = zOffset; // Offset to match the plane where the fluid is
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