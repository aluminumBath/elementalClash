// Elementborn/ToonLit — a URP cel/toon shader for a bright cel-shaded toon look.
// Banded diffuse + shadow tint + optional rim, plus a geometry-based (inverted-hull)
// outline. The outline is deliberately geometry-based, not a post-process edge effect,
// because post effects are expensive in VR and several break in stereo rendering.
//
// Targets URP for Unity 6 (6000.x). On older URP versions a macro or two may need a tweak.
Shader "Elementborn/ToonLit"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _ShadowTint ("Shadow Tint", Color) = (0.5,0.55,0.7,1)
        _RampSteps ("Ramp Steps", Range(1,5)) = 2
        _RampSmoothing ("Ramp Smoothing", Range(0,0.2)) = 0.02
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0.5,8)) = 3
        _RimAmount ("Rim Amount", Range(0,1)) = 0.0
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0,0.05)) = 0.012
        [Toggle(_VERTEXCOLOR_ON)] _VertexColorOn ("Use Vertex Colors", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        // ---- Outline (inverted hull) ------------------------------------------------
        Pass
        {
            Name "Outline"
            Cull Front

            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ShadowTint;
                float _RampSteps;
                float _RampSmoothing;
                half4 _RimColor;
                float _RimPower;
                float _RimAmount;
                half4 _OutlineColor;
                float _OutlineWidth;
                float _VertexColorOn;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionHCS : SV_POSITION; };

            Varyings OutlineVert (Attributes IN)
            {
                Varyings OUT;
                float3 inflated = IN.positionOS.xyz + IN.normalOS * _OutlineWidth;
                OUT.positionHCS = TransformObjectToHClip(inflated);
                return OUT;
            }

            half4 OutlineFrag (Varyings IN) : SV_Target { return _OutlineColor; }
            ENDHLSL
        }

        // ---- Forward toon-lit pass --------------------------------------------------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local _VERTEXCOLOR_ON
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ShadowTint;
                float _RampSteps;
                float _RampSmoothing;
                half4 _RimColor;
                float _RimPower;
                float _RimAmount;
                half4 _OutlineColor;
                float _OutlineWidth;
                float _VertexColorOn;
            CBUFFER_END

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                half4  vColor      : TEXCOORD3;
            };

            Varyings Vert (Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs nrm = GetVertexNormalInputs(IN.normalOS);
                OUT.positionHCS = pos.positionCS;
                OUT.positionWS = pos.positionWS;
                OUT.normalWS = nrm.normalWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.vColor = IN.color;
                return OUT;
            }

            // Quantize lambert into hard bands with a touch of smoothing at the steps.
            float ToonRamp (float ndotl)
            {
                float steps = max(_RampSteps, 1.0);
                float lit = saturate(ndotl);
                float scaled = lit * steps;
                float banded = floor(scaled) / steps;
                float edge = smoothstep(0.5 - _RampSmoothing, 0.5 + _RampSmoothing, frac(scaled)) / steps;
                return saturate(banded + edge);
            }

            half4 Frag (Varyings IN) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                #if defined(_VERTEXCOLOR_ON)
                    albedo *= IN.vColor; // flat per-face colours painted in Blender
                #endif
                float3 N = normalize(IN.normalWS);

                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                float ramp = ToonRamp(dot(N, mainLight.direction)) * mainLight.shadowAttenuation;

                half3 lit = albedo.rgb * mainLight.color;
                half3 shadowed = albedo.rgb * _ShadowTint.rgb;
                half3 color = lerp(shadowed, lit, ramp);

                color += albedo.rgb * SampleSH(N) * 0.3; // soft ambient fill

                float3 V = normalize(GetWorldSpaceViewDir(IN.positionWS));
                float rim = pow(1.0 - saturate(dot(V, N)), _RimPower) * _RimAmount;
                color += _RimColor.rgb * rim;

                return half4(color, albedo.a);
            }
            ENDHLSL
        }

        // ---- Shadow casting ---------------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ShadowTint;
                float _RampSteps;
                float _RampSmoothing;
                half4 _RimColor;
                float _RimPower;
                float _RimAmount;
                half4 _OutlineColor;
                float _OutlineWidth;
                float _VertexColorOn;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionHCS : SV_POSITION; };

            Varyings ShadowVert (Attributes IN)
            {
                Varyings OUT;
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normWS = TransformObjectToWorldNormal(IN.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(posWS, normWS, _LightDirection));
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                OUT.positionHCS = positionCS;
                return OUT;
            }

            half4 ShadowFrag (Varyings IN) : SV_Target { return 0; }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
