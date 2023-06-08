namespace SpeedPainters.UI;

public partial class MapIcon : Panel
{
	protected void VoteMap()
	{
		MapVoteState.SetVote( Ident );
	}
}
