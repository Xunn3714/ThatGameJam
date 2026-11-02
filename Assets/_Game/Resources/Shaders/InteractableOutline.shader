Shader "SeedGame/InteractableOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1, .84, .3, .8)
        _OutlinePixels ("Outline Width", Float) = 3
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
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
            float4 _MainTex_TexelSize;
            fixed4 _GlowColor;
            float _OutlinePixels;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 stepUV = _MainTex_TexelSize.xy * _OutlinePixels;
                float center = tex2D(_MainTex, input.uv).a;
                float a = tex2D(_MainTex, input.uv + float2(stepUV.x, 0)).a;
                float b = tex2D(_MainTex, input.uv - float2(stepUV.x, 0)).a;
                float c = tex2D(_MainTex, input.uv + float2(0, stepUV.y)).a;
                float d = tex2D(_MainTex, input.uv - float2(0, stepUV.y)).a;
                float e = tex2D(_MainTex, input.uv + stepUV).a;
                float f = tex2D(_MainTex, input.uv - stepUV).a;
                float g = tex2D(_MainTex, input.uv + float2(stepUV.x, -stepUV.y)).a;
                float h = tex2D(_MainTex, input.uv + float2(-stepUV.x, stepUV.y)).a;
                float outer = max(max(max(a, b), max(c, d)), max(max(e, f), max(g, h)));
                float inner = min(min(min(a, b), min(c, d)), min(min(e, f), min(g, h)));
                float edge = saturate(max(outer - center, center - inner) * 1.6);
                return fixed4(_GlowColor.rgb, edge * _GlowColor.a * input.color.a);
            }
            ENDCG
        }
    }
}
