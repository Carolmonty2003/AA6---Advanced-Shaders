using UnityEngine;

public class GlobalMushroomController : MonoBehaviour
{
    [Header("Configuracion Empuje")]
    public Transform playerTransform;
    public float pushRadius = 3f;
    public float pushStrength = 1.5f;

    
    private static readonly int PlayerPosID = Shader.PropertyToID("_PlayerPos");
    private static readonly int PushRadiusID = Shader.PropertyToID("_PushRadius");
    private static readonly int PushStrengthID = Shader.PropertyToID("_PushStrength");

    void Update()
    {
        if (playerTransform != null)
        {
            
            Shader.SetGlobalVector(PlayerPosID, playerTransform.position);
            Shader.SetGlobalFloat(PushRadiusID, pushRadius);
            Shader.SetGlobalFloat(PushStrengthID, pushStrength);
        }
    }

}
