Shader "Custom/PortalEffect"
{
    Properties
    {
        [Header(Base Settings)]
        _Color ("Color", Color) = (1, 0, 0, 1)
        _MainTex ("Mask Texture", 2D) = "white" {}
        
        [Header(Animation)]
        _Speed ("Speed", Float) = 1.0
        
        [Header(Twirl Settings)]
        _Strength ("Twirl Strength", Float) = 10.0
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        
        [Header(Voronoi Settings)]
        _CellScale ("Cell Scale", Float) = 5.0
        _PowerScale ("Power Scale", Float) = 3.0
    }

    SubShader
    {
        // 设置渲染标签：透明、URP管线
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            // 开启混合模式实现透明效果
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off // 关闭深度写入，通常半透明特效不需要写入深度
            Cull Off   // 双面渲染 (Render Face: Both)

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 引入 URP 核心库
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ---------------------------------------------------------
            // 变量定义 (CBuffer 用于 SRP Batcher 优化)
            // ---------------------------------------------------------
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST; // 纹理的 Tiling/Offset
                float _Speed;
                float _Strength;
                float4 _Center;
                float _CellScale;
                float _PowerScale;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // ---------------------------------------------------------
            // 结构体定义
            // ---------------------------------------------------------
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // ---------------------------------------------------------
            // 辅助函数：对应 ShaderGraph 中的节点
            // ---------------------------------------------------------

            // 1. Twirl (扭曲) 算法实现
            float2 Twirl(float2 UV, float2 Center, float Strength, float2 Offset)
            {
                float2 delta = UV - Center;
                float dist = length(delta);
                float angle = Strength * dist;
                
                // 旋转矩阵逻辑
                float s, c;
                sincos(angle, s, c);
                
                // 旋转坐标
                float2 rotatedUV = float2(
                    delta.x * c - delta.y * s,
                    delta.x * s + delta.y * c
                );
                
                // 加上偏移和中心点还原
                return rotatedUV + Center + Offset;
            }

            // 2. Random 伪随机函数 (Voronoi 需要)
            float2 Random2(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453);
            }

            // 3. Voronoi (泰森多边形) 算法实现
            float Voronoi(float2 UV, float AngleOffset, float CellDensity)
            {
                float2 g = floor(UV * CellDensity);
                float2 f = frac(UV * CellDensity);
                float t = AngleOffset; // 这里可以传入时间做额外动画
                float res = 8.0;

                // 遍历周围 3x3 的格子
                for(int y = -1; y <= 1; y++)
                {
                    for(int x = -1; x <= 1; x++)
                    {
                        float2 lattice = float2(x, y);
                        float2 offset = Random2(lattice + g);
                        // 简单的动画偏移（虽然你的Graph里好像没连这个，但我补上以防万一）
                        offset = 0.5 + 0.5 * sin(t + 6.2831 * offset);
                        
                        float2 distVector = lattice + offset - f;
                        float d = length(distVector);

                        if(d < res)
                        {
                            res = d;
                        }
                    }
                }
                return res;
            }

            // ---------------------------------------------------------
            // 顶点着色器
            // ---------------------------------------------------------
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            // ---------------------------------------------------------
            // 片元着色器 (核心逻辑)
            // ---------------------------------------------------------
            half4 frag(Varyings input) : SV_Target
            {
                // [Step 1]: 计算时间驱动的 Offset
                // 对应节点：Time -> Multiply(A), Speed -> Multiply(B)
                // 注意：ShaderGraph的Time节点输出通常包含 (t/20, t, t*2, t*3)，_Time.y 是正常时间
                float2 timeOffset = float2(_Time.y * _Speed, _Time.y * _Speed);

                // [Step 2]: Twirl 扭曲 UV
                // 对应节点：Twirl
                // 你的连线是用 Time 驱动 Twirl 的 Offset，这会让纹理产生一种“平移+扭曲”的吸入感
                float2 twirledUV = Twirl(input.uv, _Center.xy, _Strength, timeOffset);

                // [Step 3]: 生成 Voronoi 噪声
                // 对应节点：Voronoi
                // 注意：Unity ShaderGraph 的 Voronoi 输出是 (1-距离)，所以看起来是中心亮的晶格
                // 我们这里的 Voronoi 函数通常输出的是“到特征点的距离”（中心黑，边缘白）
                // 所以下面可能需要用 1.0 - val 或者直接调整
                float noiseVal = Voronoi(twirledUV, 2.0, _CellScale);
                
                // ShaderGraph 的 Voronoi 实际上输出类似 (1-d) 的效果，或者我们需要反转它
                // 根据你的预览图（红色旋涡），中心是亮的，说明我们需要反相或者用 Power 压暗背景
                noiseVal = 1.0 - noiseVal; // 反相，使晶胞中心变亮

                // [Step 4]: Power 调整对比度
                // 对应节点：Power
                float powerVal = pow(abs(noiseVal), _PowerScale);

                // [Step 5]: 采样遮罩贴图
                // 对应节点：Sample Texture 2D
                half4 maskColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // [Step 6]: 组合颜色
                // 对应节点：Multiply -> Multiply
                // 逻辑：PowerResult * Mask.r * Color
                half3 finalColor = powerVal * maskColor.r * _Color.rgb;
                half finalAlpha = powerVal * maskColor.r * _Color.a;

                // 对应 Emission：直接作为颜色输出即可（因为是 Unlit 风格或者是自发光）
                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}