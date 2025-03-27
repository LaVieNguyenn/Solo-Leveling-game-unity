using UnityEngine;

public class CreateRenderTexture : MonoBehaviour
{
    void Start()
    {
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
        renderTexture.Create();
        Debug.Log("Render Texture Created!");
    }
}
