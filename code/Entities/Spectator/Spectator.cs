namespace ParkourPainters.Entities;

/// <summary>
/// A pawn that can spectate other players in the game.
/// </summary>
internal sealed class Spectator : Entity
{
	/// <summary>
	/// The instance of <see cref="SpectatorCamera"/> on this entity.
	/// </summary>
	[BindComponent] private SpectatorCamera Camera { get; }

	/// <summary>
	/// Normalized accumulation of Input.AnalogLook
	/// </summary>
	[ClientInput] internal Angles LookInput { get; private set; }

	/// <summary>
	/// The index into <see cref="Game.Clients"/> to spectate.
	/// </summary>
	[ClientInput] private int ClientIndex { get; set; }

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		EnableDrawing = false;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = false;
		EnableLagCompensation = false;

		Components.Create<SpectatorCamera>();
	}

	/// <inheritdoc/>
	public sealed override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		// Bail before we cause problems.
		if ( ClientIndex >= Game.Clients.Count )
			return;

		Position = Game.Clients.ElementAt( ClientIndex ).Pawn?.Position ?? Vector3.Zero;
	}

	/// <inheritdoc/>
	public sealed override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		// Bail before we cause problems.
		if ( ClientIndex >= Game.Clients.Count )
			return;

		Position = Game.Clients.ElementAt( ClientIndex ).Pawn?.Position ?? Vector3.Zero;
		Camera?.Update( this );
	}

	/// <inheritdoc/>
	public sealed override void BuildInput()
	{
		if ( Input.Pressed( InputButton.PrimaryAttack ) )
		{
			// Move up until we find a valid client.
			do
			{
				ClientIndex++;

				if ( ClientIndex >= Game.Clients.Count )
					ClientIndex = 0;
			} while ( Game.Clients.ElementAt( ClientIndex ).Pawn is not Player );
		}

		if ( Input.Pressed( InputButton.SecondaryAttack ) )
		{
			// Move down until we find a valid client.
			do
			{
				ClientIndex--;

				if ( ClientIndex < 0 )
					ClientIndex = Game.Clients.Count - 1;
			} while ( Game.Clients.ElementAt( ClientIndex ).Pawn is not Player );
		}

		// Do this in case someone disconnected and made the index invalid.
		if ( ClientIndex >= Game.Clients.Count )
		{
			ClientIndex = Game.Clients.Count;

			// Move down until we find a valid client.
			do
			{
				ClientIndex--;

				if ( ClientIndex < 0 )
					ClientIndex = Game.Clients.Count - 1;
			} while ( Game.Clients.ElementAt( ClientIndex ).Pawn is not Player );
		}

		SpectatorCamera.SpectatedPlayer = Game.Clients.ElementAt( ClientIndex ).Pawn as Player;

		var lookInput = (LookInput + Input.AnalogLook).Normal;
		LookInput = lookInput.WithPitch( lookInput.pitch.Clamp( -90f, 90f ) );
	}
}
