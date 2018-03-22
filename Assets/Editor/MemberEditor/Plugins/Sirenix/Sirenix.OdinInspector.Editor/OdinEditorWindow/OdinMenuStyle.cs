#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinMenuStyle.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
    using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// <para>The style settings used by <see cref="OdinMenuItem"/>.</para>
    /// <para>
    /// A nice trick to style your menu is to add the tree.DefaultMenuStyle to the tree itself, 
    /// and style it live. Once you are happy, you can hit the Copy CSharp Snippet button, 
    /// remove the style from the menu tree, and paste the style directly into your code.
    /// </para>
    /// </summary>
    /// <seealso cref="OdinMenuTree" />
    /// <seealso cref="OdinMenuItem" />
    /// <seealso cref="OdinMenuTreeSelection" />
    /// <seealso cref="OdinMenuTreeExtensions" />
    /// <seealso cref="OdinMenuEditorWindow" />
    public class OdinMenuStyle
    {
        /// <summary>
        /// The height of the menu item.
        /// </summary>
        [BoxGroup("General")]
        public int Height = 30;

        /// <summary>
        /// The global offset of the menu item content
        /// </summary>
        [BoxGroup("General")]
        public float Offset = 16;

        /// <summary>
        /// The number of pixels to indent per level indent level.
        /// </summary>
        [BoxGroup("General")]
        public float IndentAmount = 15;

        /// <summary>
        /// The size of the icon.
        /// </summary>
        [BoxGroup("Icons")]
        public float IconSize = 16;

        /// <summary>
        /// The size of the icon.
        /// </summary>
        [BoxGroup("Icons")]
        public float IconOffset = 0;

        /// <summary>
        /// The transparency of icons when the menu item is not selected.
        /// </summary>
        [BoxGroup("Icons"), Range(0, 1)]
        public float NotSelectedIconAlpha = 0.85f;

        /// <summary>
        /// The padding between the icon and other content.
        /// </summary>
        [BoxGroup("Icons")]
        public float IconPadding = 3;

        /// <summary>
        /// The size of the foldout triangle icon.
        /// </summary>
        [BoxGroup("Triangle")]
        public float TriangleSize = 17;

        /// <summary>
        /// The padding between the foldout triangle icon and other content.
        /// </summary>
        [BoxGroup("Triangle")]
        public float TrianglePadding = 8;

        /// <summary>
        /// Whether or not to align the triangle left or right of the content.
        /// If right, then the icon is pushed all the way to the right at a fixed position ignoring the indent level.
        /// </summary>
        [BoxGroup("Triangle")]
        public bool AlignTriangleLeft = false;

        /// <summary>
        /// Whether to draw borders between menu items.
        /// </summary>
        [BoxGroup("Borders")]
        public bool Borders = true;

        /// <summary>
        /// The horizontal border padding.
        /// </summary>
        [BoxGroup("Borders"), EnableIf("Borders")]
        public float BorderPadding = 13;

        /// <summary>
        /// The border alpha.
        /// </summary>
        [BoxGroup("Borders"), EnableIf("Borders"), Range(0, 1)]
        public float BorderAlpha = 0.5f;

        /// <summary>
        /// The border alpha.
        /// </summary>
        [BoxGroup("Colors")]
        public Color SelectedColorDarkSkin = new Color(0.243f, 0.373f, 0.588f, 1.000f);

        /// <summary>
        /// The border alpha.
        /// </summary>
        [BoxGroup("Colors")]
        public Color SelectedColorLightSkin = new Color(0.243f, 0.49f, 0.9f, 1.000f);

        /// <summary>
        /// Creates and returns an instance of a menu style that makes it look like Unity's project window.
        /// </summary>
        public static OdinMenuStyle TreeViewStyle
        {
            get
            {
                return new OdinMenuStyle()
                {
                    BorderPadding = 0f,
                    AlignTriangleLeft = true,
                    TriangleSize = 16,
                    TrianglePadding = 0,
                    Offset = 20,
                    Height = 23,
                    IconPadding = 0,
                    BorderAlpha = 0.323f
                };
            }
        }

        [Button("Copy C# Snippet", ButtonSizes.Large)]
        private void CopyCSharpSnippet()
        {
            Clipboard.Copy(@"new OdinMenuStyle()
{
    Height = " + this.Height + @",
    Offset = " + this.Offset.ToString("F2") + @"f,
    IndentAmount = " + this.IndentAmount.ToString("F2") + @"f,
    IconSize = " + this.IconSize.ToString("F2") + @"f,
    IconOffset = " + this.IconOffset.ToString("F2") + @"f,
    NotSelectedIconAlpha = " + this.NotSelectedIconAlpha.ToString("F2") + @"f,
    IconPadding = " + this.IconPadding.ToString("F2") + @"f,
    TriangleSize = " + this.TriangleSize.ToString("F2") + @"f,
    TrianglePadding = " + this.TrianglePadding.ToString("F2") + @"f,
    AlignTriangleLeft = " + this.AlignTriangleLeft.ToString().ToLower() + @",
    Borders = " + this.Borders.ToString().ToLower() + @",
    BorderPadding = " + this.BorderPadding.ToString("F2") + @"f,
    BorderAlpha = " + this.BorderAlpha.ToString("F2") + @"f,
    SelectedColorDarkSkin = new Color(" +
        this.SelectedColorDarkSkin.r.ToString("F3") + @"f, " +
        this.SelectedColorDarkSkin.g.ToString("F3") + @"f, " +
        this.SelectedColorDarkSkin.b.ToString("F3") + @"f, " +
        this.SelectedColorDarkSkin.a.ToString("F3") + @"f),
    SelectedColorLightSkin = new Color(" +
        this.SelectedColorLightSkin.r.ToString("F3") + @"f, " +
        this.SelectedColorLightSkin.g.ToString("F3") + @"f, " +
        this.SelectedColorLightSkin.b.ToString("F3") + @"f, " +
        this.SelectedColorLightSkin.a.ToString("F3") + @"f)" + 
    "};");
        }
    }
}
#endif