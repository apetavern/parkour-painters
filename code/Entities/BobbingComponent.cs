namespace ParkourPainters.Entities;

/// <summary>
/// A component that bobs and rotates the entity it is attached to.
/// </summary>
internal sealed partial class BobbingComponent : EntityComponent<ModelEntity>, ISingletonComponent
{
	/// <summary>
	/// Any kind of offset to add to the <see cref="AnchorPosition"/>.
	/// </summary>
	internal Vector3 PositionOffset { get; set; }

	/// <summary>
	/// The starting offset of the yaw rotation.
	/// </summary>
	internal float RandomOffset { get; set; }

	/// <summary>
	/// Whether or not pitch should be applied to the rotation.
	/// </summary>
	internal bool NoPitch { get; set; }

	/// <summary>
	/// The position to anchor the entity at. Initializes to the position of the entity.
	/// </summary>
	internal Vector3 AnchorPosition { get; set; }

	/// <inheritdoc/>
	protected sealed override void OnActivate()
	{
		base.OnActivate();

		AnchorPosition = Entity.Position;
		RandomOffset = Random.Shared.Float( 0, 360 );
	}

	/// <summary>
	/// Bobs entity that this component is attached to.
	/// </summary>
	[GameEvent.Tick.Server]
	private void Bob()
	{
		Entity.Rotation = Rotation.From( NoPitch ? 0 : 45, (Time.Now * 90f) + RandomOffset, 0 );

		// bob up and down
		var bobbingOffset = Vector3.Up * MathF.Sin( Time.Now * 2 );
		Entity.Position = AnchorPosition + (bobbingOffset + PositionOffset) * Entity.Scale;
	}
}
