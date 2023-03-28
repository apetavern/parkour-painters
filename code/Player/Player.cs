using Sandbox;

namespace GangJam;

[Prefab]
public sealed partial class Player : AnimatedEntity
{
	[BindComponent] public PlayerController Controller { get; }
	[BindComponent] public PlayerAnimator Animator { get; }
	[BindComponent] public PlayerCamera Camera { get; }

	[BindComponent] public WallJumpMechanic WallJumpMechanic { get; }
	[BindComponent] public LedgeGrabMechanic LedgeGrabMechanic { get; }
	[BindComponent] public DashMechanic DashMechanic { get; }

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
	public bool IsDazed => TimeSinceDazed <= GangJam.DazeTime;
	/// <summary>
	/// Whether or not the player is currently immune to dazing.
	/// </summary>
	public bool IsImmune => TimeSinceDazed > GangJam.DazeTime && TimeSinceDazed <= GangJam.ImmuneTime;

	/// <summary>
	/// The most recent way that the player was dazed.
	/// </summary>
	[Net] public DazeType DazeType { get; private set; }

	/// <summary>
	/// The time in seconds since the player was last dazed.
	/// </summary>
	[Net] private TimeSince TimeSinceDazed { get; set; } = float.MaxValue;

	public TimeSince TimeSinceFootstep { get; private set; } = 0;

	private static readonly Model PlayerModel = Model.Load( "models/player/player_gangjam.vmdl" );
	/// <summary>
	/// The alpha value to give to immune players.
	/// </summary>
	private const float ImmuneAlpha = 0.6f;

	/// <inheritdoc/>
	public override void Spawn()
	{
		Model = PlayerModel;
		Predictable = true;

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableLagCompensation = true;
		EnableHitboxes = true;

		Tags.Add( "player" );
	}

	/// <summary>
	/// Respawns the player.
	/// </summary>
	public void Respawn()
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

		Components.Create<PlayerController>();
		Components.RemoveAny<ControllerMechanic>();

		Components.Create<WalkMechanic>();
		Components.Create<AirMoveMechanic>();
		Components.Create<JumpMechanic>();
		Components.Create<UnstuckMechanic>();
		Components.Create<WallJumpMechanic>();
		Components.Create<LedgeGrabMechanic>();
		Components.Create<DashMechanic>();

		Components.Create<PlayerAnimator>();
		Components.Create<PlayerCamera>();

		SetupClothing();

		GangJam.Current.MoveToSpawnpoint( this );
		ResetInterpolation();
	}

	/// <summary>
	/// Dazes the player.
	/// </summary>
	/// <param name="attacker">The person that caused the daze to occur.</param>
	/// <param name="dazeType">The way that the person is going to be dazed.</param>
	/// <returns>Whether or not the player was actually dazed.</returns>
	public bool Daze( Player attacker, DazeType dazeType )
	{
		if ( IsDazed || IsImmune )
			return false;

		if ( GangJam.FriendlyFire && attacker.Team == Team )
			return false;

		TimeSinceDazed = 0;
		DazeType = dazeType;
		return true;
	}

	/// <inheritdoc/>
	public override void Simulate( IClient cl )
	{
		Controller?.Simulate( cl );
		Animator?.Simulate( cl );
		Carrying?.Simulate( cl );
	}

	/// <inheritdoc/>
	public override void FrameSimulate( IClient cl )
	{
		Controller?.FrameSimulate( cl );
		Camera?.Update( this );
	}

	/// <inheritdoc/>
	public override void OnKilled()
	{
		if ( LifeState != LifeState.Alive )
			return;

		LifeState = LifeState.Dead;
		EnableAllCollisions = false;
		EnableDrawing = false;

		Controller.Remove();
		Animator.Remove();
		Camera.Remove();

		// Disable all children as well.
		Children.OfType<ModelEntity>()
			.ToList()
			.ForEach( x => x.EnableDrawing = false );

		AsyncRespawn();
	}

	/// <summary>
	/// Called clientside every time we fire the footstep anim event.
	/// </summary>
	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( !Game.IsClient )
			return;

		if ( LifeState != LifeState.Alive )
			return;

		if ( TimeSinceFootstep < 0.2f )
			return;

		volume *= GetFootstepVolume();

		TimeSinceFootstep = 0;

		var tr = Sandbox.Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume );
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
	}

	[ClientRpc]
	private void SetAudioEffect( string effectName, float strength, float velocity = 20f, float fadeOut = 4f )
	{
		Audio.SetEffect( effectName, strength, velocity: 20.0f, fadeOut: 4.0f * strength );
	}

	[ConCmd.Server( "kill" )]
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
