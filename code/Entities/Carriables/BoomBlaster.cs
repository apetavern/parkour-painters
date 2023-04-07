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

	private const float HitForce = 1536f;
	private const float ExplosionRadius = 512;

	[Net] public int Charges { get; internal set; } = 1;

	protected override void OnPrimaryAttack()
	{
		base.OnPrimaryAttack();

		if ( Owner.IsDazed )
			return;

		// Play attack anim
		Owner.SetAnimParameter( "b_attack", true );

		// Play a sound
		Owner.PlaySound( "boomblaster_blast" );

		PushBlastBehavior();

		Charges -= 1;
		if ( Charges <= 0 )
			_ = WaitForAnimationFinish();
	}

	public void PushBlastBehavior()
	{
		var muzzleTransform = GetAttachment( "muzzle2" ).Value;

		// Create explosion at end position
		var playersHit = FindInSphere( muzzleTransform.Position, ExplosionRadius );

		// Fire a trace for rocketjumping-like behavior
		var tr = Trace.Ray( muzzleTransform.Position, muzzleTransform.Position + Owner.LookInput.ToRotation().Forward * ExplosionRadius )
				.Ignore( Owner )
				.Run();

		// Do instantaneous particle
		var particle = Particles.Create( "particles/weapons/boomblast_base.vpcf", muzzleTransform.Position );
		particle.SetForward( 0, tr.Direction );
		particle.SetPosition( 1, new Vector3( tr.Distance, 0, 0 ) );

		/*// Do explosion particle at end position
		var explosionParticle = Particles.Create( "particles/weapons/boomblast_hit.vpcf", tr.EndPosition );

		//This sucks we'll figure it out later
		explosionParticle.Destroy( false );*/

		// Push players away
		if ( Game.IsServer )
		{
			foreach ( var player in playersHit )
			{
				if ( player != Owner )
				{
					var playerdirection = (player.Position - muzzleTransform.Position).Normal;
					var muzzledirection = Owner.LookInput.ToRotation().Forward;

					var isInfront = Vector3.Dot( muzzledirection, playerdirection ) > 0.2f;

					Log.Info( isInfront );

					if ( isInfront )
					{
						var impulse = (Owner.LookInput.ToRotation().Forward + Vector3.Up * 5f) * HitForce;
						if ( impulse.z < 0f )
							impulse = impulse.WithZ( 0f );
						else
							impulse = impulse.WithZ( HitForce / 2 );
						player.GroundEntity = null;
						player.ApplyAbsoluteImpulse( impulse );

						DebugOverlay.Sphere( player.Position, 10f, Color.Red );
					}
				}
			}

			//Do "rocketjump"
			if ( tr.Hit && tr.Entity is not Player )
			{
				var impulse = -Owner.LookInput.ToRotation().Forward * HitForce;

				Owner.ApplyAbsoluteImpulse( impulse );
			}
		}
	}

	public void RocketlauncherBehavior()
	{
		// Fire a trace
		var muzzleTransform = GetAttachment( "muzzle2" ).Value;
		var tr = Trace.Ray( muzzleTransform.Position, muzzleTransform.Position + Owner.LookInput.ToRotation().Forward * 2048f )
				.Ignore( Owner )
				.Run();

		// Do instantaneous particle
		var particle = Particles.Create( "particles/weapons/boomblast_base.vpcf", muzzleTransform.Position );
		particle.SetForward( 0, tr.Direction );
		particle.SetPosition( 1, new Vector3( tr.Distance, 0, 0 ) );

		if ( tr.Hit )
		{
			// Create explosion at end position
			var playersHit = FindInSphere( tr.EndPosition, ExplosionRadius );

			// Do explosion particle at end position
			var explosionParticle = Particles.Create( "particles/weapons/boomblast_hit.vpcf", tr.EndPosition );

			//This sucks we'll figure it out later
			explosionParticle.Destroy( false );

			// Push players away
			if ( Game.IsServer )
			{
				foreach ( var player in playersHit )
				{
					var impulse = (player.Position - tr.EndPosition).Normal * HitForce;
					if ( impulse.z < 0f )
						impulse = impulse.WithZ( 0f );
					else
						impulse = impulse.WithZ( HitForce / 2 );
					player.ApplyAbsoluteImpulse( impulse );
				}
			}
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
}
