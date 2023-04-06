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

	private const float HitForce = 2500f;
	[Net] public int Charges { get; internal set; } = 1;

	protected override void OnPrimaryAttack()
	{
		base.OnPrimaryAttack();

		if ( Owner.IsDazed )
			return;

		// Play attack anim
		Owner.SetAnimParameter( "b_attack", true );

		// Play a sound

		// Fire a trace
		var muzzleTransform = GetAttachment( "muzzle2" ).Value;
		var tr = Trace.Ray( muzzleTransform.Position, Owner.LookInput.ToRotation().Forward * 2048f )
				.Ignore( Owner )
				.Run();

		if ( tr.Hit )
		{
			// Do instantaneous particle
			_ = Particles.Create( "particles/weapons/boomblast_base.vpcf", muzzleTransform.Position );

			// Create explosion at end position
			var playersHit = FindInSphere( tr.EndPosition, 64 );
			DebugOverlay.Sphere( tr.EndPosition, 64f, Color.Red, 5f );

			// Do explosion particle at end position

			// Push players away
			if ( Game.IsServer )
			{
				Log.Info( playersHit.Count() );
				foreach ( var player in playersHit )
				{
					Log.Info( player );
					var impulse = (player.Position - tr.EndPosition).Normal * HitForce;
					if ( impulse.z < 0f )
						impulse = impulse.WithZ( 0f );
					Log.Info( impulse );
					player.ApplyAbsoluteImpulse( impulse );
				}
			}
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
