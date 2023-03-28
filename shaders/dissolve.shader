
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

		float4 local0 = Tex2DS( g_tColor, g_sSampler0, i.vTextureCoords.xy );
		float local1 = g_flSmoothStepMin;
		float local2 = g_flSmoothStepMax;
		float local3 = g_flFadeamount;
		float4 local4 = Tex2DS( g_tTranslucency, g_sSampler0, i.vTextureCoords.xy );
		float4 local5 = float4( local3, local3, local3, local3 ) * local4;
		float local6 = lerp( 0, 1, local5.x );
		float local7 = smoothstep( local1, local2, local6 );
		float local8 = saturate( local7 );
		float local9 = g_flRoughness;
		float local10 = g_flMetallic;

		m.Albedo = local0.xyz;
		m.Opacity = local8;
		m.Roughness = local9;
		m.Metalness = local10;
		m.AmbientOcclusion = 1;

		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}
