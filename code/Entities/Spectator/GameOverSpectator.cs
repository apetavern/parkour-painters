namespace ParkourPainters.Entities;

/// <summary>
/// A pawn that looks at all of the <see cref="Spray"/>s at the end of the game.
/// </summary>
internal sealed partial class GameOverSpectator : Entity
{
	/// <summary>
	/// The amount of time in seconds we wait per spot.
	/// </summary>
	public static float TimePerSpot => Math.Min( 0.9f, 10f / GameOverState.Instance.OwnedSpots.Count );

	/// <summary>
	/// Whether or not the spectator has finished looking at all the <see cref="Spray"/>s.
	/// </summary>
	internal bool Finished => SpotIndex >= GameOverState.Instance.OwnedSpots.Count;

	/// <summary>
	/// The current <see cref="GraffitiArea"/> the spectator is looking at.
	/// </summary>
	internal GraffitiArea CurrentSpot => GameOverState.Instance.OwnedSpots[SpotIndex];

	/// <summary>
	/// The current index that the spectator is at in the <see cref="GameOverState.OwnedSpots"/> list.
	/// </summary>
	private int SpotIndex { get; set; }

	/// <summary>
	/// The time in seconds since we marked the last spot.
	/// </summary>
	private TimeSince TimeSinceLastSpot { get; set; } = 0;

	/// <summary>
	/// UI panel showing the results of the game.
	/// </summary>
	private GameResults _gameResultsPanel;

	/// <summary>
	/// The camera position we calculate and then lerp towards.
	/// </summary>
	private Vector3 CameraPosition { get; set; }

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

	[GameEvent.Tick.Client]
	private void ClientTick()
	{
		if ( GameOverState.Instance is null || Finished )
			return;

		if ( CameraPosition != default )
		{
			Camera.Position = Vector3.Lerp( Camera.Position, CameraPosition, Time.Delta );
			Camera.Rotation = Rotation.Lerp( Camera.Rotation, Rotation.LookAt( Rotation.Down ), Time.Delta );

			if ( TimeSinceLastSpot >= TimePerSpot && !Finished )
			{
				TimeSinceLastSpot = 0;
				_gameResultsPanel.AddScore( CurrentSpot.AreaOwner, (int)CurrentSpot.PointsType + 1 );
				Game.RootPanel.AddChild( new ScoreMarker( CurrentSpot ) );
				SpotIndex++;
			}

			if ( Finished )
				_gameResultsPanel.ShowWinner();

			return;
		}

		var minPoint = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
		var maxPoint = new Vector3( float.MinValue, float.MinValue, float.MinValue );

		var graffitiAreas = All.OfType<GraffitiArea>();

		foreach ( var area in graffitiAreas )
		{
			minPoint = Vector3.Min( area.Position, minPoint );
			maxPoint = Vector3.Max( area.Position, maxPoint );
		}

		var center = (minPoint + maxPoint) / 2;

		var bounds = new BBox( center, Vector3.Zero );
		foreach ( var area in graffitiAreas )
		{
			bounds = bounds.AddPoint( area.Position );
		}

		var cameraDistance = bounds.Size.Length / MathF.Tan( Camera.FieldOfView * 0.5f * (MathF.PI / 180) );
		CameraPosition = center.WithZ( cameraDistance );
	}
}
