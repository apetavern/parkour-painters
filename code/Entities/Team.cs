namespace GangJam.State;

/// <summary>
/// Contains functionality for a group of members working together during the <see cref="PlayState"/>.
/// </summary>
[Category( "Setup" )]
public sealed partial class Team : Entity
{
	/// <summary>
	/// A list containing all of the teams currently participating.
	/// </summary>
	public static new IReadOnlyList<Team> All => PlayState.Instance?.Teams as IReadOnlyList<Team>;

	/// <summary>
	/// The group that this team represents.
	/// </summary>
	[Net] public GroupResource Group { get; set; }

	/// <summary>
	/// A list of all clients that are a part of this team.
	/// </summary>
	[Net] internal IList<IClient> Members { get; private set; }

	/// <summary>
	/// The current score that the team has.
	/// </summary>
	public int Score => Entity.All.OfType<GraffitiSpot>()
		.Where( spot => spot.SprayOwner == this && spot.IsSprayCompleted )
		.Count();

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
	/// <param name="group">The group archetype the team represents.</param>
	/// <param name="members">A list of all clients that are a part of the team.</param>
	internal Team( GroupResource group, IEnumerable<IClient> members )
	{
		Game.AssertServer();

		Group = group;

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
