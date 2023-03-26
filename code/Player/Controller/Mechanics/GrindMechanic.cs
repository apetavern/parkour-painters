namespace GangJam;

public class GrindMechanic : ControllerMechanic
{
	private GenericPathEntity Path;
	private int Node;
	private float Alpha;
	private TimeSince TimeSinceJump;
	private bool Reverse;

	protected override bool ShouldStart()
	{
		if ( TimeSinceJump < .3f )
			return false;

		foreach ( var path in Sandbox.Entity.All.OfType<GrindSpot>() )
		{
			if ( path.PathNodes.Count < 2 ) continue;

			var pa = path.NearestPoint( Controller.Position + Controller.Velocity * Time.Delta, false, out int na, out float ta );
			var pb = path.NearestPoint( Controller.Position + Controller.Velocity * Time.Delta, true, out int nb, out float tb );

			var dista = pa.Distance( Controller.Position );
			var distb = pb.Distance( Controller.Position );

			if ( dista < 30 && (na == 0 || dista < distb) )
			{
				Path = path;
				Node = na;
				Alpha = ta;
				Reverse = false;
				return true;
			}

			if ( distb < 30 && (nb == path.PathNodes.Count - 1 || distb < dista) )
			{
				Path = path;
				Node = nb;
				Alpha = tb;
				Reverse = true;
				return true;
			}
		}

		return false;
	}

	protected override void Simulate()
	{
		Alpha += Time.Delta;

		if ( Alpha >= 1 )
		{
			Alpha = 0;

			bool reachedEnd;

			if ( Reverse )
			{
				Node--;
				reachedEnd = Node <= 0;
			}
			else
			{
				Node++;
				reachedEnd = Node >= Path.PathNodes.Count - 1;
			}

			if ( reachedEnd )
			{
				IsActive = false;
				Path = null;

				TimeSinceJump = 0;
				IsActive = false;

				// todo: add velocity up from rail normal,
				// and fix getting grounded immediately so we don't have to set position
				Controller.Velocity = Controller.Velocity.WithZ( 320f );
				Controller.Position = Controller.Position.WithZ( Controller.Position.z + 10 );

				return;
			}
		}

		var currentNodeIdx = Node;
		var nextNodeIdx = Reverse ? (Node - 1) : (Node + 1);

		var node = Path.PathNodes[currentNodeIdx];
		var nextNode = Path.PathNodes[nextNodeIdx];
		var currentPosition = Controller.Position;
		var nextPosition = Path.GetPointBetweenNodes( node, nextNode, Alpha, Reverse );

		Controller.Velocity = (nextPosition - currentPosition).Normal * 300f;
		Controller.Position = nextPosition;

		var rot = Rotation.LookAt( Controller.Velocity.Normal ).Angles();
		rot.roll = 0;

		Player.Rotation = Rotation.From( rot );
		Controller.GroundEntity = Path;

		Controller.GroundEntity = Path;

		if ( Input.Pressed( InputButton.Jump ) )
		{
			TimeSinceJump = 0;
			IsActive = false;

			// todo: add velocity up from rail normal,
			// and fix getting grounded immediately so we don't have to set position
			Controller.Velocity = Controller.Velocity.WithZ( 320f );
			Controller.Position = Controller.Position.WithZ( Controller.Position.z + 10 );
		}
	}
}
