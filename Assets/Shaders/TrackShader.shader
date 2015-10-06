Shader "Custom/TrackShader" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGBA)", 2D) = "white" {}
		_PlayerT ("PlayerT", Float) = 0.1 
	}
    SubShader 
	{
        Pass 
		{
			Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
			
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"
			
			fixed4		_Color;
			sampler2D	_MainTex;
			float		_PlayerT;
			
            struct fragmentInput
			{
                float4 position : SV_POSITION;
                float4 texcoord0 : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
            };
			
            fragmentInput vert(appdata_full i)
			{
                fragmentInput o;
                o.position = mul (UNITY_MATRIX_MVP, i.vertex);
                o.texcoord0 = i.texcoord;
				o.texcoord1 = i.texcoord1;
                return o;
            }

            fixed4 frag(fragmentInput i) : SV_Target 
			{
				float4 uv = i.texcoord0;
				float4 d = i.texcoord1;
				uv.y *= 10.0f;
				float4 c = tex2D(_MainTex, uv) * _Color;
				
				float a = abs(d.r-0.5f);
				float b = abs(_PlayerT-0.5f);
				
				float val =1.0 -  clamp(abs(a - b),0.0f,1.0f);
				val = pow(val, 16.0f);
				c *= val;
				
				c.a = max(c.a, 0.25f);
				
				return c;
            }

            ENDCG
        }
    }
}