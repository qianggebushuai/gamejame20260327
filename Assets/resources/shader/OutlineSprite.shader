Shader "Custom/SpriteOutline2D"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,0,1)
        _OutlineWidth ("Outline Width", Float) = 0.01
        _PulseSpeed ("Pulse Speed", Float) = 3.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent+10"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _PulseSpeed;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                fixed4 col = tex2D(_MainTex, uv);

                // 如果当前像素是透明的，检查周围是否有不透明像素
                if (col.a < 0.1)
                {
                    float outline = 0;
                    float2 offsets[8] = {
                        float2(-1,0), float2(1,0), float2(0,-1), float2(0,1),
                        float2(-1,-1), float2(1,-1), float2(-1,1), float2(1,1)
                    };

                    for (int j = 0; j < 8; j++)
                    {
                        float2 sampleUV = uv + offsets[j] * _OutlineWidth;
                        fixed4 sampleCol = tex2D(_MainTex, sampleUV);
                        outline = max(outline, sampleCol.a);
                    }

                    if (outline > 0.1)
                    {
                        // 闪烁效果
                        float pulse = lerp(0.4, 1.0, (sin(_Time.y * _PulseSpeed * 3.14159) + 1.0) * 0.5);
                        return fixed4(_OutlineColor.rgb, outline * pulse * _OutlineColor.a);
                    }
                }

                // 原始像素设为透明（只显示边缘）
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}