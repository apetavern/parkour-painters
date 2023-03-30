namespace GangJam.Entities;

public sealed partial class UnstuckMechanic : ControllerMechanic
{
	[Net, Predicted] private int _stuckTries { get; set; }

	protected override bool ShouldStart()
	{
		if ( Player.LedgeGrabMechanic.IsActive )
			return false;

		return true;
	}

	protected override void Simulate()
	{
		var result = Controller.TraceBBox( Controller.Position, Controller.Position );

		// Not stuck, we cool
		if ( !result.StartedSolid )
		{
			_stuckTries = 0;
			return;
		}

		int AttemptsPerTick = 20;

		for ( int i = 0; i < AttemptsPerTick; i++ )
		{
			var pos = Controller.Position + Vector3.Random.Normal * (_stuckTries / 2.0f);

			// First try the up direction for moving platforms
			if ( i == 0 )
				pos = Controller.Position + Vector3.Up * 5;

			result = Controller.TraceBBox( pos, pos );

			if ( !result.StartedSolid )
			{

				Controller.Position = pos;
				return;
			}
		}

		_stuckTries++;
	}
}
