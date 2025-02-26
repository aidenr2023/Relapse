Shader "Custom/ToonParticleShader"
{
    Properties
    {
        _MainTex ("Albedo Map", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1, 1, 1, 1)
        _ShadingSteps ("Shading Steps", Range(2, 10)) = 4
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float _ShadingSteps;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = tex2D(_MainTex, v.uv);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half3 color = i.color.rgb;
                // Simple toon shading by stepping on the lighting
                float intensity = dot(color, half3(0.299, 0.587, 0.114));
                intensity = floor(intensity * _ShadingSteps) / _ShadingSteps;  // Toon step shading
                return half4(color * intensity, 1.0);
            }
            ENDCG
        }
    }
}
