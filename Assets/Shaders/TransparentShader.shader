    Shader "Lit/Transparent With Shadows"
{
    Properties
    {
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _ShadowColor("Shadow Color", Color) = (0.35,0.4,0.45,1.0)
    }
    SubShader
    {
        Tags { 
            "RenderPipeline" = "UniversalPipeline" 
            "UniversalMaterialType" = "Lit" 
            "Queue" = "AlphaTest+51" 
            "IgnoreProjector" = "True" 
            "RenderType" = "TransparentCutout" 
        }
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _ShadowColor;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        ENDHLSL

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward"}
            Blend DstColor Zero, Zero One
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            struct Attributes {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD0;
                float2 uv           : TEXCOORD1;
                float fogCoord      : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes i) {
                Varyings o = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_TRANSFER_INSTANCE_ID(i, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(i.positionOS.xyz);
                o.positionCS = vertexInput.positionCS;
                o.positionWS = vertexInput.positionWS;
                o.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);

                o.uv = i.uv;

                return o;
            }

            half4 frag(Varyings input) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 color = half4(1,1,1,1);

            #ifdef _MAIN_LIGHT_SHADOWS
                VertexPositionInputs vertexInput = (VertexPositionInputs)0;
                vertexInput.positionWS = input.positionWS;

                float4 shadowCoord = GetShadowCoord(vertexInput);
                half shadowAttenutation = MainLightRealtimeShadow(shadowCoord);
                color = lerp(half4(1,1,1,1), _ShadowColor, (1.0 - shadowAttenutation) * _ShadowColor.a);
                color.rgb = MixFogColor(color.rgb, half3(1,1,1), input.fogCoord);
            #endif

                return color;
            }

            ENDHLSL
            //CGPROGRAM
            //#pragma vertex vert
            //#pragma fragment frag

            //#include "UnityCG.cginc"
            //#include "Lighting.cginc"

            //#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

            //#include "AutoLight.cginc"

            ////struct appdata
            ////{
            ////    float4 vertex : POSITION;
            ////    float2 uv : TEXCOORD0;
            ////};

            //struct v2f
            //{
            //    SHADOW_COORDS(0)
            //    fixed3 diff : COLOR0;
            //    fixed3 ambient : COLOR1;
            //    float4 pos : SV_POSITION;
            //};

            ////float4 _MainTex_ST;

            //v2f vert (appdata_base v)
            //{
            //    v2f o;
            //    o.pos = UnityObjectToClipPos(v.vertex);
            //    half3 worldNormal = UnityObjectToWorldNormal(v.normal);
            //    half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
            //    o.diff = nl * _LightColor0.rgb;
            //    o.ambient = ShadeSH9(half4(worldNormal, 1));
            //    // compute shadows data
            //    TRANSFER_SHADOW(o)
            //    return o;
            //}

            //sampler2D _MainTex;

            //fixed4 frag(v2f i) : SV_Target
            //{
            //    // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
            //    fixed atten = SHADOW_ATTENUATION(i);

            //    // only draw shadows
            //    fixed4 col = fixed4(atten, atten, atten, 0.0f);
            //    return col;
            //}
            //ENDCG
        }

        // shadow casting support
        //UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
