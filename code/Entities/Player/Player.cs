using Sandbox;

namespace SpeedPainters.Entities;

[Prefab]
public sealed partial class Player : AnimatedEntity
{
	[BindComponent] internal PlayerController Controller { get; }
	[BindComponent] internal PlayerAnimator Animator { get; }
	[BindComponent] internal PlayerCamera Camera { get; }
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
	/// Whether or not the player is currently climbing.
	/// </summary>
	public bool IsClimbing => LedgeGrabMechanic.IsActive || WallJumpMechanic.IsActive;

	/// <summary>
	/// The time in seconds since the player was last sprayed.
	/// </summary>
	[Net] private TimeSince TimeSinceSprayed { get; set; } = float.MaxValue;

	/// <summary>
	/// The spray particles that come out when using the can.
	/// </summary>
	private Particles SprayParticles { get; set; }

	/// <summary>
	/// Grind particles.
	/// </summary>
	private Particles GrindParticles { get; set; }

	/// <summary>
	/// Grind sound.
	/// </summary>
	private Sound GrindLoop { get; set; }

	private bool _isPlayingGrind { get; set; } = false;

	/// <summary>
	/// The time in seconds since the last footstep animation event happened.
	/// </summary>
	private TimeSince TimeSinceFootstep { get; set; } = 0;

	/// <summary>
	/// Clientside voice chat indicator.
	/// </summary>
	private VoiceChatIndicator VoiceChatIndicator { get; set; }

	private static readonly Model PlayerModel = Model.Load( "models/player/player_gangjam.vmdl" );

	/// <summary>
	/// The alpha value to give to immune players.
	/// </summary>
	private const float ImmuneAlpha = 0.6f;

	private NameWorldPanel _nameTag { get; set; }

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

		Components.Create<WalkMechanic>();
		Components.Create<AirMoveMechanic>();
		Components.Create<JumpMechanic>();
		Components.Create<UnstuckMechanic>();
		Components.Create<WallJumpMechanic>();
		Components.Create<LedgeGrabMechanic>();
		Components.Create<GrindMechanic>();
		Components.Create<DashMechanic>();

	}

	/// <inheritdoc/>
	public sealed override void ClientSpawn()
	{
		if ( !IsLocalPawn )
			_nameTag = new NameWorldPanel( this );
	}

	/// <inheritdoc/>
	public sealed override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( CurrentPowerup is not null && CurrentPowerup.TimeSinceAdded >= CurrentPowerup.ExpiryTime )
			CurrentPowerup.Remove();

		HandleGrindParticle();

		Controller?.Simulate( cl );
		Animator?.Simulate( cl );

		if ( !ReachedEnd )
			StopWatch += RealTime.SmoothDelta;
	}

	/// <inheritdoc/>
	public sealed override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		Controller?.FrameSimulate( cl );
		Camera?.Update( this );

		if ( Voice.IsRecording )
			IsSpeaking();
	}

	/// <inheritdoc/>
	public sealed override void OnKilled()
	{
		if ( LifeState != LifeState.Alive )
			return;

		LifeState = LifeState.Dead;
		EnableAllCollisions = false;
		EnableDrawing = false;

		UnsetHeldItemInput( To.Single( this ) );

		Animator?.Remove();
		Camera?.Remove();

		// Disable all children as well.
		Children.OfType<ModelEntity>()
			.ToList()
			.ForEach( x => x.EnableDrawing = false );

		AsyncRespawn();
	}

	protected sealed override void OnDestroy()
	{
		OnDestroyClient( To.Everyone );
		base.OnDestroy();
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

		if ( !tr.Hit )
			return;

		tr.Surface.DoFootstep( this, tr, foot, volume );
	}

	public void IsSpeaking()
	{
		if ( !VoiceChatIndicator.IsValid() )
			VoiceChatIndicator = new VoiceChatIndicator( this );

		VoiceChatIndicator.IsSpeaking();
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
		StopWatch = 0;
		ReachedEnd = false;
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

	[ClientRpc]
	private void SetAudioEffect( string effectName, float strength, float velocity = 20f, float fadeOut = 4f )
	{
		Audio.SetEffect( effectName, strength, velocity: 20.0f, fadeOut: 4.0f * strength );
	}

	[ClientRpc]
	private void OnDestroyClient()
	{
		if ( !IsLocalPawn )
			_nameTag.Delete();
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

	private void HandleGrindParticle()
	{
		if ( !Game.IsServer )
			return;

		if ( GrindMechanic.IsActive )
		{
			if ( !_isPlayingGrind )
			{
				GrindLoop = PlaySound( "grind_loop" );
				_isPlayingGrind = true;
			}

			GrindParticles ??= Particles.Create( "particles/sparks/sparks_base.vpcf", this );
			GrindParticles.SetEntityBone( 0, this, GetBoneIndex( "ankle_L" ) );
		}
		else
		{
			_isPlayingGrind = false;
			GrindLoop.Stop();
			GrindParticles?.Destroy( true );
			GrindParticles = null;
		}
	}
}
