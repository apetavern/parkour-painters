namespace ParkourPainters.Entities;

/// <summary>
/// A spot that a player can graffiti.
/// </summary>
[Title( "Graffiti Area" ), Category( "Parkour Painters" )]
[HammerEntity]
[BoundsHelper( "Mins", "Maxs" ), DrawAngles]
public sealed partial class GraffitiArea : ModelEntity
{
	/// <summary>
	/// The mins property populated by the BoundsHelper when used in Hammer.
	/// </summary>
	[Property] public Vector3 Mins { get; set; }

	/// <summary>
	/// The maxs property populated by the BoundsHelper when used in Hammer.
	/// </summary>
	[Property] public Vector3 Maxs { get; set; }

	/// <summary>
	/// How difficult is it to reach this zone?
	/// </summary>
	[Property, Net] public GraffitiAreaDifficulty PointsType { get; set; } = GraffitiAreaDifficulty.Easy;

	/// <summary>
	/// Returns the spray that was last fully completed.
	/// </summary>
	public Spray LastCompletedSpray => Sprays.LastOrDefault( x => x.IsSprayCompleted );

	/// <summary>
	/// The team that currently owns the last completed spray.
	/// </summary>
	public Team AreaOwner => LastCompletedSpray?.TeamOwner;

	/// <summary>
	/// The sprays contained within this area.
	/// </summary>
	[Net] public IList<Spray> Sprays { get; set; }

	/// <summary>
	/// Returns whether or not the area has been sprayed.
	/// </summary>
	public bool IsAreaSprayed => Sprays.Any( x => x.IsSprayCompleted );

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromAABB( PhysicsMotionType.Static, Mins, Maxs );

		Tags.Add( "graffiti_area" );
	}

	/// <summary>
	/// Invoked when the spot has been sprayed by a player.
	/// </summary>
	/// <param name="player">The player that is spraying on the spot.</param>
	/// <param name="wishPosition">The position that player wants to spray at.</param>
	public void OnSprayReceived( Player player, Vector3 wishPosition )
	{
		// Do nothing if the player isn't assigned to a team.
		if ( player.Team is null )
			return;

		var mostRecentSpray = Sprays.LastOrDefault();

		// No sprays
		if ( mostRecentSpray is null )
		{
			if ( Game.IsServer )
			{
				// By default the spray sprays quite low, offset vertically it manually.
				var verticalOffsetZ = 20f;

				Sprays.Add( Spray.CreateFrom( player.Team, new Transform().WithPosition( wishPosition + Vector3.Up * verticalOffsetZ ).WithRotation( Rotation * Rotation.FromPitch( 90 ) ) ) );
			}
		}
		else
		{
			var distanceToleranceUnits = 50;

			// Do nothing if the player is spraying too far away.
			if ( Vector3.DistanceBetween( wishPosition, mostRecentSpray.Position ) > distanceToleranceUnits )
			{
				// Convey to the player that they're spraying too far away.
				// TODO: This ^
				return;
			}

			// Check if most recent spray belongs to the players team.
			if ( mostRecentSpray.TeamOwner == player.Team )
			{
				// Continue spray if it isn't already completed.
				if ( mostRecentSpray.IsSprayCompleted )
					return;

				mostRecentSpray.ReceiveSprayFrom( player );
			}
			else
			{
				// Overwrite other teams spray.
				Event.Run( ParkourPainters.Events.GraffitiSpotTampered, mostRecentSpray.TeamOwner, player.Team, player );

				if ( Game.IsServer )
					Sprays.Add( Spray.CreateFrom( player.Team, new Transform().WithPosition( wishPosition ).WithRotation( Rotation * Rotation.FromPitch( 90 ) ) ) );
			}
		}
	}

	/// <summary>
	/// Resets the <see cref="GraffitiArea"/> back to default once the <see cref="PlayState"/> has been entered/exited.
	/// </summary>
	[ParkourPainters.Events.EnterGameState]
	private void CleanupOnStateChange( IGameState newGameState, IGameState oldGameState )
	{
		if ( newGameState is not PlayState && oldGameState is not PlayState )
			return;

		// Destroy all the created sprays.
		foreach ( var spray in Sprays )
			spray.Delete();

		// Recreate list.
		Sprays = new List<Spray>();
	}

#if DEBUG
	[Event.Tick.Client]
	private void DebugDraw()
	{
		DebugOverlay.Text( $"{Sprays.Count} sprays (Latest from {(AreaOwner?.ToString() ?? "No one")})", Position );

		foreach ( var spray in Sprays )
			DebugOverlay.Text( $"{spray.SprayProgress} ({spray.TeamOwner})", spray.Position );
	}
#endif
}
