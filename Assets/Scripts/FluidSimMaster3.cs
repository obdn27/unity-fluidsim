using UnityEngine;

public class FluidSimMaster3 : MonoBehaviour
{
    public int gridWidth = 100;
    public int gridHeight = 100;
    public int iters = 20;
    public float radius = 5f;
    public float mouseScaling = 2f;
    public float timescale = 1f;
    public ComputeShader fluidComputeShader;
    public float zOffset = 10f;
    public float damping = 0.98f;
    public float viscosity = 1f;
    public float diffusionCoeff = 1f;

    public FluidSimVisualiser2 visualiser;

    public RenderTexture xVelTexture, yVelTexture;
    public RenderTexture pressureTexture, divergenceTexture;
    public RenderTexture densityTexture;

    private bool stopDecay = false;
    private Vector2 mousePosition;
    private float actualDamping;

    void OnEnable()
    {
        // Initialize the required textures
        xVelTexture = CreateTexture();
        yVelTexture = CreateTexture();
        pressureTexture = CreateTexture();
        divergenceTexture = CreateTexture();
        densityTexture = CreateTexture();

        // Set textures on the compute shader
        SetTexturesOnShader();
        visualiser.Initialize(gridWidth, gridHeight);
    }

    void Update()
    {
        // Check for spacebar press to reset
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetTextures();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            stopDecay = !stopDecay;
        }

        if (stopDecay)
        {
            actualDamping = 1.0f;
        } else
        {
            actualDamping = damping;
        }

        mousePosition = GetMousePositionInGrid();
        Vector2 mouseAcceleration = GetMouseAcceleration();

        // Dispatch the AddMouseVelocityAndAdvection kernel
        DispatchMouseVelocityAdvection(mousePosition, mouseAcceleration);

        // Run the Diffuse kernel iteratively
        for (int i = 0; i < iters; i++)
        {
            fluidComputeShader.Dispatch(1, Mathf.CeilToInt(gridWidth / 8f), Mathf.CeilToInt(gridHeight / 8f), 1); // Diffuse kernel
        }

        // Compute Divergence after diffusion
        fluidComputeShader.Dispatch(2, Mathf.CeilToInt(gridWidth / 8f), Mathf.CeilToInt(gridHeight / 8f), 1); // ComputeDivergence kernel

        // Run the SolvePressure kernel iteratively
        for (int i = 0; i < iters; i++)
        {
            fluidComputeShader.Dispatch(3, Mathf.CeilToInt(gridWidth / 8f), Mathf.CeilToInt(gridHeight / 8f), 1); // SolvePressure kernel
        }

        // Dispatch the ApplyCorrection kernel to correct the velocities based on pressure
        fluidComputeShader.Dispatch(4, Mathf.CeilToInt(gridWidth / 8f), Mathf.CeilToInt(gridHeight / 8f), 1); // ApplyCorrection kernel

        // Visualize the velocity field after all operations
        visualiser.VisualizeField(xVelTexture, yVelTexture, densityTexture, gridWidth, gridHeight);
    }

    void DispatchMouseVelocityAdvection(Vector2 mousePosition, Vector2 mouseAcceleration)
    {
        fluidComputeShader.SetVector("mousePosition", new Vector4(mousePosition.x, mousePosition.y, 0, 0));
        fluidComputeShader.SetVector("mouseAcceleration", mouseAcceleration * mouseScaling);
        fluidComputeShader.SetFloat("dt", Time.deltaTime * timescale);
        fluidComputeShader.SetFloat("radius", radius);
        fluidComputeShader.SetFloat("damping", actualDamping);
        fluidComputeShader.SetFloat("densityDamping", damping);
        fluidComputeShader.SetFloat("viscosity", viscosity);
        fluidComputeShader.SetFloat("diffusionCoeff", diffusionCoeff);
        fluidComputeShader.SetInts("res", new int[] { gridWidth, gridHeight });
        fluidComputeShader.Dispatch(0, Mathf.CeilToInt(gridWidth / 8f), Mathf.CeilToInt(gridHeight / 8f), 1);
    }

    RenderTexture CreateTexture()
    {
        RenderTexture texture = new RenderTexture(gridWidth, gridHeight, 0, RenderTextureFormat.RFloat);
        texture.enableRandomWrite = true;
        texture.filterMode = FilterMode.Point;
        texture.Create();
        return texture;
    }

    void SetTexturesOnShader()
    {
        // Kernel 0: AddMouseVelocityAndAdvection
        fluidComputeShader.SetTexture(0, "xVelTexture", xVelTexture);
        fluidComputeShader.SetTexture(0, "yVelTexture", yVelTexture);
        fluidComputeShader.SetTexture(0, "divergenceTexture", divergenceTexture);
        fluidComputeShader.SetTexture(0, "pressureTexture", pressureTexture);
        fluidComputeShader.SetTexture(0, "densityTexture", densityTexture);

        // Kernel 1: Diffuse
        fluidComputeShader.SetTexture(1, "xVelTexture", xVelTexture);
        fluidComputeShader.SetTexture(1, "yVelTexture", yVelTexture);
        fluidComputeShader.SetTexture(1, "densityTexture", densityTexture);

        // Kernel 2: ComputeDivergence
        fluidComputeShader.SetTexture(2, "xVelTexture", xVelTexture);
        fluidComputeShader.SetTexture(2, "yVelTexture", yVelTexture);
        fluidComputeShader.SetTexture(2, "divergenceTexture", divergenceTexture);
        fluidComputeShader.SetTexture(2, "pressureTexture", pressureTexture);

        // Kernel 3: SolvePressure
        fluidComputeShader.SetTexture(3, "pressureTexture", pressureTexture);
        fluidComputeShader.SetTexture(3, "divergenceTexture", divergenceTexture);

        // Kernel 4: ApplyCorrection
        fluidComputeShader.SetTexture(4, "xVelTexture", xVelTexture);
        fluidComputeShader.SetTexture(4, "yVelTexture", yVelTexture);
        fluidComputeShader.SetTexture(4, "pressureTexture", pressureTexture);
        fluidComputeShader.SetTexture(4, "densityTexture", densityTexture);
    }


    void ResetTextures()
    {
        // Clear all textures by releasing and recreating them
        ReleaseTexture(xVelTexture);
        ReleaseTexture(yVelTexture);
        ReleaseTexture(pressureTexture);
        ReleaseTexture(divergenceTexture);
        ReleaseTexture(densityTexture);

        xVelTexture = CreateTexture();
        yVelTexture = CreateTexture();
        pressureTexture = CreateTexture();
        divergenceTexture = CreateTexture();
        densityTexture = CreateTexture();

        SetTexturesOnShader();
    }

    void OnDisable()
    {
        // Release all textures
        ReleaseTexture(xVelTexture);
        ReleaseTexture(yVelTexture);
        ReleaseTexture(pressureTexture);
        ReleaseTexture(divergenceTexture);
        ReleaseTexture(densityTexture);
    }

    void ReleaseTexture(RenderTexture texture)
    {
        if (texture != null)
        {
            texture.Release();
        }
    }

    Vector2 GetMousePositionInGrid()
    {
        Vector3 mousePosition = Input.mousePosition;

        return new Vector2((mousePosition.x / Screen.width) * gridWidth, (mousePosition.y / Screen.height) * gridHeight);
    }

    Vector2 GetMouseAcceleration()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        return new Vector2(mouseX, mouseY);
    }
}