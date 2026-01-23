Shader "Custom/GlowEffect"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1, 0.8, 0.2, 1)
        _GlowIntensity ("Glow Intensity", Range(0.0, 5.0)) = 2.0
        _PulseSpeed ("Pulse Speed", Range(0.0, 10.0)) = 2.0
        _GlowSize ("Glow Size", Range(0.0, 0.5)) = 0.1
    }
    
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        LOD 100
        
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GlowColor;
            float _GlowIntensity;
            float _PulseSpeed;
            float _GlowSize;
            
            v2f vert(appdata v)
            {
                v2f o;
                
                // Expand vertices for glow effect
                float3 norm = normalize(v.normal);
                v.vertex.xyz += norm * _GlowSize;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Fresnel effect for edge glow
                float fresnel = 1.0 - saturate(dot(normalize(i.viewDir), normalize(i.normal)));
                fresnel = pow(fresnel, 2.0);
                
                // Animated pulse
                float pulse = 0.5 + 0.5 * sin(_Time.y * _PulseSpeed);
                
                // Combine effects
                float glowStrength = fresnel * _GlowIntensity * pulse;
                
                // Final color
                fixed4 col = _GlowColor;
                col.a = glowStrength * _GlowColor.a;
                col.rgb *= glowStrength;
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/Diffuse"
}
