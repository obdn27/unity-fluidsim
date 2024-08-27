using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FluidSimUIController : MonoBehaviour
{
    public FluidSimMaster3 fluidSim; // Reference to the main fluid simulation script

    // UI elements for controlling simulation parameters
    public Slider radiusSlider;
    public Slider mouseScalingSlider;
    public Slider timescaleSlider;
    public Slider dampingSlider;
    public Slider viscositySlider;
    public InputField iterationsInputField;
    public InputField xResField;
    public InputField yResField;

    void Start()
    {
        // Initialize sliders and input fields with default values from FluidSimMaster3
        radiusSlider.value = fluidSim.radius;
        mouseScalingSlider.value = fluidSim.mouseScaling;
        timescaleSlider.value = fluidSim.timescale;
        dampingSlider.value = fluidSim.damping;
        iterationsInputField.text = fluidSim.iters.ToString();
        viscositySlider.value = fluidSim.viscosity;
        xResField.text = fluidSim.gridWidth.ToString();
        yResField.text = fluidSim.gridHeight.ToString();

        // Add listeners for value changes
        radiusSlider.onValueChanged.AddListener(OnRadiusChanged);
        mouseScalingSlider.onValueChanged.AddListener(OnMouseScalingChanged);
        timescaleSlider.onValueChanged.AddListener(OnTimescaleChanged);
        dampingSlider.onValueChanged.AddListener(OnDampingChanged);
        viscositySlider.onValueChanged.AddListener(OnViscosityChanged);
        iterationsInputField.onEndEdit.AddListener(OnIterationsChanged);
        xResField.onEndEdit.AddListener(OnWidthChanged);
        yResField.onEndEdit.AddListener(OnHeightChanged);
    }

    void OnRadiusChanged(float value)
    {
        fluidSim.radius = value;
    }

    void OnMouseScalingChanged(float value)
    {
        fluidSim.mouseScaling = value;
    }

    void OnTimescaleChanged(float value)
    {
        fluidSim.timescale = value;
    }

    void OnDampingChanged(float value)
    {
        fluidSim.damping = value;
    }

    void OnViscosityChanged(float value)
    {
        fluidSim.viscosity = value;
    }

    void OnIterationsChanged(string value)
    {
        if (int.TryParse(value, out int iterations))
        {
            fluidSim.iters = iterations;
        }
    }
    void OnWidthChanged(string value)
    {
        if (int.TryParse(value, out int width))
        {
            fluidSim.gridWidth = width;
        }
    }

    void OnHeightChanged(string value)
    {
        if (int.TryParse(value, out int height))
        {
            fluidSim.gridHeight = height;
        }
    }
}