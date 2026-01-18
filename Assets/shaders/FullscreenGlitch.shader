Shader "UI/GlitchOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // RawImage needs a main texture slot
        _Intensity ("Intensity", Range(0,1)) = 0
        _GlitchOpacity ("Glitch Opacity", Range(0,1)) = 0.6

        _NoiseScale ("Noise Scale", Range(1,200)) = 60
        _RGBSplit ("RGB Split", Range(0,0.02)) = 0.006
        _BlockSize ("Block Size", Range(20,400)) = 120
        _Scanline ("Scanline Strength", Range(0,1)) = 0.25
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Pass
        {
            Name "UI_GlitchOverlay"
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Intensity;
            float _GlitchOpacity;

            float _NoiseScale;
            float _RGBSplit;
            float _BlockSize;
            float _Scanline;

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float2 uv    : TEXCOORD0;
                float4 color : COLOR;
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            v2f Vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 Frag(v2f i) : SV_Target
            {
                // Original frame from RawImage texture
                float2 baseUV = i.uv;

                // If your RawImage texture is a RenderTexture of the camera, this samples the scene.
                fixed4 original = tex2D(_MainTex, baseUV) * i.color;

                float t = _Time.y;
                float2 uv = baseUV;

                // --- Block glitch ---
                float2 blockUV = floor(uv * _BlockSize) / _BlockSize;
                float n = hash21(blockUV + t);
                float mask = step(1.0 - (_Intensity * 0.6), n);

                float shift = (hash21(float2(blockUV.y, t)) - 0.5) * 0.08 * _Intensity;
                uv.x += shift * mask;

                // --- Wavy distortion ---
                float wave = sin((uv.y * _NoiseScale) + t * 18.0) * 0.01 * _Intensity;
                uv.x += wave;

                // --- RGB split ---
                float2 split = float2(_RGBSplit * _Intensity, 0);
                fixed r = tex2D(_MainTex, uv + split).r;
                fixed g = tex2D(_MainTex, uv).g;
                fixed b = tex2D(_MainTex, uv - split).b;

                fixed4 glitchCol = fixed4(r, g, b, original.a);

                // --- Scanlines (on glitch only) ---
                float scan = sin((baseUV.y + t * 6.0) * 900.0) * 0.5 + 0.5;
                glitchCol.rgb *= lerp(1.0, scan, _Scanline * _Intensity);

                // --- Grain (on glitch only) ---
                float grain = (hash21(baseUV * (300 + _NoiseScale) + t) - 0.5) * 0.18 * _Intensity;
                glitchCol.rgb += grain;

                // --- Transparent overlay blend ---
                float overlayA = saturate(_Intensity * _GlitchOpacity);

                // Mix original with glitch, but keep original visibility
                float3 finalRGB = lerp(original.rgb, glitchCol.rgb, overlayA);

                // Alpha controls how much this UI element covers what's behind it
                // Since this is an overlay RawImage, keep alpha = 1 so it shows the camera RT,
                // but we already blended the glitch into the camera image.
                return fixed4(finalRGB, 1.0);
            }
            ENDHLSL
        }
    }
}
