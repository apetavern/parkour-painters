namespace GangJam;

[Library( "grind_spot" )]
[Title( "Grind Spot" ), Category( "Movement" )]
[HammerEntity, Path( "path_generic_node" )]
internal class GrindSpot : GenericPathEntity
{
	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}
}
