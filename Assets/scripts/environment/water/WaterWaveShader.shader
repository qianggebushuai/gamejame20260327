Shader "Custom/WaterWave2D"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveAmplitude ("Wave Amplitude", Float) = 0.1
        _WaveFrequency ("Wave Frequency", Float) = 5.0
    }
    
    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
        }
        
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
            
            sampler2D _MainTex;
            fixed4 _Color;
            float _WaveSpeed;
            float _WaveAmplitude;
            float _WaveFrequency;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // ²¨ÎÆÐ§¹û
                float wave = sin((v.vertex.x * _WaveFrequency) + (_Time.y * _WaveSpeed)) * _WaveAmplitude;
                v.vertex.y += wave;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                return col;
            }
            ENDCG
        }
    }
}