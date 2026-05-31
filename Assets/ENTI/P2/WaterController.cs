using UnityEngine;

public class WaterController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Material  simulationMaterial;
    public Material  waterMaterial;
    public Transform waterZone;

    [Header("Resolución")]
    public Vector2Int resolution = new Vector2Int(512, 512);

    private RenderTexture[] _buffers = new RenderTexture[2];
    private int _currentBuffer = 0;
    private Vector3 _prevPosition;
    private Vector3 _velocity;
    private float _planeSizeX;
    private float _planeSizeZ;

    private static readonly int PropPlayerUV = Shader.PropertyToID("_PlayerUV");
    private static readonly int PropUVVelocity = Shader.PropertyToID("_UVVelocity");
    private static readonly int PropFlowmap = Shader.PropertyToID("_Flowmap");

    void Start()
    {
        _planeSizeX = 10f * waterZone.localScale.x;
        _planeSizeZ = 10f * waterZone.localScale.z;

        for (int i = 0; i < 2; i++)
        {
            _buffers[i] = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.ARGBHalf);
            _buffers[i].filterMode = FilterMode.Bilinear;
            _buffers[i].wrapMode = TextureWrapMode.Clamp;
            _buffers[i].Create();

            RenderTexture.active = _buffers[i];
            GL.Clear(false, true, new Color(0.5f, 0.5f, 0.5f, 1f));
            RenderTexture.active = null;
        }

        if (player != null) _prevPosition = player.position;
    }

    void LateUpdate()
    {
        if (player == null || simulationMaterial == null || waterZone == null) return;

        _velocity = (player.position - _prevPosition) / Time.deltaTime;
        _prevPosition = player.position;

        Vector3 relPos = player.position - waterZone.position;
        
        Vector2 playerUV = new Vector2(
            0.5f - relPos.x / _planeSizeX,
            0.5f - relPos.z / _planeSizeZ
        );
        
        Vector2 uvVelocity = new Vector2(
            -_velocity.x / _planeSizeX,
            -_velocity.z / _planeSizeZ
        );

        simulationMaterial.SetVector(PropPlayerUV,
            new Vector4(playerUV.x, playerUV.y, 0f, 0f));
        simulationMaterial.SetVector(PropUVVelocity,
            new Vector4(uvVelocity.x, uvVelocity.y, 0f, 0f));

        int prev = _currentBuffer;
        int curr = 1 - _currentBuffer;

        Graphics.Blit(_buffers[prev], _buffers[curr], simulationMaterial);

        if (waterMaterial != null)
            waterMaterial.SetTexture(PropFlowmap, _buffers[curr]);

        _currentBuffer = curr;
    }

    void OnDestroy()
    {
        foreach (RenderTexture rt in _buffers)
            if (rt != null) rt.Release();
    }

    void OnDrawGizmosSelected()
    {
        if (waterZone == null) return;
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawWireCube(waterZone.position,
            new Vector3(_planeSizeX > 0 ? _planeSizeX : 10f * waterZone.localScale.x,
                        0.1f,
                        _planeSizeZ > 0 ? _planeSizeZ : 10f * waterZone.localScale.z));
    }
}