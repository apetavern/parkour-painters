using Sandbox.Citizen;

public sealed class PlayerMovement : Component
{
	// Movement Properties
	[Property] public float GroundControl { get; set; } = 4f;
	[Property] public float AirControl { get; set; } = 0.1f;
	[Property] public float MaxForce { get; set; } = 50f;
	[Property] public float Speed { get; set; } = 300f;
	[Property] public float RunSpeed { get; set; } = 500f;
	[Property] public float CrouchSpeed { get; set; } = 100f;
	[Property] public float JumpForce { get; set; } = 400f;

	// Object References
	[Property] public GameObject Head { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] public CameraMovement Camera { get; set; }


	[Sync] public PlayerMovementState MoveState { get; set; } = PlayerMovementState.Walking;

	// Member Variables
	public Vector3 WishVelocity = Vector3.Zero;
	[Sync] public bool IsCrouching { get; set; } = false;
	[Sync] public bool IsSprinting { get; set; } = false;

	private CharacterController characterController;
	private CitizenAnimationHelper animationHelper;

	protected override void OnAwake()
	{
		characterController = Components.Get<CharacterController>();
		animationHelper = Components.Get<CitizenAnimationHelper>();
	}

	protected override void OnUpdate()
	{
		RotateBody();
		// Set our sprinting and crouching states
		UpdateCrouch();
		UpdateAnimations();

		if ( IsProxy )
		{
			return;
		}


		//TODO: Move to a process input function
		IsSprinting = Input.Down( "Run" );
		if ( Input.Pressed( "Jump" ) ) Jump();

	}

	protected override void OnFixedUpdate()
	{
		BuildWishVelocity();
		Move();
	}

	void BuildWishVelocity()
	{
		WishVelocity = 0;

		switch ( MoveState )
		{
			case PlayerMovementState.Walking:

				WishVelocity = GroundMovement( WishVelocity );
				break;
			default:
				WishVelocity = GroundMovement( WishVelocity );
				break;
		}
	}

	public Vector3 GroundMovement( Vector3 wishVelocity )
	{
		var rot = Rotation.LookAt( Camera.AimAngles );
		if ( Input.Down( "Forward" ) ) wishVelocity += rot.Forward;
		if ( Input.Down( "Backward" ) ) wishVelocity += rot.Backward;
		if ( Input.Down( "Left" ) ) wishVelocity += rot.Left;
		if ( Input.Down( "Right" ) ) wishVelocity += rot.Right;

		wishVelocity = wishVelocity.WithZ( 0 );
		if ( !wishVelocity.IsNearZeroLength ) wishVelocity = wishVelocity.Normal;

		if ( IsCrouching ) wishVelocity *= CrouchSpeed;
		else if ( IsSprinting ) wishVelocity *= RunSpeed;
		else wishVelocity *= Speed;

		return wishVelocity;
	}

	void Move()
	{
		// Get gravity from our scene
		var gravity = Scene.PhysicsWorld.Gravity;

		if ( characterController.IsOnGround )
		{
			// Apply friction/acceleration
			characterController.Velocity = characterController.Velocity.WithZ( 0 );
			characterController.Accelerate( WishVelocity );
			characterController.ApplyFriction( GroundControl );
		}
		else
		{
			// Apply air control / gravity
			characterController.Velocity += gravity * Time.Delta * 0.5f;
			characterController.Accelerate( WishVelocity.ClampLength( MaxForce ) );
			characterController.ApplyFriction( AirControl );
		}

		// Move the character controller
		characterController.Move();

		// Apply the second half of gravity after movement
		if ( !characterController.IsOnGround )
		{
			characterController.Velocity += gravity * Time.Delta * 0.5f;
		}
		else
		{
			characterController.Velocity = characterController.Velocity.WithZ( 0 );
		}
	}
	void RotateBody()
	{
		if ( Body is null ) return;

		var targetAngle = new Angles( 0, Head.Transform.Rotation.Yaw(), 0 ).ToRotation();
		float rotateDifference = Body.Transform.Rotation.Distance( targetAngle );

		if ( rotateDifference > 50f || characterController.Velocity.Length > 10f )
		{
			Body.Transform.Rotation = Rotation.Lerp( Body.Transform.Rotation, targetAngle, Time.Delta * 10f );
		}
	}

	void Jump()
	{
		if ( !characterController.IsOnGround ) return;
		{
			characterController.Punch( Vector3.Up * JumpForce );
			animationHelper.TriggerJump();
		}
	}

	void UpdateAnimations()
	{
		if ( animationHelper is null ) return;
		animationHelper.WithWishVelocity( WishVelocity );
		animationHelper.WithVelocity( characterController.Velocity );
		animationHelper.AimAngle = Head.Transform.Rotation;
		animationHelper.IsGrounded = characterController.IsOnGround;
		animationHelper.WithLook( Head.Transform.Rotation.Forward, 1f, 0.75f, 0.5f );
		animationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Auto;
		animationHelper.DuckLevel = IsCrouching ? 1f : 0f;
	}

	void UpdateCrouch()
	{
		if ( characterController is null ) return;

		if ( Input.Pressed( "Duck" ) && !IsCrouching )
		{
			IsCrouching = true;
			characterController.Height /= 2f;
		}

		if ( Input.Released( "Duck" ) && IsCrouching )
		{
			IsCrouching = false;
			characterController.Height *= 2f;
		}
	}
}