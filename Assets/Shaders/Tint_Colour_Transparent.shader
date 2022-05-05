// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Tint_Colour_Transparent"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_Normal("Normal", 2D) = "bump" {}
		[Toggle]_TwoColours("Two Colours", Float) = 0
		[Toggle]_NormalMode("Normal Mode", Float) = 1
		_Noise("Noise", 2D) = "white" {}
		[HDR]_Color1("Color 1", Color) = (1,1,1,0)
		[Toggle]_UVPanning("UV Panning", Float) = 0
		[HDR]_Color2("Color 2", Color) = (1,1,1,0)
		[HDR]_TintAmount("Tint Amount", Range( 0 , 10)) = 5
		[HDR]_EmissivePower("Emissive Power", Range( 0 , 15)) = 0
		[HDR]_StrobeSpeed("Strobe Speed", Float) = 0
		[HDR]_UPanningSpeed("U Panning Speed", Range( -1 , 1)) = 0
		[HDR]_VPanningSpeed("V Panning Speed", Range( -1 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float2 texcoord_0;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform float _UVPanning;
		uniform float _TwoColours;
		uniform float4 _Color1;
		uniform float4 _Color2;
		uniform float _StrobeSpeed;
		uniform float _NormalMode;
		uniform float _TintAmount;
		uniform sampler2D _Noise;
		uniform float _UPanningSpeed;
		uniform float _VPanningSpeed;
		uniform float _EmissivePower;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.texcoord_0.xy = v.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			float3 tex2DNode24 = UnpackNormal( tex2D( _Normal,uv_Normal) );
			o.Normal = tex2DNode24;
			float3 worldViewDir = normalize( UnityWorldSpaceViewDir( i.worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			float fresnelFinalVal18 = (0.0 + 1.0*pow( 1.0 - dot( lerp(vertexNormal,BlendNormals( tex2DNode24 , vertexNormal ),_NormalMode), worldViewDir ) , _TintAmount));
			float temp_output_18_0 = fresnelFinalVal18;
			float4 temp_output_17_0 = ( lerp(_Color1,lerp( _Color1 , _Color2 , (0.0 + (sin( ( _StrobeSpeed * _Time.y ) ) - -1.0) * (1.0 - 0.0) / (1.0 - -1.0)) ),_TwoColours) * temp_output_18_0 );
			float4 appendResult33 = float4( _UPanningSpeed , _VPanningSpeed , 0 , 0 );
			o.Emission = ( lerp(temp_output_17_0,( temp_output_17_0 * tex2D( _Noise,( float4( i.texcoord_0, 0.0 , 0.0 ) + ( appendResult33 * _Time.y ) ).xy) ),_UVPanning) * _EmissivePower ).xyz;
			o.Alpha = temp_output_18_0;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			# include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 worldPos : TEXCOORD6;
				float4 tSpace0 : TEXCOORD1;
				float4 tSpace1 : TEXCOORD2;
				float4 tSpace2 : TEXCOORD3;
				float4 texcoords01 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				fixed3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.texcoords01 = float4( v.texcoord.xy, v.texcoord1.xy );
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.texcoords01.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=7003
2208;176;1368;809;113.6367;860.6349;1.9;True;True
Node;AmplifyShaderEditor.RangedFloatNode;4;-952,-261;Float;False;Property;_StrobeSpeed;Strobe Speed;9;1;[HDR];0;0;0;FLOAT
Node;AmplifyShaderEditor.SimpleTimeNode;6;-947,-169;Float;False;0;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-742,-230;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;11;-613,-417;Float;False;Constant;_Float1;Float 1;0;0;0;0;0;FLOAT
Node;AmplifyShaderEditor.SinOpNode;8;-572,-196;Float;False;0;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;12;-613.2063,-494.9561;Float;False;Constant;_Float2;Float 2;0;0;-1;0;0;FLOAT
Node;AmplifyShaderEditor.NormalVertexDataNode;20;-1020.684,156.6606;Float;False;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;24;405.9786,-1081.248;Float;True;Property;_Normal;Normal;1;0;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;40;-611.5332,-333.4323;Float;False;Constant;_Float1;Float 1;0;0;1;0;0;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;35;-696.6735,571.9722;Float;False;Property;_VPanningSpeed;V Panning Speed;11;1;[HDR];0;-1;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;34;-697.7986,501.0973;Float;False;Property;_UPanningSpeed;U Panning Speed;10;1;[HDR];0;-1;1;FLOAT
Node;AmplifyShaderEditor.AppendNode;33;-340.7715,557.347;Float;False;FLOAT4;0;0;0;0;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;FLOAT4
Node;AmplifyShaderEditor.BlendNormalsNode;21;-781.3947,71.26066;Float;False;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT3
Node;AmplifyShaderEditor.ColorNode;15;-396.2181,-628.1647;Float;False;Property;_Color1;Color 1;5;1;[HDR];1,1,1,0;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;14;-398.3491,-451.3131;Float;False;Property;_Color2;Color 2;6;1;[HDR];1,1,1,0;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.TFHCRemap;9;-402,-237;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;0.0;False;4;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.LerpOp;13;-130.9409,-244.6313;Float;False;0;COLOR;0.0,0,0,0;False;1;COLOR;0.0;False;2;FLOAT;0.0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-154.0214,511.2221;Float;False;0;FLOAT4;0.0;False;1;FLOAT;0.0;False;FLOAT4
Node;AmplifyShaderEditor.RangedFloatNode;44;-636.0337,337.3619;Float;False;Property;_TintAmount;Tint Amount;7;1;[HDR];5;0;10;FLOAT
Node;AmplifyShaderEditor.ToggleSwitchNode;19;-566.2883,196.6611;Float;False;Property;_NormalMode;Normal Mode;2;1;[Toggle];1;0;FLOAT3;0.0;False;1;FLOAT3;0.0;False;FLOAT3
Node;AmplifyShaderEditor.TextureCoordinatesNode;31;-209.1461,371.7222;Float;False;0;-1;2;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;FLOAT2;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ToggleSwitchNode;16;81.51022,-249.3382;Float;False;Property;_TwoColours;Two Colours;2;1;[Toggle];0;0;COLOR;0.0;False;1;COLOR;0.0;False;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;30;59.10388,526.9722;Float;False;0;FLOAT2;0.0;False;1;FLOAT4;0.0,0;False;FLOAT4
Node;AmplifyShaderEditor.FresnelNode;18;-344.5016,152.6522;Float;False;0;FLOAT3;0,0,0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;5.0;False;FLOAT
Node;AmplifyShaderEditor.SamplerNode;29;230.059,432.5502;Float;True;Property;_Noise;Noise;4;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;374.8109,-190.4367;Float;False;0;COLOR;0,0,0,0;False;1;FLOAT;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;583.1595,32.90694;Float;False;0;COLOR;0.0;False;1;FLOAT4;0,0,0,0;False;FLOAT4
Node;AmplifyShaderEditor.RangedFloatNode;37;844.7253,77.57057;Float;False;Property;_EmissivePower;Emissive Power;8;1;[HDR];0;0;15;FLOAT
Node;AmplifyShaderEditor.ToggleSwitchNode;27;730.8192,-196.6072;Float;False;Property;_UVPanning;UV Panning;6;1;[Toggle];0;0;COLOR;0.0;False;1;FLOAT4;0.0;False;FLOAT4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;1092.225,-201.429;Float;False;0;FLOAT4;0,0,0,0;False;1;FLOAT;0,0,0,0;False;FLOAT4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1625.465,-429.4818;Float;False;True;2;Float;ASEMaterialInspector;0;Standard;Tint_Colour_Transparent;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Transparent;0.5;True;True;0;False;Transparent;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;SrcAlpha;OneMinusSrcAlpha;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;Relative;0;;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;13;OBJECT;0.0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False
WireConnection;7;0;4;0
WireConnection;7;1;6;0
WireConnection;8;0;7;0
WireConnection;33;0;34;0
WireConnection;33;1;35;0
WireConnection;21;0;24;0
WireConnection;21;1;20;0
WireConnection;9;0;8;0
WireConnection;9;1;12;0
WireConnection;9;2;40;0
WireConnection;9;3;11;0
WireConnection;9;4;40;0
WireConnection;13;0;15;0
WireConnection;13;1;14;0
WireConnection;13;2;9;0
WireConnection;32;0;33;0
WireConnection;32;1;6;0
WireConnection;19;0;20;0
WireConnection;19;1;21;0
WireConnection;16;0;15;0
WireConnection;16;1;13;0
WireConnection;30;0;31;0
WireConnection;30;1;32;0
WireConnection;18;0;19;0
WireConnection;18;3;44;0
WireConnection;29;1;30;0
WireConnection;17;0;16;0
WireConnection;17;1;18;0
WireConnection;28;0;17;0
WireConnection;28;1;29;0
WireConnection;27;0;17;0
WireConnection;27;1;28;0
WireConnection;36;0;27;0
WireConnection;36;1;37;0
WireConnection;0;1;24;0
WireConnection;0;2;36;0
WireConnection;0;9;18;0
ASEEND*/
//CHKSM=ABCCA96C04F9DD93681ED164C7B56A521B32A648