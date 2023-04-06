namespace ParkourPainters.Entities.Carriables;


/**
 * Notes:
 * - Instantaneous Fire
 * - Shoots a beam towards aim location, explosion where it lands
 * - Explosion knocks back all players within range, relative to center of
 * end position
 */
public partial class BoomBlaster : BaseCarriable
{
	public override string CarriableName => "Boom Blaster";

	public override string SlotText => Charges.ToString();
	public override bool CanUseWhileClimbing => false;

	protected override string ModelPath => "models/entities/boomblaster.vmdl";

	[Net] public int Charges { get; internal set; } = 1;

	protected override void OnPrimaryAttack()
	{
		base.OnPrimaryAttack();

		if ( Owner.IsDazed )
			return;

		// Play a sound

		// Fire a trace
		var muzzleTransform = GetAttachment( "muzzle2" ).Value;
		var tr = Trace.Ray( muzzleTransform.Position, muzzleTransform.Rotation.Forward * 2048f )
				.Ignore( Owner )
				.Run();

		if ( tr.Hit )
		{
			// Do instantaneous particle
			_ = Particles.Create( "particles/weapons/boomblast_base.vpcf", muzzleTransform.Position );

			// Create explosion at end position

			// Do explosion particle at end position

			// Push players away
		}

		Charges -= 1;
		if ( Charges <= 0 )
			_ = WaitForAnimationFinish();
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
}
