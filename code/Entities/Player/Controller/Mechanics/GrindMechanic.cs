namespace ParkoutPainters.Entities;

public partial class GrindMechanic : ControllerMechanic
{
	[Net, Predicted] private GenericPathEntity _path { get; set; }
	[Net, Predicted] private int _currentNodeIndex { get; set; }
	[Net, Predicted] private bool _isGrinding { get; set; }
	[Net, Predicted] private float _alpha { get; set; }
	[Net, Predicted] private bool _isReverse { get; set; }
	private TimeSince _timeSinceExit { get; set; }

	/// <summary>
	/// The spark particles from the grinding mechanic
	/// </summary>
	private Particles SparkParticles { get; set; }

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
		HandleSparkParticles();

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
			Player.Rotation = Controller.Velocity.Normal.EulerAngles.WithPitch( 0 ).ToRotation();

			if ( Input.Pressed( InputButton.Jump ) )
			{
				Player.JumpMechanic.Start();
				Stop();
			}

			return;
		}

		Stop();
	}

	private void HandleSparkParticles()
	{
		if ( IsActive )
		{
			if ( SparkParticles is null )
			{
				SparkParticles = Particles.Create( "particles/sparks/sparks_base.vpcf", Player );
				SparkParticles.SetEntityBone( 0, Player, Player.GetBoneIndex( "ankle_L" ) );
			}
		}
	}

	protected override void OnStop()
	{
		IsActive = false;
		_isGrinding = false;
		_timeSinceExit = 0;

		SparkParticles?.Destroy();
		SparkParticles = null;
	}
}
