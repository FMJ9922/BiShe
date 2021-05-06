Shader "Custom/GpuTest"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _CenterX("CenterX",float) = 0.5
        _CenterY("CenterY",float) = 0.5
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" "Queue" = "Transparent" }
            LOD 100
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_instancing

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    UNITY_FOG_COORDS(1)
                    float4 vertex : SV_POSITION;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                UNITY_INSTANCING_BUFFER_START(Props)
                    UNITY_DEFINE_INSTANCED_PROP(float, _CenterX)
                    UNITY_DEFINE_INSTANCED_PROP(float, _CenterY)
                UNITY_INSTANCING_BUFFER_END(Props)

                sampler2D _MainTex;
                float4 _MainTex_ST;

                v2f vert(appdata v)
                {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    fixed4 col = tex2D(_MainTex, i.uv);
                    float cx = UNITY_ACCESS_INSTANCED_PROP(Props, _CenterX);
                    float cy = UNITY_ACCESS_INSTANCED_PROP(Props, _CenterY);
                    float2 offset = abs(i.uv - float2(cx, cy));
                    return fixed4(col.rgb,offset.x + offset.y);
                }
                ENDCG
            }
        }
}
