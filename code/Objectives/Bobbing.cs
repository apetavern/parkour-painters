using Sandbox;
using System;

public sealed class Bobbing : Component
{
	[Property] bool UseRandomRotation = true;

	internal float RandomOffset { get; set; }

	protected override void OnStart()
	{
		if ( UseRandomRotation )
		{
			RandomOffset = Random.Shared.Float( 0, 360 );
		}
		else return;
	}

	protected override void OnFixedUpdate()
	{
		GameObject.Transform.LocalRotation = Rotation.From( 45, (Time.Now * 90f) + RandomOffset, 0 );
		GameObject.Transform.LocalPosition = GameObject.Transform.LocalPosition + Vector3.Up * MathF.Sin( Time.Now * 5 );
	}
}
