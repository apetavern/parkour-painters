using Sandbox;

public enum CollectableType
{
	Spraycan,
	other
}

public sealed class Collectable : Component, Component.ITriggerListener
{
	[Property] public CollectableType CollectableType;

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		GameObject.Destroy();
	}
}
