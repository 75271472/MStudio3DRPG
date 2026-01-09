Shader "UI/SimpleVignette"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1) // 这里会接收 Image 组件的颜色
        _Intensity ("Intensity", Range(0, 3)) = 1.5 // 控制渐变强度
        _Roundness ("Roundness", Range(0, 1)) = 0.5 // 控制圆形大小
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            float _Intensity;
            float _Roundness;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 计算 UV 距离中心的距离 (中心点变为 0,0)
                float2 uv = IN.texcoord - 0.5;
                
                // 计算距离长度
                float dist = length(uv);

                // 使用 smoothstep 制作柔和的渐变
                // dist * _Intensity 越大，边缘越明显
                float vignette = smoothstep(_Roundness, 0.8, dist * _Intensity);

                // 只要把 Alpha 改掉即可
                IN.color.a *= vignette;

                return IN.color;
            }
            ENDCG
        }
    }
}