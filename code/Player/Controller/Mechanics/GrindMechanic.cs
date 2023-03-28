namespace GangJam;

public partial class GrindMechanic : ControllerMechanic
{

	private GenericPathEntity _path;
	private int _currentNodeIndex;
	private bool _isGrinding { get; set; }

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
		if ( _currentNodeIndex >= 0 && _currentNodeIndex < _path.PathNodes.Count - 1 )
		{
			_isGrinding = true;

			var nextNodeIndex = _currentNodeIndex + 1;
			var nextPosition = _path.GetPointBetweenNodes( _path.PathNodes[_currentNodeIndex], _path.PathNodes[nextNodeIndex], _alpha );

			_alpha += Time.Delta * 2f;
			if ( _alpha > 1f )
			{
				_alpha = 0;
				_currentNodeIndex += 1;
			}

			Controller.GroundEntity = _path;
			Controller.Velocity = (nextPosition - Controller.Position).Normal * 300f;
			Controller.Position = nextPosition;

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
