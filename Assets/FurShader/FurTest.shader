// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/FurTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FurTex ("Fur pattern", 2D) = "white" {}
        _Diffuse ("Diffuse value", Range(0, 1)) = 1
        _FurLength ("Fur length", Float) = 0.5
        _CutOff ("Alpha cutoff", Range(0, 1)) = 0.5
        _Blur ("Blur", Range(0, 1)) = 0.5
        _Thickness ("Thickness", Range(0, 0.5)) = 0
    }
    CGINCLUDE

        static const fixed _Diffuse = 1;

        inline fixed4 LambertDiffuse(float3 worldNormal)
        {
            float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
            float NdotL = max(0, dot(worldNormal, lightDir));
            return NdotL * _Diffuse;
        }

    ENDCG
    SubShader
    {
        Tags { "RenderType"="Transparent" "IgnoreProjector"="True" "Queue"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            ZWrite On
            Blend Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal: NORMAL;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= LambertDiffuse(i.normal);
                col.a = 1;
                return col;
            }
            ENDCG
        }

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.05
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.1
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.15
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.2
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.25
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.3
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.35
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.4
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.45
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.5
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.55
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.6
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.65
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.7
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.75
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.8
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.85
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.9
            #include "FurPass.cginc"
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define FUR_STEP 0.95
            #include "FurPass.cginc"
            ENDCG
        }
    }
}
