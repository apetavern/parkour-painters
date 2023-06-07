namespace ParkourPainters.UI.GameMenu;

public class GameMenuEvent
{
	public static string Closed = "game_menu.closed";

	public class ClosedAttribute : EventAttribute
	{
		public ClosedAttribute() : base( Closed ) { }
	}
}
