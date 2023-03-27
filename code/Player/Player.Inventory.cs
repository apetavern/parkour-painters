namespace GangJam;

public sealed partial class Player : AnimatedEntity
{
	[Net]
	public IList<BaseCarriable> Carriables { get; set; }

	[Net]
	public BaseCarriable Carrying { get; set; }

	public void Equip( BaseCarriable carriable )
	{
		// Holster anything we're currently carrying.
		Holster();

		Log.Info( $"Equipped {carriable}" );

		if ( !Carriables.Contains( carriable ) )
			Carriables.Add( carriable );

		carriable?.OnEquipped( this );
		Carrying = carriable;
	}

	public void Holster()
	{
		Carrying?.OnHolstered();
	}

	public bool CanEquip( Type carriable )
	{
		return !Carriables.Any( x => carriable.Name == x.ClassName );
	}
}
