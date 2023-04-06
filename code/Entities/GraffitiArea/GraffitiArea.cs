namespace ParkourPainters.Entities;

/// <summary>
/// A spot that a player can graffiti.
/// </summary>
[Title( "Graffiti Area" ), Category( "Parkour Painters" )]
[HammerEntity]
[DrawAngles, Solid, AutoApplyMaterial( "materials/sprays/spray_area_hatching.vmat" )]
public sealed partial class GraffitiArea : ModelEntity
{
	/// <summary>
	/// How difficult is it to reach this zone?
	/// </summary>
	[Property, Net] public GraffitiAreaDifficulty PointsType { get; set; } = GraffitiAreaDifficulty.Easy;

	/// <summary>
	/// How big should sprays appear in this graffiti area?
	/// </summary>
	[Property]
	public float SprayScale { get; set; } = 3;

	/// <summary>
	/// Returns the spray that was last fully completed.
	/// </summary>
	public Spray LastCompletedSpray => Sprays?.LastOrDefault( x => x.IsSprayCompleted );

	/// <summary>
	/// Whether the graffiti area is allowed to receive sprays.
	/// </summary>
	public bool IsLocked => Sprays?.LastOrDefault( x => x.IsLocked ) is not null;

	/// <summary>
	/// The team that currently owns the last completed spray.
	/// </summary>
	public Team AreaOwner => LastCompletedSpray?.TeamOwner;

	/// <summary>
	/// The sprays contained within this area.
	/// </summary>
	[Net] public IList<Spray> Sprays { get; set; }

	/// <summary>
	/// The player that is currently spraying this GraffitiArea
	/// </summary>
	[Net] private Player SprayingPlayer { get; set; }

	[Net] private TimeSince TimeSinceLastSprayed { get; set; }

	/// <summary>
	/// Returns whether or not the area has been sprayed.
	/// </summary>
	public bool IsAreaSprayed => Sprays.Any( x => x.IsSprayCompleted );

	private TimeSince TimeSinceLastSprayFailEffect { get; set; }

	private float SprayHintOffset { get; set; }

	/// <summary>
	/// The vertical offset from the trace hit position.
	/// </summary>
	private Vector3 SprayPositionZOffset = Vector3.Up * 10f;

	private LockWorldPanel LockWorldPanel { get; set; }

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromModel( PhysicsMotionType.Static );

		Tags.Add( "graffiti_area" );

		Transmit = TransmitType.Always;

