namespace SpeedPainters.Entities;

/// <summary>
/// Contains functionality for a group of members working together during the <see cref="PlayState"/>.
/// </summary>
[Category( "Setup" )]
public sealed partial class Team : Entity
{
	/// <summary>
	/// A list containing all of the teams currently participating.
	/// </summary>
	public static new IReadOnlyList<Team> All => PlayState.Instance?.Teams;

	/// <summary>
	/// The group that this team represents.
	/// </summary>
	[Net] public GroupResource Group { get; private set; }

	/// <summary>
	/// A list of all clients that are a part of this team.
	/// </summary>
	[Net] internal IList<IClient> members { get; private set; }
	/// <summary>
	/// A readonly list of all clients that are a part of this team.
	/// </summary>
	public IReadOnlyList<IClient> Members => members as IReadOnlyList<IClient>;

	/// <summary>
	/// Initializes a new instance of <see cref="Team"/>. This should only be used by s&box on the client-side.
	/// </summary>
	public Team()
	{
		Game.AssertClient();
	}

	/// <summary>
	/// Initializes a new instance of <see cref="Team"/>. This can only be used on the server-side.
	/// </summary>
	/// <param name="group">The group archetype the team represents.</param>
	/// <param name="members">A list of all clients that are a part of the team.</param>
	internal Team( GroupResource group, IEnumerable<IClient> members )
	{
		Game.AssertServer();

		Group = group;

		foreach ( var member in members )
			this.members.Add( member );
	}

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}
}
