namespace ParkourPainters.UI;

public class VoiceChatIndicators : Panel
{
	public static VoiceChatIndicators Instance { get; private set; }
	private readonly List<VoiceChatIndicator> _indicators = new();

	public VoiceChatIndicators()
	{
		Instance = this;
	}

	public void IsSpeaking( IClient client )
	{
		if ( client.Pawn is not Entities.Player player )
			return;

		var indicator = _indicators.FirstOrDefault( i => i.SteamId == client.SteamId ) ?? new VoiceChatIndicator( player );
		indicator.IsSpeaking( client );
	}

	public override void Tick()
	{
		// Show the voice indicator if the local player is speaking.
		if ( Voice.IsRecording )
			IsSpeaking( Game.LocalClient );
	}
}
