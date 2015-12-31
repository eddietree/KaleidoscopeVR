Shader "Unlit/MeshLine"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			//#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				//UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 color: TEXCOORD6;

			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;

				float4 pos = v.vertex;
				//pos.y += v.uv.y*0.1f;

				float4 posNDC = mul(UNITY_MATRIX_MVP, pos);
				posNDC.y += v.uv.y * posNDC.w * 0.1;
				//posNDC.w += 1.0f;

				o.color = float4(1.0, 1.0, 1.0, 1.0);
				o.color.xy = posNDC.xy / posNDC.w;
				o.color.z = 0.0;
				o.color.w = 1.0;

				o.vertex = posNDC;
				o.normal = v.normal;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 col = i.color;
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
