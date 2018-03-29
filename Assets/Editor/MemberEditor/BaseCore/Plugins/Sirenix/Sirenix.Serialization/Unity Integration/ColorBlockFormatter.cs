//-----------------------------------------------------------------------
// <copyright file="ColorBlockFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Custom formatter for the <see cref="ColorBlock"/> type.
    /// </summary>
    /// <seealso cref="Sirenix.Serialization.MinimalBaseFormatter{UnityEngine.UI.ColorBlock}" />
    [CustomFormatter]
    public class ColorBlockFormatter : MinimalBaseFormatter<ColorBlock>
    {
        private static readonly Serializer<float> FloatSerializer = Serialization.Serializer.Get<float>();
        private static readonly Serializer<Color> ColorSerializer = Serialization.Serializer.Get<Color>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref ColorBlock value, IDataReader reader)
        {
            value.normalColor = ColorBlockFormatter.ColorSerializer.ReadValue(reader);
            value.highlightedColor = ColorBlockFormatter.ColorSerializer.ReadValue(reader);
            value.pressedColor = ColorBlockFormatter.ColorSerializer.ReadValue(reader);
            value.disabledColor = ColorBlockFormatter.ColorSerializer.ReadValue(reader);
            value.colorMultiplier = ColorBlockFormatter.FloatSerializer.ReadValue(reader);
            value.fadeDuration = ColorBlockFormatter.FloatSerializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref ColorBlock value, IDataWriter writer)
        {
            ColorBlockFormatter.ColorSerializer.WriteValue(value.normalColor, writer);
            ColorBlockFormatter.ColorSerializer.WriteValue(value.highlightedColor, writer);
            ColorBlockFormatter.ColorSerializer.WriteValue(value.pressedColor, writer);
            ColorBlockFormatter.ColorSerializer.WriteValue(value.disabledColor, writer);
            ColorBlockFormatter.FloatSerializer.WriteValue(value.colorMultiplier, writer);
            ColorBlockFormatter.FloatSerializer.WriteValue(value.fadeDuration, writer);
        }
    }
}