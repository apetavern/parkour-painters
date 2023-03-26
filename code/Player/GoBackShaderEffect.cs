
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GangJam;

[SceneCamera.AutomaticRenderHook]
public partial class GoBackShaderEffect : RenderHook
{
	RenderAttributes attributes = new RenderAttributes();
	Material effectMaterial = Material.Load( "materials/screen.vmat" );

	public override void OnStage( SceneCamera target, Stage renderStage )
	{
		if ( renderStage == Stage.AfterUI )
		{
			Graphics.GrabFrameTexture( "ColorBuffer", attributes );
			Graphics.GrabDepthTexture( "DepthBuffer", attributes );

			// Draw our effect material, which is probably using a special post process
			// shader that uses all the attributes to do cool effects
			Graphics.Blit( effectMaterial, attributes );
		}
	}
}
