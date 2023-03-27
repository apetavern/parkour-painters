namespace GangJam;

class LedgeGrabMechanic : ControllerMechanic
{
	private Vector3 _grabNormal;
	private Vector3 _ledgeGrabLocation;
	private TimeSince _timeSinceDrop;

	protected override bool ShouldStart()
	{
		if ( Controller.GroundEntity.IsValid() )
			return false;

		if ( _timeSinceDrop < 0.9f )
			return false;

		return CanGrabLedge();
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

		// Tracing forwards looking for a wall.
		var tr = Trace.Ray( center, center + (Player.Rotation.Forward.WithZ( 0 ).Normal * 48.0f) )
			.Ignore( Player )
			.WithoutTags( "player" )
			.Radius( 8 )
			.Run();

		if ( !tr.Hit )
			return false;

		// Make sure there is nothing above the players head.
		var trUpwards = Trace.Ray( center, center + (Player.Rotation.Up * 48.0f) )
			.Ignore( Player )
			.WithoutTags( "player" )
			.Radius( 8 )
			.Run();

		if ( trUpwards.Hit )
			return false;

		var normal = tr.Normal;
		var destinationTestPos = tr.EndPosition - (normal * 16.0f) + (Vector3.Up * 50.0f);
		var originTestPos = tr.EndPosition + (normal * 9.0f);

		// Test to see if what we are attempting to grab is actually a ledge.
		tr = Trace.Ray( destinationTestPos, destinationTestPos - (Vector3.Up * 64.0f) )
			.Ignore( Player )
			.WithoutTags( "player" )
			.Radius( 4 )
			.Run();

		if ( !tr.Hit )
			return false;

		destinationTestPos = tr.EndPosition;
		originTestPos = originTestPos.WithZ( destinationTestPos.z - 64.0f );

		// One last check to make sure the player can exist in the space above the ledge.
		tr = Trace.Ray( destinationTestPos + (Vector3.Up * 16.0f + 1.0f), destinationTestPos + (Vector3.Up * 32.0f) )
			.Ignore( Player )
			.WithoutTags( "player" )
			.Radius( 16f )
			.Run();

		if ( tr.Hit )
			return false;

		_ledgeGrabLocation = originTestPos;
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
