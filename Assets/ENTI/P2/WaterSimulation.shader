Shader "Custom/WaterSimulation"
{
    Properties
    {
        _MainTex ("Previous Frame", 2D) = "grey" {}
        _PlayerUV ("Player UV", Vector) = (0.5, 0.5, 0, 0)
        _UVVelocity ("UV Velocity", Vector) = (0, 0, 0, 0)
        _PlayerRadius ("Player Radius", Float) = 0.08
        _PlayerHardness ("Player Hardness", Float) = 2
        _EffectSpeed ("Effect Speed", Float) = 0.99
    }
    SubShader
    {
        ZWrite Off ZTest Always Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _PlayerUV;
            float4 _UVVelocity;
            float _PlayerRadius;
            float _PlayerHardness;
            float _EffectSpeed;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;

                float2 diff = uv - _PlayerUV.xy;
                float dist = length(diff);
                float influence = pow(saturate(1.0 - dist / _PlayerRadius), _PlayerHardness);

                float4 prev = tex2D(_MainTex, uv);
                float flowX = (prev.r * 2.0 - 1.0) * _EffectSpeed;
                float flowZ = (prev.g * 2.0 - 1.0) * _EffectSpeed;

                flowX = clamp(flowX + _UVVelocity.x * influence * 10.0, -1.0, 1.0);
                flowZ = clamp(flowZ + _UVVelocity.y * influence * 10.0, -1.0, 1.0);

                return fixed4(flowX * 0.5 + 0.5, flowZ * 0.5 + 0.5, 0.5, 1.0);
            }
            ENDCG
        }
    }
}