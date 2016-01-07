Shader "Unlit/MeshLineCool"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color0("Color 0", Color) = (1.0,1.0,1.0,1.0)
		_Color1("Color 1", Color) = (1.0,1.0,1.0,1.0)
		_Thickness("Thickness", Float) = 0.02
		_ShadowRadius("Shadow Radius", Float) = 0.3
		_ShadowAlpha("Shadow Alpha", Float) = 1.0
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
	{
		Cull Off

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
		// make fog work
#pragma multi_compile_fog

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
		UNITY_FOG_COORDS(1)

			float2 uv : TEXCOORD0;
		float3 normal : NORMAL;
		float4 vertex : SV_POSITION;
		float4 color: TEXCOORD6;

	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	float4 _Color0;
	float4 _Color1;
	float _Thickness;
	float _ShadowRadius;
	float _ShadowAlpha;

	v2f vert(appdata v)
	{
		v2f o;

		float aspect = _ScreenParams.y / _ScreenParams.x;

		float3 posCurr = v.vertex.xyz;
		float3 posNext = v.tangent.xyz;
		float3 posPrev = v.normal.xyz;
		float thickness = v.tangent.w;

		// NDC
		float4 posCurrNDC = mul(UNITY_MATRIX_MVP, float4(posCurr.x, posCurr.y, posCurr.z, 1.0f));
		float4 posNextNDC = mul(UNITY_MATRIX_MVP, float4(posNext.x, posNext.y, posNext.z, 1.0f));
		float4 posPrevNDC = mul(UNITY_MATRIX_MVP, float4(posPrev.x, posPrev.y, posPrev.z, 1.0f));

		// projected
		float2 posCurrXY = posCurrNDC.xy / posCurrNDC.w;
		float2 posNextXY = posNextNDC.xy / posNextNDC.w;
		float2 posPrevXY = posPrevNDC.xy / posPrevNDC.w;

		// calculate screen-space move angle
		float2 vecPrev = posCurrXY - posPrevXY;
		float2 vecNext = posNextXY - posCurrXY;
		float lenVecPrev = length(vecPrev);
		float lenVecNext = length(vecNext);

		float epsilon = 0.001;
		if (lenVecPrev > epsilon && lenVecNext > epsilon)
		{
			float2 vec0 = vecPrev / lenVecPrev;
			float2 vec1 = vecNext / lenVecNext;
			float2 vecForwardAvg = normalize(vec0 + vec1);
			float2 vecUp = float2(-vecForwardAvg.y * aspect, vecForwardAvg.x);

			// move position thickness
			float sinPulsate = sin(-_Time.y*10.0 + v.uv.x*0.3)*0.5 + 0.5;
			posCurrNDC.xy += vecUp * v.uv.y * posCurrNDC.w * _Thickness * (thickness) * sinPulsate;
		}

		//o.color = float4(sin(v.uv.x*0.1)*0.5 + 0.5,0.0,0.0, 1.0);

		float shadow = smoothstep(0.0, _ShadowRadius, length(posCurrXY));
		shadow = lerp(1.0f, shadow, _ShadowAlpha);

		o.color = lerp(_Color0, _Color1, (sin(v.uv.x*0.1 )*0.5 + 0.5)*lerp(0.95,1.0,v.uv.y)) * shadow;
		o.vertex = posCurrNDC;
		o.normal = v.normal;
		o.uv = v.uv;// TRANSFORM_TEX(v.uv, _MainTex);

		UNITY_TRANSFER_FOG(o,o.vertex);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		// sample the texture
		//fixed4 col = tex2D(_MainTex, i.uv);

		float4 color = i.color;
		color.xyz += pow(abs(i.uv.y), 15.0) * 0.05;
		//color.xyz += lerp(0.0,0.015,sin(i.uv.y*25.0)*0.5+0.5);

		fixed4 col = color;

	// apply fog
	UNITY_APPLY_FOG(i.fogCoord, col);

	return col;
	}
		ENDCG
	}
	}
}
