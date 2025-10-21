// Made with Amplify Shader Editor v1.9.8.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Water_Final"
{
	Properties
	{
		_Depth("Depth", Float) = 2.1
		_Smoothness("Smoothness", Float) = 0.9
		_ShallowWaterColor("ShallowWaterColor", Color) = (0,0.09626541,0.5188679,0)
		_DeepWaterColor("DeepWaterColor", Color) = (0.07186722,0.3411596,0.8962264,0)
		_Normal01("Normal01", 2D) = "bump" {}
		_Normal01Strength("Normal01Strength", Float) = 1.4
		_UV4Normal01("UV4Normal01", Vector) = (1,1,0,0)
		_Normal02("Normal02", 2D) = "bump" {}
		_Normal02Strength("Normal02Strength", Float) = 1.4
		_UV4Normal02("UV4Normal02", Vector) = (-1,1,0,0)
		_NormalSpeed("NormalSpeed", Float) = 0.8
		_FoamAmount("FoamAmount", Float) = 2.3
		_FoamScale("FoamScale", Float) = 50
		_FoamSpeed("FoamSpeed", Float) = 0.6
		_FoamCutoff("FoamCutoff", Float) = 1.2
		_FoamColor("FoamColor", Color) = (1,1,1,0.682353)
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
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
		};

		uniform sampler2D _Normal01;
		uniform float2 _UV4Normal01;
		uniform float _NormalSpeed;
		uniform float _Normal01Strength;
		uniform sampler2D _Normal02;
		uniform float2 _UV4Normal02;
		uniform float _Normal02Strength;
		uniform float4 _ShallowWaterColor;
		uniform float4 _DeepWaterColor;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _Depth;
		uniform float4 _FoamColor;
		uniform float _FoamAmount;
		uniform float _FoamCutoff;
		uniform float _FoamScale;
		uniform float _FoamSpeed;
		uniform float _Smoothness;


		float2 UnityGradientNoiseDir( float2 p )
		{
			p = fmod(p , 289);
			float x = fmod((34 * p.x + 1) * p.x , 289) + p.y;
			x = fmod( (34 * x + 1) * x , 289);
			x = frac( x / 41 ) * 2 - 1;
			return normalize( float2(x - floor(x + 0.5 ), abs( x ) - 0.5 ) );
		}
		
		float UnityGradientNoise( float2 UV, float Scale )
		{
			float2 p = UV * Scale;
			float2 ip = floor( p );
			float2 fp = frac( p );
			float d00 = dot( UnityGradientNoiseDir( ip ), fp );
			float d01 = dot( UnityGradientNoiseDir( ip + float2( 0, 1 ) ), fp - float2( 0, 1 ) );
			float d10 = dot( UnityGradientNoiseDir( ip + float2( 1, 0 ) ), fp - float2( 1, 0 ) );
			float d11 = dot( UnityGradientNoiseDir( ip + float2( 1, 1 ) ), fp - float2( 1, 1 ) );
			fp = fp * fp * fp * ( fp * ( fp * 6 - 15 ) + 10 );
			return lerp( lerp( d00, d01, fp.y ), lerp( d10, d11, fp.y ), fp.x ) + 0.5;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float temp_output_19_0 = ( _Time.y * _NormalSpeed );
			float2 temp_cast_0 = (( temp_output_19_0 / 50.0 )).xx;
			float2 uv_TexCoord24 = i.uv_texcoord * _UV4Normal01 + temp_cast_0;
			float2 temp_cast_1 = (( temp_output_19_0 / -40.0 )).xx;
			float2 uv_TexCoord25 = i.uv_texcoord * _UV4Normal02 + temp_cast_1;
			o.Normal = ( UnpackScaleNormal( tex2D( _Normal01, uv_TexCoord24 ), _Normal01Strength ) + UnpackScaleNormal( tex2D( _Normal02, uv_TexCoord25 ), _Normal02Strength ) );
			float4 ase_positionSS = float4( i.screenPos.xyz , i.screenPos.w + 1e-7 );
			float4 ase_positionSSNorm = ase_positionSS / ase_positionSS.w;
			ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
			float depthLinearEye4 = LinearEyeDepth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_positionSSNorm.xy ) );
			float4 temp_output_2_0_g7 = ase_positionSS;
			float4 lerpResult39 = lerp( _ShallowWaterColor , _DeepWaterColor , saturate( ( ( depthLinearEye4 - (temp_output_2_0_g7).w ) / _Depth ) ));
			float depthLinearEye44 = LinearEyeDepth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_positionSSNorm.xy ) );
			float4 temp_output_2_0_g6 = ase_positionSS;
			float2 temp_cast_2 = (_FoamScale).xx;
			float2 temp_cast_3 = (( _Time.y * _FoamSpeed )).xx;
			float2 uv_TexCoord54 = i.uv_texcoord * temp_cast_2 + temp_cast_3;
			float gradientNoise56 = UnityGradientNoise(uv_TexCoord54,1.0);
			float4 temp_output_2_0_g11 = _FoamColor;
			float4 lerpResult62 = lerp( lerpResult39 , _FoamColor , ( step( ( saturate( ( ( depthLinearEye44 - (temp_output_2_0_g6).w ) / _FoamAmount ) ) * _FoamCutoff ) , gradientNoise56 ) * (temp_output_2_0_g11).a ));
			o.Albedo = lerpResult62.rgb;
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
Node;AmplifyShaderEditor.ScreenPosInputsNode;42;-2112,896;Float;True;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenDepthNode;44;-1936,608;Inherit;False;0;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;63;-1904,704;Inherit;False;Alpha Split;-1;;6;07dab7960105b86429ac8eebd729ed6d;0;1;2;FLOAT4;0,0,0,0;False;2;FLOAT3;0;FLOAT;6
Node;AmplifyShaderEditor.SimpleSubtractOpNode;46;-1616,640;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-1760,1040;Inherit;False;Property;_FoamAmount;FoamAmount;11;0;Create;True;0;0;0;False;0;False;2.3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-2053.171,1525.393;Inherit;False;Property;_FoamSpeed;FoamSpeed;13;0;Create;True;0;0;0;False;0;False;0.6;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;52;-2007.007,1376.277;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;1;-2112,288;Float;True;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;47;-1424,880;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-1758.007,1464.277;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-1664,1328;Inherit;False;Property;_FoamScale;FoamScale;12;0;Create;True;0;0;0;False;0;False;50;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;17;-2192,-576;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-2192,-480;Inherit;False;Property;_NormalSpeed;NormalSpeed;10;0;Create;True;0;0;0;False;0;False;0.8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;4;-1952,-16;Inherit;False;0;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;64;-1875.294,134.4;Inherit;False;Alpha Split;-1;;7;07dab7960105b86429ac8eebd729ed6d;0;1;2;FLOAT4;0,0,0,0;False;2;FLOAT3;0;FLOAT;6
Node;AmplifyShaderEditor.SaturateNode;48;-1152,896;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;49;-1136,1200;Inherit;False;Property;_FoamCutoff;FoamCutoff;14;0;Create;True;0;0;0;False;0;False;1.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;54;-1440,1344;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;57;-1264,1504;Inherit;True;Constant;_Float1;Float 1;15;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-1936,-560;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-1776,416;Inherit;False;Property;_Depth;Depth;0;0;Create;True;0;0;0;False;0;False;2.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;3;-1632,16;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-864,1040;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;56;-1040,1312;Inherit;True;Gradient;False;True;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;21;-1632,-352;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;-40;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;23;-1408,-512;Inherit;False;Property;_UV4Normal02;UV4Normal02;9;0;Create;True;0;0;0;False;0;False;-1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleDivideOpNode;20;-1632,-768;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;50;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;22;-1424,-896;Inherit;False;Property;_UV4Normal01;UV4Normal01;6;0;Create;True;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleDivideOpNode;10;-1440,256;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;59;-640,1344;Inherit;False;Property;_FoamColor;FoamColor;15;0;Create;True;0;0;0;False;0;False;1,1,1,0.682353;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;34;-1072,-224;Inherit;False;Property;_Normal02Strength;Normal02Strength;8;0;Create;True;0;0;0;False;0;False;1.4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-1104,-640;Inherit;False;Property;_Normal01Strength;Normal01Strength;5;0;Create;True;0;0;0;False;0;False;1.4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;24;-1152,-864;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;37;-1008,-112;Inherit;False;Property;_ShallowWaterColor;ShallowWaterColor;2;0;Create;True;0;0;0;False;0;False;0,0.09626541,0.5188679,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;38;-1056,80;Inherit;False;Property;_DeepWaterColor;DeepWaterColor;3;0;Create;True;0;0;0;False;0;False;0.07186722,0.3411596,0.8962264,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SaturateNode;11;-1216,304;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;25;-1152,-400;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;58;-656,1072;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;66;-340.4829,1428.778;Inherit;False;Alpha Split;-1;;11;07dab7960105b86429ac8eebd729ed6d;0;1;2;COLOR;0,0,0,0;False;2;FLOAT3;0;FLOAT;6
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-160,1008;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;35;-800,-448;Inherit;True;Property;_Normal02;Normal02;7;0;Create;True;0;0;0;False;0;False;-1;c0c277cf515669843aed37f8d5cadf5d;c0c277cf515669843aed37f8d5cadf5d;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;26;-832,-864;Inherit;True;Property;_Normal01;Normal01;4;0;Create;True;0;0;0;False;0;False;-1;9802f250d9820114bbd87670cbae7482;9802f250d9820114bbd87670cbae7482;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp;39;-672,-96;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-320,-384;Inherit;False;Property;_Smoothness;Smoothness;1;0;Create;True;0;0;0;False;0;False;0.9;0.9;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-288,-480;Inherit;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;36;-480,-704;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;41;-160,112;Inherit;False;Alpha Split;-1;;12;07dab7960105b86429ac8eebd729ed6d;0;1;2;COLOR;0,0,0,0;False;2;FLOAT3;0;FLOAT;6
Node;AmplifyShaderEditor.LerpOp;62;160,624;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;384,16;Float;False;True;-1;3;AmplifyShaderEditor.MaterialInspector;0;0;Standard;Water_Final;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;63;2;42;0
WireConnection;46;0;44;0
WireConnection;46;1;63;6
WireConnection;47;0;46;0
WireConnection;47;1;45;0
WireConnection;53;0;52;0
WireConnection;53;1;51;0
WireConnection;64;2;1;0
WireConnection;48;0;47;0
WireConnection;54;0;55;0
WireConnection;54;1;53;0
WireConnection;19;0;17;0
WireConnection;19;1;18;0
WireConnection;3;0;4;0
WireConnection;3;1;64;6
WireConnection;50;0;48;0
WireConnection;50;1;49;0
WireConnection;56;0;54;0
WireConnection;56;1;57;0
WireConnection;21;0;19;0
WireConnection;20;0;19;0
WireConnection;10;0;3;0
WireConnection;10;1;8;0
WireConnection;24;0;22;0
WireConnection;24;1;20;0
WireConnection;11;0;10;0
WireConnection;25;0;23;0
WireConnection;25;1;21;0
WireConnection;58;0;50;0
WireConnection;58;1;56;0
WireConnection;66;2;59;0
WireConnection;60;0;58;0
WireConnection;60;1;66;6
WireConnection;35;1;25;0
WireConnection;35;5;34;0
WireConnection;26;1;24;0
WireConnection;26;5;33;0
WireConnection;39;0;37;0
WireConnection;39;1;38;0
WireConnection;39;2;11;0
WireConnection;36;0;26;0
WireConnection;36;1;35;0
WireConnection;41;2;39;0
WireConnection;62;0;39;0
WireConnection;62;1;59;0
WireConnection;62;2;60;0
WireConnection;0;0;62;0
WireConnection;0;1;36;0
WireConnection;0;3;31;0
WireConnection;0;4;30;0
WireConnection;0;9;41;6
ASEEND*/
//CHKSM=DB581763EA44F7A98EDB45348AD5BB74407EA41D