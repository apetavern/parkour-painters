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
	[Net, Predicted] private int SpotIndex { get; set; }
	/// <summary>
	/// The time in seconds since the current travel started.
	/// </summary>
	[Net, Predicted] private TimeSince TimeSinceTravelStarted { get; set; } = 0;

	private ScoreWorldPanel ScoreWorldPanel { get; set; }

	/// <summary>
	/// The time in seconds it takes to travel between <see cref="Spray"/>s.
	/// </summary>
	internal const float TravelTimeToSpot = 2;
	/// <summary>
	/// The time in seconds that the spectator will stare at the <see cref="Spray"/>.
	/// </summary>
	internal const float StareTime = 2;

	public GameOverSpectator()
	{
		if ( !Game.IsClient )
			return;

		ScoreWorldPanel = new ScoreWorldPanel();
	}

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		Tags.Add( "player" );
	}

	/// <inheritdoc/>
	public sealed override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		LerpToSpot();
	}

	/// <inheritdoc/>
	public sealed override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		LerpToSpot();
	}

	/// <inheritdoc/>
	protected sealed override void OnDestroy()
	{
		base.OnDestroy();

		if ( Game.IsClient )
			ScoreWorldPanel.Delete();
	}

	/// <summary>
	/// Lerps to the next spot to look at.
	/// </summary>
	private void LerpToSpot()
	{
		if ( Finished )
			return;

		if ( Game.IsClient )
			ScoreWorldPanel.Area = CurrentSpot;

		var startPos = LastSpot is null
			? Position
			: LastSpot.Position + LastSpot.Sprays[0].Rotation.Up * 200;
		var targetPos = CurrentSpot.Position + CurrentSpot.Sprays[0].Rotation.Up * 200;

		var startRot = LastSpot is null
			? Rotation
			: Rotation.LookAt( LastSpot.Sprays[0].Position - Camera.Position );
		var targetRot = Rotation.LookAt( CurrentSpot.Sprays[0].Position - Camera.Position );

		var fraction = TimeSinceTravelStarted / TravelTimeToSpot;

		Camera.Position = startPos.LerpTo( targetPos, fraction );
		Camera.Rotation = Rotation.Lerp( startRot, targetRot, fraction );

		if ( TimeSinceTravelStarted < TravelTimeToSpot + StareTime )
			return;

		SpotIndex++;
		TimeSinceTravelStarted = 0;
	}
}
