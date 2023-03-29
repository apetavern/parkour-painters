namespace GangJam.Entities;

/// <summary>
/// A spot that a player can graffiti.
/// </summary>
[Library( "func_graffiti_spot" )]
[Title( "Graffiti Spot" ), Category( "Spray Down" )]
[HammerEntity, Solid, DrawAngles]
public sealed partial class GraffitiSpot : ModelEntity
{
	/// <summary>
	/// The team that is currently working on spraying this spot.
	/// </summary>
	[Net] public Team SprayOwner { get; private set; }

	/// <summary>
	/// The percentage progress the <see ref="SprayOwner"/> has made on completing the graffiti.
	/// </summary>
	[Net] public float SprayProgress { get; private set; }

	/// <summary>
	/// The time in seconds since the spot was last sprayed on.
	/// </summary>
	[Net] public TimeSince TimeSinceLastSprayed { get; private set; }

	/// <summary>
	/// Returns whether or not the spot has been completely sprayed.
	/// </summary>
	public bool IsSprayCompleted => SprayProgress >= 100;

	/// <summary>
	/// The particle system that is shown when spraying on the spot.
	/// </summary>
	private Particles SprayCloud { get; set; }

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromModel( PhysicsMotionType.Static );

		Tags.Add( "graffiti_spot" );
	}

	/// <summary>
	/// Invoked when the spot has been sprayed by a player.
	/// </summary>
	/// <param name="player">The player that is spraying on the spot.</param>
	public void OnSprayReceived( Player player )
	{
		// Reset spray progress if the spray owner is the new sprayer.
		if ( player.Team != SprayOwner )
		{
			var oldTeam = SprayOwner;
			SprayProgress = 0;
			SprayOwner = player.Team;

			if ( SprayOwner?.Group?.AvailableSprays is not null )
				SetMaterialOverride( Material.Load( Game.Random.FromList( SprayOwner.Group.AvailableSprays ) ) );

			if ( oldTeam is not null )
				Event.Run( GangJam.Events.GraffitiSpotTampered, oldTeam, SprayOwner, player );
		}

		// Bail if the spray has already been completed.
		if ( IsSprayCompleted )
			return;

		SprayProgress = Math.Clamp( SprayProgress + player.SprayAmount, 0, 100 );

		if ( Game.IsClient )
			SceneObject.Attributes.Set( "fade_amount", SprayProgress / 10 );

		// Create spray cloud clientside.
		if ( Game.IsClient && SprayCloud is null )
		{
			SprayCloud = Particles.Create( "particles/paint/spray_cloud.vpcf", Position );

			if ( player.Team?.Group is null )
				return;

			SprayCloud.SetPosition( 1, player.Team.Group.SprayColor.ToVector3() );
		}

		TimeSinceLastSprayed = 0;

		if ( IsSprayCompleted )
			OnSprayCompleted( player );
	}

	/// <summary>
	/// Invoked once a spray has been completed by a player.
	/// </summary>
	/// <param name="sprayer">The player that completed the <see cref="GraffitiSpot"/>.</param>
	private void OnSprayCompleted( Player sprayer )
	{
		Event.Run( GangJam.Events.GraffitiSpotCompleted, sprayer.Team, sprayer );
	}

	/// <summary>
	/// Checks whether or not the spray cloud needs to be cleaned up.
	/// </summary>
	[Event.Tick.Client]
	private void SprayCleanup()
	{
		if ( TimeSinceLastSprayed <= 0.2f )
			return;

		SprayCloud?.Destroy();
		SprayCloud = null;
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

		SprayCloud?.Destroy();
		SprayCloud = null;
	}

#if DEBUG
	/// <summary>
	/// Debug draws information relating to the <see cref="GraffitiSpot"/>.
	/// </summary>
	[Event.Tick.Server]
	private void DebugDraw()
	{
		DebugOverlay.Text( $"{SprayProgress}/100", Position );
		DebugOverlay.Text( $"{SprayOwner?.Name}", Position + Vector3.Up * 10 );
	}
#endif
}
