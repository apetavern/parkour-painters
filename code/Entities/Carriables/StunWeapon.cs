namespace ParkourPainters.Entities.Carriables;

public sealed partial class StunWeapon : BaseCarriable
{
	public override string CarriableName => "Stun Weapon";

	public override string SlotText => "";

	public override bool CanUseWhileClimbing => false;

	protected override string ModelPath => "models/entities/melee_weapons/melee_weapons.vmdl";

	[Net] private int _bodyGroup { get; set; }
	[Net] private bool _holdtypeAttack { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		// This should probably be handled from the carriable spawner.
		_bodyGroup = Game.Random.Int( 0, 2 );
		SetBodyGroup( "weapontype", _bodyGroup );
	}

	protected override void OnPrimaryAttack()
	{
		base.OnPrimaryAttack();

		_holdtypeAttack = !_holdtypeAttack;
		Owner.SetAnimParameter( "holdtype_attack", _holdtypeAttack ? 0 : 1 );
		Owner.SetAnimParameter( "b_attack", true );

		var armPosition = Owner.EyePosition - Vector3.Up * 16f;
		var tr = Trace.Ray( armPosition, armPosition + Owner.LookInput.ToRotation().Forward * 50f )
			.Ignore( Owner )
			.Run();

		if ( tr.Hit && tr.Entity is Player player )
		{
			player.Daze( Owner, DazeType.PhysicalTrauma );
		}
	}

	public override void OnHolstered()
	{
		base.OnHolstered();

		if ( Game.IsServer )
			HolsterToBack();
	}
}
