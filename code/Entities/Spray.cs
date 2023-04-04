namespace ParkourPainters.Entities;

public partial class Spray : ModelEntity
{
	/// <summary>
	/// The team that owns this spray.
	/// </summary>
	[Net] public Team TeamOwner { get; set; }

	/// <summary>
	/// The percentage progress the <see ref="SprayOwner"/> has made on completing the graffiti.
	/// </summary>
	[Net] public float SprayProgress { get; set; }

	/// <summary>
	/// The time in seconds since the spot was last sprayed on.
	/// </summary>
	[Net] public TimeSince TimeSinceLastSprayed { get; private set; }

	/// <summary>
	/// The particle system that is shown when spraying on the spot.
	/// </summary>
	private Particles SprayCloud { get; set; }

	public bool IsSprayCompleted => SprayProgress >= 100;

	private float GlowAmount { get; set; }

	private bool HasGlowed { get; set; }

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		EnableShadowCasting = false;

		SetModel( "models/entities/spray_plane/spray_plane.vmdl" );

		SprayProgress = 0;

		if ( TeamOwner is not null )
			SetMaterialOverride( Material.Load( Random.Shared.FromList( TeamOwner.Group.AvailableSprays ) ) );
	}

	public void ReceiveSprayFrom( Player player )
	{
		// First time being sprayed.
		if ( SprayProgress <= 0 )
		{
			if ( TeamOwner is not null )
				SetMaterialOverride( Material.Load( Random.Shared.FromList( TeamOwner.Group.AvailableSprays ) ) );
		}

		SprayProgress = Math.Clamp( SprayProgress + player.SprayAmount, 0, 100 );
		TimeSinceLastSprayed = 0;

		if ( Game.IsClient )
		{
			SceneObject.Attributes.Set( "fade_amount", SprayProgress / 10 );

			// Create spray cloud clientside.
			if ( SprayCloud is null && Prediction.FirstTime )
			{
				SprayCloud = Particles.Create( "particles/paint/spray_cloud.vpcf", Position );

				if ( player.Team?.Group?.SprayColor is not null )
					SprayCloud.SetPosition( 1, player.Team.Group.SprayColor.ToVector3() );
			}
		}

		if ( IsSprayCompleted )
			OnSprayCompleted( player );
	}

	/// <summary>
	/// Invoked once a spray has been completed by a player.
	/// </summary>
	/// <param name="sprayer">The player that completed the <see cref="GraffitiArea"/>.</param>
	private void OnSprayCompleted( Player sprayer )
	{
		Event.Run( ParkourPainters.Events.GraffitiSpotCompleted, sprayer.Team, sprayer );

		Sound.FromWorld( "spray_completed", Position );
	}

	public static Spray CreateFrom( Team team, Transform transform )
	{
		return new Spray()
		{
			TeamOwner = team,
			Transform = transform,
			Scale = transform.Scale // Scale in this instance applies n * 16
		};
	}

	/// <summary>
	/// An effect for when the spray has been completed.
	/// </summary>
	private void DoCompletedGlow()
	{
		float glowSpeed = 18f;

		if ( !HasGlowed )
		{
			GlowAmount = Math.Clamp( GlowAmount + 0.1f * Time.Delta * glowSpeed, 0, 1 );

			if ( GlowAmount >= 1 )
				HasGlowed = true;
		}
		else
			GlowAmount = Math.Clamp( GlowAmount - 0.1f * Time.Delta * glowSpeed, 0, 1 );

		SceneObject?.Attributes.Set( "glow_amount", GlowAmount );
	}

	/// <summary>
	/// Performs various client-side checks for the <see cref="Spray"/>.
	/// </summary>
	[Event.Tick.Client]
	private void OnTickClient()
	{
		if ( IsSprayCompleted )
			DoCompletedGlow();

		if ( TimeSinceLastSprayed <= 0.2f )
			return;

		SprayCloud?.Destroy();
		SprayCloud = null;
	}

	/// <summary>
	/// Debug draws information relating to the <see cref="GraffitiArea"/>.
	/// </summary>
	[Event.Tick.Server]
	private void DebugDraw()
	{
		if ( !ParkourPainters.DebugMode )
			return;

		DebugOverlay.Text( $"{SprayProgress}/100 ({TeamOwner})", Position );
	}
}
