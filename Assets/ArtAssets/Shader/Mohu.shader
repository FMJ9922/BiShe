// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Mohu"
{
    properties
    {
        _MainTex("MainTex",2D) = ""{}
    }
        Subshader
    {
        pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"      
            sampler2D  _MainTex;
            float4 _MainTex_ST;

            struct v2f
            {
                float4 pos : POSITION;
                fixed2 uv : TEXCOORD0;
                fixed2 uv2 : TEXCOORD1;
                float z : TEXCOORD2;
            };
            v2f vert(appdata_full a)
            {
                v2f v;
                v.pos = UnityObjectToClipPos(a.vertex);
                v.uv = TRANSFORM_TEX(a.texcoord,_MainTex);
                v.z = mul(unity_ObjectToWorld,a.vertex).z;
                return v;
            }
            float4 frag(v2f f) :COLOR
            {
                float data = 0.01;
                fixed4 color;
                /*  // 1.顶点偏移多次采样
                float2 uv = f.uv;
                color = tex2D(_MainTex,f.uv);
                uv.x = f.uv.x + data;
                color += tex2D(_MainTex,uv);
                uv.x = f.uv.x - data;
                color += tex2D(_MainTex,uv);
                uv.y = f.uv.y + data;
                color += tex2D(_MainTex,uv);
                uv.y = f.uv.y - data;
                color += tex2D(_MainTex,uv);
                color/=5;
　　　　　　　　　　*/

          // 2.  内置函数实现
          //color = tex2D(_MainTex,f.uv,float2(0.01,0.01),float2(0.01,0.01));

          //   3.偏导数计算方案    旋转物体动态改变模糊度  正面清晰，其他面都模糊，可以试试
          float2 dsdx = ddx(f.z) * 5;
          float2 dsdy = ddy(f.z) * 5;
          color = tex2D(_MainTex,f.uv,dsdx,dsdy);

          return  color;
      }

      ENDCG
  }
    }
}