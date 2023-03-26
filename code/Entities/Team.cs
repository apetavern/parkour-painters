namespace GangJam.State;

/// <summary>
/// Contains functionality for a group of members working together during the <see cref="PlayState"/>.
/// </summary>
[Category( "Setup" )]
internal sealed partial class Team : Entity
{
	/// <summary>
	/// The group archetype the team represents.
	/// </summary>
	[Net] internal TeamType Type { get; private set; }

	/// <summary>
	/// A list of all clients that are a part of this team.
	/// </summary>
	[Net] internal IList<IClient> Members { get; private set; }
	/// <summary>
	/// The current score that the team has.
	/// </summary>
	[Net] internal int Score { get; private set; }

	/// <summary>
	/// Initializes a new instance of <see cref="Team"/>. This should only be used by S&box on the client-side.
	/// </summary>
	public Team()
	{
		Game.AssertClient();
	}

	/// <summary>
	/// Initializes a new instance of <see cref="Team"/>. This can only be used on the server-side.
	/// </summary>
	/// <param name="type">The group archetype the team represents.</param>
	/// <param name="members">A list of all clients that are a part of the team.</param>
	public Team( TeamType type, IEnumerable<IClient> members )
	{
		Game.AssertServer();

		Type = type;

		foreach ( var member in members )
		{
			Members.Add( member );

			if ( member.Pawn is not Player player )
				break;

			player.Team = type;
			player.Respawn();
		}
	}

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}
}
