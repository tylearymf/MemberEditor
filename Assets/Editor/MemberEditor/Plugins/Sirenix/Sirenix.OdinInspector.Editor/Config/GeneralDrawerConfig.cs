#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GeneralDrawerConfig.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// <para>Contains general configuration for all Odin drawers.</para>
    /// <para>
    /// You can modify the configuration in the Odin Preferences window found in 'Tools -> Odin Inspector -> Preferences -> Drawers -> General',
    /// or by locating the configuration file stored as a serialized object in the Sirenix folder under 'Odin Inspector/Config/Editor/GeneralDrawerConfig'.
    /// </para>
    /// </summary>
    [InitializeOnLoad]
    [SirenixEditorConfig]
    public class GeneralDrawerConfig : GlobalConfig<GeneralDrawerConfig>
    {
        [HideInInspector, SerializeField, FormerlySerializedAs("ShowMonoScriptInEditor")] private bool showMonoScriptInEditor = true;
        [HideInInspector, SerializeField, FormerlySerializedAs("ExpandFoldoutByDefault")] private bool expandFoldoutByDefault = true;
        [HideInInspector, SerializeField, FormerlySerializedAs("HideFoldoutWhileEmpty")] private bool hideFoldoutWhileEmpty = true;
        [HideInInspector, SerializeField, FormerlySerializedAs("OpenListsByDefault")] private bool openListsByDefault = true;
        [HideInInspector, SerializeField, FormerlySerializedAs("ShowItemCount")] private bool showItemCount = true;
        [HideInInspector, SerializeField, FormerlySerializedAs("NumberOfItemsPrPage")] private int numberOfItemsPrPage = 15;
        [HideInInspector, SerializeField, FormerlySerializedAs("HidePagingWhileCollapsed")] private bool hidePagingWhileCollapsed = true;
        [HideInInspector, SerializeField, FormerlySerializedAs("HidePagingWhileOnlyOnePage")] private bool hidePagingWhileOnlyOnePage = true;
        [HideInInspector, SerializeField, FormerlySerializedAs("ShowExpandButton")] private bool showExpandButton = true;
        [HideInInspector, SerializeField, FormerlySerializedAs("QuaternionDrawMode")] private QuaternionDrawMode quaternionDrawMode = QuaternionDrawMode.Eulers;
        [HideInInspector, SerializeField] private bool showPrefabModificationsDisabledMessage = true;
        [HideInInspector, SerializeField] private int maxRecursiveDrawDepth = 10;
        [HideInInspector, SerializeField] private bool showIndexLabels;
        [HideInInspector, SerializeField] private float squareUnityObjectFieldHeight = 50;
        [HideInInspector, SerializeField] private ObjectFieldAlignment squareUnityObjectAlignment = ObjectFieldAlignment.Right;
        private bool prefsAreLoaded;

        private void LoadEditorPrefs()
        {
            if (this.prefsAreLoaded)
            {
                return;
            }

            this.prefsAreLoaded = true;

            this.showIndexLabels = EditorPrefs.GetBool("GeneralDrawerConfig.ShowIndexLabels", false);
            this.showMonoScriptInEditor = EditorPrefs.GetBool("GeneralDrawerConfig.ShowMonoScriptInEditor", this.showMonoScriptInEditor);
            this.showPrefabModificationsDisabledMessage = EditorPrefs.GetBool("GeneralDrawerConfig.ShowPrefabModificationsDisabledMessage", this.showPrefabModificationsDisabledMessage);
            this.expandFoldoutByDefault = EditorPrefs.GetBool("GeneralDrawerConfig.ExpandFoldoutByDefault", this.expandFoldoutByDefault);
            this.hideFoldoutWhileEmpty = EditorPrefs.GetBool("GeneralDrawerConfig.HideFoldoutWhileEmpty", this.hideFoldoutWhileEmpty);
            this.openListsByDefault = EditorPrefs.GetBool("GeneralDrawerConfig.OpenListsByDefault", this.openListsByDefault);
            this.showItemCount = EditorPrefs.GetBool("GeneralDrawerConfig.ShowItemCount", this.showItemCount);
            this.numberOfItemsPrPage = EditorPrefs.GetInt("GeneralDrawerConfig.NumberOfItemsPrPage", this.numberOfItemsPrPage);
            this.hidePagingWhileCollapsed = EditorPrefs.GetBool("GeneralDrawerConfig.HidePagingWhileCollapsed", this.hidePagingWhileCollapsed);
            this.hidePagingWhileOnlyOnePage = EditorPrefs.GetBool("GeneralDrawerConfig.HidePagingWhileOnlyOnePage", this.hidePagingWhileOnlyOnePage);
            this.showExpandButton = EditorPrefs.GetBool("GeneralDrawerConfig.ShowExpandButton", this.showExpandButton);
            this.quaternionDrawMode = (QuaternionDrawMode)EditorPrefs.GetInt("GeneralDrawerConfig.QuaternionDrawMode", (int)this.quaternionDrawMode);
            this.maxRecursiveDrawDepth = EditorPrefs.GetInt("GeneralDrawerConfig.MaxRecursiveDrawDepth", this.maxRecursiveDrawDepth);
            this.squareUnityObjectFieldHeight = EditorPrefs.GetFloat("GeneralDrawerConfig.squareUnityObjectFieldHeight", this.squareUnityObjectFieldHeight);
            this.squareUnityObjectAlignment = (ObjectFieldAlignment)EditorPrefs.GetInt("GeneralDrawerConfig.squareUnityObjectAlignment", (int)this.squareUnityObjectAlignment);
            this.squareUnityObjectEnableFor = (UnityObjectType)EditorPrefs.GetInt("GeneralDrawerConfig.squareUnityObjectEnableFor", (int)this.squareUnityObjectEnableFor);
        }

        #region General

        /// <summary>
        /// Specify whether or not the script selector above components should be drawn.
        /// </summary>
        [FoldoutGroup("General")]
        [ShowInInspector]
        [PropertyTooltip("Specify whether or not the script selector above components should be drawn")]
        public bool ShowMonoScriptInEditor
        {
            get { return this.showMonoScriptInEditor; }
            set
            {
                this.showMonoScriptInEditor = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.ShowMonoScriptInEditor", value);
            }
        }

        /// <summary>
        /// Specify whether or not the warning for properties that do not support prefab modifications should be shown in the inspector.
        /// </summary>
        [FoldoutGroup("General")]
        [ShowInInspector]
        [PropertyTooltip("Specify whether or not the warning for properties that do not support prefab modifications should be shown in the inspector")]
        public bool ShowPrefabModificationsDisabledMessage
        {
            get { return this.showPrefabModificationsDisabledMessage; }
            set
            {
                this.showPrefabModificationsDisabledMessage = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.ShowPrefabModificationsDisabledMessage", value);
            }
        }

        /// <summary>
        /// Specifies the maximum depth to which a property can draw itself recursively before the system refuses to draw it any deeper.
        /// </summary>
        [FoldoutGroup("General")]
        [ShowInInspector]
        [PropertyTooltip("Specifies the maximum depth to which a property can draw itself recursively before the system refuses to draw it any deeper.")]
        [MinValue(1)]
        [MaxValue(100)]
        public int MaxRecursiveDrawDepth
        {
            get { return Mathf.Clamp(this.maxRecursiveDrawDepth, 1, 100); }
            set
            {
                value = Mathf.Clamp(value, 1, 100);
                this.maxRecursiveDrawDepth = value;
                EditorPrefs.SetInt("GeneralDrawerConfig.MaxRecursiveDrawDepth", value);
            }
        }

        /// <summary>
        /// If set to true, most foldouts throughout the inspector will be expanded by default.
        /// </summary>
        [FoldoutGroup("General")]
        [ShowInInspector]
        [PropertyTooltip("If set to true, most foldouts throughout the inspector will be expanded by default.")]
        public bool ExpandFoldoutByDefault
        {
            get { return this.expandFoldoutByDefault; }
            set
            {
                this.expandFoldoutByDefault = value;
                SirenixEditorGUI.ExpandFoldoutByDefault = value;
                EditorPrefs.SetBool("SirenixEditorGUI.ExpandFoldoutByDefault", value);
            }
        }

        #endregion

        #region Animations
        /// <summary>
        /// Specify the animation speed for most foldouts throughout the inspector.
        /// </summary>
        [FoldoutGroup("Animations")]
        [ShowInInspector]
        [PropertyRange(0.001f, 4f)]
        [PropertyTooltip("Specify the animation speed for most foldouts throughout the inspector.")]
        public float GUIFoldoutAnimationDuration
        {
            get { return SirenixEditorGUI.DefaultFadeGroupDuration; }
            set { SirenixEditorGUI.DefaultFadeGroupDuration = value; }
        }

        /// <summary>
        /// Specify the shaking duration for most shaking animations throughout the inspector.
        /// </summary>
        [FoldoutGroup("Animations")]
        [PropertyTooltip("Specify the shaking duration for most shaking animations throughout the inspector.")]
        [PropertyRange(0f, 4f)]
        public float ShakingAnimationDuration
        {
            get { return SirenixEditorGUI.ShakingAnimationDuration; }
            set { SirenixEditorGUI.ShakingAnimationDuration = value; }
        }

        /// <summary>
        /// Specify the animation speed for <see cref="Sirenix.OdinInspector.TabGroupAttribute"/>
        /// </summary>
        [FoldoutGroup("Animations")]
        [PropertyRange(0.001f, 4f)]
        public float TabPageSlideAnimationDuration
        {
            get { return SirenixEditorGUI.TabPageSlideAnimationDuration; }
            set { SirenixEditorGUI.TabPageSlideAnimationDuration = value; }
        }
        #endregion

        #region Structs

        /// <summary>
        /// When <c>true</c> the component labels, for vector fields, will be hidden when the field is too narrow.
        /// </summary>
        [FoldoutGroup("Structs")]
        [ShowInInspector, PropertyTooltip("When on the component labels, for vector fields, will be hidden when the field is too narrow.\nThis allows more space for the actual component fields themselves.")]
        public bool ResponsiveVectorComponentFields
        {
            get { return SirenixEditorFields.ResponsiveVectorComponentFields; }
            set { SirenixEditorFields.ResponsiveVectorComponentFields = value; }
        }

        /// <summary>
        /// Specify how the Quaternion struct should be shown in the inspector.
        /// </summary>
        [FoldoutGroup("Structs")]
        [EnumToggleButtons]
        [ShowInInspector, PropertyTooltip("Current mode for how quaternions are edited in the inspector.\n\nEuler: Rotations as yaw, pitch and roll.\n\nAngle axis: Rotations as a axis of rotation, and an angle of rotation around that axis.\n\nRaw: Directly edit the x, y, z and w components of a quaternion.")]
        public QuaternionDrawMode QuaternionDrawMode
        {
            get { return this.quaternionDrawMode; }
            set
            {
                this.quaternionDrawMode = value;
                EditorPrefs.SetInt("GeneralDrawerConfig.QuaternionDrawMode", (int)value);
            }
        }

        [ShowInInspector]
        [FoldoutGroup("Structs")]
        private Quaternion ExampleQuaternion { get; set; }

        [ShowInInspector]
        [FoldoutGroup("Structs")]
        private Vector3 ExampleVector { get; set; }

        #endregion

        #region Lists

        /// <summary>
        /// Specify whether or not a list should hide the foldout triangle when the list is empty.
        /// </summary>
        [FoldoutGroup("Lists")]
        [InfoBox("All list settings - and more - can be overridden for individual lists by using the ListDrawerSettings attribute.")]
        [PropertyTooltip("Specifies whether or not a list should hide the foldout triangle when the list is empty.")]
        public bool HideFoldoutWhileEmpty
        {
            get
            {
                return this.hideFoldoutWhileEmpty;
            }
            set
            {
                this.hideFoldoutWhileEmpty = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.HideFoldoutWhileEmpty", value);
            }
        }

        /// <summary>
        /// Specify whether or not lists should hide the paging buttons when the list is collapsed.
        /// </summary>
        [FoldoutGroup("Lists")]
        [PropertyTooltip("Specify whether or not lists should hide the paging buttons when the list is collapsed.")]
        public bool HidePagingWhileCollapsed
        {
            get
            {
                return this.hidePagingWhileCollapsed;
            }
            set
            {
                this.hidePagingWhileCollapsed = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.HidePagingWhileCollapsed", value);
            }
        }

        /// <summary>
        /// Specify whether or not lists should hide the paging buttons when there is only one page.
        /// </summary>
        [FoldoutGroup("Lists")]
        public bool HidePagingWhileOnlyOnePage
        {
            get
            {
                return this.hidePagingWhileOnlyOnePage;
            }
            set
            {
                this.hidePagingWhileOnlyOnePage = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.HidePagingWhileOnlyOnePage", value);
            }
        }

        /// <summary>
        /// Specify the number of elements drawn per page.
        /// </summary>
        [FoldoutGroup("Lists")]
        [OnValueChanged("ResizeExampleList"), MaxValue(500), MinValue(2)]
        [PropertyTooltip("Specify the number of elements drawn per page.")]
        public int NumberOfItemsPrPage
        {
            get
            {
                return this.numberOfItemsPrPage;
            }
            set
            {
                this.numberOfItemsPrPage = value;
                EditorPrefs.SetInt("GeneralDrawerConfig.NumberOfItemsPrPage", value);
            }
        }

        /// <summary>
        /// Specify whether or not lists should be expanded or collapsed by default.
        /// </summary>
        [FoldoutGroup("Lists")]
        [PropertyTooltip("Specify whether or not lists should be expanded or collapsed by default.")]
        public bool OpenListsByDefault
        {
            get
            {
                return this.openListsByDefault;
            }
            set
            {
                this.openListsByDefault = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.OpenListsByDefault", value);
            }
        }

        /// <summary>
        /// Specify whether or not to include a button which expands the list, showing all pages at once.
        /// </summary>
        [FoldoutGroup("Lists")]
        [PropertyTooltip("Specify whether or not to include a button which expands the list, showing all pages at once")]
        public bool ShowExpandButton
        {
            get
            {
                return this.showExpandButton;
            }
            set
            {
                this.showExpandButton = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.ShowExpandButton", value);
            }
        }

        /// <summary>
        /// Specify whether or not lists should show item count.
        /// </summary>
        [FoldoutGroup("Lists")]
        [PropertyTooltip("Specify whether or not lists should show item count.")]
        public bool ShowItemCount
        {
            get
            {
                return this.showItemCount;
            }
            set
            {
                this.showItemCount = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.ShowItemCount", value);
            }
        }

        /// <summary>
        /// Specify whether or not lists should show item count.
        /// </summary>
        [FoldoutGroup("Lists")]
        [PropertyTooltip("Specify whether or not lists should show item count.")]
        public bool ShowIndexLabels
        {
            get
            {
                return this.showIndexLabels;
            }
            set
            {
                this.showIndexLabels = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.ShowIndexLabels", value);
            }
        }

#pragma warning disable 0414

        [FoldoutGroup("Lists")]
        [NonSerialized, ShowInInspector, PropertyOrder(20)]
        private List<int> exampleList = new List<int>();
        private UnityObjectType squareUnityObjectEnableFor;

        private void ResizeExampleList()
        {
            this.exampleList = Enumerable.Range(0, Math.Max(10, (int)(this.NumberOfItemsPrPage * Mathf.PI))).ToList();
        }

#pragma warning restore 0414

        #endregion

        #region ObjectFields

        /// <summary>
        /// Gets or sets the default size of the preview object field.
        /// </summary>
        [ShowInInspector]
        [FoldoutGroup("Object Fields")]
        public float SquareUnityObjectFieldHeight
        {
            get
            {
                return this.squareUnityObjectFieldHeight;
            }
            set
            {
                this.squareUnityObjectFieldHeight = value;
                EditorPrefs.SetFloat("GeneralDrawerConfig.squareUnityObjectFieldHeight", value);
            }
        }

        /// <summary>
        /// Gets or sets the default alignment of the preview object field.
        /// </summary>
        [ShowInInspector]
        [FoldoutGroup("Object Fields")]
        [EnumToggleButtons]
        public ObjectFieldAlignment SquareUnityObjectAlignment
        {
            get
            {
                return this.squareUnityObjectAlignment;
            }
            set
            {
                this.squareUnityObjectAlignment = value;
                EditorPrefs.SetFloat("GeneralDrawerConfig.squareUnityObjectAlignment", (int)value);
            }
        }

        /// <summary>
        /// Gets or sets which types should be drawn by default by the preview object field.
        /// </summary>
        [ShowInInspector]
        [LabelText("Enable Globally For")]
        [FoldoutGroup("Object Fields")]
        public UnityObjectType SquareUnityObjectEnableFor
        {
            get
            {
                return this.squareUnityObjectEnableFor;
            }
            set
            {
                this.squareUnityObjectEnableFor = value;
                EditorPrefs.SetInt("GeneralDrawerConfig.squareUnityObjectEnableFor", (int)value);
            }
        }

        [ShowInInspector, PreviewField, FoldoutGroup("Object Fields")]
        private UnityEngine.Object ExampleObject { get; set; }

        [Flags]
        public enum UnityObjectType
        {
            Textures = 1 << 1,
            Sprites = 1 << 2,
            Materials = 1 << 3,
            GameObjects = 1 << 4,
            Components = 1 << 5,
            Others = 1 << 6
        }
        #endregion

    }
}
#endif