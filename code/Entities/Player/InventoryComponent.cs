namespace ParkourPainters.Entities;

/// <summary>
/// Contains an inventory of items that the player can use.
/// </summary>
internal sealed partial class InventoryComponent : EntityComponent<Player>, ISingletonComponent
{
	/// <summary>
	/// Contains all carriable items.
	/// </summary>
	[Net] private IList<BaseCarriable> items { get; set; }
	/// <summary>
	/// A readonly list of all carriable items contained.
	/// </summary>
	internal IReadOnlyList<BaseCarriable> Items => items as IReadOnlyList<BaseCarriable>;

	/// <summary>
	/// A queue of items that are waiting to be removed from the inventory.
	/// </summary>
	private readonly HashSet<BaseCarriable> defferedItemRemoval = new();

	/// <summary>
	/// Adds a new <see cref="BaseCarriable"/> that the player can equip.
	/// </summary>
	/// <param name="carriableType">The type of the <see cref="BaseCarriable"/> to add.</param>
	/// <returns>The newly created <see cref="BaseCarriable"/>.</returns>
	/// <exception cref="ArgumentException">Thrown when the type provided is either not assignable to <see cref="BaseCarriable"/> or is already in the inventory.</exception>
	internal BaseCarriable AddToInventory( TypeDescription carriableType )
	{
		if ( !carriableType.TargetType.IsAssignableTo( typeof( BaseCarriable ) ) )
			throw new ArgumentException( $"The type {carriableType.Name} is not assignable to {nameof( BaseCarriable )}", nameof( carriableType ) );

		if ( !CanAddItem( carriableType ) )
			throw new ArgumentException( $"An item of type \"{carriableType.Name}\" is already in the inventory", nameof( carriableType ) );

		var carriable = carriableType.Create<BaseCarriable>();
		carriable.Owner = Entity;
		carriable.OnHolstered();
		items.Add( carriable );
		return carriable;
	}

	/// <summary>
	/// Adds a new <see cref="BaseCarriable"/> that the player can equip.
	/// </summary>
	/// <typeparam name="T">The type of the <see cref="BaseCarriable"/> to add.</typeparam>
	/// <returns>The newly created <see cref="BaseCarriable"/>.</returns>
	/// <exception cref="ArgumentException">Thrown when the type is already in the inventory.</exception>
	internal T AddToInventory<T>() where T : BaseCarriable, new()
	{
		if ( !CanAddItem<T>() )
			throw new ArgumentException( $"An item of type \"{typeof( T ).Name}\" is already in the inventory", nameof( T ) );

		var carriable = new T
		{
			Owner = Entity
		};
		items.Add( carriable );
		return carriable;
	}

	/// <summary>
	/// Queues a carriable item for removal from the inventory.
	/// </summary>
	/// <param name="carriable">The item to remove from the inventory.</param>
	/// <returns>True if the item is queued for removal. False if it already was.</returns>
	/// <exception cref="ArgumentException">Thrown when the item provided is not from the owners inventory.</exception>
	internal bool RemoveFromInventory( BaseCarriable carriable )
	{
		if ( !Items.Contains( carriable ) )
			throw new ArgumentException( $"{carriable} is not a part of this inventory", nameof( carriable ) );

		return defferedItemRemoval.Add( carriable );
	}

	/// <summary>
	/// Gets an item from the inventory.
	/// </summary>
	/// <param name="type">The type of item to get from the inventory.</param>
	/// <param name="fuzzy">Whether or not the type is a derivative of multiple types.</param>
	/// <returns>The item from the inventory.</returns>
	/// <exception cref="ArgumentException">Thrown when no item of type <see ref="T"/> is in the inventory.</exception>
	internal BaseCarriable GetItem( Type type, bool fuzzy = false )
	{
		if ( CanAddItem( type, fuzzy ) )
			throw new ArgumentException( $"No item of type \"{type.Name}\" is in the inventory", nameof( type ) );

		return fuzzy
			? Items.First( item => item.GetType().IsAssignableTo( type ) )
			: Items.First( item => item.GetType().Name == type.Name );
	}

	/// <summary>
	/// Gets an item from the inventory.
	/// </summary>
	/// <typeparam name="T">The type of item to get from the inventory.</typeparam>
	/// <param name="fuzzy">Whether or not the type is a derivative of multiple types.</param>
	/// <returns>The item from the inventory.</returns>
	/// <exception cref="ArgumentException">Thrown when no item of type <see ref="T"/> is in the inventory.</exception>
	internal T GetItem<T>( bool fuzzy = false ) where T : BaseCarriable => (T)GetItem( typeof( T ), fuzzy );

	/// <summary>
	/// Gets an item from the inventory. If it doesn't exist, it creates and returns it.
	/// </summary>
	/// <typeparam name="T">The type of the item to get and/or create.</typeparam>
	/// <returns>The item from the inventory.</returns>
	internal T GetItemOrAddToInventory<T>() where T : BaseCarriable, new()
	{
		if ( CanAddItem<T>() )
			return AddToInventory<T>();
		else
			return GetItem<T>();
	}

	/// <summary>
	/// Returns whether or not a type of a <see cref="BaseCarriable"/> can be added to the inventory.
	/// </summary>
	/// <param name="type">The carriable type.</param>
	/// <param name="fuzzy">Whether or not the type is a derivative of multiple types.</param>
	/// <returns>Whether or not the type of <see cref="BaseCarriable"/> can be added to the inventory.</returns>
	internal bool CanAddItem( Type type, bool fuzzy = false )
	{
		if ( !type.IsAssignableTo( typeof( BaseCarriable ) ) )
			return false;

		return fuzzy
			? !Items.Any( x => type.IsAssignableTo( type ) )
			: !Items.Any( x => type.Name == x.GetType().Name );
	}

	/// <summary>
	/// Returns whether or not a type of a <see cref="BaseCarriable"/> can be added to the inventory.
	/// </summary>
	/// <param name="typeDescription">The carriable type.</param>
	/// <param name="fuzzy">Whether or not the type is a derivative of multiple types.</param>
	/// <returns>Whether or not the type of <see cref="BaseCarriable"/> can be added to the inventory.</returns>
	internal bool CanAddItem( TypeDescription typeDescription, bool fuzzy = false ) => CanAddItem( typeDescription.TargetType, fuzzy );

	/// <summary>
	/// Returns whether or not a type of a <see cref="BaseCarriable"/> can be added to the inventory.
	/// </summary>
	/// <typeparam name="T">The carriable type.</typeparam>
	/// <param name="fuzzy">Whether or not the type is a derivative of multiple types.</param>
	/// <returns>Whether or not the type of <see cref="BaseCarriable"/> can be added to the inventory.</returns>
	internal bool CanAddItem<T>( bool fuzzy = false ) where T : BaseCarriable => CanAddItem( typeof( T ), fuzzy );

	/// <summary>
	/// Called when simulating as part of a player's tick. Like if it's a pawn.
	/// </summary>
	internal void Simulate( IClient cl )
	{
		foreach ( var item in Items )
			item.Simulate( cl );

		if ( !Game.IsServer )
			return;

		foreach ( var itemToDelete in defferedItemRemoval )
		{
			items.Remove( itemToDelete );
			itemToDelete.Delete();
		}
	}

	/// <summary>
	/// Called when simulating as part of a player's tick. Like if it's a pawn.
	/// </summary>
	internal void FrameSimulate( IClient cl )
	{
		foreach ( var item in Items )
			item.FrameSimulate( cl );
	}
}
