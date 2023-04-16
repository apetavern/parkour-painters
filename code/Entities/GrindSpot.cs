namespace ParkourPainters.Entities;

public struct GrindPoint
{
	public Vector3 Position;
	public float Alpha;
	public int FirstNodeIndex;
	public int LastNodeIndex;
}

/// <summary>
/// A spot that a palyer can grind on.
/// </summary>
[Library( "grind_spot" )]
[Title( "Grind Spot" ), Category( "Movement" )]
[HammerEntity, Path( "path_generic_node" )]
public partial class GrindSpot : GenericPathEntity
{
	public List<GrindPoint> GrindPoints { get; private set; } = new();

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		CreateGrindPoints();
	}

	public override void ClientSpawn()
	{
		base.Spawn();

		CreateGrindPoints();
	}

	private void CreateGrindPoints()
	{
		if ( PathNodes.Count < 2 )
			return;

		GrindPoints.Clear();

		for ( int i = 0; i < PathNodes.Count - 1; ++i )
		{
			var firstNode = PathNodes[i];
			var lastNode = PathNodes[i + 1];

			var curveLength = GetCurveLength( firstNode, lastNode, 10 );
			var numGrindPoints = (int)(curveLength / 60);
			var increment = 1f / numGrindPoints;

			for ( float t = 0; t <= 1f; t += increment )
			{
				GrindPoints.Add
				(
					new GrindPoint() { Position = GetPointBetweenNodes( firstNode, lastNode, t ), Alpha = t, FirstNodeIndex = i, LastNodeIndex = i + 1 }
				);
			}
		}
	}

	public void DrawGrindSpot()
	{
		foreach ( var point in GrindPoints )
			DebugOverlay.Sphere( point.Position, 1f, Color.Blue );
	}
}
