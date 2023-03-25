using Sandbox;

namespace GangJam;

public partial class PlayerCamera : EntityComponent<Player>, ISingletonComponent
{
	public virtual void Update( Player player )
	{
		Camera.Position = player.EyePosition;
		Camera.Rotation = player.EyeRotation;
		Camera.FieldOfView = Game.Preferences.FieldOfView;
		Camera.FirstPersonViewer = player;
		Camera.ZNear = 0.5f;
	}
}
