namespace SpeedPainters.Entities;

[Library( "end_trigger" )]
[Title( "End Trigger" ), Category( "Race" )]
[HammerEntity]
public partial class EndTrigger : BaseTrigger
{
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	[GameEvent.Tick]
	void OnTick()
	{
		DebugOverlay.Box( this, Color.Yellow );
	}

	public override void OnTouchStart( Entity toucher )
	{
		base.OnTouchStart( toucher );
		if ( toucher is Player ply )
		{
			var finishTime = TimeSpan.FromSeconds( ply.StopWatch );
			ply.ReachedEnd = true;
		}
	}
}

