#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinEditorWindow.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using UnityEditor;
    using UnityEngine;
    using Sirenix.Serialization;
    using Sirenix.Utilities.Editor;
    using System;
    using Sirenix.Utilities;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    /// Base class for creating editor windows using Odin.
    /// </summary>
    /// <example>
    /// <code>
    /// public class SomeWindow : OdinEditorWindow
    /// {
    ///     [MenuItem("My Game/Some Window")]
    ///     private static void OpenWindow()
    ///     {
    ///         GetWindow&lt;SomeWindow&gt;().Show();
    ///     }
    ///
    ///     [Button(ButtonSizes.Large)]
    ///     public void SomeButton() { }
    ///
    ///     [TableList]
    ///     public SomeType[] SomeTableData;
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// public class DrawSomeSingletonInAnEditorWindow : OdinEditorWindow
    /// {
    ///     [MenuItem("My Game/Some Window")]
    ///     private static void OpenWindow()
    ///     {
    ///         GetWindow&lt;DrawSomeSingletonInAnEditorWindow&gt;().Show();
    ///     }
    ///
    ///     protected override object GetTarget()
    ///     {
    ///         return MySingleton.Instance;
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// private void InspectObjectInWindow()
    /// {
    ///     OdinEditorWindow.InspectObject(someObject);
    /// }
    /// 
    /// private void InspectObjectInDropDownWithAutoHeight()
    /// {
    ///     var btnRect = GUIHelper.GetCurrentLayoutRect();
    ///     OdinEditorWindow.InspectObjectInDropDown(someObject, btnRect, btnRect.width);
    /// }
    /// 
    /// private void InspectObjectInDropDown()
    /// {
    ///     var btnRect = GUIHelper.GetCurrentLayoutRect();
    ///     OdinEditorWindow.InspectObjectInDropDown(someObject, btnRect, new Vector2(btnRect.width, 100));
    /// }
    /// 
    /// private void InspectObjectInACenteredWindow()
    /// {
    ///     var window = OdinEditorWindow.InspectObject(someObject);
    ///     window.position = GUIHelper.GetEditorWindowRect().AlignCenter(270, 200);
    /// }
    /// 
    /// private void OtherStuffYouCanDo()
    /// {
    ///     var window = OdinEditorWindow.InspectObject(this.someObject);
    /// 
    ///     window.position = GUIHelper.GetEditorWindowRect().AlignCenter(270, 200);
    ///     window.titleContent = new GUIContent("Custom title", EditorIcons.RulerRect.Active);
    ///     window.OnClose += () => Debug.Log("Window Closed");
    ///     window.OnBeginGUI += () => GUILayout.Label("-----------");
    ///     window.OnEndGUI += () => GUILayout.Label("-----------");
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="OdinMenuEditorWindow"/>
    [ShowOdinSerializedPropertiesInInspector]
    public class OdinEditorWindow : EditorWindow, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Occurs when the window is closed.
        /// </summary>
        public event Action OnClose;

        /// <summary>
        /// Occurs at the beginning the OnGUI method.
        /// </summary>
        public event Action OnBeginGUI;

        /// <summary>
        /// Occurs at the end the OnGUI method.
        /// </summary>
        public event Action OnEndGUI;

        private static bool hasUpdatedOdinEditors = false;
        private static int inspectObjectWindowCount = 3;

        [SerializeField, HideInInspector]
        private SerializationData serializationData;

        [SerializeField, HideInInspector]
        private UnityEngine.Object inspectorTargetSerialized;

        [SerializeField, HideInInspector]
        private float labelWidth = 0.33f;

        [NonSerialized]
        private object inspectTargetObject;

        [SerializeField, HideInInspector]
        private Vector4 windowPadding = new Vector4(4, 4, 4, 4);

        [SerializeField, HideInInspector]
        private bool useScrollView = true;

        [SerializeField, HideInInspector]
        private bool drawUnityEditorPreview;

        [NonSerialized]
        private bool isInitialized;
        private GUIStyle marginStyle;
        private object[] currentTargets = new object[0];
        private ImmutableList<object> currentTargetsImm;
        private Editor[] editors = new Editor[0];
        private PropertyTree[] propertyTrees = new PropertyTree[0];
        private Vector2 scrollPos;
        private int mouseDownId;
        private EditorWindow mouseDownWindow;
        private int mouseDownKeyboardControl;
        private Rect contentRect;
        private float defaultEditorPreviewHeight = 170;


        /// <summary>
        /// Gets the label width to be used. Values between 0 and 1 are treated as percentages, and values above as pixels.
        /// </summary>
        public virtual float DefaultLabelWidth
        {
            get { return this.labelWidth; }
            set { this.labelWidth = value; }
        }

        /// <summary>
        /// Gets or sets the window padding. x = left, y = right, z = top, w = bottom.
        /// </summary>
        public virtual Vector4 WindowPadding
        {
            get { return this.windowPadding; }
            set { this.windowPadding = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the window should draw a scroll view.
        /// </summary>
        public virtual bool UseScrollView
        {
            get { return this.useScrollView; }
            set { this.useScrollView = true; }
        }

        /// <summary>
        /// Gets a value indicating whether the window should draw a Unity editor preview, if possible.
        /// </summary>
        public virtual bool DrawUnityEditorPreview
        {
            get { return this.drawUnityEditorPreview; }
            set { this.drawUnityEditorPreview = value; }
        }

        /// <summary>
        /// Gets the default preview height for Unity editors.
        /// </summary>
        public virtual float DefaultEditorPreviewHeight
        {
            get { return this.defaultEditorPreviewHeight; }
            set { this.defaultEditorPreviewHeight = value; }
        }
        /// <summary>
        /// Gets the target which which the window is supposed to draw. By default it simply returns the editor window instance itself. By default, this method is called by <see cref="GetTargets"/>().
        /// </summary>
        protected virtual object GetTarget()
        {
            if (this.inspectTargetObject != null)
            {
                return this.inspectTargetObject;
            }

            if (this.inspectorTargetSerialized != null)
            {
                return this.inspectorTargetSerialized;
            }

            return this;
        }

        /// <summary>
        /// Gets the targets to be drawn by the editor window. By default this simply yield returns the <see cref="GetTarget"/> method.
        /// </summary>
        protected virtual IEnumerable<object> GetTargets()
        {
            yield return this.GetTarget();
        }

        /// <summary>
        /// At the start of each OnGUI event when in the Layout event, the GetTargets() method is called and cached into a list which you can access from here.
        /// </summary>
        protected ImmutableList<object> CurrentDrawingTargets
        {
            get { return this.currentTargetsImm; }
        }

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus. 
        /// This particular overload uses a few frames to calculate the height of the content before showing the window with a height that matches its content.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="btnRect">The BTN rect.</param>
        /// <param name="windowWidth">Width of the window.</param>
        /// <returns></returns>
        public static OdinEditorWindow InspectObjectInDropDown(object obj, Rect btnRect, float windowWidth)
        {
            return InspectObjectInDropDown(obj, btnRect, new Vector2(windowWidth, 0));
        }

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus. 
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static OdinEditorWindow InspectObjectInDropDown(object obj, Rect btnRect, Vector2 windowSize)
        {
            var curr = GUIHelper.CurrentWindow;
            var window = CreateOdinEditorWindowInstanceForObject(obj);
            window.OnBeginGUI += () => curr.Repaint();  // Also repaint parent window, when the drop down repaints.
            btnRect.position = GUIUtility.GUIToScreenPoint(btnRect.position);
            window.labelWidth = 0.33f;
            window.DrawUnityEditorPreview = true;

            // If the height is 0, then we use a couple of frames to calculate it.
            if (windowSize.y == 0)
            {
                windowSize.y = 400;
                // Render first couple of frames outside of the screen while we calculate the height.
                window.ShowAsDropDown(btnRect, windowSize);
                window.position = new Rect(-9999, -9999, windowSize.x, windowSize.y);

                var isResized = false;
                var numberOfFramesRendered = 0;
                window.OnBeginGUI += () =>
                {
                    if (!isResized)
                    {
                        if (Event.current.type == EventType.Repaint)
                        {
                            numberOfFramesRendered++;
                            GUIHelper.RequestRepaint();
                            window.Repaint();
                        }

                        if (numberOfFramesRendered > 2 && (int)window.contentRect.height > 0)
                        {
                            window.ShowAsDropDown(btnRect, new Vector2(windowSize.x, (int)window.contentRect.height));
                            isResized = true;
                        }
                    }
                };
            }
            else
            {
                window.ShowAsDropDown(btnRect, windowSize);
            }

            return window;
        }

        /// <summary>
        /// Pops up an editor window for the given object.
        /// </summary>
        public static OdinEditorWindow InspectObject(object obj)
        {
            var window = CreateOdinEditorWindowInstanceForObject(obj);
            window.Show();

            var offset = new Vector2(30, 30) * ((inspectObjectWindowCount++ % 6) - 3);
            window.position = GUIHelper.GetEditorWindowRect()
                .AlignCenter(400, 300)
                .AddPosition(offset);

            return window;
        }

        /// <summary>
        /// Creates an editor window instance for the specified object, without opening the window.
        /// </summary>
        public static OdinEditorWindow CreateOdinEditorWindowInstanceForObject(object obj)
        {
            var window = CreateInstance<OdinEditorWindow>();

            var uObj = obj as UnityEngine.Object;
            if (uObj)
            {
                // If it's a Unity object, then it's likely the reference can survive a recompile.
                window.inspectorTargetSerialized = uObj;
            }
            else
            {
                // Otherwise, it can't. In which case we don't want want to serialize it - hence inspectorTargetObject and not inspectorTargetSerialized.
                // If we did the user would be inspecting a different reference than provided.
                window.inspectTargetObject = obj;
            }

            if (uObj as Component)
            {
                window.titleContent = new GUIContent((uObj as Component).gameObject.name);
            }
            else if (uObj)
            {
                window.titleContent = new GUIContent(uObj.name);
            }
            else
            {
                window.titleContent = new GUIContent(obj.ToString());
            }

            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(600, 600);

            EditorUtility.SetDirty(window);
            return window;
        }

        /// <summary>
        /// The Odin property tree drawn.
        /// </summary>
        [Obsolete("Support for non Odin drawn editors and drawing of multiple editors has been added, so there is no longer any guarantee that there will be a PropertyTree.")]
        public PropertyTree PropertyTree
        {
            get { return this.propertyTrees == null ? null : this.propertyTrees.FirstOrDefault(); }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData);
            this.OnAfterDeserialize();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData);
            this.OnBeforeSerialize();
        }

        /// <summary>
        /// Draws the Odin Editor Window.
        /// </summary>
        protected virtual void OnGUI()
        {
            if (this.OnBeginGUI != null)
            {
                this.OnBeginGUI();
            }

            // Editor windows, can be created before Odin assigns OdinEditors to all relevent types via reflection.
            // This ensures that that happens before we render anything.
            if (!hasUpdatedOdinEditors)
            {
                InspectorConfig.Instance.UpdateOdinEditors();
                hasUpdatedOdinEditors = true;
            }

            this.Initialize();

            this.marginStyle = this.marginStyle ?? new GUIStyle() { padding = new RectOffset() };

            if (Event.current.type == EventType.Layout)
            {
                this.marginStyle.padding.left = (int)this.WindowPadding.x;
                this.marginStyle.padding.right = (int)this.WindowPadding.y;
                this.marginStyle.padding.top = (int)this.WindowPadding.z;
                this.marginStyle.padding.bottom = (int)this.WindowPadding.w;

                // Creates the editors.
                UpdateEditors();
            }

            // Removes focus from text-fields when clicking on an empty area.
            var prevType = Event.current.type;
            if (Event.current.type == EventType.MouseDown)
            {
                this.mouseDownId = GUIUtility.hotControl;
                this.mouseDownKeyboardControl = GUIUtility.keyboardControl;
                this.mouseDownWindow = focusedWindow;
            }

            // Draws the editors.
            bool useScrollWheel = this.UseScrollView;
            if (useScrollWheel)
            {
                this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);
            }

            // Update the content rect
            var r = EditorGUILayout.BeginVertical();
            if (Event.current.type == EventType.Repaint)
            {
                this.contentRect = r;
            }

            // Draw the GUI
            {
                GUIHelper.PushHierarchyMode(false);

                float labelWidth;
                if (this.DefaultLabelWidth < 1)
                {
                    labelWidth = this.contentRect.width * this.DefaultLabelWidth;
                }
                else
                {
                    labelWidth = this.DefaultLabelWidth;
                }

                GUIHelper.PushLabelWidth(labelWidth);
                GUILayout.BeginVertical(this.marginStyle);
                this.OnBeginDrawEditors();

                for (int i = 0; i < this.currentTargets.Length; i++)
                {
                    this.DrawEditor(i);
                }

                this.OnEndDrawEditors();
                GUILayout.EndVertical();
                GUIHelper.PopLabelWidth();
                GUIHelper.PopHierarchyMode();
            }
            EditorGUILayout.EndVertical();

            if (useScrollWheel)
            {
                EditorGUILayout.EndScrollView();
            }

            // This removes focus from text-fields when clicking on an empty area.
            if (Event.current.type != prevType) this.mouseDownId = -2;
            if (Event.current.type == EventType.MouseUp && GUIUtility.hotControl == this.mouseDownId && focusedWindow == this.mouseDownWindow && GUIUtility.keyboardControl == this.mouseDownKeyboardControl)
            {
                GUIHelper.RemoveFocusControl();
            }

            if (this.OnEndGUI != null)
            {
                this.OnEndGUI();
            }

            if (Event.current.isMouse || Event.current.type == EventType.Used)
            {
                this.Repaint();
            }

            if (this.currentTargets.Length == 0)
            {
                // TODO: Find out why the window doesn't repaint properly when this is 0. And then remove this if statement.
                // Try navigating a menu tree with the keyboard filled menu items with nothing to inspect.
                // It only updates when you start moving the mouse.
                this.Repaint();
            }

            this.RepaintIfRequested();
        }

        private void UpdateEditors()
        {
            this.currentTargets = this.currentTargets ?? new object[] { };
            this.editors = this.editors ?? new Editor[] { };
            this.propertyTrees = this.propertyTrees ?? new PropertyTree[] { };

            IList<object> newTargets = this.GetTargets().ToArray() ?? new object[0];

            if (this.currentTargets.Length != newTargets.Count)
            {
                Array.Resize(ref this.currentTargets, newTargets.Count);
                Array.Resize(ref this.editors, newTargets.Count);
                Array.Resize(ref this.propertyTrees, newTargets.Count);
                this.Repaint();
            }

            for (int i = 0; i < newTargets.Count; i++)
            {
                var newTarget = newTargets[i];
                var curTarget = this.currentTargets[i];
                if (!object.ReferenceEquals(newTarget, curTarget))
                {
                    GUIHelper.RequestRepaint();
                    this.currentTargets[i] = newTarget;

                    if (newTarget == null)
                    {
                        this.propertyTrees[i] = null;
                        this.editors[i] = null;
                    }
                    else
                    {
                        var editorWindow = newTarget as EditorWindow;
                        if (newTarget.GetType().InheritsFrom<UnityEngine.Object>() && !editorWindow)
                        {
                            var unityObject = newTarget as UnityEngine.Object;
                            if (unityObject)
                            {
                                this.propertyTrees[i] = null;
                                this.editors[i] = Editor.CreateEditor(unityObject);
                                var materialEditor = this.editors[i] as MaterialEditor;
                                if (materialEditor != null)
                                {
                                    typeof(MaterialEditor).GetProperty("forceVisible", Flags.AllMembers).SetValue(materialEditor, true, null);
                                }
                            }
                            else
                            {
                                this.propertyTrees[i] = null;
                                this.editors[i] = null;
                            }
                        }
                        else
                        {
                            this.editors[i] = null;
                            this.propertyTrees[i] = PropertyTree.Create(newTarget);
                        }
                    }
                }
            }

            this.currentTargetsImm = new ImmutableList<object>(this.currentTargets);
        }

        private void Initialize()
        {
            if (!isInitialized)
            {
                // Lets give it a better default name.
                if (this.titleContent != null && this.titleContent.text == this.GetType().FullName)
                {
                    this.titleContent.text = this.GetType().GetNiceName().SplitPascalCase();
                }

                // Mouse move please
                this.wantsMouseMove = true;
                Selection.selectionChanged -= this.SelectionChanged;
                Selection.selectionChanged += this.SelectionChanged;
                this.isInitialized = true;

                this.OnInitialize();
            }
        }

        private void SelectionChanged()
        {
            this.Repaint();
        }

        /// <summary>
        /// Called when the window is enabled. Remember to call base.OnEnable();
        /// </summary>
        protected virtual void OnEnable()
        {
            this.Initialize();
        }

        /// <summary>
        /// Draws the editor for the this.CurrentDrawingTargets[index].
        /// </summary>
        protected virtual void DrawEditor(int index)
        {
            var tmpPropertyTree = this.propertyTrees[index];
            var tmpEditor = this.editors[index];

            if (tmpPropertyTree != null || (tmpEditor != null && tmpEditor.target != null))
            {
                if (tmpPropertyTree != null)
                {
                    bool withUndo = tmpPropertyTree.WeakTargets.FirstOrDefault() as UnityEngine.Object;
                    tmpPropertyTree.Draw(withUndo);
                }
                else
                {
                    OdinEditor.ForceHideMonoScriptInEditor = true;
                    try
                    {
                        tmpEditor.OnInspectorGUI();
                    }
                    finally
                    {
                        OdinEditor.ForceHideMonoScriptInEditor = false;
                    }
                }
            }

            if (this.DrawUnityEditorPreview)
            {
                this.DrawEditorPreview(index, this.defaultEditorPreviewHeight);
            }
        }

        /// <summary>
        /// Uses the <see cref="UnityEditor.Editor.DrawPreview(Rect)"/> method to draw a preview for the this.CurrentDrawingTargets[index].
        /// </summary>
        protected virtual void DrawEditorPreview(int index, float height)
        {
            Editor editor = this.editors[index];

            if (editor != null && editor.HasPreviewGUI())
            {
                Rect rect = EditorGUILayout.GetControlRect(false, height);
                editor.DrawPreview(rect);
            }
        }

        /// <summary>
        /// Called when the window is destroyed. Remember to call base.OnDestroy();
        /// </summary>
        protected virtual void OnDestroy()
        {
            Selection.selectionChanged -= this.SelectionChanged;
            Selection.selectionChanged -= this.SelectionChanged;

            if (this.OnClose != null)
            {
                this.OnClose();
            }
        }

        protected virtual void OnInitialize()
        {

        }

        /// <summary>
        /// Called before starting to draw all editors for the <see cref="CurrentDrawingTargets"/>.
        /// </summary>
        protected virtual void OnEndDrawEditors()
        {
        }

        /// <summary>
        /// Called after all editors for the <see cref="CurrentDrawingTargets"/> has been drawn.
        /// </summary>
        protected virtual void OnBeginDrawEditors()
        {
        }

        /// <summary>
        /// See ISerializationCallbackReceiver.OnBeforeSerialize for documentation on how to use this method.
        /// </summary>
        protected virtual void OnAfterDeserialize()
        {
        }

        /// <summary>
        /// Implement this method to receive a callback after unity serialized your object.
        /// </summary>
        protected virtual void OnBeforeSerialize()
        {
        }
    }
}
#endif