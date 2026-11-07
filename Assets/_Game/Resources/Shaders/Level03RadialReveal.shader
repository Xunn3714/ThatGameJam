Shader "SeedGame/Level03 Radial Reveal"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _RevealCenter ("Reveal Center", Vector) = (0,0,0,0)
        _RevealRadius ("Reveal Radius", Float) = -1
        _SoftEdge ("Soft Edge", Float) = 1.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _RevealCenter;
            float _RevealRadius;
            float _SoftEdge;

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 worldXY : TEXCOORD1;
            };
            v2f vert(appdata v)
            {
                v2f o;
                float4 world = mul(unity_ObjectToWorld, v.vertex);
                o.worldXY = world.xy;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                o.uv = v.uv;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                float distanceFromPulse = distance(i.worldXY, _RevealCenter.xy);
                float coverage = 1.0 - smoothstep(_RevealRadius - _SoftEdge,
                    _RevealRadius, distanceFromPulse);
                c.a *= coverage;
                return c;
            }
            ENDCG
        }
    }
}
