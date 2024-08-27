using UnityEngine;
using UnityEngine.UI;

public class TextureDebugger : MonoBehaviour
{
    public RawImage xVelImage;
    public RawImage yVelImage;
    public RawImage pressureImage;
    public RawImage divergenceImage;
    public RawImage densityImage;

    public FluidSimMaster3 fluidSimMaster; // Reference to your fluid simulation

    void Start()
    {
        // Assign textures from the fluid simulation to the UI RawImages
        xVelImage.texture = fluidSimMaster.xVelTexture;
        yVelImage.texture = fluidSimMaster.yVelTexture;
        pressureImage.texture = fluidSimMaster.pressureTexture;
        divergenceImage.texture = fluidSimMaster.divergenceTexture;
        densityImage.texture = fluidSimMaster.densityTexture;
    }

    void Update()
    {
        // Ensure textures are kept updated each frame
        xVelImage.texture = fluidSimMaster.xVelTexture;
        yVelImage.texture = fluidSimMaster.yVelTexture;
        pressureImage.texture = fluidSimMaster.pressureTexture;
        divergenceImage.texture = fluidSimMaster.divergenceTexture;
        densityImage.texture = fluidSimMaster.densityTexture;
    }
}
