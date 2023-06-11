namespace SpeedPainters.Entities;

public partial class Player
{
	[Net]
	public float StopWatch { get; set; }

	[Net]
	public bool ReachedEnd { get; set; }

}
