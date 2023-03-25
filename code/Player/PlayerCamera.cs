namespace GangJam;

public partial class PlayerCamera : EntityComponent<Player>, ISingletonComponent
{
	private float MinDistance => 120.0f;
	private float MaxDistance => 350.0f;

	private readonly float _minFOV = 70f;
	private readonly float _maxFOV = 80f;
	private readonly float _targetDistance = 250f;
	private float _distance;
	private Vector3 _targetPosition;
	private Angles _currentForward;
	private float _cameraAdjustment;

	public virtual void Update( Player player )
	{
		if ( player.Velocity.WithZ( 0 ).Length > 0 )
		{
			var targetFwd = player.Rotation.Angles();
			_currentForward = Angles.Lerp( _currentForward, targetFwd, .1f * Time.Delta );
		}

		if ( !_cameraAdjustment.AlmostEqual( 0f ) )
		{
			var amount = _cameraAdjustment * 10f * Time.Delta;
			_cameraAdjustment = _cameraAdjustment.LerpTo( 0, 10f * Time.Delta );
			_currentForward.yaw += amount;
		}

		_distance = _distance.LerpTo( _targetDistance, 5f * Time.Delta );
		_targetPosition = Vector3.Lerp( _targetPosition, player.Position, 8f * Time.Delta );

		var distanceA = _distance.LerpInverse( MinDistance, MaxDistance );
		var height = 48f.LerpTo( 128f, distanceA );
		var center = _targetPosition + Vector3.Up * height;
		var targetPos = center + player.LookInput.Forward * -_distance;

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
		var fov = _minFOV.LerpTo( _maxFOV, spd );

		Camera.FieldOfView = Camera.FieldOfView.LerpTo( fov, Time.Delta );
		Camera.FirstPersonViewer = null;
	}
}
