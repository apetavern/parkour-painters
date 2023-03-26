HEADER
{
    CompileTargets = ( IS_SM_50 && ( PC || VULKAN ) );
    Description = "Go back - Post Processing Shader";
}

MODES
{
    Default();
    VrForward();
}

FEATURES
{
    Feature( F_TINT, 0..1, "Rendering" );
}

COMMON
{
    #include "postprocess/shared.hlsl"
}

struct VertexInput
{
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
    float2 vTexCoord : TEXCOORD0 < Semantic( LowPrecisionUv ); >;
};

struct PixelInput
{
    float2 vTexCoord : TEXCOORD0;

	// VS only
	#if ( PROGRAM == VFX_PROGRAM_VS )
		float4 vPositionPs		: SV_Position;
	#endif

	// PS only
	#if ( ( PROGRAM == VFX_PROGRAM_PS ) )
		float4 vPositionSs		: SV_ScreenPosition;
	#endif
};

VS
{
    PixelInput MainVs( VertexInput i )
    {
        PixelInput o;
        o.vPositionPs = float4(i.vPositionOs.xyz, 1.0f);
        o.vTexCoord = i.vTexCoord;
        return o;
    }
}

PS
{
    #include "postprocess/common.hlsl"

    StaticCombo( S_TINT, F_TINT, Sys( PC ) );

    RenderState( DepthWriteEnable, false );
    RenderState( DepthEnable, false );

    CreateTexture2D( g_tColorBuffer ) < Attribute( "ColorBuffer" );  	SrgbRead( true ); Filter( MIN_MAG_LINEAR_MIP_POINT ); AddressU( MIRROR ); AddressV( MIRROR ); >;
    CreateTexture2D( g_tDepthBuffer ) < Attribute( "DepthBuffer" ); 	SrgbRead( false ); Filter( MIN_MAG_MIP_POINT ); AddressU( CLAMP ); AddressV( CLAMP ); >;

    CreateInputTexture2D( TextureBayer, Linear, 8, "", "_bayer", "Bayer Matrix", Default( 0.0f ) );
    CreateTexture2DWithoutSampler( g_tBayerMatrix )< Channel( R, None( TextureBayer ), Linear ); OutputFormat( RGBA8888 ); SrgbRead( false ); >;
    
    #if S_TINT
        float3 g_vTintColor< Default3(1.0f, 1.0f, 1.0f); UiType( Color ); UiGroup( "Go Back Settings,10/10" ); >;
    #endif

    float4 g_vBayerMatrixDim < Source( TextureDim ); SourceArg( g_tBayerMatrix ); >;
    
    SamplerState g_sPointWrap < Filter( POINT ); AddressU( WRAP ); AddressV( WRAP ); >;

    int2 g_vInternalResolution< Default2(320, 240); Range2(128, 128, 720, 1280); UiGroup( "Go Back Settings,10/10" ); >;
    int g_ColorDepth< Default(32); Range(0, 8); UiGroup( "Go Back Settings,10/10" ); >;
    int g_DitherScale< Default(1.0f); Range(1.0f, 8.0f); UiGroup( "Go Back Settings,10/10" ); >;

    struct PixelOutput
    {
        float4 vColor : SV_Target0;
    };

    // Convert from RGB to YUV
    float3 RgbToYuv(float3 vColor)
    {
        // Some magic constants to help with our conversion
        const float3 vYuv0 = float3(0.299f, 0.587f, 0.114f);
        const float3 vYuv1 = float3(-0.147f, -0.289f, 0.436f);
        const float3 vYuv2 = float3(0.615f, -0.515f, -0.1f);
        
        return float3(
            dot(vColor, vYuv0),
            dot(vColor, vYuv1),
            dot(vColor, vYuv2)
        );
    }

    // YUV to RGB
    float3 YuvToRgb(float3 vColor)
    {
        const float3 vRgb0 = float3(1.0f, 0.0f, 1.4f);
        const float3 vRgb1 = float3(1.0f, -0.395f, -0.581f);
        const float3 vRgb2 = float3(1.0f, 2.032f, 0.0f);

        return float3(
            dot(vColor, vRgb0),
            dot(vColor, vRgb1),
            dot(vColor, vRgb2)
        );
    }

    // Calculate our color error for dithering
    float3 CalculateColorError(float3 vBase, float3 vMin, float3 vMax)
    {
        // Figure out our color range
        float3 vColorRange = abs( vMax - vMin );

        // Figure out how far along we are
        float3 vLowerRange = abs( vBase - vMin );

        // Return our error
        return vLowerRange / vColorRange;
    }

    // Dither our colors
    float3 DecodeDither(float3 vColorError, float2 vUvs)
    {
        // Get our current dither cell
        float flLimit = Tex2DLevelS( g_tBayerMatrix, g_sPointWrap, vUvs, 0 ).r;
        
        // Dither!!!
        return step(flLimit, vColorError);
    }

    PixelOutput MainPs( PixelInput i )
    {
        PixelOutput o;
        // Get the current screen texture coordinates
        float2 vScreenUv = i.vPositionSs.xy / g_vRenderTargetSize;
        
        // Rescale to our internal resolution
        float2 vInternalResolutionUv = floor(vScreenUv * (float2)g_vInternalResolution) / (float2)g_vInternalResolution;

        // Get the current color at a given pixel
        float3 vFrameBufferColor = Tex2D( g_tColorBuffer, vInternalResolutionUv.xy ).rgb;

        // If we're using the tint feature, lets convert to black and white first
        #if S_TINT
            vFrameBufferColor = dot(vFrameBufferColor, float3(0.299, 0.587, 0.114));
        #endif

        // RGB -> YUV
        float3 vYUVColor = RgbToYuv(vFrameBufferColor.rgb);
        
        // Get our desired color depth
        float flDepthBits = pow(2, g_ColorDepth);

        // Get the upper and lower color of our color depth
        float3 vYuvFloor = floor(vYUVColor * (float)flDepthBits) / (float)flDepthBits;
        float3 vYuvCeil = ceil(vYUVColor * (float)flDepthBits) / (float)flDepthBits;
        
        // Calculate the difference between our current color and the high/low color
        float3 vColorError = CalculateColorError(
            vYUVColor,
            vYuvFloor,
            vYuvCeil
        );

        float flAspect = (float)g_vRenderTargetSize.y / (float)g_vRenderTargetSize.x;
        float flInternalAspect = (float)g_vInternalResolution.x / (float)g_vInternalResolution.y;
        
        // Fill our screen with dithering blocks
        float2 vDitherBlock = (vScreenUv * float2(1.0f, flAspect)) * ((g_vInternalResolution.xy * float2(1.0f, flInternalAspect)) / g_vBayerMatrixDim.xy);
        vDitherBlock*= (float)g_DitherScale;
        // Build our dither table
        float3 vDithered = DecodeDither(vColorError, vDitherBlock.xy);
        
        // Dither -> desired color and convert from YUV -> RGB
        o.vColor.rgb = YuvToRgb(lerp(
            vYuvFloor,
            vYuvCeil,
            vDithered
        ));


        // If we were tinting, we're in black and white. Lets tint our output
        #if S_TINT
            o.vColor.rgb = dot(o.vColor.rgb, float3(0.299, 0.587, 0.114)) * g_vTintColor;
        #endif
        
        o.vColor.a = 1.0f;
        return o;
    }
}
