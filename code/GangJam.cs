﻿global using Editor;
global using GangJam.State;
global using Sandbox;
global using System;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.ComponentModel;
global using System.Linq;

namespace GangJam;

public partial class GangJam : GameManager
{
	public static new GangJam Current => (GangJam)GameManager.Current;

	/// <summary>
	/// The current state of the game that is running.
	/// </summary>
	[Net] internal IGameState CurrentState { get; private set; }

	public GangJam()
	{
		if ( !Game.IsServer )
			return;

		WaitingState.SetActive();
	}

	/// <inheritdoc/>
	public override void ClientJoined( IClient cl )
	{
		base.ClientJoined( cl );

		CurrentState?.ClientJoined( cl );
	}

	/// <inheritdoc/>
	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		CurrentState?.ClientDisconnected( cl, reason );
	}

	/// <inheritdoc/>
	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		CurrentState?.Simulate( cl );
	}

	/// <summary>
	/// Invoked every frame on the client-side.
	/// </summary>
	[Event.Tick.Client]
	private void ClientTick()
	{
		CurrentState?.ClientTick();
	}

	/// <summary>
	/// Invoked every tick on the server-side.
	/// </summary>
	[Event.Tick.Server]
	private void ServerTick()
	{
		CurrentState?.ServerTick();
	}

	/// <summary>
	/// Sets a new active game state. This can only be called on the server.
	/// </summary>
	/// <typeparam name="T">The type of the game state to set as active.</typeparam>
	internal void SetState<T>() where T : IGameState, new()
	{
		Game.AssertServer();

		SetState( new T() );
	}

	/// <summary>
	/// Sets a new active game state. This can only be called on the server.
	/// </summary>
	/// <param name="state">An instance of the game state to set as active.</param>
	private void SetState( IGameState state )
	{
		Game.AssertServer();

		var oldState = CurrentState;
		oldState?.Exit();
		CurrentState = state;
		CurrentState.Enter( oldState );

		oldState?.Delete();
	}
}