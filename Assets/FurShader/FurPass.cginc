#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float2 texcoord : TEXCOORD0;
};

struct v2f
{
    float4 pos : SV_POSITION;
    half2 uv : TEXCOORD0;
    half2 uv1 : TEXCOORD1;
    fixed4 diff : COLOR;
};

float _FurLength;
sampler2D _MainTex;
float4 _MainTex_ST;
sampler2D _FurTex;
float4 _FurTex_ST;
float _Blur;
float _CutOff;
float _Thickness;

v2f vert (appdata v)
{
    v2f o;
    v.vertex.xyz += v.normal * _FurLength * FUR_STEP;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
    o.uv1 = TRANSFORM_TEX(v.texcoord, _FurTex);
    float3 worldNormal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz);
    o.diff = LambertDiffuse(worldNormal);
    o.diff.a = 1 - (FUR_STEP * FUR_STEP);
    float4 worldPos = mul(unity_WorldToObject, v.vertex);
    o.diff.a += abs(dot(normalize(_WorldSpaceCameraPos.xyz - worldPos), worldNormal)) - _Blur;
    return o;
}

fixed4 frag (v2f i) : SV_Target
{
    fixed4 col = tex2D(_MainTex, i.uv);

    col = 0.3 + 0.7*col;

    fixed alpha = tex2D(_FurTex, i.uv1).r;
    col *= i.diff;
    col.a *= step(lerp(_CutOff, _CutOff + _Thickness, FUR_STEP), alpha);
    col.a = saturate(col.a);
    return col;
}
