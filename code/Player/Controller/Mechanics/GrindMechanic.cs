namespace GangJam;

public class GrindMechanic : ControllerMechanic
{
	private GenericPathEntity _path;
	private int _currentNodeIndex;
	private float _alpha;
	private bool _isReverseGrind;
	private bool _isGrinding = false;

	protected override bool ShouldStart()
	{
		if ( _isGrinding )
			return true;

		if ( !Input.Pressed( InputButton.Use ) )
			return false;

		foreach ( var path in Sandbox.Entity.All.OfType<GrindSpot>() )
		{
			if ( path.PathNodes.Count < 2 )
				continue;

			var pa = path.NearestPoint( Controller.Position + Controller.Velocity * Time.Delta, false, out int na, out float ta );
			var pb = path.NearestPoint( Controller.Position + Controller.Velocity * Time.Delta, true, out int nb, out float tb );

			var dista = pa.Distance( Controller.Position );
			var distb = pb.Distance( Controller.Position );

			if ( dista < 30 && (na == 0 || dista < distb) )
			{
				_path = path;
				_currentNodeIndex = na;
				_alpha = ta;
				_isReverseGrind = false;
				return true;
			}

			if ( distb < 30 && (nb == path.PathNodes.Count - 1 || distb < dista) )
			{
				_path = path;
				_currentNodeIndex = nb;
				_alpha = tb;
				_isReverseGrind = true;
				return true;
			}
		}

		return false;
	}

	protected override void Simulate()
	{
		if ( _currentNodeIndex >= 0 && _currentNodeIndex < _path.PathNodes.Count - 1 )
		{
			_isGrinding = true;
			var moveHelper = new MoveHelper( _path.PathNodes[_currentNodeIndex].WorldPosition, Controller.Velocity );
			moveHelper.TryMove( Time.Delta * 0.1f );

			Controller.Position = moveHelper.Position;
			Controller.Velocity = moveHelper.Velocity;

			_currentNodeIndex += 1;
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
