namespace ParkourPainters.Entities;

public struct GrindPoint
{
	public Vector3 Position;
	public Vector3 TangentIn;
	public Vector3 TangentOut;
}

/// <summary>
/// A spot that a palyer can grind on.
/// </summary>
[Library( "grind_spot" )]
[Title( "Grind Spot" ), Category( "Movement" )]
[HammerEntity, Path( "path_generic_node" )]
public partial class GrindSpot : GenericPathEntity
{
	private readonly List<GrindPoint> _grindPoints = new();

	public override void Spawn()
	{
		base.Spawn();

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

		_grindPoints.Clear();

		var firstNode = PathNodes[0];
		var lastNode = PathNodes[1];

		_grindPoints.Add( new GrindPoint() { Position = firstNode.WorldPosition, TangentIn = firstNode.WorldTangentIn, TangentOut = firstNode.WorldTangentOut } );
		_grindPoints.Add( new GrindPoint() { Position = lastNode.WorldPosition, TangentIn = lastNode.WorldTangentIn, TangentOut = lastNode.WorldTangentOut } );
	}

	public void DrawGrindSpot()
	{
		for ( var nodeid = 0; nodeid < _grindPoints.Count; nodeid++ )
		{
			var node = _grindPoints[nodeid];

			Vector3 nodePos = node.Position;

			// Nodes & IDs
			DebugOverlay.Sphere( nodePos, 4, Color.White );
			DebugOverlay.Text( $"{nodeid + 1}", nodePos + Vector3.Up * 6, Color.White, 0, 2500 );

			Vector3 nodeTanIn = node.TangentIn;
			DebugOverlay.Sphere( nodeTanIn, 2, Color.Yellow );
			DebugOverlay.Line( nodePos, nodeTanIn, Color.Yellow );

			Vector3 nodeTanOut = node.TangentOut;
			DebugOverlay.Sphere( nodeTanOut, 6, Color.Orange );
			DebugOverlay.Line( nodePos, nodeTanOut, Color.Orange );

			if ( nodeid + 1 >= _grindPoints.Count )
				continue;

			var nodeNext = _grindPoints[nodeid + 1];

			for ( int i = 1; i <= 30; i++ ) // Starting from i = 1 because i = 0 is start.Position
			{
				var lerpPos = GetPointBetweenNodes( node, nodeNext, (float)i / 30 );

				DebugOverlay.Line( nodePos, lerpPos, Color.Green );

				nodePos = lerpPos;
			}
		}
	}

	private Vector3 GetPointBetweenNodes( GrindPoint start, GrindPoint end, float t, bool reverse = false )
	{
		Vector3 tanSrc = reverse ? start.TangentIn : start.TangentOut;
		Vector3 tanTgt = reverse ? end.TangentOut : end.TangentIn;

		return Vector3.CubicBeizer( start.Position, end.Position, tanSrc, tanTgt, t );
	}

	[ConCmd.Admin( "regen" )]
	private static void Regen()
	{
		foreach ( var path in Sandbox.Entity.All.OfType<GrindSpot>() )
			path.CreateGrindPoints();

		ClientRegen();
	}

	[ClientRpc]
	public static void ClientRegen()
	{
		foreach ( var path in Sandbox.Entity.All.OfType<GrindSpot>() )
			path.CreateGrindPoints();
	}
}
