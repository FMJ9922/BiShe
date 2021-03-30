// Amplify Occlusion - Robust Ambient Occlusion for Unity Pro
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/Amplify Occlusion/Blur"
{
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

		float _AO_BlurSharpness;
		float _AO_BlurDepthThreshold;
		float _AO_BlurFalloff;

		inline half ComputeSharpness( half linearEyeDepth )
		{
			return _AO_BlurSharpness * ( saturate( 1 - linearEyeDepth ) * 100 + 1 );
		}

		inline half ComputeFalloff( const int radius )
		{
			half sigma = ( ( half ) radius ) * 0.5;
			return 1.0 / ( 2 * sigma * sigma );
		}

		inline half2 CrossBilateralWeight( const half2 r, half2 d, half d0, const half sharpness, const half falloff )
		{
			half2 diff = ( d0 - d ) * sharpness;
			return exp2( -( r * r ) * falloff - diff * diff );
		}

		inline half4 CrossBilateralWeight( const half4 r, half4 d, half d0, const half sharpness, const half falloff )
		{
			half4 diff = ( d0 - d ) * sharpness;
			return exp2( -( r * r ) * falloff - diff * diff );
		}

		half4 blur1D_1x( v2f_img IN, float2 deltaUV )
		{
			half2 center = tex2Dlod( _AO_Source, float4( IN.uv, 0, 0 ) ).xy;

			const float2 offset1 = 1 * deltaUV;

			half4 s1;
			s1.xy = tex2Dlod( _AO_Source, float4( IN.uv + offset1, 0, 0 ) ).xy;
			s1.zw = tex2Dlod( _AO_Source, float4( IN.uv - offset1, 0, 0 ) ).xy;

			const half sharpness = ComputeSharpness( center.y );
			const half falloff = ComputeFalloff( 1 );

			half2 w1 = CrossBilateralWeight( ( 1 ).xx, s1.yw, center.y, sharpness, falloff );

			half ao = center.x + dot( s1.xz, w1 );
			ao /= 1 + dot( ( 1 ).xx, w1 );

			return half4( ao, center.y, 0, 0 );
		}

		half4 blur1D_2x( v2f_img IN, float2 deltaUV )
		{
			half2 center = tex2Dlod( _AO_Source, float4( IN.uv, 0, 0 ) ).xy;

			const float2 offset1 = 1 * deltaUV;
			const float2 offset2 = 2 * deltaUV;

			half4 s1, s2;
			s2.zw = tex2Dlod( _AO_Source, float4( IN.uv - offset2, 0, 0 ) ).xy;
			s1.zw = tex2Dlod( _AO_Source, float4( IN.uv - offset1, 0, 0 ) ).xy;
			s1.xy = tex2Dlod( _AO_Source, float4( IN.uv + offset1, 0, 0 ) ).xy;
			s2.xy = tex2Dlod( _AO_Source, float4( IN.uv + offset2, 0, 0 ) ).xy;

			const half sharpness = ComputeSharpness( center.y );
			const half falloff = ComputeFalloff( 2 );

			half4 w12 = CrossBilateralWeight( half4( 1, 1, 2, 2 ), half4( s1.yw, s2.yw ), center.y, sharpness, falloff );

			half ao = center.x + dot( half4( s1.xz, s2.xz ), w12 );
			ao /= 1 + dot( ( 1 ).xxxx, w12 );

			return half4( ao, center.y, 0, 0 );
		}

		half4 blur1D_3x( v2f_img IN, float2 deltaUV )
		{
			half2 center = tex2Dlod( _AO_Source, float4( IN.uv, 0, 0 ) ).xy;

			const float2 offset1 = 1 * deltaUV;
			const float2 offset2 = 2 * deltaUV;
			const float2 offset3 = 3 * deltaUV;

			half4 s1, s2, s3;
			s3.zw = tex2Dlod( _AO_Source, float4( IN.uv - offset3, 0, 0 ) ).xy;
			s2.zw = tex2Dlod( _AO_Source, float4( IN.uv - offset2, 0, 0 ) ).xy;
			s1.zw = tex2Dlod( _AO_Source, float4( IN.uv - offset1, 0, 0 ) ).xy;
			s1.xy = tex2Dlod( _AO_Source, float4( IN.uv + offset1, 0, 0 ) ).xy;
			s2.xy = tex2Dlod( _AO_Source, float4( IN.uv + offset2, 0, 0 ) ).xy;
			s3.xy = tex2Dlod( _AO_Source, float4( IN.uv + offset3, 0, 0 ) ).xy;

			const half sharpness = ComputeSharpness( center.y );
			const half falloff = ComputeFalloff( 3 );

			half4 w12 = CrossBilateralWeight( half4( 1, 1, 2, 2 ), half4( s1.yw, s2.yw ), center.y, sharpness, falloff );
			half2 w3 = CrossBilateralWeight( ( 3 ).xx, s3.yw, center.y, sharpness, falloff );

			half ao = center.x + dot( half4( s1.xz, s2.xz ), w12 ) + dot( s3.xz, w3 );
			ao /= 1 + dot( ( 1 ).xxxx, w12 ) + dot( ( 1 ).xx, w3 );

			return half4( ao, center.y, 0, 0 );
		}

		half4 blur1D_4x( v2f_img IN, float2 deltaUV )
		{
			half2 center = tex2Dlod( _AO_Source, float4( IN.uv, 0, 0 ) ).xy;

			const float2 offset1 = 1 * deltaUV;
			const float2 offset2 = 2 * deltaUV;
			const float2 offset3 = 3 * deltaUV;
			const float2 offset4 = 4 * deltaUV;

			half4 s1, s2, s3, s4;
			s4.zw = tex2Dlod( _AO_Source, float4( IN.uv - offset4, 0, 0 ) ).xy;
			s3.zw = tex2Dlod( _AO_Source, float4( IN.uv - offset3, 0, 0 ) ).xy;
			s2.zw = tex2Dlod( _AO_Source, float4( IN.uv - offset2, 0, 0 ) ).xy;
			s1.zw = tex2Dlod( _AO_Source, float4( IN.uv - offset1, 0, 0 ) ).xy;
			s1.xy = tex2Dlod( _AO_Source, float4( IN.uv + offset1, 0, 0 ) ).xy;
			s2.xy = tex2Dlod( _AO_Source, float4( IN.uv + offset2, 0, 0 ) ).xy;
			s3.xy = tex2Dlod( _AO_Source, float4( IN.uv + offset3, 0, 0 ) ).xy;
			s4.xy = tex2Dlod( _AO_Source, float4( IN.uv + offset4, 0, 0 ) ).xy;

			const half sharpness = ComputeSharpness( center.y );
			const half falloff = ComputeFalloff( 4 );

			half4 w12 = CrossBilateralWeight( half4( 1, 1, 2, 2 ), half4( s1.yw, s2.yw ), center.y, sharpness, falloff );
			half4 w34 = CrossBilateralWeight( half4( 3, 3, 4, 4 ), half4( s3.yw, s4.yw ), center.y, sharpness, falloff );

			half ao = center.x + dot( half4( s1.xz, s2.xz ), w12 ) + dot( half4( s3.xz, s4.xz ), w34 );
			ao /= 1 + dot( ( 1 ).xxxx, w12 ) + dot( ( 1 ).xxxx, w34 );

			return half4( ao, center.y, 0, 0 );
		}

		v2f_img vert( appdata_img v )
		{
			v2f_img OUT;
			OUT.pos = mul( _AO_CameraProj, float4( v.vertex.xy, 0, 1 ) );
		#ifdef UNITY_HALF_TEXEL_OFFSET
			OUT.pos.xy += ( 1.0 / _AO_Target_TexelSize.zw ) * float2( -1, 1 );
		#endif
			OUT.uv = ComputeScreenPos( OUT.pos ).xy;
			return OUT;
		}
	ENDCG
	SubShader {
		ZTest Always Cull Off ZWrite Off

		// 0 => BLUR HORIZONTAL R:1
		Pass {
			CGPROGRAM
				half4 frag( v2f_img IN ) : SV_Target
				{
					return blur1D_1x( IN, float2( _AO_Source_TexelSize.x, 0 ) );
				}
			ENDCG
		}

		// 1 => BLUR VERTICAL R:1
		Pass {
 			CGPROGRAM
				half4 frag( v2f_img IN ) : SV_Target
				{
					return blur1D_1x( IN, float2( 0, _AO_Source_TexelSize.y ) );
				}
			ENDCG
		}

		// 2 => BLUR HORIZONTAL R:2
		Pass {
			CGPROGRAM
				half4 frag( v2f_img IN ) : SV_Target
				{
					return blur1D_2x( IN, float2( _AO_Source_TexelSize.x, 0 ) );
				}
			ENDCG
		}

		// 3 => BLUR VERTICAL R:2
		Pass {
 			CGPROGRAM
				half4 frag( v2f_img IN ) : SV_Target
				{
					return blur1D_2x( IN, float2( 0, _AO_Source_TexelSize.y ) );
				}
			ENDCG
		}

		// 4 => BLUR HORIZONTAL R:3
		Pass {
			CGPROGRAM
				half4 frag( v2f_img IN ) : SV_Target
				{
					return blur1D_3x( IN, float2( _AO_Source_TexelSize.x, 0 ) );
				}
			ENDCG
		}

		// 5 => BLUR VERTICAL R:3
		Pass {
 			CGPROGRAM
				half4 frag( v2f_img IN ) : SV_Target
				{
					return blur1D_3x( IN, float2( 0, _AO_Source_TexelSize.y ) );
				}
			ENDCG
		}

		// 6 => BLUR HORIZONTAL R:4
		Pass {
			CGPROGRAM
				half4 frag( v2f_img IN ) : SV_Target
				{
					return blur1D_4x( IN, float2( _AO_Source_TexelSize.x, 0 ) );
				}
			ENDCG
		}

		// 7 => BLUR VERTICAL R:4
		Pass {
 			CGPROGRAM
				half4 frag( v2f_img IN ) : SV_Target
				{
					return blur1D_4x( IN, float2( 0, _AO_Source_TexelSize.y ) );
				}
			ENDCG
		}
	}
	Fallback Off
}
