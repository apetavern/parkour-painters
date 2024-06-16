
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
	#define S_ALPHA_TEST 0
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 1
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
	CreateInputTexture2D( Translucency, Srgb, 8, "None", "_trans", "Translucent,1/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tColor < Channel( RGBA, Box( Color ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tTranslucency < Channel( RGBA, Box( Translucency ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	float2 g_vTexCoordScale < UiGroup( "Texture Coordinates,4/,0/0" ); Default2( 1,1 ); >;
	float2 g_vTexCoordOffset < UiGroup( "Texture Coordinates,4/,0/1" ); Default2( 0,0 ); >;
	float4 g_vGlowColor < UiType( Color ); UiGroup( "Glow,5/,0/0" ); Default4( 1.00, 1.00, 1.00, 1.00 ); >;
	float g_flGlowPower < UiGroup( "Glow,5/,0/1" ); Default1( 0 ); Range1( 0, 25 ); >;
	float g_flGlowMix < UiGroup( "Glow,5/,0/2" ); Default1( 0 ); Range1( 0, 1 ); >;
	float g_flSmoothStepMin < UiGroup( "Translucent,1/,0/2" ); Default1( 0.3 ); Range1( 0, 1 ); >;
	float g_flSmoothStepMax < UiGroup( "Translucent,1/,0/3" ); Default1( 0.5 ); Range1( 0, 1 ); >;
	float g_flFadeamount < UiGroup( "Translucent,1/,0/1" ); Default1( 0 ); Range1( 0, 10 ); >;
	float g_flRoughness < UiGroup( "Roughness,2/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;
	float g_flMetallic < UiGroup( "Metalness,3/,0/0" ); Default1( 0 ); Range1( 0, 1 ); >;
	
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
		float2 l_1 = g_vTexCoordScale;
		float2 l_2 = g_vTexCoordOffset;
		float2 l_3 = TileAndOffsetUv( l_0, l_1, l_2 );
		float4 l_4 = Tex2DS( g_tColor, g_sSampler0, l_3 );
		float4 l_5 = g_vGlowColor;
		float4 l_6 = l_5 * l_4;
		float l_7 = g_flGlowPower;
		float4 l_8 = l_6 * float4( l_7, l_7, l_7, l_7 );
		float l_9 = g_flGlowMix;
		float4 l_10 = lerp( l_4, l_8, l_9 );
		float l_11 = g_flSmoothStepMin;
		float l_12 = g_flSmoothStepMax;
		float l_13 = g_flFadeamount;
		float4 l_14 = Tex2DS( g_tTranslucency, g_sSampler0, l_3 );
		float4 l_15 = float4( l_13, l_13, l_13, l_13 ) * l_14;
		float l_16 = lerp( 0, 1, l_15.x );
		float l_17 = smoothstep( l_11, l_12, l_16 );
		float l_18 = saturate( l_17 );
		float l_19 = saturate( ( l_13 - 0 ) / ( 10 - 0 ) ) * ( 1 - 0 ) + 0;
		float l_20 = lerp( 0, l_18, l_19 );
		float l_21 = g_flRoughness;
		float l_22 = g_flMetallic;
		
		m.Albedo = l_4.xyz;
		m.Emission = l_10.xyz;
		m.Opacity = l_20;
		m.Roughness = l_21;
		m.Metalness = l_22;
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
