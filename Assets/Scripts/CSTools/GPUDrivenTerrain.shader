Shader "Custom/GPUDrivenTerrain" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _HeightMap ("Height Map", 2D) = "gray" {}
        _MainTex ("Main Texture", 2D) = "white" {}     // 大贴图
        _IndexMap ("Index Map", 2D) = "white" {}       // 索引贴图
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "UnityCG.cginc"

            StructuredBuffer<float3> _VertexBuffer;
            
            sampler2D _MainTex;
            sampler2D _IndexMap;
            float _TerrainWidth;
            float _TerrainHeight;

            struct v2f {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert (uint vid : SV_VertexID) 
            {
                v2f o;
                float3 pos = _VertexBuffer[vid];
                o.pos = UnityObjectToClipPos(pos);
                o.worldPos = mul(unity_ObjectToWorld, float4(pos,1));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 计算格子坐标（每个格子2x2单位）
                float2 worldXZ = i.worldPos.xz;
                int x = (int)floor(worldXZ.x);
                int z = (int)floor(worldXZ.y);
                
                // 转换为格子索引（每个格子对应索引贴图的一个像素）
                int gridX = x / 2;
                int gridZ = z / 2;
                
                // 获取索引（确保在纹理范围内）
                float2 indexUV = float2((gridX + 0.5) / _TerrainWidth, (gridZ + 0.5) / _TerrainHeight);
                float index = tex2Dlod(_IndexMap, float4(indexUV, 0, 0)).r * 63.0;
                int n = (int)(index + 0.5);
                
                // 计算子图UV偏移
                float tileSize = 1.0 / 8.0;
                float uOffset = (n % 8) * tileSize;
                float vOffset = (7 - n / 8) * tileSize;
                
                // 计算局部UV（0-1范围）
                float2 localUV = frac(worldXZ / 2.0); // 将2x2单位映射到0-1
                float2 mainUV = float2(uOffset, vOffset) + localUV * tileSize;
                fixed4 col = tex2D(_MainTex, mainUV);
                return col;
            }
            ENDCG
        }
    }
}