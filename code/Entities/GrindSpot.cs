namespace ParkourPainters.Entities;

/// <summary>
/// A spot that a palyer can grind on.
/// </summary>
[Library( "grind_spot" )]
[Title( "Grind Spot" ), Category( "Movement" )]
[HammerEntity, Path( "path_generic_node" )]
internal sealed class GrindSpot : GenericPathEntity
{
	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}
}
