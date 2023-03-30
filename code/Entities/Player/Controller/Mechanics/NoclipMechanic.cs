namespace ParkoutPainters.Entities;

public sealed class NoclipMechanic : ControllerMechanic
{
	[ConCmd.Admin( "noclip" )]
	private static void ToggleNoclip()
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		var noclip = player.Components.GetOrCreate<NoclipMechanic>();
		noclip._isEnabled = !noclip._isEnabled;
	}

	private bool _isEnabled = false;
	protected override bool ShouldStart() => _isEnabled;

	protected override void OnStart()
	{
		Player.Components.RemoveAny<AirMoveMechanic>();
		Player.Components.RemoveAny<UnstuckMechanic>();
	}

	protected override void OnStop()
	{
		Player.Components.GetOrCreate<AirMoveMechanic>();
		Player.Components.GetOrCreate<UnstuckMechanic>();
	}

	protected override void Simulate()
	{
		var fwd = Player.MoveInput.x.Clamp( -1f, 1f );
		var left = Player.MoveInput.y.Clamp( -1f, 1f );
		var rotation = Player.LookInput.ToRotation();

		var vel = (rotation.Forward * fwd) + (rotation.Left * left);

		if ( Input.Down( InputButton.Jump ) )
			vel += Vector3.Up * 1;

		vel = vel.Normal * 2000;

		if ( Input.Down( InputButton.Run ) )
			vel *= 5.0f;

		if ( Input.Down( InputButton.Duck ) )
			vel *= 0.2f;

		Velocity += vel * Time.Delta;

		if ( Velocity.LengthSquared > 0.01f )
			Position += Velocity * Time.Delta;

		Velocity = Velocity.Approach( 0, Velocity.Length * Time.Delta * 5.0f );

		Player.EyeRotation = rotation;
		Controller.GetMechanic<WalkMechanic>().ClearGroundEntity();
		Player.BaseVelocity = Vector3.Zero;
	}
}
