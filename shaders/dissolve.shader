
HEADER
{
	Description = "";
}

FEATURES
{
	#include "vr_common_features.fxc"
	Feature( F_ADDITIVE_BLEND, 0..1, "Blending" );
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

	#define S_UV2 1
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
	}
}

PS
{
	#include "sbox_pixel.fxc"
	#include "common/pixel.material.structs.hlsl"
	#include "common/pixel.lighting.hlsl"
	#include "common/pixel.shading.hlsl"
	#include "common/pixel.material.helpers.hlsl"
	#include "common/pixel.color.blending.hlsl"
	#include "common/proceedural.hlsl"

	SamplerState g_sSampler0 < Filter( POINT ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( Color, Srgb, 8, "None", "_color", "Color,0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Translucency, Srgb, 8, "None", "_trans", "Translucent,1/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateTexture2DWithoutSampler( g_tColor ) < Channel( RGBA, Box( Color ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	CreateTexture2DWithoutSampler( g_tTranslucency ) < Channel( RGBA, Box( Translucency ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
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
		Material m;
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = TransformNormal( i, float3( 0, 0, 1 ) );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;

		float2 local0 = i.vTextureCoords.xy * float2( 1, 1 );
		float2 local1 = g_vTexCoordScale;
		float2 local2 = g_vTexCoordOffset;
		float2 local3 = TileAndOffsetUv( local0, local1, local2 );
		float4 local4 = Tex2DS( g_tColor, g_sSampler0, local3 );
		float4 local5 = g_vGlowColor;
		float4 local6 = local5 * local4;
		float local7 = g_flGlowPower;
		float4 local8 = local6 * float4( local7, local7, local7, local7 );
		float local9 = g_flGlowMix;
		float4 local10 = lerp( local4, local8, local9 );
		float local11 = g_flSmoothStepMin;
		float local12 = g_flSmoothStepMax;
		float local13 = g_flFadeamount;
		float4 local14 = Tex2DS( g_tTranslucency, g_sSampler0, local3 );
		float4 local15 = float4( local13, local13, local13, local13 ) * local14;
		float local16 = lerp( 0, 1, local15.x );
		float local17 = smoothstep( local11, local12, local16 );
		float local18 = saturate( local17 );
		float local19 = ( saturate( ( (local13) - (0) ) / ( (10) - (0) ) ) * ((1) - (0)) ) + (0);
		float local20 = lerp( 0, local18, local19 );
		float local21 = g_flRoughness;
		float local22 = g_flMetallic;

		m.Albedo = local4.xyz;
		m.Emission = local10.xyz;
		m.Opacity = local20;
		m.Roughness = local21;
		m.Metalness = local22;
		m.AmbientOcclusion = 1;

		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}
