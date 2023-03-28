
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
	CreateInputTexture2D( Texture, Srgb, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateTexture2DWithoutSampler( g_tTexture ) < Channel( RGBA, Box( Texture ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	float g_flSmoothStepMin < UiGroup( ",0/,0/0" ); Default1( 0 ); Range1( 0, 1 ); >;
	float g_flSmoothStepMax < UiGroup( ",0/,0/0" ); Default1( 0.02 ); Range1( 0, 1 ); >;
	float g_flFadeamount < UiGroup( ",0/,0/0" ); Default1( 0 ); Range1( 0, 10 ); >;

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

		float local0 = g_flSmoothStepMin;
		float local1 = g_flSmoothStepMax;
		float local2 = g_flFadeamount;
		float4 local3 = Tex2DS( g_tTexture, g_sSampler0, i.vTextureCoords.xy );
		float4 local4 = float4( local2, local2, local2, local2 ) * local3;
		float local5 = lerp( 0, 1, local4.x );
		float local6 = smoothstep( local0, local1, local5 );
		float local7 = saturate( local6 );

		m.Opacity = local7;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;

		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}
