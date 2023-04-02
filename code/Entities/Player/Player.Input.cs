namespace ParkourPainters.Entities;

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
	/// The currently held item.
	/// </summary>
	public BaseCarriable HeldItem
	{
		get
		{
			if ( Client.IsBot && heldItemInput is not null )
			{
				if ( Inventory.CanAddItem( heldItemInput.GetType() ) )
					return Inventory.GetItem( heldItemInput.GetType() );
				else
					return null;
			}

			return (BaseCarriable)heldItemInput;
		}
	}
	/// <summary>
	/// The currently held item.
	/// </summary>
	[ClientInput]
	private Entity heldItemInput { get; set; }

	/// <summary>
	/// The last held item.
	/// </summary>
	[Net, Predicted] private BaseCarriable LastHeldItem { get; set; }

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

		if ( Input.Pressed( InputButton.Slot1 ) )
			SwitchTo( 0 );
		if ( Input.Pressed( InputButton.Slot2 ) )
			SwitchTo( 1 );
		if ( Input.Pressed( InputButton.Slot0 ) )
			SwitchTo( null );

		if ( Input.Pressed( InputButton.SlotNext ) )
			SwitchTo( Inventory.Items.IndexOf( HeldItem ) + 1 );
		if ( Input.Pressed( InputButton.SlotPrev ) )
			SwitchTo( Inventory.Items.IndexOf( HeldItem ) - 1 );
	}

	/// <summary>
	/// Switches the currently held item to one at the desired index into the <see cref="Inventory"/> items.
	/// </summary>
	/// <param name="index">The index into the <see cref="Inventory"/> items to look at.</param>
	private void SwitchTo( int? index = null )
	{
		if ( index is null )
		{
			heldItemInput = null;
			return;
		}

		while ( index >= Inventory.Items.Count )
			index -= Inventory.Items.Count;

		while ( index < 0 )
			index += Inventory.Items.Count;

		if ( HeldItem == Inventory.Items[index.Value] )
			heldItemInput = null;
		else
		{
			if ( !Inventory.Items[index.Value].CanUseWhileClimbing && IsClimbing )
			{
				heldItemInput = null;
				return;
			}

			heldItemInput = Inventory.Items[index.Value];
		}
	}
}
