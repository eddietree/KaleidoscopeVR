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
			Cull Off

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
				float4 tangent : TANGENT;
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

				float aspect = _ScreenParams.y / _ScreenParams.x;

				float3 posCurr = v.vertex.xyz;
				float3 posNext = v.tangent.xyz;
				float3 posPrev = v.normal.xyz;

				// NDC
				float4 posCurrNDC = mul(UNITY_MATRIX_MVP, float4(posCurr.x, posCurr.y, posCurr.z, 1.0f));
				float4 posNextNDC = mul(UNITY_MATRIX_MVP, float4(posNext.x, posNext.y, posNext.z, 1.0f));
				float4 posPrevNDC = mul(UNITY_MATRIX_MVP, float4(posPrev.x, posPrev.y, posPrev.z, 1.0f));

				// projected
				float2 posCurrXY = posCurrNDC.xy / posCurrNDC.w;
				float2 posNextXY = posNextNDC.xy / posNextNDC.w;
				float2 posPrevXY = posPrevNDC.xy / posPrevNDC.w;

				float2 vec0 = normalize(posCurrXY - posPrevXY);
				float2 vec1 = normalize(posNextXY - posCurrXY);
				float2 vecForwardAvg = normalize(vec0 + vec1);
				float2 vecUp = float2(-vecForwardAvg.y * aspect, vecForwardAvg.x);

				//posCurrNDC.y += v.uv.y * posCurrNDC.w * 0.1;
				//posCurrNDC.xy += vecUp * v.uv.y * posCurrNDC.w * 0.1;
				posCurrNDC.xy += vecUp * v.uv.y * posCurrNDC.w * 0.05;


				//posNDC.w += 1.0f;

				o.color = float4(0.0,0.0,0.0, 1.0);
				//o.color.xy = posCurrNDC.xy / posCurrNDC.w;
				//o.color.z = 0.0;
				//o.color.w = 1.0;

				o.vertex = posCurrNDC;
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
