// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TransparentGpuInstance"
{
    Properties{
      _MainTex("RGBA Texture Image", 2D) = "white" {}
      _Cutoff("Alpha Cutoff", Float) = 0.5
    }
        SubShader{
           Pass {
              Cull Off // since the front is partially transparent, 
                 // we shouldn't cull the back

              CGPROGRAM

              #pragma vertex vert  
              #pragma fragment frag 

              uniform sampler2D _MainTex;
              uniform float _Cutoff;

              struct vertexInput {
                 float4 vertex : POSITION;
                 float4 texcoord : TEXCOORD0;
              };
              struct vertexOutput {
                 float4 pos : SV_POSITION;
                 float4 tex : TEXCOORD0;
              };

              vertexOutput vert(vertexInput input)
              {
                 vertexOutput output;

                 output.tex = input.texcoord;
                 output.pos = UnityObjectToClipPos(input.vertex);
                 return output;
              }

              float4 frag(vertexOutput input) : COLOR
              {
                 float4 textureColor = tex2D(_MainTex, input.tex.xy);
                 if (textureColor.a < _Cutoff)
                     // alpha value less than user-specified threshold?
                  {
                     discard; // yes: discard this fragment
                  }
                  return textureColor;
               }

               ENDCG
            }
      }
      
}
