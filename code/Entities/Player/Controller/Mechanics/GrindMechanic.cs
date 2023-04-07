namespace ParkourPainters.Entities;

public partial class GrindMechanic : ControllerMechanic
{
	[Net, Predicted] private GenericPathEntity _path { get; set; }
	[Net, Predicted] private int _currentNodeIndex { get; set; }
	[Net, Predicted] private bool _isGrinding { get; set; }
	[Net, Predicted] private float _alpha { get; set; }
	[Net, Predicted] private bool _isReverse { get; set; }
	private TimeSince _timeSinceExit { get; set; }

	private float _incomingSpeed;

	protected override bool ShouldStart()
	{
		if ( _isGrinding )
			return true;

		if ( _timeSinceExit < 0.5f )
			return false;

		if ( Controller.GroundEntity != null )
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
		if ( _currentNodeIndex < 0 || _path is null || _path.PathNodes is null || _currentNodeIndex >= _path.PathNodes.Count )
		{
			Stop();
			return;
		}

		_isGrinding = true;

		var increment = _isReverse ? -1 : +1;
		var currentNode = _path.PathNodes[_currentNodeIndex];
		var nextNodeIndex = _currentNodeIndex + increment;
		var nextNode = _path.PathNodes.ElementAtOrDefault( nextNodeIndex ) ?? currentNode;
		var distanceBetweenNodes = currentNode.WorldPosition.Distance( nextNode.WorldPosition );

		var nextPosition = _path.GetPointBetweenNodes( currentNode, nextNode, _alpha, _isReverse );
		var speed = Math.Max( 300f, _incomingSpeed );

		_alpha += Time.Delta * (speed / distanceBetweenNodes);
		if ( _alpha >= 0.98f )
		{
			_alpha = 0;
			_currentNodeIndex += increment;
		}

		Controller.Velocity = (nextPosition - Controller.Position).Normal * speed;
		Controller.Position = Vector3.Lerp( Controller.Position, nextPosition, Time.Delta );
		Player.Rotation = Controller.Velocity.Normal.EulerAngles.WithPitch( 0 ).ToRotation();

		if ( Input.Pressed( InputButton.Jump ) )
		{
			Player.JumpMechanic.Start();
			Stop();
		}
	}

	protected override void OnStart()
	{
		_incomingSpeed = Player.Velocity.Length;
	}

	protected override void OnStop()
	{
		IsActive = false;
		_isGrinding = false;
		_timeSinceExit = 0;

		Player.PlaySound( "grind_exit" );
	}
}
