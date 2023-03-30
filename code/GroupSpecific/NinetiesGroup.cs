namespace ParkoutPainters.GroupSpecific;

/// <summary>
/// Contains functionality specific to the Nineties' special mechanic.
/// </summary>
internal static class NinetiesGroup
{
	/// <summary>
	/// Marks all <see cref="GraffitiSpot"/>s in the map.
	/// </summary>
	[Event.Tick.Client]
	private static void GraffitiView()
	{
		if ( Game.LocalClient.GetTeam() is not Team team )
			return;

		if ( team.Group.Name != "Nineties" )
			return;

		foreach ( var graffitiSpot in Entity.All.OfType<GraffitiSpot>() )
			DebugOverlay.Text( "GRAFFITI SPOT HERE", graffitiSpot.Position - Vector3.Up * 10, 0, 9999 );
	}
}
