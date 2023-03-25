global using Sandbox;
global using System;
global using System.Collections.Generic;
global using System.ComponentModel;
global using System.Linq;

namespace GangJam;

public partial class GangJam : GameManager
{
	public GangJam()
	{

	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		var player = new Player();
		client.Pawn = player;
		player.Respawn();

		MoveToSpawnpoint( player );
	}

	public void MoveToSpawnpoint( Player player )
	{
		var spawnpoints = All.OfType<SpawnPoint>();
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f; // raise it up
			player.Transform = tx;
		}
	}
}
