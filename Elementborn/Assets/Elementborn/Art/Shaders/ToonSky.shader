// Elementborn/ToonSky — a procedural toon skybox for a bright cel-shaded toon look.
// Vertical top/horizon/ground gradient, a soft sun disc, and drifting toon cloud bands.
// Assign to RenderSettings.skybox (SceneStyleController does this). URP / Unity 6.
Shader "Elementborn/ToonSky"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.20,0.45,0.85,1)
        _HorizonColor ("Horizon Color", Color) = (0.72,0.86,0.95,1)
        _GroundColor ("Ground Color", Color) = (0.45,0.50,0.48,1)
        _SunColor ("Sun Color", Color) = (1,0.96,0.82,1)
        _SunDirection ("Sun Direction", Vector) = (0.3,0.5,0.4,0)
        _SunSharpness ("Sun Sharpness", Range(1,400)) = 80
        _HorizonSharpness ("Horizon Sharpness", Range(0.5,8)) = 2.5
        _CloudColor ("Cloud Color", Color) = (1,1,1,1)
        _CloudAmount ("Cloud Amount", Range(0,1)) = 0.45
        _CloudScale ("Cloud Scale", Float) = 2.5
        _CloudSpeed ("Cloud Speed", Float) = 0.008
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _TopColor;
                half4 _HorizonColor;
                half4 _GroundColor;
                half4 _SunColor;
                float4 _SunDirection;
                float _SunSharpness;
                float _HorizonSharpness;
                half4 _CloudColor;
                float _CloudAmount;
                float _CloudScale;
                float _CloudSpeed;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionHCS : SV_POSITION; float3 dir : TEXCOORD0; };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.dir = IN.positionOS.xyz;
                return OUT;
            }

            float hash21 (float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float valueNoise (float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1,0));
                float c = hash21(i + float2(0,1));
                float d = hash21(i + float2(1,1));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 d = normalize(IN.dir);
                float h = d.y;

                float up = pow(saturate(h), 1.0 / _HorizonSharpness);
                float3 col = lerp(_HorizonColor.rgb, _TopColor.rgb, up);
                col = lerp(col, _GroundColor.rgb, saturate(-h * 2.0));

                float3 sunDir = normalize(_SunDirection.xyz);
                float sd = saturate(dot(d, sunDir));
                col += _SunColor.rgb * pow(sd, _SunSharpness);

                float2 uv = (d.xz / max(0.25, h + 0.35)) * _CloudScale + _Time.y * _CloudSpeed;
                float n = valueNoise(uv);
                float clouds = smoothstep(1.0 - _CloudAmount, 1.0 - _CloudAmount + 0.18, n) * saturate(h * 3.0);
                col = lerp(col, _CloudColor.rgb, clouds);

                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
