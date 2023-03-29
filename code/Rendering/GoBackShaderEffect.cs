namespace GangJam.Rendering;

[SceneCamera.AutomaticRenderHook]
internal sealed class GoBackShaderEffect : RenderHook
{
	private readonly RenderAttributes attributes = new();
	private readonly Material effectMaterial = Material.Load( "materials/screen.vmat" );

	/// <inheritdoc/>
	public sealed override void OnStage( SceneCamera target, Stage renderStage )
	{
		if ( renderStage != Stage.AfterPostProcess )
			return;

		Graphics.GrabFrameTexture( "ColorBuffer", attributes );
		Graphics.GrabDepthTexture( "DepthBuffer", attributes );

		// Draw our effect material, which is probably using a special post process
		// shader that uses all the attributes to do cool effects
		Graphics.Blit( effectMaterial, attributes );
	}
}
