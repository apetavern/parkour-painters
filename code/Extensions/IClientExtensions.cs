namespace GangJam.Extensions;

/// <summary>
/// Contains extension methods for the <see cref="IClient"/> interface.
/// </summary>
public static class IClientExtensions
{
	/// <summary>
	/// Returns the team that a client is a part of.
	/// </summary>
	/// <param name="client">The client whose team to fetch.</param>
	/// <returns>The team that a client is a part of.</returns>
	public static Team GetTeam( this IClient client )
	{
		return PlayState.Instance?.GetTeamFor( client );
	}
}
