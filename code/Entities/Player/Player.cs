using Sandbox;

namespace ParkourPainters.Entities;

[Prefab]
public sealed partial class Player : AnimatedEntity
{
	[BindComponent] internal PlayerController Controller { get; }
	[BindComponent] internal PlayerAnimator Animator { get; }
	[BindComponent] internal PlayerCamera Camera { get; }
	[BindComponent] internal InventoryComponent Inventory { get; }

	[BindComponent] internal BasePowerup CurrentPowerup { get; }

	[BindComponent] internal JumpMechanic JumpMechanic { get; }
	[BindComponent] internal WallJumpMechanic WallJumpMechanic { get; }
	[BindComponent] internal LedgeGrabMechanic LedgeGrabMechanic { get; }
	[BindComponent] internal GrindMechanic GrindMechanic { get; }
	[BindComponent] internal DashMechanic DashMechanic { get; }

	/// <summary>
	/// Returns the team that the player is a part of.
	/// </summary>
	public Team Team => Client.GetTeam();

	/// <summary>
	/// The amount of spray percent to add each tick to a <see cref="GraffitiSpot"/>.
	/// </summary>
	[Net, Prefab] public float SprayAmount { get; private set; } = 1;

	/// <summary>
	/// Whether or not the player is currently dazed.
	/// </summary>
	public bool IsDazed => TimeSinceDazed <= ParkourPainters.DazeTime;
	/// <summary>
	/// Whether or not the player is currently sprayed.
	/// </summary>
	public bool IsSprayed => TimeSinceSprayed <= ParkourPainters.SprayTime;
	/// <summary>
	/// Whether or not the player is currently immune to dazing.
	/// </summary>
	public bool IsImmune => CurrentPowerup is ShieldPowerup || TimeSinceDazed > ParkourPainters.DazeTime && TimeSinceDazed <= ParkourPainters.ImmuneTime;

	/// <summary>
	/// The current type of daze the player is experiencing.
	/// </summary>
	[Net] public DazeType DazeType { get; private set; }

	/// <summary>
	/// The time in seconds since the player was last dazed.
	/// </summary>
	[Net] private TimeSince TimeSinceDazed { get; set; } = float.MaxValue;

	/// <summary>
	/// The particles that are shown when the player is dazed.
	/// </summary>
	private Particles DazeParticles { get; set; }

	/// <summary>
	/// The time in seconds since the player was last sprayed.
	/// </summary>
	[Net] private TimeSince TimeSinceSprayed { get; set; } = float.MaxValue;

	/// <summary>
	/// The particles that are shown when the player has been sprayed.
	/// </summary>
	private Particles SprayCloud { get; set; }

	/// <summary>
	/// The time in seconds since the last footstep animation event happened.
	/// </summary>
	private TimeSince TimeSinceFootstep { get; set; } = 0;

	private static readonly Model PlayerModel = Model.Load( "models/player/player_gangjam.vmdl" );

	/// <summary>
	/// The alpha value to give to immune players.
	/// </summary>
	private const float ImmuneAlpha = 0.6f;

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		Model = PlayerModel;
		Predictable = true;

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableLagCompensation = true;
		EnableHitboxes = true;

		Tags.Add( "player" );

		Components.Create<PlayerController>();
		Components.Create<InventoryComponent>();

		Components.Create<WalkMechanic>();
		Components.Create<AirMoveMechanic>();
		Components.Create<JumpMechanic>();
		Components.Create<UnstuckMechanic>();
		Components.Create<WallJumpMechanic>();
		Components.Create<LedgeGrabMechanic>();
		Components.Create<GrindMechanic>();
		Components.Create<DashMechanic>();

