namespace GangJam.Entities;

public sealed class UnstuckMechanic : ControllerMechanic
{
	protected override bool ShouldStart() => true;
	private int _stuckTries = 0;

	protected override void Simulate()
	{
		var result = Controller.TraceBBox( Controller.Position, Controller.Position );

		// Not stuck, we cool
		if ( !result.StartedSolid )
		{
			_stuckTries = 0;
			return;
		}

		if ( Game.IsClient )
			return;

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
