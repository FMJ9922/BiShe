// Amplify Occlusion - Robust Ambient Occlusion for Unity Pro
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/Amplify Occlusion/Occlusion" {
	Properties {
		_AO_RandomTexture( "Random 4x4", 2D ) = "gray" {}
	}
	CGINCLUDE
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.0

		#include "UnityCG.cginc"

		#define NORMALS_NONE ( 0 )
		#define NORMALS_CAMERA ( 1 )
		#define NORMALS_GBUFFER ( 2 )
		#define NORMALS_GBUFFER_OCTA_ENCODED ( 3 )

		sampler2D _CameraGBufferTexture2;
		sampler2D_float _CameraDepthTexture;
		float4 _CameraDepthTexture_TexelSize;
		sampler2D _CameraDepthNormalsTexture;

		float4x4 _AO_CameraProj;
		float4x4 _AO_CameraView;

		sampler2D _AO_Source;
		float4 _AO_Source_TexelSize;
		float4 _AO_Target_TexelSize;

		sampler2D _AO_GBufferAlbedo;
		sampler2D _AO_GBufferEmission;

		sampler2D _AO_RandomTexture;
		sampler2D _AO_OcclusionTexture;

		sampler2D _AO_DepthTexture;
		sampler2D _AO_NormalTexture;

		sampler2D _AO_OcclusionAtlas;

		float2 _AO_LayerOffset;
		float4 _AO_LayerRandom;

		float2 _AO_LayerOffset0, _AO_LayerOffset1, _AO_LayerOffset2, _AO_LayerOffset3;
		float2 _AO_LayerOffset4, _AO_LayerOffset5, _AO_LayerOffset6, _AO_LayerOffset7;

		float4 _AO_Buffer_PadScale;
		float4 _AO_Buffer_TexelSize;
		float4 _AO_QuarterBuffer_TexelSize;
		float4 _AO_UVToView;

		half4 _AO_Levels;
		float _AO_HalfProjScale;
		float _AO_Radius;
		float _AO_PowExponent;
		float _AO_Bias;
		float _AO_Multiplier;

		float2 _AO_FadeParams;
		float3 _AO_FadeValues;

		struct DepthNormalOutput
		{
			float depth : SV_Target0;
			half4 normal : SV_Target1;
		};

		struct DepthOutput4
		{
			float4 depth0 : SV_Target0;
			float4 depth1 : SV_Target1;
			float4 depth2 : SV_Target2;
			float4 depth3 : SV_Target3;
		};

		struct NormalOutput4
		{
			half4 normal0 : SV_Target0;
			half4 normal1 : SV_Target1;
			half4 normal2 : SV_Target2;
			half4 normal3 : SV_Target3;
		};

		struct DeferredOutput
		{
			half4 albedo : SV_Target0;
			half4 emission : SV_Target1;
		};

		float SampleEyeDepthFromCamera( float4 uv )
		{
			// depth
			return LinearEyeDepth( SAMPLE_DEPTH_TEXTURE_LOD( _CameraDepthTexture, uv ) );

			// depth+normals
			//float linear01Depth = DecodeFloatRG( tex2Dlod( _CameraDepthNormalsTexture, uv ).zw );
			//return linear01Depth * _ProjectionParams.z - ( _ProjectionParams.z / 65535 );
		}

		float3 FetchPosition( float2 uv, bool reinterleave )
		{
			float viewDepth;
			if ( reinterleave )
				viewDepth = SAMPLE_DEPTH_TEXTURE_LOD( _AO_DepthTexture, float4( uv, 0, 0 ) );
			else
				viewDepth = SampleEyeDepthFromCamera( float4( uv * _AO_Buffer_PadScale.xy, 0, 0 ) );
			return float3( ( uv * _AO_UVToView.xy + _AO_UVToView.zw ) * viewDepth, viewDepth );
		}

		float3 FetchNormal( float2 uv )
		{
			return tex2Dlod( _AO_NormalTexture, float4( uv, 0, 0 ) ).rgb * 2 - 1;
		}

		half3 ComputeNormal( float2 uv, const int source )
		{
			if ( source == NORMALS_CAMERA )
			{
				half3 N = DecodeViewNormalStereo( tex2D( _CameraDepthNormalsTexture, uv * _AO_Buffer_PadScale.xy ) );
				return half3( N.x, -N.yz );
			}
			else if ( source == NORMALS_GBUFFER || source == NORMALS_GBUFFER_OCTA_ENCODED )
			{
				half4 gbuffer2 = tex2D( _CameraGBufferTexture2, uv * _AO_Buffer_PadScale.xy );
				half3 N = gbuffer2.rgb * 2 - 1;
				if ( source == NORMALS_GBUFFER_OCTA_ENCODED && gbuffer2.a < 1 )
				{
					N.z = 1 - abs( N.x ) - abs( N.y );
					N.xy = ( N.z >= 0 ) ? N.xy : ( ( 1 - abs( N.yx ) ) * sign( N.xy ) );
				}
				N = normalize( mul( ( float3x3 ) _AO_CameraView, N ) );
				return half3( N.x, -N.yz );
			}
			else
			{
				float3 c = FetchPosition( uv, false );
				float3 r = FetchPosition( uv + float2( +_AO_Buffer_TexelSize.x, 0 ), false );
				float3 l = FetchPosition( uv + float2( -_AO_Buffer_TexelSize.x, 0 ), false );
				float3 t = FetchPosition( uv + float2( 0, +_AO_Buffer_TexelSize.y ), false );
				float3 b = FetchPosition( uv + float2( 0, -_AO_Buffer_TexelSize.y ), false );
				float3 vr = ( r - c ), vl = ( c - l ), vt = ( t - c ), vb = ( c - b );
				float3 min_horiz = ( dot( vr, vr ) < dot( vl, vl ) ) ? vr : vl;
				float3 min_vert = ( dot( vt, vt ) < dot( vb, vb ) ) ? vt : vb;
				return normalize( cross( min_horiz, min_vert ) );
			}
		}

		float4 FullDepth( v2f_img IN )
		{
			return SampleEyeDepthFromCamera( float4( IN.uv, 0, 0 ) ).xxxx;
		}

		half4 FullNormal( v2f_img IN, const int source )
		{
			return half4( ComputeNormal( IN.uv, source ) * 0.5 + 0.5, 0 );
		}

		float4 DeinterleaveDepth1( v2f_img IN ) : SV_Target
		{
			float2 uv = ( floor( IN.uv * _AO_QuarterBuffer_TexelSize.zw ) * 4 + _AO_LayerOffset0 ) * _AO_Buffer_TexelSize.xy;
			return SampleEyeDepthFromCamera( float4( uv * _AO_Buffer_PadScale.xy, 0, 0 ) );
		}

		half4 DeinterleaveNormal1( v2f_img IN, const int source ) : SV_Target
		{
			float2 uv = ( floor( IN.uv * _AO_QuarterBuffer_TexelSize.zw ) * 4 + _AO_LayerOffset0 ) * _AO_Buffer_TexelSize.xy;
			return half4( ComputeNormal( uv, source ) * 0.5 + 0.5, 0 );
		}

		DepthOutput4 DeinterleaveDepth4( v2f_img IN )
		{
			float2 uv0 = ( floor( IN.uv * _AO_QuarterBuffer_TexelSize.zw ) * 4 + _AO_LayerOffset0 ) * _AO_Buffer_TexelSize.xy * _AO_Buffer_PadScale.xy;
			float2 uv1 = ( floor( IN.uv * _AO_QuarterBuffer_TexelSize.zw ) * 4 + _AO_LayerOffset1 ) * _AO_Buffer_TexelSize.xy * _AO_Buffer_PadScale.xy;
			float2 uv2 = ( floor( IN.uv * _AO_QuarterBuffer_TexelSize.zw ) * 4 + _AO_LayerOffset2 ) * _AO_Buffer_TexelSize.xy * _AO_Buffer_PadScale.xy;
			float2 uv3 = ( floor( IN.uv * _AO_QuarterBuffer_TexelSize.zw ) * 4 + _AO_LayerOffset3 ) * _AO_Buffer_TexelSize.xy * _AO_Buffer_PadScale.xy;

			DepthOutput4 OUT;
			OUT.depth1 = SampleEyeDepthFromCamera( float4( uv1, 0, 0 ) ).xxxx;
			OUT.depth2 = SampleEyeDepthFromCamera( float4( uv2, 0, 0 ) ).xxxx;
			OUT.depth3 = SampleEyeDepthFromCamera( float4( uv3, 0, 0 ) ).xxxx;
			OUT.depth0 = SampleEyeDepthFromCamera( float4( uv0, 0, 0 ) ).xxxx;
			return OUT;
		}

		NormalOutput4 DeinterleaveNormal4( v2f_img IN, const int source )
		{
			float2 uv0 = ( floor( IN.uv * _AO_QuarterBuffer_TexelSize.zw ) * 4 + _AO_LayerOffset0 ) * _AO_Buffer_TexelSize.xy;
			float2 uv1 = ( floor( IN.uv * _AO_QuarterBuffer_TexelSize.zw ) * 4 + _AO_LayerOffset1 ) * _AO_Buffer_TexelSize.xy;
			float2 uv2 = ( floor( IN.uv * _AO_QuarterBuffer_TexelSize.zw ) * 4 + _AO_LayerOffset2 ) * _AO_Buffer_TexelSize.xy;
			float2 uv3 = ( floor( IN.uv * _AO_QuarterBuffer_TexelSize.zw ) * 4 + _AO_LayerOffset3 ) * _AO_Buffer_TexelSize.xy;

			NormalOutput4 OUT;
			OUT.normal0 = half4( ComputeNormal( uv0, source ) * 0.5 + 0.5, 0 );
			OUT.normal1 = half4( ComputeNormal( uv1, source ) * 0.5 + 0.5, 0 );
			OUT.normal2 = half4( ComputeNormal( uv2, source ) * 0.5 + 0.5, 0 );
			OUT.normal3 = half4( ComputeNormal( uv3, source ) * 0.5 + 0.5, 0 );
			return OUT;
		}

		float ComputeDistanceFade( float distance )
		{
			return saturate( max( 0, distance - _AO_FadeParams.x ) * _AO_FadeParams.y );
		}

		half2 ComputeOcclusion( v2f_img IN, const int directionCount, const int stepCount, const bool cache, const int source )
		{
			float2 uv;
			float4 rand;
			if ( cache )
			{
				uv = ( floor( IN.uv * _AO_QuarterBuffer_TexelSize.zw ) * 4.0 + _AO_LayerOffset ) * _AO_Buffer_TexelSize.xy;
				rand = _AO_LayerRandom;
			}
			else
			{
				uv = IN.uv;
				rand = tex2Dlod( _AO_RandomTexture, float4( uv * ( _AO_Buffer_TexelSize.zw / 4 ), 0, 0 ) );
			}

			float3 p, n;
			p = FetchPosition( uv, cache );
			if ( cache )
				n = FetchNormal( uv );
			else
				n = ComputeNormal( uv, source );

			float2 values = lerp( float2( _AO_Radius, _AO_PowExponent ), _AO_FadeValues.yz, ComputeDistanceFade( p.z ).xx );
			float radius = values.x;
			float exponent = values.y;

			float radiusToScreen = radius * _AO_HalfProjScale;
			float negRcpR2 = -1.0 / ( radius * radius );

			float screenRadius;
			if ( cache )
				screenRadius = ( radiusToScreen / p.z ) / 4;
			else
				screenRadius = radiusToScreen / p.z;

			const float pi = 3.14159265f;
			const float alpha = 2.0 * pi / directionCount;
			float stepSize = screenRadius / ( stepCount + 1 );
			float occlusion = 0;

			for ( int i = 0; i < directionCount; i++ )
			{
				float angle = alpha * ( ( float ) i );
				float2 cos_sin = float2( cos( angle ), sin( angle ) );
				float2 dir = float2( cos_sin.x * rand.x - cos_sin.y * rand.y, cos_sin.x * rand.y + cos_sin.y * rand.x );
				float ray = rand.z * stepSize + 1.0;

				for ( int j = 0; j < stepCount; j++ )
				{
					float2 snapped_uv;
					if ( cache )
						snapped_uv = uv + round( ray * dir ) * _AO_QuarterBuffer_TexelSize.xy;
					else
						snapped_uv = uv + round( ray * dir ) * _AO_Buffer_TexelSize.xy;

					float3 s = FetchPosition( snapped_uv, cache );
					ray += stepSize;

					float3 v = s - p;
					float vv = dot( v, v );
					float nv = dot( n, v ) * rsqrt( vv );

					occlusion += saturate( nv - _AO_Bias ) * saturate( vv * negRcpR2 + 1.0 );
				}
			}

			occlusion *= _AO_Multiplier / ( directionCount * stepCount );
			occlusion = clamp( 1 - occlusion * 2, 0, 1 );

			occlusion = saturate( pow( occlusion, exponent ) );

			return half4( occlusion, p.z, 0, 0 );
		}

		half4 CombineDownsampledOcclusionDepth( v2f_img IN )
		{
			half occlusion = tex2D( _AO_Source, IN.uv ).r;
			float depth = SampleEyeDepthFromCamera( float4( IN.uv, 0, 0 ) );
			return half4( occlusion, depth, 0, 0 );
		}

		half4 FetchOcclusion( v2f_img IN )
		{
			half2 occlusion_depth = tex2D( _AO_OcclusionTexture, IN.uv * _AO_Buffer_PadScale.zw ).rg;

			half occlusion = occlusion_depth.x;
			half depth = occlusion_depth.y;

			half3 tintedOcclusion = lerp( _AO_Levels.rgb, ( 1 ).xxx, occlusion );

			float intensity = lerp( _AO_Levels.a, _AO_FadeValues.x, ComputeDistanceFade( depth ) );

			return lerp( ( 1 ).xxxx, half4( tintedOcclusion.rgb, occlusion ), intensity );
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

		// 0-4 => FULL GATHER --------------------------------------------------------------
		Pass { CGPROGRAM float4 frag( v2f_img IN ) : SV_Target { return FullDepth( IN ); } ENDCG }
		Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return FullNormal( IN, NORMALS_NONE ); } ENDCG }
		Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return FullNormal( IN, NORMALS_CAMERA ); } ENDCG }
		Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return FullNormal( IN, NORMALS_GBUFFER ); } ENDCG }
		Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return FullNormal( IN, NORMALS_GBUFFER_OCTA_ENCODED ); } ENDCG }

		// 5-9 => CACHE-AWARE GATHER DEINTERLEAVE 1 --------------------------------------------------------------
		Pass { CGPROGRAM float4 frag( v2f_img IN ) : SV_Target { return DeinterleaveDepth1( IN ); } ENDCG }
		Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return DeinterleaveNormal1( IN, NORMALS_NONE ); } ENDCG }
		Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return DeinterleaveNormal1( IN, NORMALS_CAMERA ); } ENDCG }
		Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return DeinterleaveNormal1( IN, NORMALS_GBUFFER ); } ENDCG }
		Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return DeinterleaveNormal1( IN, NORMALS_GBUFFER_OCTA_ENCODED ); } ENDCG }

		// 10-14 => CACHE-AWARE GATHER DEINTERLEAVE 4 --------------------------------------------------------------
		Pass { CGPROGRAM DepthOutput4 frag( v2f_img IN ) { return DeinterleaveDepth4( IN ); } ENDCG }
		Pass { CGPROGRAM NormalOutput4 frag( v2f_img IN ) { return DeinterleaveNormal4( IN, NORMALS_NONE ); } ENDCG }
		Pass { CGPROGRAM NormalOutput4 frag( v2f_img IN ) { return DeinterleaveNormal4( IN, NORMALS_CAMERA ); } ENDCG }
		Pass { CGPROGRAM NormalOutput4 frag( v2f_img IN ) { return DeinterleaveNormal4( IN, NORMALS_GBUFFER ); } ENDCG }
		Pass { CGPROGRAM NormalOutput4 frag( v2f_img IN ) { return DeinterleaveNormal4( IN, NORMALS_GBUFFER_OCTA_ENCODED ); } ENDCG }

		// 15-18 => CACHE-AWARE OCCLUSION --------------------------------------------------------------
		Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 4, 4, true, NORMALS_NONE ), 0, 0 ); } ENDCG }
		Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 6, 4, true, NORMALS_NONE ), 0, 0 ); } ENDCG }
        Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 8, 4, true, NORMALS_NONE ), 0, 0 ); } ENDCG }
        Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 10, 6, true, NORMALS_NONE ), 0, 0 ); } ENDCG }

        // -- CACHE-AWARE REINTERLEAVE --------------------------------------------------------------
		// 19 => REINTERLEAVE
		Pass {
			CGPROGRAM
				half4 frag( v2f_img IN ) : SV_Target
				{
					float2 offset = fmod( floor( IN.uv * _AO_Buffer_TexelSize.zw ), 4 );
					return tex2Dlod( _AO_OcclusionAtlas, float4( ( IN.uv + offset ) * 0.25, 0, 0 ) );
				}
			ENDCG
		}

		// 20-23 => FULL OCCLUSION - LOW QUALITY --------------------------------------------------------------
		Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 4, 4, false, NORMALS_NONE ), 0, 0 ); } ENDCG }
        Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 4, 4, false, NORMALS_CAMERA ), 0, 0 ); } ENDCG }
        Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 4, 4, false, NORMALS_GBUFFER ), 0, 0 ); } ENDCG }
        Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 4, 4, false, NORMALS_GBUFFER_OCTA_ENCODED ), 0, 0 ); } ENDCG }

        // 24-27 => FULL OCCLUSION / MEDIUM QUALITY --------------------------------------------------------------
        Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 6, 4, false, NORMALS_NONE ), 0, 0 ); } ENDCG }
        Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 6, 4, false, NORMALS_CAMERA ), 0, 0 ); } ENDCG }
        Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 6, 4, false, NORMALS_GBUFFER ), 0, 0 ); } ENDCG }
        Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 6, 4, false, NORMALS_GBUFFER_OCTA_ENCODED ), 0, 0 ); } ENDCG }

        // 28-31 => FULL OCCLUSION - HIGH QUALITY --------------------------------------------------------------
		Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 8, 4, false, NORMALS_NONE ), 0, 0 ); } ENDCG }
        Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 8, 4, false, NORMALS_CAMERA ), 0, 0 ); } ENDCG }
        Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 8, 4, false, NORMALS_GBUFFER ), 0, 0 ); } ENDCG }
        Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 8, 4, false, NORMALS_GBUFFER_OCTA_ENCODED ), 0, 0 ); } ENDCG }

        // 32-35 => FULL OCCLUSION / VERYHIGH QUALITY --------------------------------------------------------------
		Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 10, 6, false, NORMALS_NONE ), 0, 0 ); } ENDCG }
        Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 10, 6, false, NORMALS_CAMERA ), 0, 0 ); } ENDCG }
        Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 10, 6, false, NORMALS_GBUFFER ), 0, 0 ); } ENDCG }
        Pass { CGPROGRAM half4 frag( v2f_img IN ) : SV_Target { return half4( ComputeOcclusion( IN, 10, 6, false, NORMALS_GBUFFER_OCTA_ENCODED ), 0, 0 ); } ENDCG }

		// -- APPLICATION METHODS --------------------------------------------------------------
		// 36 => APPLY DEBUG
		Pass {
			CGPROGRAM
				half4 frag( v2f_img IN ) : SV_Target
				{
					return half4( LinearToGammaSpace( FetchOcclusion( IN ).rgb ), 1 );
				}
			ENDCG
		}

		// 37 => APPLY DEFERRED
		Pass {
			Blend DstColor Zero, DstAlpha Zero
			CGPROGRAM
				DeferredOutput frag( v2f_img IN )
				{
					half4 occlusion = FetchOcclusion( IN );

					DeferredOutput OUT;
					OUT.albedo = half4( 1, 1, 1, occlusion.a );
					OUT.emission = half4( occlusion.rgb, 1 );
					return OUT;
				}
			ENDCG
		}

		// 38 => APPLY DEFERRED (LOG)
		Pass {
			CGPROGRAM
				DeferredOutput frag( v2f_img IN )
				{
					half4 occlusion = FetchOcclusion( IN );

					half4 albedo = tex2D( _AO_GBufferAlbedo, IN.uv );
					half4 emission = tex2D( _AO_GBufferEmission, IN.uv );

					albedo.a *= occlusion.a;

					emission.rgb = -log2( emission.rgb );
					emission.rgb *= occlusion.rgb;
					emission.rgb = exp2( -emission.rgb );

					DeferredOutput OUT;
					OUT.albedo = albedo;
					OUT.emission = emission;
					return OUT;
				}
			ENDCG
		}

		// 39 => APPLY POST-EFFECT
		Pass {
			Blend DstColor Zero
			CGPROGRAM
				half4 frag( v2f_img IN ) : SV_Target
				{
					return half4( FetchOcclusion( IN ).rgb, 1 );
				}
			ENDCG
		}

		// 40 => APPLY POST-EFFECT (LOG)
		Pass {
			CGPROGRAM
				half4 frag( v2f_img IN ) : SV_Target
				{
					half4 occlusion = FetchOcclusion( IN );

					half4 emission = tex2D( _AO_GBufferEmission, IN.uv );

					emission.rgb = -log2( emission.rgb );
					emission.rgb *= occlusion.rgb;
					emission.rgb = exp2( -emission.rgb );

					return emission;
				}
			ENDCG
		}

		// 41 => COMBINE DOWNSAMPLED OCCLUSION DEPTH
		Pass {
			CGPROGRAM
				half4 frag( v2f_img IN ) : SV_Target
				{
					return CombineDownsampledOcclusionDepth( IN );
				}
			ENDCG
		}
    }
	Fallback Off
}
