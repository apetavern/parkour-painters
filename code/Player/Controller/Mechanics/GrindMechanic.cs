namespace GangJam;

public class GrindMechanic : ControllerMechanic
{
	private GenericPathEntity _path;
	private int _currentNodeIndex;
	private bool _isGrinding = false;
	private float _alpha;

	protected override bool ShouldStart()
	{
		if ( _isGrinding )
			return true;

		foreach ( var path in Sandbox.Entity.All.OfType<GrindSpot>() )
		{
			if ( path.PathNodes.Count < 2 )
				continue;

			var closestNode = path.PathNodes.MinBy( p => Controller.Position.Distance( p.WorldPosition ) );
			if ( Controller.Position.Distance( closestNode.WorldPosition ) > 30 )
				continue;

			_currentNodeIndex = path.PathNodes.IndexOf( closestNode );
			_path = path;

			return true;
		}

		return false;
	}

	protected override void Simulate()
	{
		_alpha += Time.Delta;
		if ( _currentNodeIndex >= 0 && _currentNodeIndex < _path.PathNodes.Count - 1 )
		{
			_isGrinding = true;

			if ( _alpha >= 1 )
			{
				_alpha = 0;
				_currentNodeIndex += 1;
				return;
			}

			var nextNodeIndex = _currentNodeIndex + 1;
			var nextPosition = _path.GetPointBetweenNodes( _path.PathNodes[_currentNodeIndex], _path.PathNodes[nextNodeIndex], _alpha );

			DebugOverlay.Sphere( nextPosition, 1f, Color.Red );

			Controller.Velocity = 0;
			Controller.Position = nextPosition;
			Controller.GroundEntity = _path;

			return;
		}

		ExitGrind();
	}

	private void ExitGrind()
	{
		IsActive = false;
		_isGrinding = false;
	}
}
