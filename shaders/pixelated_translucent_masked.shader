
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	VrForward();
	Depth(); 
	ToolsVis( S_MODE_TOOLS_VIS );
	ToolsWireframe( "vr_tools_wireframe.shader" );
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 1
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 0
	#endif
	
	#include "common/shared.hlsl"
	#include "procedural.hlsl"

	#define S_UV2 1
	#define CUSTOM_MATERIAL_INPUTS
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	float4 vColor : COLOR0 < Semantic( Color ); >;
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float3 vPositionOs : TEXCOORD14;
	float3 vNormalOs : TEXCOORD15;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
	float4 vColor : COLOR0;
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;

		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );

		return FinalizeVertex( i );
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler0 < Filter( POINT ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( Color, Srgb, 8, "None", "_color", "Color,0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( TintMask, Linear, 8, "None", "_mask", "Color,0/,0/4", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Translucency, Linear, 8, "None", "_trans", "Translucent,1/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Normal, Linear, 8, "NormalizeNormals", "_normal", "Normal,1/,0/1", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Roughness, Linear, 8, "None", "_rough", "Roughness,2/,0/2", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Metallic, Linear, 8, "None", "_metal", "Metalness,3/,0/3", Default4( 0.00, 0.00, 0.00, 1.00 ) );
	Texture2D g_tColor < Channel( RGBA, Box( Color ), Srgb ); OutputFormat( BC7 ); SrgbRead( True ); >;
	Texture2D g_tTintMask < Channel( RGBA, Box( TintMask ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	Texture2D g_tTranslucency < Channel( RGBA, Box( Translucency ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	Texture2D g_tNormal < Channel( RGBA, Box( Normal ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	Texture2D g_tRoughness < Channel( RGBA, Box( Roughness ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	Texture2D g_tMetallic < Channel( RGBA, Box( Metallic ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	float2 g_vTiling < UiGroup( "Texture Coordinates,5/,0/0" ); Default2( 1,1 ); >;
	float4 g_vColorTint < UiType( Color ); UiGroup( "Color,1/,0/0" ); Default4( 1.00, 1.00, 1.00, 1.00 ); >;
	float g_flModelTintAmount < UiGroup( "Color,2/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;
	float g_flSmoothStepMin < UiGroup( "Translucent,1/,0/1" ); Default1( 0 ); Range1( 0, 1 ); >;
	float g_flSmoothStepMax < UiGroup( "Translucent,1/,0/2" ); Default1( 1 ); Range1( 0, 1 ); >;
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::Init();
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		float2 l_0 = i.vTextureCoords.xy * float2( 1, 1 );
		float2 l_1 = g_vTiling;
		float2 l_2 = l_0 * l_1;
		float4 l_3 = Tex2DS( g_tColor, g_sSampler0, l_2 );
		float3 l_4 = i.vColor.rgb;
		float4 l_5 = g_vColorTint;
		float4 l_6 = float4( l_4, 0 ) * l_5;
		float4 l_7 = l_6 * l_3;
		float4 l_8 = Tex2DS( g_tTintMask, g_sSampler0, l_2 );
		float4 l_9 = float4( 1, 1, 1, 1 ) - l_8;
		float4 l_10 = lerp( l_7, l_3, l_9 );
		float l_11 = g_flModelTintAmount;
		float4 l_12 = lerp( l_3, l_10, l_11 );
		float l_13 = g_flSmoothStepMin;
		float l_14 = g_flSmoothStepMax;
		float4 l_15 = Tex2DS( g_tTranslucency, g_sSampler0, l_2 );
		float4 l_16 = smoothstep( l_13, l_14, l_15 );
		float4 l_17 = saturate( l_16 );
		float4 l_18 = Tex2DS( g_tNormal, g_sSampler0, l_2 );
		float3 l_19 = DecodeNormal( l_18.xyz );
		float4 l_20 = Tex2DS( g_tRoughness, g_sSampler0, l_2 );
		float4 l_21 = Tex2DS( g_tMetallic, g_sSampler0, l_2 );
		
		m.Albedo = l_12.xyz;
		m.Opacity = l_17.x;
		m.Normal = l_19;
		m.Roughness = l_20.x;
		m.Metalness = l_21.x;
		m.AmbientOcclusion = 1;
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );

		// Result node takes normal as tangent space, convert it to world space now
		m.Normal = TransformNormal( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );

		// for some toolvis shit
		m.WorldTangentU = i.vTangentUWs;
		m.WorldTangentV = i.vTangentVWs;
        m.TextureCoords = i.vTextureCoords.xy;
		
		return ShadingModelStandard::Shade( i, m );
	}
}
