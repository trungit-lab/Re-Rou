// Made with Amplify Shader Editor v1.9.8.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Water_Fall"
{
	Properties
	{
		_Depth("Depth", Float) = 2.1
		_Smoothness("Smoothness", Float) = 0.9
		_ShallowWaterColor("ShallowWaterColor", Color) = (0,0.09626541,0.5188679,0)
		_DeepWaterColor("DeepWaterColor", Color) = (0.07186722,0.3411596,0.8962264,0)
		_Normal01("Normal01", 2D) = "bump" {}
		_Normal01Speed("Normal01Speed", Vector) = (0,-0.05,0,0)
		_UV4Normal01("UV4Normal01", Vector) = (1,1,0,0)
		_Normal01Strength("Normal01Strength", Float) = 1.4
		_Normal02("Normal02", 2D) = "bump" {}
		_Normal02Speed("Normal02Speed", Vector) = (0,-0.08,0,0)
		_UV4Normal02("UV4Normal02", Vector) = (1,1,0,0)
		_Normal02Strength("Normal02Strength", Float) = 1.4
		_WF_Speed("WF_Speed", Float) = 0.5
		_WF_MaskScale("WF_MaskScale", Float) = 0.06
		_WF_FoamScale("WF_FoamScale", Float) = 200
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.5
		#define ASE_VERSION 19801
		#pragma surface surf Standard alpha:fade keepalpha exclude_path:deferred 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float4 uv_texcoord;
			float4 screenPos;
		};

		uniform sampler2D _Normal01;
		uniform float2 _UV4Normal01;
		uniform float2 _Normal01Speed;
		uniform float _Normal01Strength;
		uniform sampler2D _Normal02;
		uniform float2 _UV4Normal02;
		uniform float2 _Normal02Speed;
		uniform float _Normal02Strength;
		uniform float _WF_Speed;
		uniform float _WF_FoamScale;
		uniform float _WF_MaskScale;
		uniform float4 _ShallowWaterColor;
		uniform float4 _DeepWaterColor;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _Depth;
		uniform float _Smoothness;


		inline float noise_randomValue (float2 uv) { return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453); }

		inline float noise_interpolate (float a, float b, float t) { return (1.0-t)*a + (t*b); }

		inline float valueNoise (float2 uv)
		{
			float2 i = floor(uv);
			float2 f = frac( uv );
			f = f* f * (3.0 - 2.0 * f);
			uv = abs( frac(uv) - 0.5);
			float2 c0 = i + float2( 0.0, 0.0 );
			float2 c1 = i + float2( 1.0, 0.0 );
			float2 c2 = i + float2( 0.0, 1.0 );
			float2 c3 = i + float2( 1.0, 1.0 );
			float r0 = noise_randomValue( c0 );
			float r1 = noise_randomValue( c1 );
			float r2 = noise_randomValue( c2 );
			float r3 = noise_randomValue( c3 );
			float bottomOfGrid = noise_interpolate( r0, r1, f.x );
			float topOfGrid = noise_interpolate( r2, r3, f.x );
			float t = noise_interpolate( bottomOfGrid, topOfGrid, f.y );
			return t;
		}


		float SimpleNoise(float2 UV)
		{
			float t = 0.0;
			float freq = pow( 2.0, float( 0 ) );
			float amp = pow( 0.5, float( 3 - 0 ) );
			t += valueNoise( UV/freq )*amp;
			freq = pow(2.0, float(1));
			amp = pow(0.5, float(3-1));
			t += valueNoise( UV/freq )*amp;
			freq = pow(2.0, float(2));
			amp = pow(0.5, float(3-2));
			t += valueNoise( UV/freq )*amp;
			return t;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uvs_TexCoord72 = i.uv_texcoord;
			uvs_TexCoord72.xy = i.uv_texcoord.xy * _UV4Normal01 + ( _Time.y * _Normal01Speed );
			float2 uvs_TexCoord76 = i.uv_texcoord;
			uvs_TexCoord76.xy = i.uv_texcoord.xy * _UV4Normal02 + ( _Time.y * _Normal02Speed );
			o.Normal = ( UnpackScaleNormal( tex2D( _Normal01, uvs_TexCoord72.xy ), _Normal01Strength ) + UnpackScaleNormal( tex2D( _Normal02, uvs_TexCoord76.xy ), _Normal02Strength ) );
			float4 break80 = i.uv_texcoord;
			float2 appendResult87 = (float2(break80.x , ( ( break80.y + ( _Time.y * _WF_Speed ) ) * 0.1 )));
			float simpleNoise88 = SimpleNoise( appendResult87*_WF_FoamScale );
			float temp_output_91_0 = pow( abs( simpleNoise88 ) , 4.5 );
			float4 ase_positionSS = float4( i.screenPos.xyz , i.screenPos.w + 1e-7 );
			float4 ase_positionSSNorm = ase_positionSS / ase_positionSS.w;
			ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
			float depthLinearEye4 = LinearEyeDepth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_positionSSNorm.xy ) );
			float4 temp_output_2_0_g7 = ase_positionSS;
			float4 lerpResult39 = lerp( _ShallowWaterColor , _DeepWaterColor , saturate( ( ( depthLinearEye4 - (temp_output_2_0_g7).w ) / _Depth ) ));
			o.Albedo = ( step( 0.16 , ( ( ( 1.0 - break80.y ) * temp_output_91_0 ) + ( temp_output_91_0 * pow( abs( ( 1.0 - ( abs( ( i.uv_texcoord.y - 0.54 ) ) - _WF_MaskScale ) ) ) , 81.2 ) ) ) ) + lerpResult39 ).rgb;
			o.Metallic = 0.0;
			o.Smoothness = _Smoothness;
			float4 temp_output_2_0_g12 = lerpResult39;
			o.Alpha = (temp_output_2_0_g12).a;
		}

		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "AmplifyShaderEditor.MaterialInspector"
}
/*ASEBEGIN
Version=19801
Node;AmplifyShaderEditor.SimpleTimeNode;81;-2768,1120;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;82;-2768,1280;Inherit;False;Property;_WF_Speed;WF_Speed;12;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;78;-2768,832;Inherit;True;0;4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexCoordVertexDataNode;96;-2960,1552;Inherit;True;0;4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;80;-2437.017,894.4862;Inherit;True;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-2496,1168;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;97;-2630.383,1564.608;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;84;-2192,1088;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;98;-2400,1440;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0.54;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;85;-1920.299,1123.134;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;99;-1952.083,1482.593;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;101;-1904,1728;Inherit;False;Property;_WF_MaskScale;WF_MaskScale;13;0;Create;True;0;0;0;False;0;False;0.06;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;87;-1744,928;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;89;-1696,1120;Inherit;False;Property;_WF_FoamScale;WF_FoamScale;14;0;Create;True;0;0;0;False;0;False;200;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;100;-1662.35,1537.805;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;88;-1479.531,988.0123;Inherit;True;Simple;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;200;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;102;-1456,1536;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;1;-2112,288;Float;True;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.AbsOpNode;90;-1232,912;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;103;-1280,1536;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;4;-1952,-16;Inherit;False;0;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;64;-1875.294,134.4;Inherit;False;Alpha Split;-1;;7;07dab7960105b86429ac8eebd729ed6d;0;1;2;FLOAT4;0,0,0,0;False;2;FLOAT3;0;FLOAT;6
Node;AmplifyShaderEditor.PowerNode;91;-1007.343,1000.585;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;4.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;104;-1072,1536;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;81.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-1776,416;Inherit;False;Property;_Depth;Depth;0;0;Create;True;0;0;0;False;0;False;2.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;3;-1632,16;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;69;-2080,-880;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;67;-2144,-672;Inherit;False;Property;_Normal01Speed;Normal01Speed;5;0;Create;True;0;0;0;False;0;False;0,-0.05;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;68;-2080,-400;Inherit;False;Property;_Normal02Speed;Normal02Speed;9;0;Create;True;0;0;0;False;0;False;0,-0.08;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.OneMinusNode;92;-928,720;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;105;-736,1344;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;10;-1440,256;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;70;-1728,-800;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-1728,-528;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;22;-1456,-944;Inherit;False;Property;_UV4Normal01;UV4Normal01;6;0;Create;True;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;23;-1472,-624;Inherit;False;Property;_UV4Normal02;UV4Normal02;10;0;Create;True;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-649.1206,854.4048;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;37;-1008,-112;Inherit;False;Property;_ShallowWaterColor;ShallowWaterColor;2;0;Create;True;0;0;0;False;0;False;0,0.09626541,0.5188679,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;38;-1056,80;Inherit;False;Property;_DeepWaterColor;DeepWaterColor;3;0;Create;True;0;0;0;False;0;False;0.07186722,0.3411596,0.8962264,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SaturateNode;11;-1216,304;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;76;-1168,-528;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;72;-1168,-864;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;34;-912,-352;Inherit;False;Property;_Normal02Strength;Normal02Strength;11;0;Create;True;0;0;0;False;0;False;1.4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-960,-656;Inherit;False;Property;_Normal01Strength;Normal01Strength;7;0;Create;True;0;0;0;False;0;False;1.4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;94;-304,1104;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;39;-672,-96;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;26;-720,-896;Inherit;True;Property;_Normal01;Normal01;4;0;Create;True;0;0;0;False;0;False;-1;9802f250d9820114bbd87670cbae7482;9802f250d9820114bbd87670cbae7482;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;35;-704,-560;Inherit;True;Property;_Normal02;Normal02;8;0;Create;True;0;0;0;False;0;False;-1;0816264fbd2d3304cb340c9709026d57;c0c277cf515669843aed37f8d5cadf5d;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.StepOpNode;95;-48,912;Inherit;True;2;0;FLOAT;0.16;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;41;-160,112;Inherit;False;Alpha Split;-1;;12;07dab7960105b86429ac8eebd729ed6d;0;1;2;COLOR;0,0,0,0;False;2;FLOAT3;0;FLOAT;6
Node;AmplifyShaderEditor.SimpleAddOpNode;36;-272,-720;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;30;240,-128;Inherit;False;Property;_Smoothness;Smoothness;1;0;Create;True;0;0;0;False;0;False;0.9;0.9;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;304,0;Inherit;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;77;202.5684,701.2628;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;880,-64;Float;False;True;-1;3;AmplifyShaderEditor.MaterialInspector;0;0;Standard;Water_Fall;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;80;0;78;0
WireConnection;83;0;81;0
WireConnection;83;1;82;0
WireConnection;97;0;96;0
WireConnection;84;0;80;1
WireConnection;84;1;83;0
WireConnection;98;0;97;1
WireConnection;85;0;84;0
WireConnection;99;0;98;0
WireConnection;87;0;80;0
WireConnection;87;1;85;0
WireConnection;100;0;99;0
WireConnection;100;1;101;0
WireConnection;88;0;87;0
WireConnection;88;1;89;0
WireConnection;102;0;100;0
WireConnection;90;0;88;0
WireConnection;103;0;102;0
WireConnection;64;2;1;0
WireConnection;91;0;90;0
WireConnection;104;0;103;0
WireConnection;3;0;4;0
WireConnection;3;1;64;6
WireConnection;92;0;80;1
WireConnection;105;0;91;0
WireConnection;105;1;104;0
WireConnection;10;0;3;0
WireConnection;10;1;8;0
WireConnection;70;0;69;0
WireConnection;70;1;67;0
WireConnection;71;0;69;0
WireConnection;71;1;68;0
WireConnection;93;0;92;0
WireConnection;93;1;91;0
WireConnection;11;0;10;0
WireConnection;76;0;23;0
WireConnection;76;1;71;0
WireConnection;72;0;22;0
WireConnection;72;1;70;0
WireConnection;94;0;93;0
WireConnection;94;1;105;0
WireConnection;39;0;37;0
WireConnection;39;1;38;0
WireConnection;39;2;11;0
WireConnection;26;1;72;0
WireConnection;26;5;33;0
WireConnection;35;1;76;0
WireConnection;35;5;34;0
WireConnection;95;1;94;0
WireConnection;41;2;39;0
WireConnection;36;0;26;0
WireConnection;36;1;35;0
WireConnection;77;0;95;0
WireConnection;77;1;39;0
WireConnection;0;0;77;0
WireConnection;0;1;36;0
WireConnection;0;3;31;0
WireConnection;0;4;30;0
WireConnection;0;9;41;6
ASEEND*/
//CHKSM=9AD9E5779D63781AE24EEC7EFBFA2EE2C5B8FF6F