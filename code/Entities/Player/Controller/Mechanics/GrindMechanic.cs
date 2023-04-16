namespace ParkourPainters.Entities;

public partial class GrindMechanic : ControllerMechanic
{
	protected override bool ShouldStart()
	{
		foreach ( var path in Sandbox.Entity.All.OfType<GrindSpot>() )
		{
			if ( path.PathNodes.Count < 2 )
				continue;

			path.DrawGrindSpot();

			return false;
		}

		return false;
	}
}
