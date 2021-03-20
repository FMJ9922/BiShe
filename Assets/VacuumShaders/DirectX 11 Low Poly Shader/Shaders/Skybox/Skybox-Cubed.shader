// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "VacuumShaders/DirectX 11 Low Poly/Skybox" 
{
	Properties 
	{
		[VacuumShadersSkyboxType] _Skybox_Type("", Float) = 0

		_Tint ("Tint Color", Color) = (.5, .5, .5, .5)
		[Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
		_Rotation ("Rotation", Range(0, 360)) = 0
		_Blur("Roughness", Range(0, 1)) = 0
		[NoScaleOffset] _Tex ("Cubemap   (HDR)", Cube) = "grey" {}		
	}

SubShader 
{
	Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
	Cull Off ZWrite Off

	Pass {
		
		CGPROGRAM
		#pragma vertex vert
		#pragma geometry geom
		#pragma fragment frag
		#include "Assets/VacuumShaders/DirectX 11 Low Poly Shader/Shaders/cginc/Platform.cginc"

		#include "UnityCG.cginc"

		samplerCUBE _Tex;
		half4 _Tex_HDR;
		half4 _Tint;
		half _Exposure;
		float _Rotation;
		half _Blur;

		float4 RotateAroundYInDegrees (float4 vertex, float degrees)
		{
			float alpha = degrees * UNITY_PI / 180.0;
			float sina, cosa;
			sincos(alpha, sina, cosa);
			float2x2 m = float2x2(cosa, -sina, sina, cosa);
			return float4(mul(m, vertex.xz), vertex.yw).xzyw;
		}
		
		struct appdata_t 
		{
			float4 vertex : POSITION;
		};

		struct v2f_surf 
		{
			float4 pos : SV_POSITION;
			fixed4 color : COLOR0;
		};


#define V_GEOMETRY_SAVE_LOWPOLY_COLOR
#include "Assets/VacuumShaders/DirectX 11 Low Poly Shader/Shaders/cginc/Core.cginc"


    	v2f_surf vert (appdata_t v)
		{
			v2f_surf o;
			o.pos = UnityObjectToClipPos(RotateAroundYInDegrees(v.vertex, _Rotation));


			float4 rotPosition = RotateAroundYInDegrees(v.vertex, _Rotation);
			o.pos = UnityObjectToClipPos(rotPosition);
						
			o.color = texCUBElod (_Tex, float4(v.vertex.xyz, _Blur * 10));
			o.color.rgb = DecodeHDR (o.color, _Tex_HDR) * _Tint.rgb * unity_ColorSpaceDouble.rgb * _Exposure;
			o.color.a = 1;

			return o;
		}

		fixed4 frag (v2f_surf i) : SV_Target
		{
			return i.color;
		}
		ENDCG 
	}
} 	


Fallback Off

}
