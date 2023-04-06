namespace ParkourPainters.Entities.Carriables;

public sealed partial class StunWeapon : BaseCarriable
{
	public override string CarriableName => "Stun Weapon";

	public override string SlotText => Charges.ToString();

	public override bool CanUseWhileClimbing => false;

	protected override string ModelPath => "models/entities/melee_weapons/melee_weapons.vmdl";

	private const int HitForce = 1250;

	/// <summary>
	/// A boolean representation of the next attack anim type (swing at side or overhead)
	/// </summary>
	[Net] private bool _holdtypeAttack { get; set; }

	/// <summary>
	/// The number of charges before it is removed from the inventory.
	/// </summary>
	[Net] public int Charges { get; internal set; } = 3;

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

		if ( tr.Hit && tr.Entity is Player player && !player.IsImmune && !player.IsDazed )
		{
			player.Daze( Owner, DazeType.PhysicalTrauma );

			if ( Game.IsServer )
				player.ApplyAbsoluteImpulse( tr.Direction * HitForce );

			Charges -= 1;
			if ( Charges <= 0 )
				_ = WaitForAnimationFinish();
		}
	}

	public override void OnHolstered()
	{
		base.OnHolstered();

		if ( Game.IsServer )
			HolsterToBack();
	}

	private async Task WaitForAnimationFinish()
	{
		await GameTask.Delay( 150 );

		if ( !Game.IsServer )
			return;

		if ( Owner.HeldItem == this )
			Owner.UnsetHeldItemInput( To.Single( Owner ) );

		Owner.Inventory.RemoveFromInventory( this );
	}

	[ConCmd.Admin]
	public static void TestStun()
	{
		var player = ConsoleSystem.Caller.Pawn as Player;

		player.Daze( player, DazeType.PhysicalTrauma );
	}
}
