﻿Shader "Custom/CelEffectsDottedOutline"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_RampTex("Ramp", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_OutlineExtrusion("Outline Extrusion", float) = 0
		_OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
		_OutlineColor2("Outline Color2", Color) = (0, 0, 0, 1)
		_OutlineDot("Outline Dot", float) = 0.25
		_OutlineDot2("Outline Dot Distance", float) = 0.5
		_OutlineSpeed("Outline Dot Speed", float) = 50.0
		_SourcePos("Source Position", vector) = (0, 0, 0, 0)
		_Direction("Direction", int) = 1
		_Active("Active", int) = 1
	}

	SubShader
	{
		Tags {"Queue"="Transparent" "RenderType"="Transparent" }
		// Outline pass
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			// Won't draw where it sees ref value 4
			Cull OFF
			ZWrite OFF
			ZTest OFF
			Stencil
			{
				Ref 4
				Comp notequal
				Fail keep
				Pass replace
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			// Properties
			float4 _OutlineColor;
			float4 _OutlineColor2;
			float  _OutlineSize;
			float  _OutlineExtrusion;
			float  _OutlineDot;
			float  _OutlineDot2;
			float  _OutlineSpeed;
			float4 _SourcePos;
			int _Direction;
			int _Active;

			struct vertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				float4 screenCoord : TEXCOORD0;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				float4 newPos = input.vertex;

				// normal extrusion technique
				float3 normal = normalize(input.normal);
				newPos += float4(normal, 0.0) * _OutlineExtrusion;

				// convert to world space
				output.pos = UnityObjectToClipPos(newPos);

				// get screen coordinates
				output.screenCoord = ComputeScreenPos(output.pos);

				return output;
			}

			float4 frag(vertexInput input) : COLOR
			{
				// dotted line with animation
				// if you want to remove the animation, remove "+ _Time * _OutlineSpeed"
				float2 pos = input.vertex.x + _Time.y * _OutlineSpeed * _Direction;
                float skip = sin(_OutlineDot*abs(distance(_SourcePos.xy, pos))) + _OutlineDot2;
				
				float4 color = _OutlineColor;
				if (_Active > 0) {
					if (skip > 0) {
						color = _OutlineColor;
					} else {
						color = _OutlineColor2;
					}
				}
				return color;
			}

			ENDCG
		}
	}
}