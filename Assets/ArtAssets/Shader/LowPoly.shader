Shader "Custom/Lowpoly" {
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
    }

        SubShader{

            Pass {
                // FOLLOWING LINE IS NEW
               Tags {"LightMode" = "ForwardBase"}
               CGPROGRAM
               #pragma vertex vert
               #pragma fragment frag
               #include "UnityCG.cginc"
               #include "AutoLight.cginc"
               #pragma multi_compile_fwdbase
               #pragma multi_compile_fog
               uniform float4 _LightColor0;
               uniform float4 _Color;
               struct VertexInput {
                   float4 vertex : POSITION;
                   float3 normal : NORMAL;
                   float4 tangent : TANGENT;
               };
               struct VertexOutput {
                   float4 pos : SV_POSITION;
                   float4 posWorld : TEXCOORD0;
                   float3 normalDir : TEXCOORD1;
                   float3 tangentDir : TEXCOORD2;
                   float3 bitangentDir : TEXCOORD3;
                   LIGHTING_COORDS(4,5)
                   UNITY_FOG_COORDS(6)
               };
               VertexOutput vert(VertexInput v) {
                   VertexOutput o = (VertexOutput)0;
                   o.normalDir = UnityObjectToWorldNormal(v.normal);
                   o.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
                   o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                   o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                   float3 lightColor = _LightColor0.rgb;
                   o.pos = UnityObjectToClipPos(v.vertex);
                   UNITY_TRANSFER_FOG(o,o.pos);
                   TRANSFER_VERTEX_TO_FRAGMENT(o)
                   return o;
               }
               float4 frag(VertexOutput i) : COLOR {
                   i.normalDir = normalize(i.normalDir);
                   float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
                   float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                   float3 normalLocal = mul(tangentTransform, cross(normalize(ddy(i.posWorld.rgb)),normalize(ddx(i.posWorld.rgb)))).xyz.rgb;
                   float3 normalDirection = normalize(mul(normalLocal, tangentTransform)); // Perturbed normals
                   float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                   float3 lightColor = _LightColor0.rgb;
                   ////// Lighting:
                       float attenuation = LIGHT_ATTENUATION(i);
                       float3 attenColor = attenuation * _LightColor0.xyz;
                       /////// Diffuse:
                           float NdotL = max(0.0,dot(normalDirection, lightDirection));
                           float3 directDiffuse = max(0.0, NdotL) * attenColor;
                           float3 indirectDiffuse = float3(0,0,0);
                           indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                           float3 diffuseColor = _Color.rgb;
                           float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
                           /// Final Color:
                               float3 finalColor = diffuse;
                               fixed4 finalRGBA = fixed4(finalColor,1); 
                               UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                               return finalRGBA;
                           }
                           ENDCG
                       }
    }
        Fallback "VertexLit"
}