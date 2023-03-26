namespace GangJam;

[Library( "grind_spot" )]
[Title( "Grind Spot" ), Category( "Movement" )]
[HammerEntity, Path( "path_generic_node" )]
internal partial class GrindSpot : GenericPathEntity
{
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}
}