		EnableShadowCasting = false;
	}

	/// <summary>
	/// Invoked when the spot has been sprayed by a player.
	/// </summary>
	/// <param name="player">The player that is spraying on the spot.</param>
	/// <param name="hitTrace">The TraceResult that this method is fired with.</param>
	public void OnSprayReceived( Player player, TraceResult hitTrace )
	{
		var wishPosition = hitTrace.HitPosition;
		var hitNormal = hitTrace.Normal;

		// Do nothing if the player isn't assigned to a team.
		if ( player.Team is null )
			return;

		// Do nothing if this is already being sprayed by a different player.
		if ( SprayingPlayer is not null && SprayingPlayer != player )
			return;

		if ( IsLocked )
			return;

		var mostRecentSpray = Sprays.LastOrDefault();

		// No sprays
		if ( mostRecentSpray is null )
		{
			// Do nothing if the spray won't fit in this area, or overlaps an edge of this graffiti area.
			if ( !InPermittedSprayZone( hitTrace ) )
			{
				DoSprayFailEffects( wishPosition );
				return;
			}

			if ( Game.IsServer )
			{
				var sprayLookDirection = (player.Position - wishPosition).Normal.ProjectOnNormal( hitNormal );
				Sprays.Add( Spray.CreateFrom( player.Team, new Transform().WithPosition( wishPosition + SprayPositionZOffset ).WithRotation( Rotation.LookAt( sprayLookDirection, Vector3.Up ) * Rotation.FromPitch( 90 ) ).WithScale( SprayScale ) ) );
			}

			SprayingPlayer = player;
			TimeSinceLastSprayed = 0;
		}
		else
		{
			var distanceToleranceUnits = 50;

			// Do nothing if the player is spraying too far away.
			if ( Vector3.DistanceBetween( wishPosition, mostRecentSpray.Position ) > distanceToleranceUnits )
			{
				// Convey to the player that they're spraying too far away.
				DoSprayFailEffects( wishPosition );
				return;
			}

			// Check if most recent spray belongs to the players team.
			if ( mostRecentSpray.TeamOwner == player.Team )
			{
				// Continue spray if it isn't already completed.
				if ( mostRecentSpray.IsSprayCompleted )
				{
					DoSprayFailEffects( wishPosition );
					return;
				}

				mostRecentSpray.ReceiveSprayFrom( player );
			}
			else
			{
				// Do nothing if the spray won't fit in this area, or overlaps an edge of this graffiti area.
				if ( !InPermittedSprayZone( hitTrace ) )
				{
					DoSprayFailEffects( wishPosition );
					return;
				}

				var maxOverlappedSprays = 2;
				var sprayForwardOverlapUnits = 1f;

				if ( Game.IsServer && Sprays.Count + 1 > maxOverlappedSprays )
				{
					// Cleanup old spray.
					var sprayToDelete = Sprays.First();
					var deletedSprayForward = sprayToDelete.Rotation.Up;
					Sprays.Remove( sprayToDelete );
					sprayToDelete.Delete();

					// Move remaining spray backwards.
					var remainingSpray = Sprays.Last();
					remainingSpray.Position -= deletedSprayForward * sprayForwardOverlapUnits;
				}

				// Overwrite other teams spray.
				Event.Run( ParkourPainters.Events.GraffitiSpotTampered, mostRecentSpray.TeamOwner, player.Team, player );

				if ( Game.IsServer )
				{
					var sprayLookDirection = (player.Position - wishPosition).Normal.ProjectOnNormal( hitNormal );
					Sprays.Add( Spray.CreateFrom( player.Team, new Transform().WithPosition( wishPosition + SprayPositionZOffset + (Sprays.Last().Rotation.Up * sprayForwardOverlapUnits) ).WithRotation( Rotation.LookAt( sprayLookDirection, Vector3.Up ) * Rotation.FromPitch( 90 ) ).WithScale( SprayScale ) ) );

					Sprays.First().UpdateRenderOrder( false );
					Sprays.Last().UpdateRenderOrder( true );
				}
			}

			SprayingPlayer = player;
			TimeSinceLastSprayed = 0;
		}
	}

	private void DoSprayFailEffects( Vector3 position )
	{
		if ( Game.IsServer )
			return;

		if ( TimeSinceLastSprayFailEffect < 0.2f )
			return;

		Particles.Create( "particles/paint/spray_fail.vpcf", position + Rotation.Forward );
		TimeSinceLastSprayFailEffect = 0;
	}

	/// <summary>
	/// Returns whether or not spraying in this position will overlap the bounds of the GraffitiArea.
	/// </summary>
	/// <param name="hitTrace"></param>
	/// <returns></returns>
	private bool InPermittedSprayZone( TraceResult hitTrace )
	{
		var spraySize = 16 * SprayScale;
		var wishPosition = hitTrace.HitPosition + SprayPositionZOffset;
		var leftRotation = Vector3.Cross( Vector3.Down, hitTrace.Normal );
		var rightRotation = Vector3.Cross( Vector3.Up, hitTrace.Normal );

		Vector3[] tracePositions = new Vector3[]
		{
			wishPosition + (leftRotation + Vector3.Up) * spraySize / 2,
			wishPosition + (rightRotation + Vector3.Up) * spraySize / 2,
			wishPosition + (leftRotation + Vector3.Down) * spraySize / 2,
			wishPosition + (rightRotation + Vector3.Down) * spraySize / 2
		};

		foreach ( var testPoint in tracePositions )
		{
			var backwardTrace = Trace.Ray( testPoint + hitTrace.Normal * 10, testPoint + -hitTrace.Normal * 60f ).WithTag( "graffiti_area" ).Run();

			//DebugOverlay.Sphere( testPoint, 2f, Color.White, 20f );

			if ( !backwardTrace.Hit )
				return false;
		}

		return true;
	}

	[Event.Tick.Server]
	public void OnTickServer()
	{
		if ( TimeSinceLastSprayed > 1f )
			SprayingPlayer = null;
	}

	[Event.Tick.Client]
	public void OnTickClient()
	{
		if ( SceneObject is null )
			return;

		if ( IsLocked && !LockWorldPanel.IsValid() )
			LockWorldPanel = new LockWorldPanel( LastCompletedSpray, AreaOwner.Group.SprayColor, LastCompletedSpray.TimeSinceLastCompletedSpray );

		if ( !IsLocked && LockWorldPanel.IsValid() )
			LockWorldPanel.Delete( true );

		// Hatching based on player distance
		var player = Game.LocalPawn;

		if ( player is not null && !Sprays.Any() )
		{
			SceneObject.Attributes.Set( "glow_amount", Vector3.DistanceBetween( player.Position, Position ) / 60 );
			SceneObject.Attributes.Set( "texture_offset", SprayHintOffset );

			SprayHintOffset += 0.03f * Time.Delta;
		}
		else
			SceneObject.Attributes.Set( "glow_amount", -1f );
	}

	/// <summary>
	/// Resets the <see cref="GraffitiArea"/> back to default once the <see cref="PlayState"/> has been entered/exited.
	/// </summary>
	[ParkourPainters.Events.EnterGameState]
	private void CleanupOnStateChange( IGameState newGameState, IGameState oldGameState )
	{
		if ( oldGameState is PlayState && newGameState is GameOverState )
			return;

		// Destroy all the created sprays.
		foreach ( var spray in Sprays )
			spray.Delete();

		Sprays.Clear();
	}

	[Event.Tick.Client]
	private void DebugDraw()
	{
		if ( !ParkourPainters.DebugMode )
			return;

		DebugOverlay.Text( $"{Sprays.Count} sprays (Latest from {(AreaOwner?.ToString() ?? "No one")})", Position );
	}
}
