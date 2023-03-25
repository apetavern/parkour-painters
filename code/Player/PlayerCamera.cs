namespace GangJam;

public partial class PlayerCamera : EntityComponent<Player>, ISingletonComponent
{
	private float distance;
	private float targetDistance = 250f;
	private Vector3 targetPosition;
	private Angles currentForward;
	private float CameraAdjustment;

	public float MinDistance => 120.0f;
	public float MaxDistance => 350.0f;
	public float DistanceStep => 60.0f;

	public virtual void Update( Player player )
	{
		if ( player.Velocity.WithZ( 0 ).Length > 0 )
		{
			var targetFwd = player.Rotation.Angles();
			currentForward = Angles.Lerp( currentForward, targetFwd, .1f * Time.Delta );
		}

		if ( !CameraAdjustment.AlmostEqual( 0f ) )
		{
			var amount = CameraAdjustment * 10f * Time.Delta;
			CameraAdjustment = CameraAdjustment.LerpTo( 0, 10f * Time.Delta );
			currentForward.yaw += amount;
		}

		distance = distance.LerpTo( targetDistance, 5f * Time.Delta );
		targetPosition = Vector3.Lerp( targetPosition, player.Position, 8f * Time.Delta );

		var distanceA = distance.LerpInverse( MinDistance, MaxDistance );
		var height = 48f.LerpTo( 128f, distanceA );
		var center = targetPosition + Vector3.Up * height;
		var targetPos = center + player.LookInput.Forward * -distance;

		var tr = Trace.Ray( center, targetPos )
			.Ignore( player )
			.Radius( 8 )
			.Run();

		var endpos = tr.EndPosition;

		Camera.Position = endpos;
		Camera.Rotation = player.LookInput.ToRotation();
		Camera.Rotation *= Rotation.FromPitch( distanceA * 10f );

		var rot = player.Rotation.Angles() * .015f;
		rot.yaw = 0;

		Camera.Rotation *= Rotation.From( rot );

		var spd = player.Velocity.WithZ( 0 ).Length / 350f;
		var fov = 70f.LerpTo( 80f, spd );

		Camera.FieldOfView = Camera.FieldOfView.LerpTo( fov, Time.Delta );

		Camera.FirstPersonViewer = null;
	}

	[Event.Client.BuildInput]
	public void BuildInput()
	{
		Entity.ViewAngles = currentForward;

		if ( Input.MouseWheel != 0 )
		{
			targetDistance += -Input.MouseWheel * DistanceStep;
			targetDistance = targetDistance.Clamp( MinDistance, MaxDistance );
		}
	}
}
