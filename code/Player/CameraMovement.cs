using Sandbox;

public sealed class CameraMovement : Component
{
	//Properties
	[Property] public PlayerMovement Player { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] public GameObject Head { get; set; }
	[Property] public float Distance { get; set; } = 150f;

	//Variables
	private CameraComponent Camera;
	private Vector3 CurrentOffset = Vector3.Zero;

	protected override void OnAwake()
	{
		Camera = Components.Get<CameraComponent>();
	}

	protected override void OnUpdate()
	{
		//Rotate head base on mouse movement
		var eyeAngles = Head.Transform.Rotation.Angles();
		eyeAngles.pitch += Input.MouseDelta.y * 0.1f;
		eyeAngles.yaw -= Input.MouseDelta.x * 0.1f;
		eyeAngles.roll = 0f;
		eyeAngles.pitch = eyeAngles.pitch.Clamp( -89.9f, 89.9f );
		Head.Transform.Rotation = eyeAngles.ToRotation();

		//Set the current camera offset
		var targetOffset = Vector3.Zero;
		if ( Player.IsCrouching ) targetOffset += Vector3.Down * 32f;
		CurrentOffset = Vector3.Lerp( CurrentOffset, targetOffset, Time.Delta * 10f );

		//Set the position of the camera
		if ( Camera is not null )
		{
			var camPos = Head.Transform.Position + CurrentOffset;

			//Perform a trace backwards to see where we can safely place the camera
			var camForward = eyeAngles.ToRotation().Forward;
			var camTrace = Scene.Trace.Ray( camPos, camPos - (camForward * Distance) )
				.WithoutTags( "player", "trigger" )
				.Run();
			if ( camTrace.Hit )
			{
				camPos = camTrace.HitPosition + camTrace.Normal;
			}
			else
			{
				camPos = camTrace.EndPosition;
			}
			//Set the position of camera to calculated position
			Camera.Transform.Position = camPos;
			Camera.Transform.Rotation = eyeAngles.ToRotation();
		}
	}
}
