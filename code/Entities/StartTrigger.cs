namespace SpeedPainters.Entities;

[Library( "start_trigger" )]
[Title( "Start Trigger" ), Category( "Race" )]
[HammerEntity]
public partial class StartTrigger : BaseTrigger
{
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	[Event.Tick]
	void OnTick()
	{
		DebugOverlay.Box( this, Color.Green );
	}
}
