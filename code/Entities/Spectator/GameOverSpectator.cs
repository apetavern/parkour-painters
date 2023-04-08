namespace ParkourPainters.Entities;

/// <summary>
/// A pawn that looks at all of the <see cref="Spray"/>s at the end of the game.
/// </summary>
internal sealed partial class GameOverSpectator : Entity
{
	/// <summary>
	/// Whether or not the spectator has finished looking at all the <see cref="Spray"/>s.
	/// </summary>
	internal bool Finished => SpotIndex >= GameOverState.Instance.Spots.Count;
	/// <summary>
	/// Whether or not the spectator is looking at a <see cref="Spray"/>.
	/// </summary>
	internal bool Staring => TimeSinceTravelStarted > TravelTimeToSpot && TimeSinceTravelStarted <= TravelTimeToSpot + StareTime;

	/// <summary>
	/// The current <see cref="GraffitiArea"/> the spectator is looking at.
	/// </summary>
	internal GraffitiArea CurrentSpot => GameOverState.Instance.Spots[SpotIndex];
	/// <summary>
	/// The last <see cref="GraffitiArea"/> the spectator was looking at. Null if <see ref="LastSpot"/> is 0.
	/// </summary>
	internal GraffitiArea LastSpot => SpotIndex == 0 ? null : GameOverState.Instance.Spots[SpotIndex - 1];

	/// <summary>
	/// The current index that the spectator is at in the <see cref="GameOverState.Spots"/> list.
	/// </summary>
	private int SpotIndex { get; set; }
	/// <summary>
	/// The time in seconds since the current travel started.
	/// </summary>
	private TimeSince TimeSinceTravelStarted { get; set; } = 0;

	private GameResults _gameResultsPanel;

	/// <summary>
	/// The time in seconds it takes to travel between <see cref="Spray"/>s.
	/// </summary>
	internal static float TravelTimeToSpot => (3f / GameOverState.Instance.Spots.Count);
	/// <summary>
	/// The time in seconds that the spectator will stare at the <see cref="Spray"/>.
	/// </summary>
	internal static float StareTime => (2f / GameOverState.Instance.Spots.Count);
	/// <summary>
	/// Whether or not the score has been created for the game over spot.
	/// </summary>
	internal bool _createdScore = false;

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		Tags.Add( "player" );
	}

	/// <inheritdoc/>
	public sealed override void ClientSpawn()
	{
		_gameResultsPanel = Game.RootPanel.AddChild<GameResults>();
	}

	/// <inheritdoc/>
	protected sealed override void OnDestroy()
	{
		base.OnDestroy();

		if ( Game.IsClient )
			_gameResultsPanel.Delete();
	}

	/// <summary>
	/// Lerps to the next spot to look at.
	/// </summary>
	[Event.Tick.Client]
	private void LerpToSpot()
	{
		if ( GameOverState.Instance is null || Finished )
			return;

		var lastSpray = LastSpot?.LastCompletedSpray;
		var currentSpray = CurrentSpot.LastCompletedSpray;

		var startPos = LastSpot is null
			? Position
			: LastSpot.Position + lastSpray?.Rotation.Up * 200 ?? Rotation.Up * 200;
		var targetPos = CurrentSpot.Position + currentSpray?.Rotation.Up * 200 ?? Rotation.Up * 200;

		var startRot = LastSpot is null
			? Rotation
			: Rotation.LookAt( lastSpray?.Position - Camera.Position ?? LastSpot.Position - Camera.Position );
		var targetRot = Rotation.LookAt( currentSpray?.Position - Camera.Position ?? CurrentSpot.Position - Camera.Position );

		Log.Info( TravelTimeToSpot );

		var fraction = TimeSinceTravelStarted / TravelTimeToSpot;

		Camera.Position = Vector3.Lerp( startPos, targetPos, fraction );
		Camera.Rotation = Rotation.Lerp( startRot, targetRot, fraction );

		if ( TimeSinceTravelStarted < TravelTimeToSpot + StareTime - 0.5f )
			return;

		if ( Game.IsClient && CurrentSpot is not null && CurrentSpot.AreaOwner is not null && !_createdScore )
		{
			_gameResultsPanel.AddScore( CurrentSpot.AreaOwner, (int)CurrentSpot.PointsType + 1 );
			_ = new ScoreWorldPanel( CurrentSpot.AreaOwner, currentSpray.Position + currentSpray.Rotation.Up * 10f + Camera.Rotation.Backward * 50f );
			_createdScore = true;
		}

		if ( TimeSinceTravelStarted < TravelTimeToSpot + StareTime )
			return;

		SpotIndex++;
		TimeSinceTravelStarted = 0;
		_createdScore = false;

		if ( Finished )
			_gameResultsPanel.ShowWinner();
	}
}
