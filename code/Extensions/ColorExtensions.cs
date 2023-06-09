﻿namespace ParkourPainters.Extensions;

/// <summary>
/// Contains extension methods for the <see cref="Color"/> struct.
/// </summary>
internal static class ColorExtensions
{
	/// <summary>
	/// Creates a <see cref="Vector3"/> from a <see cref="Color"/>.
	/// </summary>
	/// <param name="color">The <see cref="Color"/> to create a <see cref="Vector3"/> from.</param>
	/// <returns>The <see cref="Vector3"/> created from the <see cref="Color"/>.</returns>
	internal static Vector3 ToVector3( this Color color )
	{
		var byteColor = Color32.FromRgba( color.RgbaInt );
		return new( byteColor.r, byteColor.g, byteColor.b );
	}
}
