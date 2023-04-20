namespace ParkourPainters.Entities;

public partial class GrindMechanic : ControllerMechanic
{
	private GrindSpot _grindSpot;
	private int _currentIndex;
	private bool _isGrinding;
	private bool _isReverse;
	private float _alpha;
	private TimeSince _timeSinceExit;

	protected override bool ShouldStart()
	{
		if ( ParkourPainters.DebugMode )
		{
			foreach ( var grindSpot in Sandbox.Entity.All.OfType<GrindSpot>() )
				grindSpot.DrawGrindSpot();
		}

		if ( _isGrinding )
			return true;

		if ( _timeSinceExit < 0.5f )
			return false;

		if ( Controller.GroundEntity != null )
			return false;

		if ( Controller.Velocity.z > 0 )
			return false;

		foreach ( var grindSpot in Sandbox.Entity.All.OfType<GrindSpot>() )
		{
			if ( grindSpot.PathNodes.Count < 2 || grindSpot.GrindPoints.Count < 2 )
				continue;

			var closestGrindPoint = grindSpot.GrindPoints[0];
			var closestGrindPointIndex = 0;

			for ( int i = 0; i < grindSpot.GrindPoints.Count; ++i )
			{
				var grindPoint = grindSpot.GrindPoints[i];
				if ( Controller.Position.Distance( grindPoint.Position ) < Controller.Position.Distance( closestGrindPoint.Position ) )
				{
					closestGrindPoint = grindPoint;
					closestGrindPointIndex = i;
				}
			}

			if ( Controller.Position.Distance( closestGrindPoint.Position ) > 45 )
				continue;

			GrindPoint secondClosestGrindPoint = closestGrindPointIndex - 1 >= 0 ? grindSpot.GrindPoints[closestGrindPointIndex - 1] : grindSpot.GrindPoints[closestGrindPointIndex];
			var dot = Vector3.Dot( Player.Rotation.Forward, secondClosestGrindPoint.Position - closestGrindPoint.Position );

			_isReverse = dot > 0;
			_currentIndex = _isReverse ? closestGrindPoint.LastNodeIndex : closestGrindPoint.FirstNodeIndex;
			_grindSpot = grindSpot;
			_alpha = _isReverse ? 1f - closestGrindPoint.Alpha : closestGrindPoint.Alpha;

			return true;
		}

		return false;
	}

	protected override void Simulate()
	{
		if ( _currentIndex < 0 || _grindSpot is null || _grindSpot.PathNodes is null || _currentIndex >= _grindSpot.PathNodes.Count )
		{
			Stop();
			return;
		}

		_isGrinding = true;

		var increment = _isReverse ? -1 : +1;
		var currentNode = _grindSpot.PathNodes[_currentIndex];
		var nextNodeIndex = _currentIndex + increment;
		var nextNode = _grindSpot.PathNodes.ElementAtOrDefault( nextNodeIndex ) ?? currentNode;
		var distanceBetweenNodes = currentNode.WorldPosition.Distance( nextNode.WorldPosition );

		var nextPosition = _grindSpot.GetPointBetweenNodes( currentNode, nextNode, _alpha, _isReverse );
		var speed = 300f;

		_alpha += Time.Delta * (speed / distanceBetweenNodes);
		if ( _alpha >= 0.97f )
		{
			_alpha = 0;
			_currentIndex += increment;
		}

		Controller.Velocity = (nextPosition - Controller.Position).Normal * speed;
		Controller.Position = Vector3.Lerp( Controller.Position, nextPosition, Time.Delta );
		Player.Rotation = Controller.Velocity.Normal.EulerAngles.WithPitch( 0 ).ToRotation();

		if ( Input.Pressed( InputAction.Jump ) )
		{
			Player.JumpMechanic.Start();
			Stop();
		}
	}

	protected override void OnStop()
	{
		IsActive = false;
		_isGrinding = false;
		_timeSinceExit = 0;

		Player.PlaySound( "grind_exit" );
	}
}
