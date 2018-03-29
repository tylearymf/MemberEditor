//-----------------------------------------------------------------------
// <copyright file="ColorPaletteAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>ColorPalette is used on any Color property, and allows for choosing colors from different definable palettes.</para>
    /// <para>Use this to allow the user to choose from a set of predefined color options.</para>
    /// </summary>
    /// <remarks>
    /// <para>See and edit the color palettes in Tools > Odin Inspector > Drawers > Color Palettes.</para>
    /// <note type="note">The color property is not tied to the color palette, and can be edited. Therefore the color will also not update if the ColorPalette is edited.</note>
    /// </remarks>
    /// <example>
    /// <para>The following example shows how ColorPalette is applied to a property. The user can freely choose between all available ColorPalettes.</para>
    /// <code>
    /// public class ColorPaletteExamples : MonoBehaviour
    /// {
    ///     [ColorPalette]
    ///     public Color ColorOptions;
    /// 
    ///     [ColorPalette("Underwater")]
    ///     public Color UnderwaterColor;
    /// 
    ///     [ColorPalette("Fall"), HideLabel]
    ///     public Color WideColorPalette;
    /// 
    ///     [ColorPalette("My Palette")]
    ///     public Color MyColor;
    /// 
    ///     [ColorPalette("Clovers")]
    ///     public Color[] ColorArray;
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ColorPaletteAttribute : Attribute
    {
		/// <summary>
		/// Adds a color palette options to a Color property.
		/// </summary>
		public ColorPaletteAttribute()
        {
            this.PaletteName = null;
            this.ShowAlpha = true;
        }

		/// <summary>
		/// Adds color options to a Color property from a specific palette.
		/// </summary>
		/// <param name="paletteName">Name of the palette.</param>
		public ColorPaletteAttribute(string paletteName)
        {
            this.PaletteName = paletteName;
            this.ShowAlpha = true;
        }

		/// <summary>
		/// Gets the name of the palette.
		/// </summary>
		public string PaletteName { get; private set; }

		/// <summary>
		/// Indicates if the color palette should show alpha values or not.
		/// </summary>
		public bool ShowAlpha { get; set; }
    }
}