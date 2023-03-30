namespace ParkoutPainters.Entities;

partial class Player
{
	/// <summary>
	/// Contains all carriable items that the player has.
	/// </summary>
	[Net] public IList<BaseCarriable> Carriables { get; private set; }

	/// <summary>
	/// The item that the player is currently using.
	/// </summary>
	[Net] public BaseCarriable Carrying { get; private set; }

	/// <summary>
	/// Equips a new <see cref="BaseCarriable"/>.
	/// </summary>
	/// <param name="carriable">The item to equip.</param>
	public void Equip( BaseCarriable carriable )
	{
		// Holster anything we're currently carrying.
		Holster();

		if ( !Carriables.Contains( carriable ) )
			Carriables.Add( carriable );

		carriable?.OnEquipped( this );
		Carrying = carriable;
	}

	/// <summary>
	/// Holsters any equipped item.
	/// </summary>
	public void Holster()
	{
		Carrying?.OnHolstered();
		Carrying = null;
	}

	/// <summary>
	/// Returns whether or not a type of a <see cref="BaseCarriable"/> can be equipped.
	/// </summary>
	/// <param name="carriable">The carriable type.</param>
	/// <returns>Whether or not the type of <see cref="BaseCarriable"/> can be equipped.</returns>
	public bool CanEquip( Type carriable )
	{
		if ( !carriable.IsAssignableTo( typeof( BaseCarriable ) ) )
			return false;

		return !Carriables.Any( x => carriable.Name == x.ClassName );
	}
}
