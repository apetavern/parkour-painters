namespace ParkourPainters.Entities;

partial class Player
{
	/// <summary>
	/// Contains all carriable items that the player has.
	/// </summary>
	[Net] public IList<BaseCarriable> HeldItems { get; private set; }
	/// <summary>
	/// The last held item.
	/// </summary>
	[Net, Predicted] private BaseCarriable LastHeldItem { get; set; }

	/// <summary>
	/// Adds a new <see cref="BaseCarriable"/> that the player can equip.
	/// </summary>
	/// <param name="carriableType">The type of the <see cref="BaseCarriable"/> to add.</param>
	/// <returns>The newly created <see cref="BaseCarriable"/>.</returns>
	/// <exception cref="ArgumentException">Thrown when the type provided is either not assignable to <see cref="BaseCarriable"/> or is already in the inventory.</exception>
	public BaseCarriable AddToInventory( TypeDescription carriableType )
	{
		if ( !carriableType.TargetType.IsAssignableTo( typeof( BaseCarriable ) ) )
			throw new ArgumentException( $"The type {carriableType.Name} is not assignable to {nameof( BaseCarriable )}", nameof( carriableType ) );

		if ( !CanAddItem( carriableType ) )
			throw new ArgumentException( $"An item of type \"{carriableType.Name}\" is already in the inventory", nameof( carriableType ) );

		var carriable = carriableType.Create<BaseCarriable>();
		HeldItems.Add( carriable );
		return carriable;
	}

	/// <summary>
	/// Adds a new <see cref="BaseCarriable"/> that the player can equip.
	/// </summary>
	/// <typeparam name="T">The type of the <see cref="BaseCarriable"/> to add.</typeparam>
	/// <returns>The newly created <see cref="BaseCarriable"/>.</returns>
	/// <exception cref="ArgumentException">Thrown when the type is already in the inventory.</exception>
	public T AddToInventory<T>() where T : BaseCarriable, new()
	{
		if ( !CanAddItem<T>() )
			throw new ArgumentException( $"An item of type \"{typeof( T ).Name}\" is already in the inventory", nameof( T ) );

		var carriable = new T();
		HeldItems.Add( carriable );
		return carriable;
	}

	/// <summary>
	/// </summary>
	/// <param name="type">The carriable type.</param>
	/// <returns>Whether or not the type of <see cref="BaseCarriable"/> can be added to the inventory.</returns>
	public bool CanAddItem( Type type )
	{
		if ( !type.IsAssignableTo( typeof( BaseCarriable ) ) )
			return false;

		return !HeldItems.Any( x => type.Name == x.GetType().Name );
	}

	/// <summary>
	/// Returns whether or not a type of a <see cref="BaseCarriable"/> can be added to the inventory.
	/// </summary>
	/// <param name="typeDescription">The carriable type.</param>
	/// <returns>Whether or not the type of <see cref="BaseCarriable"/> can be added to the inventory.</returns>
	public bool CanAddItem( TypeDescription typeDescription ) => CanAddItem( typeDescription.TargetType );

	/// <summary>
	/// Returns whether or not a type of a <see cref="BaseCarriable"/> can be added to the inventory.
	/// </summary>
	/// <typeparam name="T">The carriable type.</typeparam>
	/// <returns>Whether or not the type of <see cref="BaseCarriable"/> can be added to the inventory.</returns>
	public bool CanAddItem<T>() where T : BaseCarriable => CanAddItem( typeof( T ) );
}
