using System.Buffers;

namespace ParkourPainters.Entities;

public partial class LedgeGrabMechanic : ControllerMechanic
{
	[Net, Predicted] private Vector3 _grabNormal { get; set; }
	[Net, Predicted] private Vector3 _ledgeGrabLocation { get; set; }
	private TimeSince TimeSinceDrop { get; set; }
	private TimeSince _timeSinceLastSound = 0;
	private TraceResult _grabTrace;

	protected override bool ShouldStart()
	{
		if ( Player.IsDazed )
			return false;

		if ( Controller.GroundEntity.IsValid() )
			return false;

		if ( TimeSinceDrop < 0.9f )
			return false;

		if ( Controller.Velocity.z > 0 )
			return false;

		return CanGrabLedge();
	}

	protected override void Simulate()
	{
		Controller.Velocity = 0;
		Controller.Position = Vector3.Lerp( Controller.Position, _ledgeGrabLocation, Time.Delta * 10.0f );
		Player.Rotation = (-_grabNormal).EulerAngles.WithPitch( 0 ).ToRotation();

		var wishVelocity = Controller.GetWishVelocity();
		if ( wishVelocity.Length > 0 && _timeSinceLastSound > 0.5 )
		{
			_grabTrace.Surface.DoFootstep( Player, _grabTrace, Random.Shared.Int( 0, 1 ), 1f );
			_timeSinceLastSound = 0;
		}

		// The player is moving in a direction away from the ledge, therefore we should drop them.
		var isMovingFromLedge = Vector3.Dot( wishVelocity, Player.Rotation.Forward ) < -30;

		if ( Input.Pressed( InputAction.Drop ) || isMovingFromLedge )
		{
			Stop();
			return;
		}

		if ( Input.Pressed( InputAction.Jump ) )
		{
			Vault();
			return;
		}
	}

	private bool CanGrabLedge()
	{
		var girth = Controller.BodyGirth * 0.5f;
		var center = Controller.Position;

		// Trace from the bottom looking for a wall.
		var trDownwards = Trace.Ray( center, center + (Player.Rotation.Forward.WithZ( 0 ).Normal * 20f) )
			.Ignore( Player )
			.WithoutTags( "player" )
			.WithTag( "solid" )
			.Radius( 3 )
			.Run();

		// Tracing forwards looking for a wall.
		center.z += 50;
		var tr = Trace.Ray( center, center + (Player.Rotation.Forward.WithZ( 0 ).Normal * 40f) )
			.Ignore( Player )
			.WithoutTags( "player" )
			.WithTag( "solid" )
			.Radius( 3 )
			.Run();

		// Make sure we aren't trying to grab slope.
		if ( trDownwards.Hit && !trDownwards.Distance.AlmostEqual( tr.Distance, 5f ) )
			return false;

		if ( !tr.Hit )
			return false;

		// Make sure there is nothing above the players head.
		var trUpwards = Trace.Ray( center, center + (Player.Rotation.Up * Controller.BodyGirth) )
			.Ignore( Player )
			.WithoutTags( "player" )
			.WithTag( "solid" )
			.Radius( 16 )
			.Run();

		if ( trUpwards.Hit )
			return false;

		var normal = tr.Normal;
		var destinationTestPos = tr.EndPosition - (normal * 5f) + (Vector3.Up * 25.0f);
		var originTestPos = tr.EndPosition + (normal.EulerAngles.WithPitch( 0 ).ToRotation().Forward * girth);

		// Test to see if what we are attempting to grab is actually a ledge.
		tr = Trace.Ray( destinationTestPos, destinationTestPos - (Vector3.Up * Controller.EyeHeight) )
			.Ignore( Player )
			.WithoutTags( "player" )
			.WithTag( "solid" )
			.Radius( 2 )
			.Run();

		if ( !tr.Hit )
			return false;

		destinationTestPos = tr.EndPosition;
		originTestPos = originTestPos.WithZ( destinationTestPos.z - 60.0f );

		// One last check to make sure the player can exist in the space above the ledge.
		tr = Trace.Ray( destinationTestPos + (Vector3.Up * girth + 1.0f), destinationTestPos + (Vector3.Up * Controller.BodyGirth) )
			.Ignore( Player )
			.WithoutTags( "player" )
			.WithTag( "solid" )
			.Radius( girth )
			.Run();

		if ( tr.Hit )
			return false;

		_ledgeGrabLocation = originTestPos;
		_grabNormal = normal;
		_grabTrace = tr;

		return true;
	}

	protected override void OnStart()
	{
		if ( Player.HeldItem is not null && !Player.HeldItem.CanUseWhileClimbing )
			Player.UnsetHeldItemInput( To.Single( Player ) );

		_grabTrace.Surface.DoFootstep( Player, _grabTrace, Random.Shared.Int( 0, 1 ), 1f );
	}

	protected override void OnStop()
	{
		IsActive = false;
		TimeSinceDrop = 0;
	}

	private void Vault()
	{
		// Get unstuck from non-axis-aligned ledges
		if ( Math.Abs( _grabNormal.x ) != 1 || Math.Abs( _grabNormal.y ) != 1 || Math.Abs( _grabNormal.z ) != 1 )
			Position += _grabNormal * 5;

		float flGroundFactor = 1.0f;
		float flMul = 350f;
		float startz = Velocity.z;

		Velocity = Velocity.WithZ( startz + flMul * flGroundFactor );

		Player.PlaySound( "wall_jump" );

		Stop();
	}
}
