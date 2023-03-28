namespace GangJam;

[Library( "func_graffiti_spot" )]
[Title( "Graffiti Spot" ), Category( "Spray Down" )]
[Solid, DrawAngles]
public sealed partial class GraffitiSpot : ModelEntity
{
	[Net]
	public Player SprayOwner { get; set; }

	[Net]
	public float SprayProgress { get; set; }

	public bool IsSprayCompleted => SprayProgress >= 100;

	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromModel( PhysicsMotionType.Static );

		Tags.Add( "graffiti_spot" );
	}


	[Event.Tick.Server]
	public void OnTick()
	{
		DebugOverlay.Text( $"{SprayProgress}/100", Position );
		DebugOverlay.Text( $"{SprayOwner?.Client.Name}", Position + Vector3.Up * 10 );
	}

	public void OnSprayReceived( Player player )
	{
		// Reset spray progress if the spray owner is the new sprayer.
		if ( player != SprayOwner )
		{
			SprayProgress = 0;
			SprayOwner = player;
		}

		SprayProgress += 1;
		SprayProgress = SprayProgress.Clamp( 0, 100 );

		if ( IsSprayCompleted )
			OnSprayCompleted( player );
	}

	public void OnSprayCompleted( Player sprayer )
	{
		Log.Info( "Spray completed" );
	}
}
