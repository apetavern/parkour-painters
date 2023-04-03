namespace ParkourPainters.Util;

/// <summary>
/// A helper struct to make working with citizen related animations easier.
/// </summary>
internal readonly ref struct CustomAnimationHelper
{
	/// <summary>
	/// The <see cref="AnimatedEntity"/> to apply the changes to.
	/// </summary>
	private AnimatedEntity Owner { get; }

	/// <summary>
	/// Initializes a new instance of <see cref="CustomAnimationHelper"/>.
	/// </summary>
	/// <param name="entity">The <see cref="AnimatedEntity"/> to apply changes to.</param>
	internal CustomAnimationHelper( AnimatedEntity entity )
	{
		Owner = entity;
	}

	/// <summary>
	/// Have the player look at this point in the world
	/// </summary>
	internal void WithLookAt( Vector3 look, float eyesWeight = 1.0f, float headWeight = 1.0f, float bodyWeight = 1.0f )
	{
		var aimRay = Owner.AimRay;

		Owner.SetAnimLookAt( "aim_eyes", aimRay.Position, look );
		Owner.SetAnimLookAt( "aim_head", aimRay.Position, look );
		Owner.SetAnimLookAt( "aim_body", aimRay.Position, look );

		Owner.SetAnimParameter( "aim_eyes_weight", eyesWeight );
		Owner.SetAnimParameter( "aim_head_weight", headWeight );
		Owner.SetAnimParameter( "aim_body_weight", bodyWeight );
	}

	/// <summary>
	/// Sets actual velocity related parameters.
	/// </summary>
	/// <param name="Velocity">The velocity to apply.</param>
	internal void WithVelocity( Vector3 Velocity )
	{
		var dir = Velocity;
		var forward = Owner.Rotation.Forward.Dot( dir );
		var sideward = Owner.Rotation.Right.Dot( dir );

		var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

		Owner.SetAnimParameter( "move_direction", angle );
		Owner.SetAnimParameter( "move_speed", Velocity.Length );
		Owner.SetAnimParameter( "move_groundspeed", Velocity.WithZ( 0 ).Length );
		Owner.SetAnimParameter( "move_y", sideward );
		Owner.SetAnimParameter( "move_x", forward );
		Owner.SetAnimParameter( "move_z", Velocity.z );
	}

	/// <summary>
	/// Sets the desired velocity related parameters.
	/// </summary>
	/// <param name="Velocity">The desired velocity to apply.</param>
	internal void WithWishVelocity( Vector3 Velocity )
	{
		var dir = Velocity;
		var forward = Owner.Rotation.Forward.Dot( dir );
		var sideward = Owner.Rotation.Right.Dot( dir );

		var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

		Owner.SetAnimParameter( "wish_direction", angle );
		Owner.SetAnimParameter( "wish_speed", Velocity.Length );
		Owner.SetAnimParameter( "wish_groundspeed", Velocity.WithZ( 0 ).Length );
		Owner.SetAnimParameter( "wish_y", sideward );
		Owner.SetAnimParameter( "wish_x", forward );
		Owner.SetAnimParameter( "wish_z", Velocity.z );
	}

	/// <summary>
	/// Sets the aim angle of citizen.
	/// </summary>
	internal Rotation AimAngle
	{
		set
		{
			value = Owner.Rotation.Inverse * value;
			var ang = value.Angles();

			Owner.SetAnimParameter( "aim_body_pitch", ang.pitch );
			Owner.SetAnimParameter( "aim_body_yaw", ang.yaw );
		}
	}

	/// <summary>
	/// Gets/sets the "aim_eyes_weight" parameter.
	/// </summary>
	internal float AimEyesWeight
	{
		get => Owner.GetAnimParameterFloat( "aim_eyes_weight" );
		set => Owner.SetAnimParameter( "aim_eyes_weight", value );
	}

	/// <summary>
	/// Gets/sets the "aim_head_weight" parameter.
	/// </summary>
	internal float AimHeadWeight
	{
		get => Owner.GetAnimParameterFloat( "aim_head_weight" );
		set => Owner.SetAnimParameter( "aim_head_weight", value );
	}

	/// <summary>
	/// Gets/sets the "aim_body_weight" parameter.
	/// </summary>
	internal float AimBodyWeight
	{
		get => Owner.GetAnimParameterFloat( "aim_body_weight" );
		set => Owner.SetAnimParameter( "aim_headaim_body_weight_weight", value );
	}

	/// <summary>
	/// Gets/sets the "move_shuffle" parameter.
	/// </summary>
	internal float FootShuffle
	{
		get => Owner.GetAnimParameterFloat( "move_shuffle" );
		set => Owner.SetAnimParameter( "move_shuffle", value );
	}

	/// <summary>
	/// Gets/sets the "duck" parameter.
	/// </summary>
	internal float DuckLevel
	{
		get => Owner.GetAnimParameterFloat( "duck" );
		set => Owner.SetAnimParameter( "duck", value );
	}

	/// <summary>
	/// Gets/sets the "voice" parameter.
	/// </summary>
	internal float VoiceLevel
	{
		get => Owner.GetAnimParameterFloat( "voice" );
		set => Owner.SetAnimParameter( "voice", value );
	}

	/// <summary>
	/// Gets/sets the "b_sit" parameter.
	/// </summary>
	internal bool IsSitting
	{
		get => Owner.GetAnimParameterBool( "b_sit" );
		set => Owner.SetAnimParameter( "b_sit", value );
	}

	/// <summary>
	/// Gets/sets the "b_grounded" parameter.
	/// </summary>
	internal bool IsGrounded
	{
		get => Owner.GetAnimParameterBool( "b_grounded" );
		set => Owner.SetAnimParameter( "b_grounded", value );
	}

	/// <summary>
	/// Gets/sets the "b_swim" parameter.
	/// </summary>
	internal bool IsSwimming
	{
		get => Owner.GetAnimParameterBool( "b_swim" );
		set => Owner.SetAnimParameter( "b_swim", value );
	}

	/// <summary>
	/// Gets/sets the "b_climbing" parameter.
	/// </summary>
	internal bool IsClimbing
	{
		get => Owner.GetAnimParameterBool( "b_climbing" );
		set => Owner.SetAnimParameter( "b_climbing", value );
	}

	/// <summary>
	/// Gets/sets the "b_noclip" parameter.
	/// </summary>
	internal bool IsNoclipping
	{
		get => Owner.GetAnimParameterBool( "b_noclip" );
		set => Owner.SetAnimParameter( "b_noclip", value );
	}

	/// <summary>
	/// Gets/sets the "b_weapon_lower" parameter.
	/// </summary>
	internal bool IsWeaponLowered
	{
		get => Owner.GetAnimParameterBool( "b_weapon_lower" );
		set => Owner.SetAnimParameter( "b_weapon_lower", value );
	}

	/// <summary>
	/// Represents a way for an item to be held.
	/// </summary>
	internal enum HoldTypes
	{
		None,
		Pistol,
		Rifle,
		Shotgun,
		HoldItem,
		Punch,
		Swing,
		RPG
	}

	/// <summary>
	/// Gets/sets the "holdtype" parameter.
	/// </summary>
	internal HoldTypes HoldType
	{
		get => (HoldTypes)Owner.GetAnimParameterInt( "holdtype" );
		set => Owner.SetAnimParameter( "holdtype", (int)value );
	}

	/// <summary>
	/// Represents a special way to move in the world.
	/// </summary>
	internal enum SpecialMovementTypes
	{
		None,
		LedgeGrab,
		Roll,
		WallSlide,
		Grind,
		LedgeGrabDangle,
	}

	/// <summary>
	/// Gets/sets the "special_movement_states" parameter.
	/// </summary>
	internal SpecialMovementTypes SpecialMovementType
	{
		get => (SpecialMovementTypes)Owner.GetAnimParameterInt( "special_movement_states" );
		set => Owner.SetAnimParameter( "special_movement_states", (int)value );
	}

	/// <summary>
	/// Represents the handedness of holding an item.
	/// </summary>
	internal enum Hand
	{
		Both,
		Right,
		Left
	}

	/// <summary>
	/// Gets/sets the "holdtype_handedness" parameter.
	/// </summary>
	internal Hand Handedness
	{
		get => (Hand)Owner.GetAnimParameterInt( "holdtype_handedness" );
		set => Owner.SetAnimParameter( "holdtype_handedness", (int)value );
	}

	/// <summary>
	/// Gets/sets the "daze_state" parameter.
	/// </summary>
	internal DazeType DazedState
	{
		get => (DazeType)Owner.GetAnimParameterInt( "daze_state" );
		set => Owner.SetAnimParameter( "daze_state", (int)value );
	}

	/// <summary>
	/// Gets/sets the "b_haspaint" parameter.
	/// </summary>
	internal bool HasPaint
	{
		get => Owner.GetAnimParameterBool( "b_haspaint" );
		set => Owner.SetAnimParameter( "b_spray", value );
	}

	/// <summary>
	/// Gets/sets the "b_spray" parameter.
	/// </summary>
	internal bool Spraying
	{
		get => Owner.GetAnimParameterBool( "b_spray" );
		set => Owner.SetAnimParameter( "b_spray", value );
	}

	/// <summary>
	/// Triggers the "b_jump" parameter.
	/// </summary>
	internal void TriggerJump()
	{
		Owner.SetAnimParameter( "b_jump", true );
	}

	/// <summary>
	/// Triggers the "b_deploy" parameter.
	/// </summary>
	internal void TriggerDeploy()
	{
		Owner.SetAnimParameter( "b_deploy", true );
	}
}
