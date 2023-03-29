namespace GangJam.Entities;

public partial class GrindMechanic : ControllerMechanic
{
	private GenericPathEntity _path;
	private int _currentNodeIndex;
	private bool _isGrinding;
	private float _alpha;
	private bool _isReverse;
	private TimeSince _timeSinceExit;

	protected override bool ShouldStart()
	{
		if ( _isGrinding )
			return true;

		if ( _timeSinceExit < 0.5f )
			return false;

		foreach ( var path in Sandbox.Entity.All.OfType<GrindSpot>() )
		{
			if ( path.PathNodes.Count < 2 )
				continue;

			var closestNode = path.PathNodes.OrderBy( p => Controller.Position.Distance( p.WorldPosition ) ).First();
			if ( Controller.Position.Distance( closestNode.WorldPosition ) > 50 )
				continue;

			if ( Controller.Position.z < closestNode.WorldPosition.z )
				continue;

			var directionToFirstNode = path.PathNodes[0].WorldPosition - closestNode.WorldPosition;
			var dot = Vector3.Dot( Player.Rotation.Forward, directionToFirstNode );

			// Lerp the player to the starting node to make the grind seem smoother.
			Player.Position = Vector3.Lerp( Controller.Position, closestNode.WorldPosition, Time.Delta );

			_isReverse = dot > 0;
			_currentNodeIndex = path.PathNodes.IndexOf( closestNode );
			_path = path;
			return true;
		}

		return false;
	}

	protected override void Simulate()
	{
		if ( _currentNodeIndex >= 0 && _currentNodeIndex < _path.PathNodes.Count )
		{
			_isGrinding = true;

			var increment = _isReverse ? -1 : +1;
			var currentNode = _path.PathNodes[_currentNodeIndex];
			var nextNodeIndex = _currentNodeIndex + increment;
			var nextNode = _path.PathNodes.ElementAtOrDefault( nextNodeIndex ) ?? currentNode;
			var distanceBetweenNodes = currentNode.WorldPosition.Distance( nextNode.WorldPosition );

			var nextPosition = _path.GetPointBetweenNodes( currentNode, nextNode, _alpha, _isReverse );

			_alpha += Time.Delta * (300f / distanceBetweenNodes);
			if ( _alpha >= 0.98f )
			{
				_alpha = 0;
				_currentNodeIndex += increment;
			}

			Controller.Velocity = (nextPosition - Controller.Position).Normal * 300f;
			Controller.Position = Vector3.Lerp( Controller.Position, nextPosition, Time.Delta );

			if ( Input.Pressed( InputButton.Jump ) )
			{
				Player.JumpMechanic.Start();
				Stop();
			}

			return;
		}

		Stop();
	}

	protected override void OnStop()
	{
		IsActive = false;
		_isGrinding = false;
		_timeSinceExit = 0;
	}
}
