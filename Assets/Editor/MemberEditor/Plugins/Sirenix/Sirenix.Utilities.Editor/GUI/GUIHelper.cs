#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUIUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Utilities;
    using UnityEditor;
    using UnityEngine;
    using UnityEditorInternal;

    /// <summary>
    /// Various helper function for GUI.
    /// </summary>
    [InitializeOnLoad]
    public static class GUIHelper
    {
        [NonSerialized]
        private static object defaultConfigKey = new object();

        private static readonly GUIContent tmpContent = new GUIContent("");
        private static readonly GUIScopeStack<bool> GUIEnabledStack = new GUIScopeStack<bool>();
        private static readonly GUIScopeStack<bool> HierarchyModeStack = new GUIScopeStack<bool>();
        private static readonly GUIScopeStack<bool> IsBoldLabelStack = new GUIScopeStack<bool>();
        private static readonly GUIScopeStack<Matrix4x4> MatrixStack = new GUIScopeStack<Matrix4x4>();
        private static readonly GUIScopeStack<Color> ColorStack = new GUIScopeStack<Color>();
        private static readonly GUIScopeStack<EventType> EventTypeStack = new GUIScopeStack<EventType>();
        private static readonly GUIScopeStack<Color> ContentColorStack = new GUIScopeStack<Color>();
        private static readonly GUIScopeStack<Color> LabelColorStack = new GUIScopeStack<Color>();
        private static readonly GUIScopeStack<int> IndentLevelStack = new GUIScopeStack<int>();
        private static readonly GUIScopeStack<float> LabelWidthStack = new GUIScopeStack<float>();
        private static readonly GUIScopeStack<GUIContext<LayoutMeasureInfo>> LayoutMeasureInfoStack = new GUIScopeStack<GUIContext<LayoutMeasureInfo>>();
        private static readonly GUIScopeStack<bool> ResponsiveVectorComponentFieldsStack = new GUIScopeStack<bool>();
        private static readonly GUIScopeStack<float> FadeGroupDurationStack = new GUIScopeStack<float>();
        private static readonly GUIScopeStack<float> TabPageSlideAnimationDurationStack = new GUIScopeStack<float>();
        private static readonly GUIScopeStack<bool> IsDrawingDictionaryKeyStack = new GUIScopeStack<bool>();
        private static readonly LayoutMeasureInfo LayoutMesuringInfoDummy = new LayoutMeasureInfo();
        private static readonly WeakValueGetter<Rect> GetRectOnGUILayoutEntry;
        private static readonly Func<EditorWindow> CurrentWindowGetter;
        private static readonly Func<Vector2> EditorScreenPointOffsetGetter;
        private static readonly Func<RectOffset> CurrentWindowBorderSizeGetter;
        private static readonly Func<int> GUILayoutEntriesCursorIndexGetter;
        private static readonly Func<IList> GUILayoutEntriesGetter;
        private static readonly Func<int> CurrentWindowIDGetter;
        private static readonly Func<float> CurrentIndentAmountGetter;
        private static readonly Func<bool> CurrentWindowHasFocusGetter;
        private static readonly Action<bool> SetBoldDefaultFontSetter;
        private static readonly Func<bool> GetBoldDefaultFontGetter;
        private static readonly Func<Rect> GetTopLevelLayoutRectGetter;
        private static readonly Func<EditorWindow, bool> GetIsDockedWindowGetter;
        private static readonly Func<Rect> GetEditorWindowRectGetter;
        private static readonly Type inspectorWindowType;
        private static readonly Func<float> ContextWidthGetter;
        private static readonly Func<float> ActualLabelWidthGetter;
        //private static readonly Action<float> ActualLabelWidthSetter;

        /// <summary>
        /// Gets the bold default font.
        /// </summary>
        public static bool GetBoldDefaultFont()
        {
            return GetBoldDefaultFontGetter();
        }

        static GUIHelper()
        {
            var guiLayoutEntryType = typeof(GUI).Assembly.GetType("UnityEngine.GUILayoutEntry");
            var hostViewType = typeof(Editor).Assembly.GetType("UnityEditor.HostView");
            var guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
            inspectorWindowType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");

            GetRectOnGUILayoutEntry = EmitUtilities.CreateWeakInstanceFieldGetter<Rect>(guiLayoutEntryType, guiLayoutEntryType.GetField("rect", Flags.AllMembers));
            EditorScreenPointOffsetGetter = DeepReflection.CreateValueGetter<Vector2>(typeof(GUIUtility), "s_EditorScreenPointOffset");
            CurrentWindowIDGetter = DeepReflection.CreateValueGetter<int>(guiViewType, "current.GetInstanceID()");
            CurrentWindowHasFocusGetter = DeepReflection.CreateValueGetter<bool>(guiViewType, "current.hasFocus");
            GetIsDockedWindowGetter = DeepReflection.CreateValueGetter<EditorWindow, bool>("docked");
            ContextWidthGetter = DeepReflection.CreateValueGetter<float>(typeof(EditorGUIUtility), "contextWidth");
            ActualLabelWidthGetter = EmitUtilities.CreateStaticFieldGetter<float>(typeof(EditorGUIUtility).GetField("s_LabelWidth", Flags.AllMembers));
            //ActualLabelWidthSetter = EmitUtilities.CreateStaticFieldSetter<float>(typeof(EditorGUIUtility).GetField("s_LabelWidth", Flags.AllMembers));

            var guiViewGetter = DeepReflection.CreateWeakStaticValueGetter(guiViewType, guiViewType, "current");
            var actualViewGetter = DeepReflection.CreateWeakInstanceValueGetter<EditorWindow>(hostViewType, "actualView");
            var borderSizeGetter = DeepReflection.CreateWeakInstanceValueGetter<RectOffset>(hostViewType, "borderSize");

            CurrentWindowGetter = () => actualViewGetter(guiViewGetter());
            CurrentWindowBorderSizeGetter = () => borderSizeGetter(guiViewGetter());

            SetBoldDefaultFontSetter = (Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>),
                typeof(EditorGUIUtility).FindMember()
                .IsStatic()
                .IsMethod()
                .HasParameters<bool>()
                .ReturnsVoid()
                .IsNamed("SetBoldDefaultFont")
                .GetMember<MethodInfo>(), true);

            GetBoldDefaultFontGetter = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>),
                typeof(EditorGUIUtility).FindMember()
                .IsStatic()
                .IsMethod()
                .HasNoParameters()
                .HasReturnType<bool>()
                .IsNamed("GetBoldDefaultFont")
                .GetMember<MethodInfo>(), true);

            GUILayoutEntriesCursorIndexGetter = DeepReflection.CreateValueGetter<int>(typeof(GUILayoutUtility), "current.topLevel.m_Cursor");
            GUILayoutEntriesGetter = DeepReflection.CreateValueGetter<IList>(typeof(GUILayoutUtility), "current.topLevel.entries");
            CurrentIndentAmountGetter = DeepReflection.CreateValueGetter<float>(typeof(EditorGUI), "indent");
            GetTopLevelLayoutRectGetter = DeepReflection.CreateValueGetter<Rect>(typeof(GUILayoutUtility), "current.topLevel.rect");
            GetEditorWindowRectGetter = DeepReflection.CreateValueGetter<Rect>(typeof(Editor).Assembly.GetType("UnityEditor.Toolbar"), "get.parent.screenPosition");
        }

        /// <summary>
        /// An alternative to GUI.FocusControl(null), which does not take focus away from the current GUI.Window.
        /// </summary>
        public static void RemoveFocusControl()
        {
            GUIUtility.hotControl = 0;
            DragAndDrop.activeControlID = 0;
            GUIUtility.keyboardControl = 0;
        }

        /// <summary>
        /// Whether the inspector is currently in the progress of drawing a dictionary key.
        /// </summary>
        public static bool IsDrawingDictionaryKey { get { return IsDrawingDictionaryKeyStack.Count == 0 ? false : IsDrawingDictionaryKeyStack.Peek(); } }

        /// <summary>
        /// Hides the following draw calls. Remember to call <see cref="EndDrawToNothing"/> when done.
        /// </summary>
        public static void BeginDrawToNothing()
        {
            GUILayout.BeginArea(new Rect(-9999, -9999, 1, 1), SirenixGUIStyles.None);
        }

        /// <summary>
        /// Unhides the following draw calls after having called <see cref="BeginDrawToNothing"/>.
        /// </summary>
        public static void EndDrawToNothing()
        {
            GUILayout.EndArea();
        }

        /// <summary>
        /// Determines whether the specified EditorWindow is docked.
        /// </summary>
        /// <param name="window">The editor window.</param>
        /// <returns><c>true</c> if the editor window is docked. Otherwise <c>false</c>.</returns>
        public static bool IsDockedWindow(EditorWindow window)
        {
            return GetIsDockedWindowGetter(window);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static Rect GetEditorWindowRect()
        {
            return GetEditorWindowRectGetter();
        }

        /// <summary>
        /// Opens a new inspector window for the specified object.
        /// </summary>
        /// <param name="unityObj">The unity object.</param>
        /// <exception cref="System.ArgumentNullException">unityObj</exception>
        public static void OpenInspectorWindow(UnityEngine.Object unityObj)
        {
            if (unityObj == null) throw new ArgumentNullException("unityObj");

            var windowRect = GUIHelper.GetEditorWindowRect();
            var windowSize = new Vector2(450, Mathf.Min(windowRect.height * 0.7f, 500));
            var rect = new Rect(windowRect.center - windowSize * 0.5f, windowSize);

            var inspectorInstance = ScriptableObject.CreateInstance(inspectorWindowType) as EditorWindow;
            inspectorInstance.Show();

            var prevSelection = Selection.objects;
            Selection.activeObject = unityObj;
            inspectorWindowType
                .GetProperty("isLocked", Flags.AllMembers)
                .GetSetMethod()
                .Invoke(inspectorInstance, new object[] { true });
            Selection.objects = prevSelection;
            inspectorInstance.position = rect;

            if (unityObj.GetType().InheritsFrom(typeof(Texture2D)))
            {
                EditorApplication.delayCall += () =>
                {
                    // Unity's Texture drawer changes Selection.object
                    EditorApplication.delayCall += () =>
                    {
                        // Lets change that back shall we?
                        Selection.objects = prevSelection;
                    };
                };
            }
        }

        internal static void OpenEditorInOdinDropDown(UnityEngine.Object obj, Rect btnRect)
        {
            var odinEditorWindow = AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Editor.OdinEditorWindow");
            odinEditorWindow.GetMethods(Flags.StaticPublic)
                .First(x => x.Name == "InspectObjectInDropDown" && x.GetParameters().Last().ParameterType == typeof(float))
                .Invoke(null, new object[] { obj, btnRect, 400 });
        }

        /// <summary>
        /// Gets or sets a value indicating whether labels are currently bold.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is bold label; otherwise, <c>false</c>.
        /// </value>
        public static bool IsBoldLabel
        {
            get { return GetBoldDefaultFontGetter(); }
            set { SetBoldDefaultFontSetter(value); }
        }

        /// <summary>
        /// Gets the size of the current window border.
        /// </summary>
        /// <value>
        /// The size of the current window border.
        /// </value>
        public static RectOffset CurrentWindowBorderSize
        {
            get { return CurrentWindowBorderSizeGetter(); }
        }

        /// <summary>
        /// Gets the editor screen point offset.
        /// </summary>
        /// <value>
        /// The editor screen point offset.
        /// </value>
        public static Vector2 EditorScreenPointOffset
        {
            get { return EditorScreenPointOffsetGetter(); }
        }

        /// <summary>
        /// Gets the current editor gui context width.
        /// </summary>
        public static float ContextWidth
        {
            get { return ContextWidthGetter(); }
        }

        /// <summary>
        /// Gets the current indent amount.
        /// </summary>
        /// <value>
        /// The current indent amount.
        /// </value>
        public static float CurrentIndentAmount
        {
            get
            {
                return CurrentIndentAmountGetter();
            }
        }

        /// <summary>
        /// Gets the mouse screen position.
        /// </summary>
        /// <value>
        /// The mouse screen position.
        /// </value>
        public static Vector2 MouseScreenPosition
        {
            get
            {
                return Event.current.mousePosition + EditorScreenPointOffset;
            }
        }

        /// <summary>
        /// Gets the current editor window.
        /// </summary>
        /// <value>
        /// The current editor window.
        /// </value>
        public static EditorWindow CurrentWindow
        {
            get { return CurrentWindowGetter(); }
        }

        /// <summary>
        /// Gets a value indicating whether the current editor window is focused.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current window has focus. Otherwise, <c>false</c>.
        /// </value>
        public static bool CurrentWindowHasFocus
        {
            get
            {
                return CurrentWindowHasFocusGetter();
            }
        }

        /// <summary>
        /// Gets the ID of the current editor window.
        /// </summary>
        /// <value>
        /// The ID of the current editor window.
        /// </value>
        public static int CurrentWindowInstanceID
        {
            get
            {
                return CurrentWindowIDGetter();
            }
        }

        /// <summary>
        /// Gets a value indicating whether a repaint has been requested.
        /// </summary>
        /// <value>
        ///   <c>true</c> if repaint has been requested. Otherwise <c>false</c>.
        /// </value>
        public static bool RepaintRequested { get; private set; }

        /// <summary>
        /// Gets or sets the actual EditorGUIUtility.LabelWidth, regardless of the current hierarchy mode or context width.
        /// </summary>
        public static float ActualLabelWidth
        {
            get { return ActualLabelWidthGetter(); }
            set { EditorGUIUtility.labelWidth = value; }
        }

        /// <summary>
        /// Requests a repaint.
        /// </summary>
        public static void RequestRepaint()
        {
            RepaintRequested = true;
        }

        /// <summary>
        /// Begins the layout measuring. Remember to end with <see cref="EndLayoutMeasuring"/>.
        /// </summary>
        /// <returns></returns>
        public static Rect BeginLayoutMeasuring()
        {
            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            if (Event.current.type != EventType.Layout)
            {
                var measureInfo = GetTemporaryContext(LayoutMeasureInfoStack, controlId, LayoutMesuringInfoDummy);
                LayoutMeasureInfoStack.Push(measureInfo);

                measureInfo.Value.CursorIndex = GUILayoutEntriesCursorIndexGetter();

                return measureInfo.Value.Rect;
            }
            return new Rect(0, 0, 0, 0);
        }

        /// <summary>
        /// Ends the layout measuring started by <see cref="BeginLayoutMeasuring"/>
        /// </summary>
        /// <returns>The measured rect.</returns>
        public static Rect EndLayoutMeasuring()
        {
            if (Event.current.type != EventType.Layout)
            {
                var measureInfo = LayoutMeasureInfoStack.Pop();

                var from = measureInfo.Value.CursorIndex;
                var to = GUILayoutEntriesCursorIndexGetter();
                IList entries = GUILayoutEntriesGetter();

                from = Mathf.Min(from, entries.Count - 1);
                to = Mathf.Min(to, entries.Count);
                if (from >= 0)
                {
                    var entry = entries[from];
                    var rect = GetRectOnGUILayoutEntry(ref entry);

                    if (from == to)
                    {
                        rect.width = 0;
                        rect.height = 0;
                    }
                    else
                    {
                        for (int i = from + 1; i < to; i++)
                        {
                            var entry1 = entries[i];
                            var tmpRect = GetRectOnGUILayoutEntry(ref entry1);

                            rect.xMin = Mathf.Min(rect.xMin, tmpRect.xMin);
                            rect.yMin = Mathf.Min(rect.yMin, tmpRect.yMin);
                            rect.xMax = Mathf.Max(rect.xMax, tmpRect.xMax);
                            rect.yMax = Mathf.Max(rect.yMax, tmpRect.yMax);
                        }
                    }

                    measureInfo.Value.Rect = rect;
                }

                return measureInfo.Value.Rect;
            }

            return new Rect(0, 0, 0, 0);
        }

        /// <summary>
        /// Gets the current layout rect.
        /// </summary>
        /// <returns>The current layout rect.</returns>
        public static Rect GetCurrentLayoutRect()
        {
            return GetTopLevelLayoutRectGetter();
        }

        /// <summary>
        /// Gets the playmode color tint.
        /// </summary>
        /// <returns>The playmode color tint.</returns>
        public static Color GetPlaymodeTint()
        {
            Color tint = Color.white;

            // Preference string is formatted as "playmode tint;R;G;B;A"
            var a = EditorPrefs.GetString("Playmode tint", "playmode tint;1;1;1;1").Split(';');
            float.TryParse(a[1], out tint.r);
            float.TryParse(a[2], out tint.g);
            float.TryParse(a[3], out tint.b);
            float.TryParse(a[4], out tint.a);

            return tint;
        }

        /// <summary>
        /// Pushes a color to the GUI color stack. Remember to pop the color with <see cref="PopColor"/>.
        /// </summary>
        /// <param name="color">The color to push the GUI color..</param>
        /// <param name="blendAlpha">if set to <c>true</c> blend with alpha.</param>
        public static void PushColor(Color color, bool blendAlpha = false)
        {
            ColorStack.Push(GUI.color);

            if (blendAlpha)
            {
                color.a = color.a * GUI.color.a;
            }

            GUI.color = color;
        }

        /// <summary>
        /// Takes a screenshot of the GUI within the specified rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <returns>The screenshot as a render texture.</returns>
        public static RenderTexture TakeGUIScreenshot(Rect rect)
        {
            RenderTexture rt = RenderTexture.GetTemporary((int)rect.width, (int)rect.height);
            Graphics.Blit(RenderTexture.active, rt);
            return rt;
        }

        /// <summary>
        /// Pops the GUI color pushed by <see cref="PushColor(Color, bool)"/>.
        /// </summary>
        public static void PopColor()
        {
            GUI.color = ColorStack.Pop();
        }

        /// <summary>
        /// Pushes a state to the GUI enabled stack. Remember to pop the state with <see cref="PopGUIEnabled"/>.
        /// </summary>
        /// <param name="enabled">If set to <c>true</c> GUI will be enabled. Otherwise GUI will be disabled.</param>
        public static void PushGUIEnabled(bool enabled)
        {
            GUIEnabledStack.Push(GUI.enabled);
            GUI.enabled = enabled;
        }

        /// <summary>
        /// Pops the GUI enabled pushed by <see cref="PushGUIEnabled(bool)"/>
        /// </summary>
        public static void PopGUIEnabled()
        {
            GUI.enabled = GUIEnabledStack.Pop();
        }

        /// <summary>
        /// Pushes a state to the IsDrawingDictionaryKey stack. Remember to pop the state with <see cref="PopIsDrawingDictionaryKey"/>.
        /// </summary>
        public static void PushIsDrawingDictionaryKey(bool enabled)
        {
            IsDrawingDictionaryKeyStack.Push(enabled);
        }

        /// <summary>
        /// Pops the state pushed by <see cref="PushIsDrawingDictionaryKey(bool)"/>
        /// </summary>
        public static void PopIsDrawingDictionaryKey()
        {
            IsDrawingDictionaryKeyStack.Pop();
        }

        /// <summary>
        /// Pushes the hierarchy mode to the stack. Remember to pop the state with <see cref="PopHierarchyMode"/>.
        /// </summary>
        /// <param name="hierarchyMode">The hierachy mode state to push.</param>
        /// <param name="preserveCurrentLabelWidth">Changing hierachy mode also changes how label-widths are calcualted. By default, we try to keep the current label width.</param>
        public static void PushHierarchyMode(bool hierarchyMode, bool preserveCurrentLabelWidth = true)
        {
            var actualLabelWidth = ActualLabelWidth;
            LabelWidthStack.Push(actualLabelWidth);
            var currentLabelWidth = preserveCurrentLabelWidth ? EditorGUIUtility.labelWidth : actualLabelWidth;
            HierarchyModeStack.Push(EditorGUIUtility.hierarchyMode);
            EditorGUIUtility.hierarchyMode = hierarchyMode;
            EditorGUIUtility.labelWidth = currentLabelWidth;
        }

        /// <summary>
        /// Pops the hierarchy mode pushed by <see cref="PushHierarchyMode(bool)"/>.
        /// </summary>
        public static void PopHierarchyMode()
        {
            EditorGUIUtility.hierarchyMode = HierarchyModeStack.Pop();
            ActualLabelWidth = LabelWidthStack.Pop();
        }

        /// <summary>
        /// Pushes bold label state to the stack. Remember to pop with <see cref="PopIsBoldLabel"/>.
        /// </summary>
        /// <param name="isBold">Value indicating if labels should be bold or not.</param>
        public static void PushIsBoldLabel(bool isBold)
        {
            IsBoldLabelStack.Push(IsBoldLabel);
            IsBoldLabel = isBold;
        }

        /// <summary>
        /// Pops the bold label state pushed by <see cref="PushIsBoldLabel(bool)"/>.
        /// </summary>
        public static void PopIsBoldLabel()
        {
            IsBoldLabel = IsBoldLabelStack.Pop();
        }

        /// <summary>
        /// Pushes the indent level to the stack. Remember to pop with <see cref="PopIndentLevel"/>.
        /// </summary>
        /// <param name="indentLevel">The indent level to push.</param>
        public static void PushIndentLevel(int indentLevel)
        {
            IndentLevelStack.Push(EditorGUI.indentLevel);
            EditorGUI.indentLevel = indentLevel;
        }

        /// <summary>
        /// Pops the indent level pushed by <see cref="PushIndentLevel(int)"/>.
        /// </summary>
        public static void PopIndentLevel()
        {
            EditorGUI.indentLevel = IndentLevelStack.Pop();
        }

        /// <summary>
        /// Pushes the content color to the stack. Remember to pop with <see cref="PopContentColor"/>.
        /// </summary>
        /// <param name="color">The content color to push..</param>
        /// <param name="blendAlpha">If set to <c>true</c> blend with alpha.</param>
        public static void PushContentColor(Color color, bool blendAlpha = false)
        {
            ContentColorStack.Push(GUI.color);

            if (blendAlpha)
            {
                color.a = color.a * GUI.contentColor.a;
            }

            GUI.contentColor = color;
        }

        /// <summary>
        /// Pops the content color pushed by <see cref="PushContentColor(Color, bool)"/>.
        /// </summary>
        public static void PopContentColor()
        {
            GUI.contentColor = ContentColorStack.Pop();
        }

        /// <summary>
        /// Pushes the label color to the stack. Remember to pop with <see cref="PopLabelColor"/>.
        /// </summary>
        /// <param name="color">The label color to push.</param>
        public static void PushLabelColor(Color color)
        {
            LabelColorStack.Push(EditorStyles.label.normal.textColor);
            EditorStyles.label.normal.textColor = color;
            SirenixGUIStyles.Foldout.normal.textColor = color;
            SirenixGUIStyles.Foldout.onNormal.textColor = color;
        }

        /// <summary>
        /// Pops the label color pushed by <see cref="PushLabelColor(Color)"/>.
        /// </summary>
        public static void PopLabelColor()
        {
            var color = LabelColorStack.Pop();
            EditorStyles.label.normal.textColor = color;
            SirenixGUIStyles.Foldout.normal.textColor = color;
            SirenixGUIStyles.Foldout.onNormal.textColor = color;
        }

        /// <summary>
        /// Pushes the GUI position offset to the stack. Remember to pop with <see cref="PopGUIPositionOffset"/>.
        /// </summary>
        /// <param name="offset">The GUI offset.</param>
        public static void PushGUIPositionOffset(Vector2 offset)
        {
            PushMatrix(GUI.matrix * Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one));
        }

        /// <summary>
        /// Pops the GUI position offset pushed by <see cref="PushGUIPositionOffset(Vector2)"/>.
        /// </summary>
        public static void PopGUIPositionOffset()
        {
            PopMatrix();
        }

        /// <summary>
        /// Pushes a GUI matrix to the stack. Remember to pop with <see cref="PopMatrix"/>.
        /// </summary>
        /// <param name="matrix">The GUI matrix to push.</param>
        public static void PushMatrix(Matrix4x4 matrix)
        {
            MatrixStack.Push(GUI.matrix);
            GUI.matrix = matrix;
        }

        /// <summary>
        /// Pops the GUI matrix pushed by <see cref="PushMatrix(Matrix4x4)"/>.
        /// </summary>
        public static void PopMatrix()
        {
            GUI.matrix = MatrixStack.Pop();
        }

        /// <summary>
        /// Ignores input on following GUI calls. Remember to end with <see cref="EndIgnoreInput"/>.
        /// </summary>
        public static void BeginIgnoreInput()
        {
            var e = Event.current.type;
            PushEventType(e == EventType.Layout || e == EventType.Repaint || e == EventType.Used ? e : EventType.Ignore);
        }

        /// <summary>
        /// Ends the ignore input started by <see cref="BeginIgnoreInput"/>.
        /// </summary>
        public static void EndIgnoreInput()
        {
            PopEventType();
        }

        /// <summary>
        /// Pushes the event type to the stack. Remember to pop with <see cref="PopEventType"/>.
        /// </summary>
        /// <param name="eventType">The type of event to push.</param>
        public static void PushEventType(EventType eventType)
        {
            EventTypeStack.Push(Event.current.type);
            Event.current.type = eventType;
        }

        /// <summary>
        /// Pops the event type pushed by <see cref="PopEventType"/>.
        /// </summary>
        public static void PopEventType()
        {
            Event.current.type = EventTypeStack.Pop();
        }

        /// <summary>
        /// Pushes the width to the editor GUI label width to the stack. Remmeber to Pop with <see cref="PopLabelWidth"/>.
        /// </summary>
        /// <param name="labelWidth">The editor GUI label width to push.</param>
        public static void PushLabelWidth(float labelWidth)
        {
            LabelWidthStack.Push(ActualLabelWidth);
            EditorGUIUtility.labelWidth = labelWidth;
        }

        /// <summary>
        /// Pops editor gui label widths pushed by <see cref="PushLabelWidth(float)"/>.
        /// </summary>
        public static void PopLabelWidth()
        {
            EditorGUIUtility.labelWidth = LabelWidthStack.Pop();
        }

        /// <summary>
        /// Pushes the value to the responsive vector component fields stack. Remeber to pop with <see cref="PopResponsiveVectorComponentFields"/>.
        /// </summary>
        public static void PushResponsiveVectorComponentFields(bool responsive)
        {
            ResponsiveVectorComponentFieldsStack.Push(SirenixEditorFields.ResponsiveVectorComponentFields);
            SirenixEditorFields.ResponsiveVectorComponentFields = responsive;
        }

        /// <summary>
        /// Pops responsive vector component fields value pushed by <see cref="PushResponsiveVectorComponentFields"/>.
        /// </summary>
        public static void PopResponsiveVectorComponentFields()
        {
            SirenixEditorFields.ResponsiveVectorComponentFields = ResponsiveVectorComponentFieldsStack.Pop();
        }

        /// <summary>
        /// Pushes the value to the fade group duration stack. Remeber to pop with <see cref="PopFadeGroupDuration"/>.
        /// </summary>
        public static void PushFadeGroupDuration(float duration)
        {
            FadeGroupDurationStack.Push(SirenixEditorGUI.DefaultFadeGroupDuration);
            SirenixEditorGUI.DefaultFadeGroupDuration = duration;
        }

        /// <summary>
        /// Pops fade group duration value pushed by <see cref="PushFadeGroupDuration"/>.
        /// </summary>
        public static void PopFadeGroupDuration()
        {
            SirenixEditorGUI.DefaultFadeGroupDuration = FadeGroupDurationStack.Pop();
        }

        /// <summary>
        /// Pushes the value to the tab page slide animation duration stack. Remember to pop with <see cref="PopTabPageSlideAnimationDuration"/>.
        /// </summary>
        /// <param name="duration"></param>
        public static void PushTabPageSlideAnimationDuration(float duration)
        {
            TabPageSlideAnimationDurationStack.Push(SirenixEditorGUI.TabPageSlideAnimationDuration);
            SirenixEditorGUI.TabPageSlideAnimationDuration = duration;
        }

        /// <summary>
        /// Pops tab page slide animation duration value pushed by <see cref="PushTabPageSlideAnimationDuration"/>.
        /// </summary>
        public static void PopTabPageSlideAnimationDuration()
        {
            SirenixEditorGUI.TabPageSlideAnimationDuration = TabPageSlideAnimationDurationStack.Pop();
        }

        /// <summary>
        /// Clears the repaint request.
        /// </summary>
        public static void ClearRepaintRequest()
        {
            RepaintRequested = false;
        }

        /// <summary>
        /// Gets a temporary value context.
        /// </summary>
        /// <typeparam name="TValue">The type of the config value.</typeparam>
        /// <param name="key">The key for the config.</param>
        /// <param name="name">The name of the config.</param>
        /// <returns>GUIConfig for the specified key and name.</returns>
        public static GUIContext<TValue> GetTemporaryContext<TValue>(object key, string name) where TValue : class, new()
        {
            var config = GUIContextCache<object, string, TValue>.GetConfig(key, name);
            if (config.HasValue == false)
            {
                config.HasValue = true;
                config.Value = new TValue();
            }
            return config;
        }

        /// <summary>
        /// Gets a temporary value context.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key for the config.</param>
        /// <param name="id">The ID for the config.</param>
        /// <returns>GUIConfig for the specified key and ID.</returns>
        public static GUIContext<TValue> GetTemporaryContext<TValue>(object key, int id) where TValue : class, new()
        {
            var config = GUIContextCache<object, int, TValue>.GetConfig(key, id);
            if (config.HasValue == false)
            {
                config.HasValue = true;
                config.Value = new TValue();
            }
            return config;
        }

        /// <summary>
        /// Gets a temporary value context.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="primaryKey">The primary key.</param>
        /// <param name="secondaryKey">The secondary key.</param>
        /// <returns>GUIConfig for the specified primary and secondary key.</returns>
        public static GUIContext<TValue> GetTemporaryContext<TValue>(object primaryKey, object secondaryKey) where TValue : class, new()
        {
            var config = GUIContextCache<object, object, TValue>.GetConfig(primaryKey, secondaryKey);
            if (config.HasValue == false)
            {
                config.HasValue = true;
                config.Value = new TValue();
            }
            return config;
        }

        /// <summary>
        /// Gets a temporary value context.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key for the context.</param>
        /// <returns>GUIConfig for the specified key.</returns>
        public static GUIContext<TValue> GetTemporaryContext<TValue>(object key) where TValue : class, new()
        {
            var config = GUIContextCache<object, object, TValue>.GetConfig(defaultConfigKey, key);
            if (config.HasValue == false)
            {
                config.HasValue = true;
                config.Value = new TValue();
            }
            return config;
        }

        /// <summary>
        /// Gets a temporary nullable value context.
        /// </summary>
		/// <param name="key">Key for context.</param>
		/// <param name="name">Name for the context.</param>
        public static GUIContext<TValue> GetTemporaryNullableContext<TValue>(object key, string name) where TValue : class
        {
            return GUIContextCache<object, string, TValue>.GetConfig(key, name);
        }

        /// <summary>
        /// Gets a temporary nullable value context.
        /// </summary>
		/// <param name="key">Key for context.</param>
		/// <param name="id">Id of the context.</param>
        public static GUIContext<TValue> GetTemporaryNullableContext<TValue>(object key, int id) where TValue : class
        {
            return GUIContextCache<object, int, TValue>.GetConfig(key, id);
        }

        /// <summary>
        /// Gets a temporary nullable value context.
        /// </summary>
		/// <param name="primaryKey">Primary key for the context.</param>
		/// <param name="secondaryKey">Secondary key for the context.</param>
        public static GUIContext<TValue> GetTemporaryNullableContext<TValue>(object primaryKey, object secondaryKey) where TValue : class
        {
            return GUIContextCache<object, object, TValue>.GetConfig(primaryKey, secondaryKey);
        }

        /// <summary>
        /// Gets a temporary nullable value context.
        /// </summary>
		/// <param name="key">Key for the context.</param>
        public static GUIContext<TValue> GetTemporaryNullableContext<TValue>(object key) where TValue : class
        {
            return GUIContextCache<object, object, TValue>.GetConfig(defaultConfigKey, key);
        }

        /// <summary>
        /// Gets a temporary context.
        /// </summary>
		/// <param name="key">Key for the context.</param>
		/// <param name="name">Name for the context.</param>
		/// <param name="defaultValue">Default value of the context.</param>
        public static GUIContext<TValue> GetTemporaryContext<TValue>(object key, string name, TValue defaultValue) where TValue : struct
        {
            var config = GUIContextCache<object, string, TValue>.GetConfig(key, name);
            if (config.HasValue == false)
            {
                config.HasValue = true;
                config.Value = defaultValue;
            }
            return config;
        }

        /// <summary>
        /// Gets a temporary context.
        /// </summary>
        /// <param name="key">Key for the context.</param>
        /// <param name="id">Id for the context.</param>
        /// <param name="defaultValue">Default value of the context.</param>
        public static GUIContext<TValue> GetTemporaryContext<TValue>(object key, int id, TValue defaultValue) where TValue : struct
        {
            var config = GUIContextCache<object, int, TValue>.GetConfig(key, id);
            if (config.HasValue == false)
            {
                config.HasValue = true;
                config.Value = defaultValue;
            }
            return config;
        }

        /// <summary>
        /// Gets a temporary context.
        /// </summary>
        /// <param name="primaryKey">Primary key for the context.</param>
        /// <param name="secondaryKey">Secondary key for the context.</param>
        /// <param name="defaultValue">Default value of the context.</param>
        public static GUIContext<TValue> GetTemporaryContext<TValue>(object primaryKey, object secondaryKey, TValue defaultValue) where TValue : struct
        {
            var config = GUIContextCache<object, object, TValue>.GetConfig(primaryKey, secondaryKey);
            if (config.HasValue == false)
            {
                config.HasValue = true;
                config.Value = defaultValue;
            }
            return config;
        }

        /// <summary>
        /// Gets a temporary context.
        /// </summary>
        /// <param name="key">Key for the context.</param>
        /// <param name="defaultValue">Default value of the context.</param>
        public static GUIContext<TValue> GetTemporaryContext<TValue>(object key, TValue defaultValue) where TValue : struct
        {
            var config = GUIContextCache<object, object, TValue>.GetConfig(defaultValue, key);
            if (config.HasValue == false)
            {
                config.HasValue = true;
                config.Value = defaultValue;
            }
            return config;
        }

        /// <summary>
        /// Gets a temporary GUIContent with the specified text.
        /// </summary>
        /// <param name="t">The text for the GUIContent.</param>
        /// <returns>Temporary GUIContent instance.</returns>
        public static GUIContent TempContent(string t)
        {
            tmpContent.image = null;
            tmpContent.text = t;
            tmpContent.tooltip = null;
            return tmpContent;
        }

        /// <summary>
        /// Gets a temporary GUIContent with the specified text and tooltip.
        /// </summary>
        /// <param name="t">The text for the GUIContent.</param>
        /// <param name="tooltip">The tooltip for the GUIContent.</param>
        /// <returns>Temporary GUIContent instance.</returns>
        public static GUIContent TempContent(string t, string tooltip)
        {
            tmpContent.image = null;
            tmpContent.text = t;
            tmpContent.tooltip = tooltip;
            return tmpContent;
        }

        /// <summary>
        /// Gets a temporary GUIContent with the specified image and tooltip.
        /// </summary>
        /// <param name="image">The image for the GUIContent.</param>
        /// <param name="tooltip">The tooltip for the GUIContent.</param>
        /// <returns>Temporary GUIContent instance.</returns>
        public static GUIContent TempContent(Texture image, string tooltip = null)
        {
            tmpContent.image = image;
            tmpContent.text = null;
            tmpContent.tooltip = tooltip;
            return tmpContent;
        }

        /// <summary>
        /// Gets a temporary GUIContent with the specified text, image and tooltip.
        /// </summary>
        /// <param name="text">The text for the GUIContent.</param>
        /// <param name="image">The image for the GUIContent.</param>
        /// <param name="tooltip">The tooltip for the GUIContent.</param>
        /// <returns>Temporary GUIContent instance.</returns>
        public static GUIContent TempContent(string text, Texture image, string tooltip = null)
        {
            tmpContent.image = image;
            tmpContent.text = text;
            tmpContent.tooltip = tooltip;
            return tmpContent;
        }

        /// <summary>
        /// Indents the rect by the current indent amount.
        /// </summary>
        /// <param name="rect">The rect to indent.</param>
        /// <returns>Indented rect.</returns>
        public static Rect IndentRect(Rect rect)
        {
            var indent = CurrentIndentAmount;
            rect.x += indent;
            rect.width -= indent;
            return rect;
        }

        /// <summary>
        /// Indents the rect by the current indent amount.
        /// </summary>
        /// <param name="rect">The rect to indent.</param>
        public static void IndentRect(ref Rect rect)
        {
            var indent = CurrentIndentAmount;
            rect.x += indent;
            rect.width -= indent;
        }

        /// <summary>
        /// Repaints the EditorWindow if a repaint has been requested.
        /// </summary>
        /// <param name="window">The window to repaint.</param>
        public static void RepaintIfRequested(this EditorWindow window)
        {
            if (GUIHelper.RepaintRequested)
            {
                EditorTimeHelper.Time.Update();
                window.Repaint();
                if (Event.current.type == EventType.Repaint)
                {
                    EditorTimeHelper.RemoveOldWindowTimeHelper();
                    GUIHelper.ClearRepaintRequest();
                }
            }
        }

        /// <summary>
        /// Repaints the editor if a repaint has been requested. If the currently rendering window is not an InspectorWindow, Repaint() will be called on the current window as well.
        /// </summary>
        /// <param name="editor">The editor to repaint.</param>
        public static void RepaintIfRequested(this Editor editor)
        {
            EditorTimeHelper.Time.Update();
            if (GUIHelper.RepaintRequested)
            {
                editor.Repaint();

                if (CurrentWindow != null && CurrentWindow.GetType() != inspectorWindowType)
                {
                    CurrentWindow.Repaint();
                }

                if (Event.current.type == EventType.Repaint)
                {
                    EditorTimeHelper.RemoveOldWindowTimeHelper();
                    GUIHelper.ClearRepaintRequest();
                }
            }
        }

        /// <summary>
        /// Gets the a thumbnail icon for the given object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <param name="preferObjectPreviewOverFileIcon"></param>
        /// <returns></returns>
        public static Texture2D GetAssetThumbnail(UnityEngine.Object obj, Type type, bool preferObjectPreviewOverFileIcon)
        {
            Texture2D icon = null;

            if (preferObjectPreviewOverFileIcon)
            {
                icon = AssetPreview.GetAssetPreview(obj);
            }

            if (icon == null)
            {
                var assetPath = AssetDatabase.GetAssetPath(obj);
                if (assetPath != null)
                {
                    icon = InternalEditorUtility.GetIconForFile(assetPath);
                }
            }

            if (icon == null || icon == EditorGUIUtility.FindTexture("DefaultAsset Icon"))
            {
                icon = AssetPreview.GetMiniThumbnail(obj);
            }

            return icon;
        }

        private struct LayoutMeasureInfo
        {
            public int CursorIndex;
            public Rect Rect;
        }
    }
}
#endif