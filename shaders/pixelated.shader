
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
#define S_TRANSLUCENT 0
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
	CreateInputTexture2D( TintMask, Linear, 8, "None", "_mask", "Color,0/,0/4", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Normal, Linear, 8, "NormalizeNormals", "_normal", "Normal,1/,0/1", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Roughness, Linear, 8, "None", "_rough", "Roughness,2/,0/2", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Metallic, Linear, 8, "None", "_metal", "Metalness,3/,0/3", Default4( 0.00, 0.00, 0.00, 1.00 ) );
	CreateTexture2DWithoutSampler( g_tColor ) < Channel( RGBA, Box( Color ), Srgb ); OutputFormat( BC7 ); SrgbRead( True ); >;
	CreateTexture2DWithoutSampler( g_tTintMask ) < Channel( RGBA, Box( TintMask ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	CreateTexture2DWithoutSampler( g_tNormal ) < Channel( RGBA, Box( Normal ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	CreateTexture2DWithoutSampler( g_tRoughness ) < Channel( RGBA, Box( Roughness ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	CreateTexture2DWithoutSampler( g_tMetallic ) < Channel( RGBA, Box( Metallic ), Linear ); OutputFormat( BC7 ); SrgbRead( False ); >;
	float2 g_vTiling < UiGroup( "Texture Coordinates,5/,0/0" ); Default2( 1,1 ); >;
	float4 g_vColorTint < UiType( Color ); UiGroup( "Color,1/,0/0" ); Default4( 1.00, 1.00, 1.00, 1.00 ); >;
	float g_flModelTintAmount < UiGroup( "Color,2/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;

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
		float2 local1 = g_vTiling;
		float2 local2 = local0 * local1;
		float4 local3 = Tex2DS( g_tColor, g_sSampler0, local2 );
		float4 local4 = g_vColorTint;
		float4 local5 = local4 * local3;
		float4 local6 = Tex2DS( g_tTintMask, g_sSampler0, local2 );
		float4 local7 = float4( 1, 1, 1, 1 ) - local6;
		float4 local8 = lerp( local5, local3, local7 );
		float local9 = g_flModelTintAmount;
		float4 local10 = lerp( local3, local8, local9 );
		float4 local11 = Tex2DS( g_tNormal, g_sSampler0, local2 );
		float3 local12 = TransformNormal( i, DecodeNormal( local11.xyz ) );
		float4 local13 = Tex2DS( g_tRoughness, g_sSampler0, local2 );
		float4 local14 = Tex2DS( g_tMetallic, g_sSampler0, local2 );

		m.Albedo = local10.xyz;
		m.Opacity = 1;
		m.Normal = local12;
		m.Roughness = local13.x;
		m.Metalness = local14.x;
		m.AmbientOcclusion = 1;

		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}
