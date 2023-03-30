namespace ParkoutPainters.GroupSpecific;

/// <summary>
/// Contains functionality specific to the Nerds special mechanic.
/// </summary>
internal static partial class NerdGroup
{
	/// <summary>
	/// Notifies all nerds when one of their <see cref="GraffitiSpot"/>s are being tampered with by another team.
	/// </summary>
	[ParkoutPainters.Events.GraffitiSpotTampered]
	private static void GraffitiSpotTampered( Team oldTeam, Team newTeam, Entities.Player tamperer )
	{
		if ( oldTeam.Group.Name != "Nerds" )
			return;

		NotifyGraffitiSpotTampered( To.Multiple( oldTeam.Members ), tamperer );
	}

	/// <summary>
	/// A client-side receiver for getting who is tampering with the local clients <see cref="GraffitiSpot"/>s.
	/// </summary>
	[ClientRpc]
	internal static void NotifyGraffitiSpotTampered( Entity tamperer )
	{
		DebugOverlay.Skeleton( tamperer, Color.Red, 5 );
		Log.Info( $"{tamperer.Client} IS MESSING WITH YOUR GRAFFITI" );
	}
}
