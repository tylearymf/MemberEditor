#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ProgressBarAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
	using UnityEngine;
	using Sirenix.OdinInspector.Editor;
	using UnityEditor;
	using Sirenix.Utilities.Editor;
	using Sirenix.Utilities;
	using System.Reflection;
	using System;

	// The ProgressBar drawer is quite complex with all the color member refering,
	// so I've move the drawer out of the drawer, and into this static class to have it all in one place.
	internal static class ProgressBarDrawer
	{
		private class ProgressBarContext<T>
		{
			public string ErrorMessage;

			public Func<Color> StaticColorGetter;
			public Func<object, Color> InstanceColorGetter;
			public Func<object, Color> InstanceColorMethod;
			public Func<object, T, Color> InstanceColorParameterMethod;

			public Func<Color> StaticBackgroundColorGetter;
			public Func<object, Color> InstanceBackgroundColorGetter;
			public Func<object, Color> InstanceBackgroundColorMethod;
			public Func<object, T, Color> InstanceBackgroundColorParameterMethod;
		}
		
		public static float Draw<T>(OdinDrawer drawerInstance, IPropertyValueEntry<T> entry, float progress, ProgressBarAttribute attribute, GUIContent label)
		{
			PropertyContext<ProgressBarContext<T>> contextBuffer;
			if (entry.Context.Get<ProgressBarContext<T>>(drawerInstance, "ProgressBarContext", out contextBuffer))
			{
				var parentType = entry.Property.FindParent(PropertyValueCategory.Member, true).ParentType;
				contextBuffer.Value = new ProgressBarContext<T>();

				if (!attribute.ColorMember.IsNullOrWhitespace())
				{
					MemberInfo member;
					if (MemberFinder.Start(parentType)
						.IsNamed(attribute.ColorMember)
						.HasReturnType<Color>()
						.TryGetMember(out member, out contextBuffer.Value.ErrorMessage))
					{
						if (member is FieldInfo || member is PropertyInfo)
						{
							if (member.IsStatic())
							{
								contextBuffer.Value.StaticColorGetter = DeepReflection.CreateValueGetter<Color>(parentType, attribute.ColorMember);
							}
							else
							{
								contextBuffer.Value.InstanceColorGetter = DeepReflection.CreateWeakInstanceValueGetter<Color>(parentType, attribute.ColorMember);
							}
						}
						else if (member is MethodInfo)
						{
							if (member.IsStatic())
							{
								contextBuffer.Value.ErrorMessage = "Static method members are currently not supported.";
							}
							else
							{
								var method = member as MethodInfo;
								var p = method.GetParameters();

								if (p.Length == 0)
								{
									contextBuffer.Value.InstanceColorMethod = EmitUtilities.CreateWeakInstanceMethodCallerFunc<Color>(method);
								}
								else if (p.Length == 1 && p[0].ParameterType == typeof(T))
								{
									contextBuffer.Value.InstanceColorParameterMethod = EmitUtilities.CreateWeakInstanceMethodCallerFunc<T, Color>(method);
								}
							}
						}
						else
						{
							contextBuffer.Value.ErrorMessage = "Unsupported member type.";
						}
					}
				}
				if (!attribute.BackgroundColorMember.IsNullOrWhitespace())
				{
					MemberInfo member;
					if (MemberFinder.Start(parentType)
						.IsNamed(attribute.BackgroundColorMember)
						.HasReturnType<Color>()
						.TryGetMember(out member, out contextBuffer.Value.ErrorMessage))
					{
						if (member is FieldInfo || member is PropertyInfo)
						{
							if (member.IsStatic())
							{
								contextBuffer.Value.StaticBackgroundColorGetter = DeepReflection.CreateValueGetter<Color>(parentType, attribute.BackgroundColorMember);
							}
							else
							{
								contextBuffer.Value.InstanceBackgroundColorGetter = DeepReflection.CreateWeakInstanceValueGetter<Color>(parentType, attribute.BackgroundColorMember);
							}
						}
						else if (member is MethodInfo)
						{
							if (member.IsStatic())
							{
								contextBuffer.Value.ErrorMessage = "Static method members are currently not supported.";
							}
							else
							{
								var method = member as MethodInfo;
								var p = method.GetParameters();

								if (p.Length == 0)
								{
									contextBuffer.Value.InstanceBackgroundColorMethod = EmitUtilities.CreateWeakInstanceMethodCallerFunc<Color>(method);
								}
								else if (p.Length == 1 && p[0].ParameterType == typeof(T))
								{
									contextBuffer.Value.InstanceBackgroundColorParameterMethod = EmitUtilities.CreateWeakInstanceMethodCallerFunc<T, Color>(method);
								}
							}
						}
						else
						{
							contextBuffer.Value.ErrorMessage = "Unsupported member type.";
						}
					}
				}
			}

			var context = contextBuffer.Value;

			// Error message
			if (context.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(context.ErrorMessage);
			}

			// Construct rect.
			Rect rect;
			if (label != null)
			{
				rect = EditorGUILayout.GetControlRect(true, attribute.Height > EditorGUIUtility.singleLineHeight ? attribute.Height : EditorGUIUtility.singleLineHeight);
				rect = EditorGUI.PrefixLabel(rect, label);
				rect = rect.AlignMiddle(attribute.Height);
			}
			else
			{
				rect = EditorGUILayout.GetControlRect(false, attribute.Height);
				GUIHelper.IndentRect(ref rect);
			}

			// Draw
			if (Event.current.type == EventType.Repaint)
			{
				var parent = entry.Property.FindParent(PropertyValueCategory.Member, true).ParentValues[0];

				Color color =
					context.StaticColorGetter != null ? context.StaticColorGetter() :
					context.InstanceColorGetter != null ? context.InstanceColorGetter(parent) :
					context.InstanceColorMethod != null ? context.InstanceColorMethod(parent) :
					context.InstanceColorParameterMethod != null ? context.InstanceColorParameterMethod(parent, entry.SmartValue) :
					new Color(attribute.R, attribute.G, attribute.B, 1f);

				Color backgroundColor =
					context.StaticBackgroundColorGetter != null ? context.StaticBackgroundColorGetter() :
					context.InstanceBackgroundColorGetter != null ? context.InstanceBackgroundColorGetter(parent) :
					context.InstanceBackgroundColorMethod != null ? context.InstanceBackgroundColorMethod(parent) :
					context.InstanceBackgroundColorParameterMethod != null ? context.InstanceBackgroundColorParameterMethod(parent, entry.SmartValue) :
					new Color(0.16f, 0.16f, 0.16f, 1f);
				
				SirenixEditorGUI.DrawSolidRect(rect, backgroundColor);
				SirenixEditorGUI.DrawSolidRect(rect.AlignLeft(rect.width * Mathf.Clamp01(progress)), color);
				SirenixEditorGUI.DrawBorders(rect, 1, new Color(0.16f, 0.16f, 0.16f, 1f));
			}

			if (GUI.enabled)
			{
				int controlID = GUIUtility.GetControlID(FocusType.Passive);
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition) ||
					GUIUtility.hotControl == controlID && (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag))
				{
					Event.current.Use();
					GUIUtility.hotControl = controlID;
					GUIHelper.RequestRepaint();
					GUI.changed = true;

					progress = (Event.current.mousePosition.x - rect.xMin) / rect.width;
				}
				else if (GUIUtility.hotControl == controlID && Event.current.rawType == EventType.MouseUp)
				{
					GUIUtility.hotControl = 0;
				}
			}

			return progress;
		}
	}
	
	/// <summary>
	/// Draws values decorated with <see cref="ProgressBarAttribute"/>.
	/// </summary>
	/// <seealso cref="PropertyRangeAttribute"/>
	/// <seealso cref="MinMaxSliderAttribute"/>
	[OdinDrawer]
	public sealed class ProgressBarAttributeByteDrawer : OdinAttributeDrawer<ProgressBarAttribute, byte>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(IPropertyValueEntry<byte> entry, ProgressBarAttribute attribute, GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			float progress = (float)(((double)entry.SmartValue - attribute.Min) / (attribute.Max - attribute.Min));
			progress = ProgressBarDrawer.Draw<byte>(this, entry, progress, attribute, label);

			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = (byte)(attribute.Min + (attribute.Max - attribute.Min) * progress);
				GUI.changed = true;
			}
		}
	}
	
	/// <summary>
	/// Draws values decorated with <see cref="ProgressBarAttribute"/>.
	/// </summary>
	/// <seealso cref="PropertyRangeAttribute"/>
	/// <seealso cref="MinMaxSliderAttribute"/>
	[OdinDrawer]
	public sealed class ProgressBarAttributeSbyteDrawer : OdinAttributeDrawer<ProgressBarAttribute, sbyte>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(IPropertyValueEntry<sbyte> entry, ProgressBarAttribute attribute, GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			float progress = (float)(((double)entry.SmartValue - attribute.Min) / (attribute.Max - attribute.Min));
			progress = ProgressBarDrawer.Draw<sbyte>(this, entry, progress, attribute, label);
			
			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = (sbyte)(attribute.Min + (attribute.Max - attribute.Min) * progress);
				GUI.changed = true;
			}
		}
	}
	
	/// <summary>
	/// Draws values decorated with <see cref="ProgressBarAttribute"/>.
	/// </summary>
	/// <seealso cref="PropertyRangeAttribute"/>
	/// <seealso cref="MinMaxSliderAttribute"/>
	[OdinDrawer]
	public sealed class ProgressBarAttributeShortDrawer : OdinAttributeDrawer<ProgressBarAttribute, short>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(IPropertyValueEntry<short> entry, ProgressBarAttribute attribute, GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			float progress = (float)(((double)entry.SmartValue - attribute.Min) / (attribute.Max - attribute.Min));
			progress = ProgressBarDrawer.Draw<short>(this, entry, progress, attribute, label);
			
			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = (short)(attribute.Min + (attribute.Max - attribute.Min) * progress);
				GUI.changed = true;
			}
		}
	}

	/// <summary>
	/// Draws values decorated with <see cref="ProgressBarAttribute"/>.
	/// </summary>
	/// <seealso cref="PropertyRangeAttribute"/>
	/// <seealso cref="MinMaxSliderAttribute"/>
	[OdinDrawer]
	public sealed class ProgressBarAttributeUshortDrawer : OdinAttributeDrawer<ProgressBarAttribute, ushort>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(IPropertyValueEntry<ushort> entry, ProgressBarAttribute attribute, GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			float progress = (float)(((double)entry.SmartValue - attribute.Min) / (attribute.Max - attribute.Min));
			progress = ProgressBarDrawer.Draw<ushort>(this, entry, progress, attribute, label);
			
			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = (ushort)(attribute.Min + (attribute.Max - attribute.Min) * progress);
				GUI.changed = true;
			}
		}
	}

	/// <summary>
	/// Draws values decorated with <see cref="ProgressBarAttribute"/>.
	/// </summary>
	/// <seealso cref="PropertyRangeAttribute"/>
	/// <seealso cref="MinMaxSliderAttribute"/>
	[OdinDrawer]
	public sealed class ProgressBarAttributeInt32Drawer : OdinAttributeDrawer<ProgressBarAttribute, int>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(IPropertyValueEntry<int> entry, ProgressBarAttribute attribute, GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			float progress = (float)(((double)entry.SmartValue - attribute.Min) / (attribute.Max - attribute.Min));
			progress = ProgressBarDrawer.Draw<int>(this, entry, progress, attribute, label);

			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = (int)(attribute.Min + (attribute.Max - attribute.Min) * progress);
				GUI.changed = true;
			}
		}
	}
	
	/// <summary>
	/// Draws values decorated with <see cref="ProgressBarAttribute"/>.
	/// </summary>
	/// <seealso cref="PropertyRangeAttribute"/>
	/// <seealso cref="MinMaxSliderAttribute"/>
	[OdinDrawer]
	public sealed class ProgressBarAttributeUintDrawer : OdinAttributeDrawer<ProgressBarAttribute, uint>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(IPropertyValueEntry<uint> entry, ProgressBarAttribute attribute, GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			float progress = (float)(((double)entry.SmartValue - attribute.Min) / (attribute.Max - attribute.Min));
			progress = ProgressBarDrawer.Draw<uint>(this, entry, progress, attribute, label);
			
			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = (uint)(attribute.Min + (attribute.Max - attribute.Min) * progress);
				GUI.changed = true;
			}
		}
	}
	
	/// <summary>
	/// Draws values decorated with <see cref="ProgressBarAttribute"/>.
	/// </summary>
	/// <seealso cref="PropertyRangeAttribute"/>
	/// <seealso cref="MinMaxSliderAttribute"/>
	[OdinDrawer]
	public sealed class ProgressBarAttributeLongDrawer : OdinAttributeDrawer<ProgressBarAttribute, long>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(IPropertyValueEntry<long> entry, ProgressBarAttribute attribute, GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			float progress = (float)(((double)entry.SmartValue - attribute.Min) / (attribute.Max - attribute.Min));
			progress = ProgressBarDrawer.Draw<long>(this, entry, progress, attribute, label);
			
			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = (long)(attribute.Min + (attribute.Max - attribute.Min) * progress);
				GUI.changed = true;
			}
		}
	}
	
	/// <summary>
	/// Draws values decorated with <see cref="ProgressBarAttribute"/>.
	/// </summary>
	/// <seealso cref="PropertyRangeAttribute"/>
	/// <seealso cref="MinMaxSliderAttribute"/>
	[OdinDrawer]
	public sealed class ProgressBarAttributeUlongDrawer : OdinAttributeDrawer<ProgressBarAttribute, ulong>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(IPropertyValueEntry<ulong> entry, ProgressBarAttribute attribute, GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			float progress = (float)(((double)entry.SmartValue - attribute.Min) / (attribute.Max - attribute.Min));
			progress = ProgressBarDrawer.Draw<ulong>(this, entry, progress, attribute, label);
			
			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = (ulong)(attribute.Min + (attribute.Max - attribute.Min) * progress);
				GUI.changed = true;
			}
		}
	}

	/// <summary>
	/// Draws values decorated with <see cref="ProgressBarAttribute"/>.
	/// </summary>
	/// <seealso cref="PropertyRangeAttribute"/>
	/// <seealso cref="MinMaxSliderAttribute"/>
	[OdinDrawer]
	public sealed class ProgressBarAttributeFloatDrawer : OdinAttributeDrawer<ProgressBarAttribute, float>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(IPropertyValueEntry<float> entry, ProgressBarAttribute attribute, GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			float progress = (float)(((double)entry.SmartValue - attribute.Min) / (attribute.Max - attribute.Min));
			progress = ProgressBarDrawer.Draw<float>(this, entry, progress, attribute, label);

			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = (float)(attribute.Min + (attribute.Max - attribute.Min) * progress);
				GUI.changed = true;
			}
		}
	}
	
	/// <summary>
	/// Draws values decorated with <see cref="ProgressBarAttribute"/>.
	/// </summary>
	/// <seealso cref="PropertyRangeAttribute"/>
	/// <seealso cref="MinMaxSliderAttribute"/>
	[OdinDrawer]
	public sealed class ProgressBarAttributeDoubleDrawer : OdinAttributeDrawer<ProgressBarAttribute, double>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(IPropertyValueEntry<double> entry, ProgressBarAttribute attribute, GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			float progress = (float)(((double)entry.SmartValue - attribute.Min) / (attribute.Max - attribute.Min));
			progress = ProgressBarDrawer.Draw<double>(this, entry, progress, attribute, label);

			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = (double)(attribute.Min + (attribute.Max - attribute.Min) * progress);
				GUI.changed = true;
			}
		}
	}
	
	/// <summary>
	/// Draws values decorated with <see cref="ProgressBarAttribute"/>.
	/// </summary>
	/// <seealso cref="PropertyRangeAttribute"/>
	/// <seealso cref="MinMaxSliderAttribute"/>
	[OdinDrawer]
	public sealed class ProgressBarAttributeDecimalDrawer : OdinAttributeDrawer<ProgressBarAttribute, decimal>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(IPropertyValueEntry<decimal> entry, ProgressBarAttribute attribute, GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			float progress = (float)(((double)entry.SmartValue - attribute.Min) / (attribute.Max - attribute.Min));
			progress = ProgressBarDrawer.Draw<decimal>(this, entry, progress, attribute, label);

			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = (decimal)(attribute.Min + (attribute.Max - attribute.Min) * progress);
				GUI.changed = true;
			}
		}
	}
}
#endif