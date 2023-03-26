namespace GangJam;

class LedgeGrabMechanic : ControllerMechanic
{
	private float _playerRadius => 32.0f;
	private Vector3 _grabNormal;
	private Vector3 _ledgeGrabLocation;
	private TimeSince _timeSinceDrop;

	protected override bool ShouldStart()
	{
		if ( Controller.GroundEntity.IsValid() )
			return false;

		if ( _timeSinceDrop < 0.7f )
			return false;

		if ( CanGrabLedge() )
			return true;

		return false;
	}

	protected override void Simulate()
	{
		Controller.Velocity = 0;
		Controller.Position = Vector3.Lerp( Controller.Position, _ledgeGrabLocation, Time.Delta * 10.0f );
		Player.Rotation = (-_grabNormal).EulerAngles.WithPitch( 0 ).ToRotation();

		if ( Input.Pressed( InputButton.Duck ) )
			Drop();

		if ( Input.Pressed( InputButton.Jump ) )
			Vault();
	}

	private bool CanGrabLedge()
	{
		var center = Controller.Position;
		center.z += 55;
		var dest = center + (Player.Rotation.Forward.WithZ( 0 ).Normal * 10.0f);

		// Tracing forwards looking for a wall.
		var tr = Trace.Ray( center, dest )
			.Ignore( Player )
			.WithoutTags( "player" )
			.Radius( 8 )
			.Run();

		if ( !tr.Hit )
			return false;

		var normal = tr.Normal;
		var destinationTestPos = tr.EndPosition - (normal * _playerRadius) + (Vector3.Up * 50.0f);
		var originTestPos = tr.EndPosition + (normal * 9.0f);

		// Trace to see if what we are grabbing is actually a ledge.
		tr = Trace.Ray( destinationTestPos, destinationTestPos - (Vector3.Up * 64.0f) )
			.Ignore( Player )
			.WithoutTags( "player" )
			.Radius( 4 )
			.Run();

		if ( !tr.Hit )
			return false;

		destinationTestPos = tr.EndPosition;
		_ledgeGrabLocation = originTestPos.WithZ( destinationTestPos.z - 64.0f );
		_grabNormal = normal;

		return true;
	}

	private void Drop()
	{
		IsActive = false;
		_timeSinceDrop = 0;
	}

	private void Vault()
	{
		Drop();

		float flGroundFactor = 1.0f;
		float flMul = 350f;
		float startz = Velocity.z;

		Velocity = Velocity.WithZ( startz + flMul * flGroundFactor );
	}
}
