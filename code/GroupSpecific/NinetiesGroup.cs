namespace ParkourPainters.GroupSpecific;

/// <summary>
/// Contains functionality specific to the Nineties' special mechanic.
/// </summary>
internal static class NinetiesGroup
{
	/// <summary>
	/// Marks all <see cref="GraffitiArea"/>s in the map.
	/// </summary>
	[GameEvent.Tick.Client]
	private static void GraffitiView()
	{
		// if ( Game.LocalClient.GetTeam() is not Team team )
		// 	return;

		// if ( team.Group.Name != "Nineties" )
		// 	return;

		// foreach ( var graffitiSpot in Entity.All.OfType<GraffitiArea>() )
		// 	DebugOverlay.Text( "GRAFFITI SPOT HERE", graffitiSpot.Position - Vector3.Up * 10, 0, 9999 );
	}
}
