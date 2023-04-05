namespace ParkourPainters.Entities.Carriables;

public sealed partial class StunWeapon : BaseCarriable
{
	public override string CarriableName => "Stun Weapon";

	public override string SlotText => "";

	public override bool CanUseWhileClimbing => false;

	protected override string ModelPath => "models/entities/melee_weapons/melee_weapons.vmdl";

	/// <summary>
	/// A boolean representation of the next attack anim type (swing at side or overhead)
	/// </summary>
	[Net] private bool _holdtypeAttack { get; set; }

	public override void Spawn()
	{
		base.Spawn();
	}

	protected override void OnPrimaryAttack()
	{
		base.OnPrimaryAttack();

		if ( Owner.IsDazed )
			return;

		_holdtypeAttack = !_holdtypeAttack;
		Owner.SetAnimParameter( "holdtype_attack", _holdtypeAttack ? 0 : 1 );
		Owner.SetAnimParameter( "b_attack", true );

		PlaySound( "sounds/weapons/bat_swing.sound" );

		var armPosition = Owner.EyePosition - Vector3.Up * 16f;
		var tr = Trace.Ray( armPosition, armPosition + Owner.LookInput.ToRotation().Forward * 65f )
			.Size( 8f )
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
