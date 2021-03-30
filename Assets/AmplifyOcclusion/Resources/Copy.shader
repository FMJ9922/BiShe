// Amplify Occlusion - Robust Ambient Occlusion for Unity Pro
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/Amplify Occlusion/Copy" {
	Properties { }
	CGINCLUDE
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.0
		#include "UnityCG.cginc"

		float4x4 _AO_CameraProj;

		sampler2D _AO_Source;
		float4 _AO_Source_TexelSize;

		float4 _AO_Target_TexelSize;
		float2 _AO_Target_Position;

		v2f_img vert( appdata_img v )
		{
			float2 src_scale = _AO_Source_TexelSize.zw / _AO_Target_TexelSize.zw;
			float2 dst_offset = _AO_Target_Position * ( 1.0 / _AO_Target_TexelSize.zw );

		#ifdef UNITY_UV_STARTS_AT_TOP
			src_scale.y *= -1;
			dst_offset.y = 1 - dst_offset.y;
		#endif

			v2f_img OUT;
			OUT.pos = mul( _AO_CameraProj, float4( v.vertex.xy * src_scale + dst_offset, 0, 1 ) );
			OUT.uv = v.texcoord.xy;
		#ifdef UNITY_HALF_TEXEL_OFFSET
			OUT.pos.xy += ( 1.0 / _AO_Target_TexelSize.zw ) * float2( -1, 1 );
		#endif
			return OUT;
		}
	ENDCG
	SubShader {
		ZTest Always Cull Off ZWrite Off

		// 0 => COPY
		Pass {
			CGPROGRAM
				float4 frag( v2f_img IN ) : SV_Target
				{
					return tex2Dlod( _AO_Source, float4( IN.uv, 0, 0 ) );
				}
			ENDCG
		}
	}
	Fallback Off
}
