namespace GangJam.Extensions;

internal static class ColorExtensions
{
	/// <summary>
	/// Converts a Color into a Vector3 with alpha set to 1, useful for setting particle control points.
	/// </summary>
	/// <param name="color"></param>
	/// <returns></returns>
	internal static Vector3 ToVector3( this Color color )
	{
		return new Vector3( color.r, color.g, color.b );
	}
}
