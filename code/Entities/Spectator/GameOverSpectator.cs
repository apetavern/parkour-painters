namespace ParkourPainters.Entities;

/// <summary>
/// A pawn that looks at all of the <see cref="Spray"/>s at the end of the game.
/// </summary>
internal sealed partial class GameOverSpectator : Entity
{
	private GameResults _gameResultsPanel;

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

	[Event.Tick.Client]
	private void LerpToSpot()
	{
		var minPoint = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
		var maxPoint = new Vector3( float.MinValue, float.MinValue, float.MinValue );

		foreach ( var area in All.OfType<GraffitiArea>() )
		{
			minPoint = Vector3.Min( area.Position, minPoint );
			maxPoint = Vector3.Max( area.Position, maxPoint );
		}

		var center = (minPoint + maxPoint) / 2;

		var bounds = new BBox( center, Vector3.Zero );
		foreach ( var area in All.OfType<GraffitiArea>() )
		{
			bounds = bounds.AddPoint( area.Position );
		}

		var cameraDistance = bounds.Size.Length / MathF.Tan( Camera.FieldOfView * 0.5f * (MathF.PI / 180) );

		Camera.Position = center.WithZ( cameraDistance );
		Camera.Rotation = Rotation.LookAt( Vector3.Down );
	}
}
