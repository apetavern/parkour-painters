namespace GangJam;

[Library( "ent_carriable_spawner" )]
[Title( "Carriable Spawner" ), Category( "Spray Down" )]
[EditorModel( "models/entities/spray_paint/spray_paint.vmdl" )]
[HammerEntity]
public sealed partial class CarriableSpawner : AnimatedEntity
{
	[Net]
	public TimeSince TimeSinceLastPickup { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/entities/spray_paint/spray_paint.vmdl" );
		Scale = 2f;
		EnableTouch = true;

		_ = new PickupTrigger() { Position = Position, Parent = this };
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is not Player player )
			return;

		if ( player.CanEquip( typeof( SprayCan ) ) )
			player.Equip( new SprayCan() );
		else
			Log.Info( "Already have one, greedy bugger" );

		TimeSinceLastPickup = 0f;
	}
}
