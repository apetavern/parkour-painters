namespace GangJam;

public partial class PlayerCamera : EntityComponent<Player>, ISingletonComponent
{
	public virtual void Update( Player player )
	{
		Camera.FirstPersonViewer = null;
		Camera.Rotation = player.EyeRotation;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		var center = player.Position + Vector3.Up * 64;
		var pos = center;
		var rot = Camera.Rotation * Rotation.FromAxis( Vector3.Up, -16 );

		float distance = 130.0f * player.Scale;
		var targetPos = pos + rot.Right * ((player.CollisionBounds.Mins.x + 32) * player.Scale);
		targetPos += rot.Forward * -distance;

		var tr = Trace.Ray( pos, targetPos )
			.WithAnyTags( "solid" )
			.Ignore( player )
			.Radius( 8 )
			.Run();

		Camera.Position = tr.EndPosition;
	}
}
