Shader "SeedGame/InteractableOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1, .84, .3, .8)
        _CorePixels ("Solid Core Width", Float) = 2.25
        _GlowPixels ("Glow Width", Float) = 7
        _CoreOpacity ("Solid Core Opacity", Range(0, 1)) = .92
        _GlowOpacity ("Outer Glow Opacity", Range(0, 1)) = .45
        [HideInInspector] _SpriteUVRect ("Sprite UV Rect", Vector) = (0, 0, 1, 1)
        [HideInInspector] _SpriteBounds ("Sprite Bounds", Vector) = (-.5, -.5, 1, 1)
        [HideInInspector] _PixelWorldSize ("Pixel World Size", Vector) = (.01, .01, 0, 0)
        [HideInInspector] _GeometryPaddingPixels ("Geometry Padding", Float) = 10
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
            float _CorePixels;
            float _GlowPixels;
            float _CoreOpacity;
            float _GlowOpacity;
            float4 _SpriteUVRect;
            float4 _SpriteBounds;
            float2 _PixelWorldSize;
            float _GeometryPaddingPixels;

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
                float2 boundsSize = max(_SpriteBounds.zw, float2(.0001, .0001));
                float2 vertexPosition = saturate((input.vertex.xy - _SpriteBounds.xy) / boundsSize);
                float2 vertexDirection = lerp(float2(-1, -1), float2(1, 1),
                    step(float2(.5, .5), vertexPosition));
                float2 uvCenter = _SpriteUVRect.xy + _SpriteUVRect.zw * .5;
                float2 uvDirection = lerp(float2(-1, -1), float2(1, 1), step(uvCenter, input.uv));
                input.vertex.xy += vertexDirection * _PixelWorldSize * _GeometryPaddingPixels;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv + uvDirection * _MainTex_TexelSize.xy * _GeometryPaddingPixels;
                output.color = input.color;
                return output;
            }

            float SampleAlpha(float2 uv)
            {
                float2 rectMax = _SpriteUVRect.xy + _SpriteUVRect.zw;
                float inside = step(_SpriteUVRect.x, uv.x) * step(uv.x, rectMax.x)
                    * step(_SpriteUVRect.y, uv.y) * step(uv.y, rectMax.y);
                return tex2D(_MainTex, uv).a * inside;
            }

            float SampleRing(float2 uv, float radius)
            {
                float2 cardinal = _MainTex_TexelSize.xy * radius;
                float2 diagonal = cardinal * .70710678;
                float alpha = SampleAlpha(uv + float2(cardinal.x, 0));
                alpha = max(alpha, SampleAlpha(uv - float2(cardinal.x, 0)));
                alpha = max(alpha, SampleAlpha(uv + float2(0, cardinal.y)));
                alpha = max(alpha, SampleAlpha(uv - float2(0, cardinal.y)));
                alpha = max(alpha, SampleAlpha(uv + diagonal));
                alpha = max(alpha, SampleAlpha(uv - diagonal));
                alpha = max(alpha, SampleAlpha(uv + float2(diagonal.x, -diagonal.y)));
                alpha = max(alpha, SampleAlpha(uv + float2(-diagonal.x, diagonal.y)));
                return alpha;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float center = SampleAlpha(input.uv);
                float outside = 1.0 - center;
                float glowRange = max(_GlowPixels - _CorePixels, .01);

                // A crisp, stable edge anchors the interaction cue. The wider rings
                // fill the remaining space with progressively weaker alpha.
                float core = SampleRing(input.uv, _CorePixels) * outside;
                float nearGlow = SampleRing(input.uv, _CorePixels + glowRange * .36) * outside;
                float midGlow = SampleRing(input.uv, _CorePixels + glowRange * .68) * outside;
                float farGlow = SampleRing(input.uv, _GlowPixels) * outside;

                core = saturate(core * 2.0) * _CoreOpacity;
                nearGlow = saturate(nearGlow * 1.7) * _GlowOpacity;
                midGlow = saturate(midGlow * 1.45) * _GlowOpacity * .48;
                farGlow = saturate(farGlow * 1.25) * _GlowOpacity * .16;

                float alpha = max(max(core, nearGlow), max(midGlow, farGlow));
                return fixed4(_GlowColor.rgb, alpha * _GlowColor.a * input.color.a);
            }
            ENDCG
        }
    }
}
