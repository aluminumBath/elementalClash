// Elementborn/ToonWater — a stylised toon water surface for the Wind Waker look.
// Deep/shallow gradient, banded (cel) lighting, moving foam lines, gentle vertex waves,
// and a fresnel sparkle. Opaque, lit by the main directional light. URP / Unity 6.
Shader "Elementborn/ToonWater"
{
    Properties
    {
        _ShallowColor ("Shallow", Color) = (0.30,0.70,0.74,1)
        _DeepColor ("Deep", Color) = (0.06,0.28,0.45,1)
        _FoamColor ("Foam", Color) = (1,1,1,1)
        _FoamBands ("Foam Frequency", Float) = 2.0
        _FoamWidth ("Foam Width", Range(0,0.6)) = 0.12
        _FoamSpeed ("Foam Speed", Float) = 0.12
        _WaveAmp ("Wave Amplitude", Float) = 0.18
        _WaveFreq ("Wave Frequency", Float) = 0.35
        _WaveSpeed ("Wave Speed", Float) = 1.1
        _LightBands ("Light Bands", Range(1,5)) = 3
        _Sparkle ("Sparkle", Range(0,1)) = 0.25
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor;
                half4 _DeepColor;
                half4 _FoamColor;
                float _FoamBands;
                float _FoamWidth;
                float _FoamSpeed;
                float _WaveAmp;
                float _WaveFreq;
                float _WaveSpeed;
                float _LightBands;
                float _Sparkle;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionHCS : SV_POSITION; float3 positionWS : TEXCOORD0; float3 normalWS : TEXCOORD1; };

            float WaveHeight (float3 p)
            {
                return (sin(p.x * _WaveFreq + _Time.y * _WaveSpeed)
                      + cos(p.z * _WaveFreq * 1.3 + _Time.y * _WaveSpeed * 0.85)) * _WaveAmp;
            }

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 ws = TransformObjectToWorld(IN.positionOS.xyz);
                ws.y += WaveHeight(ws);
                OUT.positionWS = ws;
                OUT.positionHCS = TransformWorldToHClip(ws);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                Light mainLight = GetMainLight();
                float3 n = normalize(IN.normalWS);
                float ndl = saturate(dot(n, mainLight.direction));
                float bands = max(1.0, _LightBands);
                float toon = floor(ndl * bands) / bands;

                float depthT = saturate(0.5 + WaveHeight(IN.positionWS) * 0.5);
                float3 baseCol = lerp(_DeepColor.rgb, _ShallowColor.rgb, depthT);
                float3 col = baseCol * (0.45 + 0.55 * toon) * mainLight.color.rgb;

                float w = frac((IN.positionWS.x + IN.positionWS.z) * (_FoamBands * 0.01) + _Time.y * _FoamSpeed);
                float foam = smoothstep(1.0 - _FoamWidth, 1.0, w);
                col = lerp(col, _FoamColor.rgb, foam * 0.65);

                float3 viewDir = normalize(GetCameraPositionWS() - IN.positionWS);
                float fres = pow(1.0 - saturate(dot(n, viewDir)), 4.0);
                col += _Sparkle * fres * mainLight.color.rgb;

                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
