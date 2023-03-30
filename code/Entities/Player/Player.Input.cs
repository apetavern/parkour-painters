namespace GangJam.Entities;

public partial class Player
{
	/// <summary>
	/// Should be Input.AnalogMove
	/// </summary>
	[ClientInput] public Vector2 MoveInput { get; private set; }

	/// <summary>
	/// Normalized accumulation of Input.AnalogLook
	/// </summary>
	[ClientInput] public Angles LookInput { get; private set; }

	/// <summary>
	/// Position a player should be looking from in world space.
	/// </summary>
	[Browsable( false )]
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	/// <summary>
	/// Position a player should be looking from in local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Vector3 EyeLocalPosition { get; set; }

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity.
	/// </summary>
	[Browsable( false )]
	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	/// <summary>
	/// Rotation of the entity's "eyes" in local space.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Rotation EyeLocalRotation { get; set; }

	/// <inheritdoc/>
	public sealed override Ray AimRay => new( EyePosition, EyeRotation.Forward );

	/// <inheritdoc/>
	public sealed override void BuildInput()
	{
		MoveInput = Input.AnalogMove;
		var lookInput = (LookInput + Input.AnalogLook).Normal;

		LookInput = lookInput.WithPitch( lookInput.pitch.Clamp( -90f, 90f ) );
	}
}