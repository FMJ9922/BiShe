Shader "Custom/PlantImageEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SizeX("SizeX", Float) = 4
        _SizeY("SizeY", Float) = 4
        _Progress("Pregress", Float) = 0
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 400
        Cull Off

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _SizeX;
            fixed _SizeY;
            fixed _Progress;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                int index = _SizeX * _SizeY * (_Progress-1)-1;
                int indexY = floor(index / _SizeX);
                int indexX = index - indexY * _SizeX;

                half2 uv = i.uv + half2(indexX, -indexY-1);
                uv.x /= _SizeX;
                uv.y /= _SizeY;

                fixed4 c = tex2D(_MainTex, uv);
                return c;
            }
            ENDCG
        }
    }
}
