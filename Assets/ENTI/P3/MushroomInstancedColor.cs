using UnityEngine;

public class MushroomInstancedColor : MonoBehaviour
{
    
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

        
        rend.GetPropertyBlock(propBlock);

        
        Color randomColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);

        
        propBlock.SetColor(BaseColorID, randomColor);

        
        rend.SetPropertyBlock(propBlock);
    }

}
