namespace GangJam.State;

/// <summary>
/// Contains functionality for a group of members working together during the <see cref="PlayState"/>.
/// </summary>
[Category( "Setup" )]
internal partial class Team : Entity
{
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
	/// <param name="members">A list of all clients that are a part of the team.</param>
	public Team( IEnumerable<IClient> members )
	{
		Game.AssertServer();

		foreach ( var member in members )
			Members.Add( member );
	}

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}
}
