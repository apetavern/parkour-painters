namespace ParkourPainters.Entities.Carriables;

public sealed partial class StunWeapon : BaseCarriable
{
	public override string CarriableName => "Stun Weapon";

	public override string SlotText => "Single Use";

	protected override string ModelPath => "models/entities/melee_weapons/melee_weapons.vmdl";

	[Net] private int _bodyGroup { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		// This should probably be handled from the carriable spawner.
		_bodyGroup = Game.Random.Int( 0, 2 );
		SetBodyGroup( "weapontype", _bodyGroup );
	}
}
