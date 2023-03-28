namespace GangJam;

[Library( "func_graffiti_spot" )]
[Title( "Graffiti Spot" ), Category( "Spray Down" )]
[Solid, DrawAngles]
public sealed partial class GraffitiSpot : ModelEntity
{
	[Net] public Team SprayOwner { get; private set; }

	[Net] public float SprayProgress { get; private set; }

	public bool IsSprayCompleted => SprayProgress >= 100;

	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromModel( PhysicsMotionType.Static );

		Tags.Add( "graffiti_spot" );
	}

	public void OnSprayReceived( Player player )
	{
		// Reset spray progress if the spray owner is the new sprayer.
		if ( player.Team != SprayOwner )
		{
			SprayProgress = 0;
			SprayOwner = player.Team;
		}

		// Bail if the spray has already been completed.
		if ( IsSprayCompleted )
			return;

		SprayProgress = Math.Clamp( SprayProgress + 1, 0, 100 );

		if ( IsSprayCompleted )
			OnSprayCompleted( player );
	}

	private void OnSprayCompleted( Player sprayer )
	{
	}

	[Event.Tick.Server]
	public void OnTick()
	{
		DebugOverlay.Text( $"{SprayProgress}/100", Position );
		DebugOverlay.Text( $"{SprayOwner?.Name}", Position + Vector3.Up * 10 );
	}

	/// <summary>
	/// Resets the <see cref="GraffitiSpot"/> back to default once the <see cref="PlayState"/> has been entered/exited.
	/// </summary>
	[GangJam.Events.EnterGameState]
	private void CleanupOnStateChange( IGameState newGameState, IGameState oldGameState )
	{
		if ( newGameState is not PlayState && oldGameState is not PlayState )
			return;

		SprayOwner = null;
		SprayProgress = 0;
	}
}
