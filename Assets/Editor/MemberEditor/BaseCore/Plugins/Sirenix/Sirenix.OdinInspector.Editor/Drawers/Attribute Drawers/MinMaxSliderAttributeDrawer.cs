#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Editor.Drawers
{
	using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Draws Vector2 properties marked with <see cref="MinMaxSliderAttribute"/>.
    /// </summary>
	/// <seealso cref="MinMaxSliderAttribute"/>
	/// <seealso cref="MinValueAttribute"/>
	/// <seealso cref="MaxValueAttribute"/>
	/// <seealso cref="RangeAttribute"/>
	/// <seealso cref="DelayedAttribute"/>
	/// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class MinMaxSliderAttributeDrawer : OdinAttributeDrawer<MinMaxSliderAttribute, Vector2>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<Vector2> entry, MinMaxSliderAttribute attribute, GUIContent label)
        {
			entry.SmartValue = SirenixEditorFields.MinMaxSlider(label, entry.SmartValue, new Vector2(attribute.MinValue, attribute.MaxValue), attribute.ShowFields);
        }
    }
}
#endif