		var sprayCan = Inventory.AddToInventory<SprayCan>();
		// TODO: This is jank
		sprayCan.OnEquipped();
		sprayCan.OnHolstered();
	}

	/// <inheritdoc/>
	public sealed override void ClientSpawn()
	{
		if ( !IsLocalPawn )
			_ = new UI.NameWorldPanel( this );
	}

	/// <inheritdoc/>
	public sealed override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( CurrentPowerup is not null && CurrentPowerup.TimeSinceAdded >= CurrentPowerup.ExpiryTime )
			CurrentPowerup.Remove();

		if ( LastHeldItem != HeldItem )
		{
			LastHeldItem?.OnHolstered();
			HeldItem?.OnEquipped();
			LastHeldItem = HeldItem;
		}

		Controller?.Simulate( cl );
		Animator?.Simulate( cl );
		Inventory?.Simulate( cl );
	}

	/// <inheritdoc/>
	public sealed override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		Controller?.FrameSimulate( cl );
		Camera?.Update( this );
		Inventory?.FrameSimulate( cl );
	}

	/// <inheritdoc/>
	public sealed override void OnKilled()
	{
		if ( LifeState != LifeState.Alive )
			return;

		LifeState = LifeState.Dead;
		EnableAllCollisions = false;
		EnableDrawing = false;

		Animator?.Remove();
		Camera?.Remove();

		// Disable all children as well.
		Children.OfType<ModelEntity>()
			.ToList()
			.ForEach( x => x.EnableDrawing = false );

		AsyncRespawn();
	}

	/// <summary>
	/// Called clientside every time we fire the footstep anim event.
	/// </summary>
	public sealed override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( !Game.IsClient )
			return;

		if ( LifeState != LifeState.Alive )
			return;

		if ( TimeSinceFootstep < 0.2f )
			return;

		volume *= GetFootstepVolume();

		TimeSinceFootstep = 0;

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume );
	}

	/// <summary>
	/// Respawns the player.
	/// </summary>
	internal void Respawn()
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );

		Health = 100;
		LifeState = LifeState.Alive;
		EnableAllCollisions = true;
		EnableDrawing = true;

		// Re-enable all children.
		Children.OfType<ModelEntity>()
			.ToList()
			.ForEach( x => x.EnableDrawing = true );

		Components.Create<PlayerAnimator>();
		Components.Create<PlayerCamera>();

		// Only change clothes once.
		if ( ClothingContainer is null )
			SetupClothing();

		ParkourPainters.Current.MoveToSpawnpoint( this );
		ResetInterpolation();
	}

	/// <summary>
	/// Dazes the player.
	/// </summary>
	/// <param name="attacker">The person that caused the daze to occur.</param>
	/// <param name="dazeType">The way that the person is going to be dazed.</param>
	/// <returns>Whether or not the player was actually dazed.</returns>
	internal bool Daze( Player attacker, DazeType dazeType )
	{
		if ( IsDazed || IsImmune )
			return false;

		if ( ParkourPainters.FriendlyFire && attacker.Team == Team )
			return false;

		TimeSinceDazed = 0;
		DazeType = dazeType;
		DazePlayerParticles( To.Everyone );
		return true;
	}

	/// <summary>
	/// Sprays the player.
	/// </summary>
	/// <param name="attacker">The person that caused the daze to occur.</param>
	internal void Spray( Player attacker )
	{
		if ( IsImmune || ParkourPainters.FriendlyFire && attacker.Team == Team )
			return;

		var wasSprayed = IsSprayed;
		TimeSinceSprayed = 0;
		DazeType = DazeType.Inhalation;

		if ( !wasSprayed )
			SprayPlayerParticles( To.Everyone, attacker.Team?.Group?.SprayColor ?? Color.Black );
	}

	private async void AsyncRespawn()
	{
		await GameTask.DelaySeconds( 3f );
		Respawn();
	}

	private float GetFootstepVolume()
	{
		return Controller.Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 1f;
	}

	/// <summary>
	/// Slightly fades out immune players.
	/// </summary>
	[Event.Tick]
	private void ImmuneTick()
	{
		if ( IsImmune )
			RenderColor = RenderColor.WithAlpha( ImmuneAlpha );
		else
			RenderColor = RenderColor.WithAlpha( 1 );

		if ( !IsDazed && DazeType != DazeType.Inhalation )
			DazeType = DazeType.None;

		if ( !IsSprayed && DazeType == DazeType.Inhalation )
			DazeType = DazeType.None;

		if ( !Game.IsClient )
			return;

		if ( !IsDazed )
		{
			DazeParticles?.Destroy();
			DazeParticles = null;
		}

		if ( !IsSprayed )
		{
			SprayCloud?.Destroy();
			SprayCloud = null;
		}
	}

	[ClientRpc]
	private void DazePlayerParticles()
	{
		if ( !Team.Group.DazeParticles.TryGetValue( DazeType, out var particle ) )
			particle = "particles/stun/stun_base.vpcf";

		DazeParticles = Particles.Create( particle, this, "hat" );
	}

	[ClientRpc]
	private void SprayPlayerParticles( Vector3 cloudColor )
	{
		SprayCloud = Particles.Create( "particles/paint/spray_cloud.vpcf", this, "eyes" );
		SprayCloud.SetPosition( 1, cloudColor );
	}

	[ClientRpc]
	private void SetAudioEffect( string effectName, float strength, float velocity = 20f, float fadeOut = 4f )
	{
		Audio.SetEffect( effectName, strength, velocity: 20.0f, fadeOut: 4.0f * strength );
	}

	[ConCmd.Admin( "kill" )]
	public static void DoSuicide()
	{
		(ConsoleSystem.Caller.Pawn as Player)?.TakeDamage( DamageInfo.Generic( 1000f ) );
	}

	[ConCmd.Admin( "sethp" )]
	public static void SetHP( float value )
	{
		(ConsoleSystem.Caller.Pawn as Player).Health = value;
	}
}
