namespace GangJam;

internal sealed class PlayerCamera : EntityComponent<Player>, ISingletonComponent
{
	private float MinDistance => 120.0f;
	private float MaxDistance => 350.0f;

	private readonly float _minFOV = 70f;
	private readonly float _maxFOV = 80f;
	private readonly float _targetDistance = 180f;
	private float _distance;
	private Vector3 _targetPosition;

	internal void Update( Player player )
	{
		var currentDistance = _distance.LerpInverse( MinDistance, MaxDistance );

		_distance = _distance.LerpTo( _targetDistance, 5f * Time.Delta );
		_targetPosition = Vector3.Lerp( _targetPosition, player.Position, 8f * Time.Delta );

		var height = 70f.LerpTo( 96f, _distance.LerpInverse( MinDistance, MaxDistance ) );
		var center = _targetPosition + Vector3.Up * height;
		center += -player.LookInput.Forward * 8f;
		var targetPos = center + -player.LookInput.Forward * _targetDistance;

		var tr = Trace.Ray( center, targetPos )
			.Ignore( player )
			.WithAnyTags( "world", "solid" )
			.WithoutTags( "player" )
			.Radius( 8 )
			.Run();

		if ( tr.Hit )
			_distance = Math.Min( _distance, tr.Distance );

		var endpos = center + -player.LookInput.Forward * _distance;

		Camera.Position = endpos;
		Camera.Rotation = player.LookInput.ToRotation();
		Camera.Rotation *= Rotation.FromPitch( currentDistance * 10f );

		var rot = player.Rotation.Angles() * .015f;
		rot.yaw = 0;

		Camera.Rotation *= Rotation.From( rot );

		var speed = player.Velocity.WithZ( 0 ).Length / 350f;
		var fov = _minFOV.LerpTo( _maxFOV, speed );

		Camera.FieldOfView = Camera.FieldOfView.LerpTo( fov, Time.Delta );
		Camera.ZNear = 6;
		Camera.FirstPersonViewer = null;
	}
}
