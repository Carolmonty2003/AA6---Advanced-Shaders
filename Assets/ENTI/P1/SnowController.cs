using UnityEngine;

public class SnowController : MonoBehaviour
{
    [SerializeField] private Material snowMaterial;
    [SerializeField] private float cycleDuration = 5f;

    private static readonly int SnowAmountID =
        Shader.PropertyToID("_SnowAmount");

    private void Update()
    {
        float snowAmount = Mathf.PingPong(Time.time / cycleDuration, 1f);
        snowMaterial.SetFloat(SnowAmountID, snowAmount);
    }
}
