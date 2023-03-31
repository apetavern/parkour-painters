namespace ParkourPainters.Entities;

public partial class ControllerMechanic : EntityComponent<Player>
{
	/// <summary>
	/// Is this mechanic active?
	/// </summary>
	public bool IsActive { get; protected set; }

	/// <summary>
	/// How long has it been since we activated this mechanic?
	/// </summary>
	public TimeSince TimeSinceStart { get; protected set; }

	/// <summary>
	/// How long has it been since we deactivated this mechanic?
	/// </summary>
	public TimeSince TimeSinceStop { get; protected set; }

	/// <summary>
	/// Standard cooldown for mechanics.
	/// </summary>
	public TimeUntil TimeUntilCanStart { get; protected set; }

	internal PlayerController Controller => Entity.Controller;

	/// <summary>
	/// Accessor for the player.
	/// </summary>
	public Player Player => Controller.Player;

	/// <summary>
	/// Used to dictate the most important mechanic to take information such as EyeHeight, WishSpeed.
	/// </summary>
	public virtual int SortOrder { get; set; } = 0;

	/// <summary>
	/// Override the current eye height.
	/// </summary>
	public virtual float? EyeHeight { get; set; } = null;

	/// <summary>
	/// Override the current wish speed.
	/// </summary>
	public virtual float? WishSpeed { get; set; } = null;

	public Vector3 Position
	{
		get => Controller.Position;
		set => Controller.Position = value;
	}

	public Vector3 Velocity
	{
		get => Controller.Velocity;
		set => Controller.Velocity = value;
	}

	public Entity GroundEntity
	{
		get => Controller.GroundEntity;
		set => Controller.GroundEntity = value;
	}

	/// <summary>
	/// Mechanics can override friction - the Walk mechanic drives this.
	/// </summary>
	public virtual float? FrictionOverride { get; set; } = null;

	/// <summary>
	/// Lets you override the movement input scale. 
	/// Dividing this by 2 would make the player move twice as slow.
	/// </summary>
	public virtual Vector3? MoveInputScale { get; set; } = null;

	internal bool TrySimulate( PlayerController controller )
	{
		var before = IsActive;
		IsActive = ShouldStart();

		if ( IsActive )
		{
			if ( before != IsActive )
			{
				Start();
			}

			Simulate();
		}
		// Deactivate
		if ( before && !IsActive )
		{
			Stop();
		}

		return IsActive;
	}

	public void Start()
	{
		TimeSinceStart = 0;
		OnStart();
	}

	public void Stop()
	{
		TimeSinceStop = 0;
		OnStop();
	}

	/// <summary>
	/// Called when the mechanic deactivates. For example, when you stop crouching.
	/// </summary>
	protected virtual void OnStop()
	{
		//
	}

	/// <summary>
	/// Called when the mechanic activates. For example, when you start sliding.
	/// </summary>
	protected virtual void OnStart()
	{
		//
	}

	/// <summary>
	/// Returns whether or not this ability should activate and simulate this tick.
	/// By default, it's set to TimeUntilCanNextActivate, which you can set in your own mechanics.
	/// </summary>
	/// <returns></returns>
	protected virtual bool ShouldStart()
	{
		return TimeUntilCanStart;
	}

	/// <summary>
	/// Runs every Simulate even if the mechanic isn't active.
	/// </summary>
	protected virtual void Simulate()
	{
		//
	}
}
