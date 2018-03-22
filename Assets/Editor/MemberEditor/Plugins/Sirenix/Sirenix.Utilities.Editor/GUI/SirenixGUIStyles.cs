#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SirenixGUIStyles.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Collection of GUIStyles used by Sirenix.
    /// </summary>
    [InitializeOnLoad]
    public static class SirenixGUIStyles
    {
        /// <summary>
        /// Border color.
        /// </summary>
        public static readonly Color BorderColor = EditorGUIUtility.isProSkin ? new Color(0.11f * 1.0f, 0.11f * 1.0f, 0.11f * 1.0f, 1f) : new Color(0.38f, 0.38f, 0.38f, 1f);

        /// <summary>
        /// Box background color.
        /// </summary>
        public static readonly Color BoxBackgroundColor = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.05f) : new Color(1, 1, 1, 0.5f);

        /// <summary>
        /// Dark editor background color.
        /// </summary>
        public static readonly Color DarkEditorBackground = EditorGUIUtility.isProSkin ? new Color(0.192f, 0.192f, 0.192f, 1f) : new Color(0, 0, 0, 0);

        /// <summary>
        /// Editor window background color.
        /// </summary>
        public static readonly Color EditorWindowBackgroundColor = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f, 1f) : new Color(0.76f, 0.76f, 0.76f, 1f);

        /// <summary>
        /// Menu background color.
        /// </summary>
        public static readonly Color MenuBackgroundColor = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.035f) : new Color(0.870f, 0.870f, 0.870f, 1);

        /// <summary>
        /// Header box background color.
        /// </summary>
        public static readonly Color HeaderBoxBackgroundColor = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.06f) : new Color(1, 1, 1, 0.26f);

        /// <summary>
        /// Highlighted Button Color.
        /// </summary>
        public static readonly Color HighlightedButtonColor = EditorGUIUtility.isProSkin ? new Color(0.0f, 1.0f, 0, 1) : new Color(0.0f, 1f, 0, 1);

        /// <summary>
        /// Highlight text color.
        /// </summary>
        public static readonly Color HighlightedTextColor = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 1) : new Color(0, 0, 0, 1);

        /// <summary>
        /// Highlight property color.
        /// </summary>
        public static readonly Color HighlightPropertyColor = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.6f) : new Color(0, 0, 0, 0.6f);

        /// <summary>
        /// List item color for every other item.
        /// </summary>
        public static readonly Color ListItemColorEven = EditorGUIUtility.isProSkin ? new Color(0.247f * 0.83f, 0.247f * 0.83f, 0.247f * 0.83f, 1f) : new Color(0.838f, 0.838f, 0.838f, 1.000f);

        /// <summary>
        /// List item hover color for every other item.
        /// </summary>
        public static readonly Color ListItemColorHoverEven = EditorGUIUtility.isProSkin ? new Color(0.279f * 0.8f, 0.279f * 0.8f, 0.279f * 0.8f, 1f) : new Color(0.890f, 0.890f, 0.890f, 1.000f);

        /// <summary>
        /// List item hover color for every other item.
        /// </summary>
        public static readonly Color ListItemColorHoverOdd = EditorGUIUtility.isProSkin ? new Color(0.309f * 0.8f, 0.309f * 0.8f, 0.309f * 0.8f, 1f) : new Color(0.904f, 0.904f, 0.904f, 1.000f);

        /// <summary>
        /// List item color for every other item.
        /// </summary>
        public static readonly Color ListItemColorOdd = EditorGUIUtility.isProSkin ? new Color(0.272f * 0.8f, 0.272f * 0.8f, 0.272f * 0.8f, 1f) : new Color(0.801f, 0.801f, 0.801f, 1.000f);

        /// <summary>
        /// List item drag background color.
        /// </summary>
        public static readonly Color ListItemDragBg = new Color(0.1f, 0.1f, 0.1f, 1f);

        /// <summary>
        /// List item drag background color.
        /// </summary>
        public static readonly Color ListItemDragBgColor = EditorGUIUtility.isProSkin ? new Color(0.1f, 0.1f, 0.1f, 1f) : new Color(0.338f, 0.338f, 0.338f, 1.000f);

        /// <summary>
        /// List item background color for every other item.
        /// </summary>
        public static readonly Color ListItemEven = new Color(0.4f, 0.4f, 0.4f, 1f);

        /// <summary>
        /// List item background color for every other item.
        /// </summary>
        public static readonly Color ListItemOdd = new Color(0.4f, 0.4f, 0.4f, 1f);

        /// <summary>
        /// Menu button active background color.
        /// </summary>
        public static readonly Color MenuButtonActiveBgColor = EditorGUIUtility.isProSkin ? new Color(0.243f, 0.373f, 0.588f, 1.000f) : new Color(0.243f, 0.49f, 0.9f, 1.000f);

        /// <summary>
        /// Menu button border color.
        /// </summary>
        public static readonly Color MenuButtonBorderColor = new Color(EditorWindowBackgroundColor.r * 0.8f, EditorWindowBackgroundColor.g * 0.8f, EditorWindowBackgroundColor.b * 0.8f);

        /// <summary>
        /// Menu button color.
        /// </summary>
        public static readonly Color MenuButtonColor = new Color(0, 0, 0, 0);

        /// <summary>
        /// Menu button hover color.
        /// </summary>
        public static readonly Color MenuButtonHoverColor = new Color(1, 1, 1, 0.08f);

        /// <summary>
        /// A light border color.
        /// </summary>
        public static readonly Color LightBorderColor = new Color32(90, 90, 90, 255);

        private static GUIStyle boldLabel;
        private static GUIStyle boldLabelCentered;
        private static GUIStyle boxContainer;
        private static GUIStyle boxHeaderStyle;
        private static GUIStyle button;
        private static GUIStyle buttonSelected;
        private static GUIStyle buttonLeft;
        private static GUIStyle buttonLeftSelected;
        private static GUIStyle buttonMid;
        private static GUIStyle buttonMidSelected;
        private static GUIStyle buttonRight;
        private static GUIStyle buttonRightSelected;
        private static GUIStyle miniButton;
        private static GUIStyle colorFieldBackground;
        private static GUIStyle foldout;
        private static GUIStyle iconButton;
        private static GUIStyle label;
        private static GUIStyle labelCentered;
        private static GUIStyle leftAlignedGreyMiniLabel;
        private static GUIStyle leftRightAlignedWhiteMiniLabel;
        private static GUIStyle listItem;
        private static GUIStyle menuButtonBackground;
        private static GUIStyle none;
        private static GUIStyle paddingLessBox;
        private static GUIStyle propertyMessagePaddingTest;
        private static GUIStyle propertyPadding;
        private static GUIStyle compositeDrawerPadding;
        private static GUIStyle propertyMargin;
        private static GUIStyle richTextLabel;
        private static GUIStyle rightAlignedGreyMiniLabel;
        private static GUIStyle rightAlignedWhiteMiniLabel;
        private static GUIStyle sectionHeader;
        private static GUIStyle sectionHeaderCentered;
        private static GUIStyle toggleGroupBackground;
        private static GUIStyle toggleGroupCheckbox;
        private static GUIStyle toggleGroupPadding;
        private static GUIStyle toggleGroupTitleBg;
        private static GUIStyle toolbarBackground;
        private static GUIStyle toolbarButton;
        private static GUIStyle toolbarButtonSelected;
        private static GUIStyle toolbarSeachCancelButton;
        private static GUIStyle toolbarSeachTextField;
        private static GUIStyle toolbarTab;
        private static GUIStyle title;
        private static GUIStyle boldTitle;
        private static GUIStyle subtitle;
        private static GUIStyle titleRight;
        private static GUIStyle titleCentered;
        private static GUIStyle boldTitleRight;
        private static GUIStyle boldTitleCentered;
        private static GUIStyle subtitleCentered;
        private static GUIStyle subtitleRight;
        private static GUIStyle messageBox;
        private static GUIStyle detailedMessageBox;
        private static GUIStyle multiLineLabel;
        private static GUIStyle odinEditorWrapper;
        private static GUIStyle whiteLabel;
        private static GUIStyle blackLabel;
        private static GUIStyle miniButtonRightSelected;
        private static GUIStyle miniButtonRight;
        private static GUIStyle miniButtonLeftSelected;
        private static GUIStyle miniButtonLeft;
        private static GUIStyle miniButtonSelected;
        private static GUIStyle miniButtonMid;
        private static GUIStyle miniButtonMidSelected;
        private static GUIStyle centeredTextField;
        private static GUIStyle tagButton;
        private static GUIStyle centeredGreyMiniLabel;

        /// <summary>
        /// Tag Button style.
        /// </summary>
        public static GUIStyle TagButton
        {
            get
            {
                if (tagButton == null)
                {
                    tagButton = new GUIStyle("MiniToolbarButton")
                    {
                        alignment = TextAnchor.MiddleCenter,
                        padding = new RectOffset(),
                        margin = new RectOffset(),
                        contentOffset = new Vector2(0, 0),
                        fontSize = 9,
                        font = EditorStyles.standardFont,
                    };
                }

                return tagButton;
            }
        }

        /// <summary>
        /// Bold label style.
        /// </summary>
        public static GUIStyle BoldLabel
        {
            get
            {
                if (boldLabel == null)
                {
                    boldLabel = new GUIStyle(EditorStyles.boldLabel) { contentOffset = new Vector2(0, 0), margin = new RectOffset(0, 0, 0, 0) };
                }
                return boldLabel;
            }
        }

        /// <summary>
        /// Centered bold label style.
        /// </summary>
        public static GUIStyle BoldLabelCentered
        {
            get
            {
                if (boldLabelCentered == null)
                {
                    boldLabelCentered = new GUIStyle(BoldLabel) { alignment = TextAnchor.MiddleCenter };
                }
                return boldLabelCentered;
            }
        }

        /// <summary>
        /// Box container style.
        /// </summary>
        public static GUIStyle BoxContainer
        {
            get
            {
                if (boxContainer == null)
                {
                    boxContainer = new GUIStyle(EditorStyles.helpBox) { margin = new RectOffset(0, 0, 3, 3) };
                }
                return boxContainer;
            }
        }

        /// <summary>
        /// Box header style.
        /// </summary>
        public static GUIStyle BoxHeaderStyle
        {
            get
            {
                if (boxHeaderStyle == null)
                {
                    boxHeaderStyle = new GUIStyle(None) { margin = new RectOffset(0, 0, 0, 5) };
                }
                return boxHeaderStyle;
            }
        }

        /// <summary>
        /// Left button style.
        /// </summary>
        public static GUIStyle Button
        {
            get
            {
                if (button == null)
                {
                    button = new GUIStyle("Button");
                }
                return button;
            }
        }

        /// <summary>
        /// Left button selected style.
        /// </summary>
        public static GUIStyle ButtonSelected
        {
            get
            {
                if (buttonSelected == null)
                {
                    buttonSelected = new GUIStyle(Button)
                    {
                        normal = new GUIStyle(Button).onNormal
                    };
                }
                return buttonSelected;
            }
        }

        /// <summary>
        /// Left button style.
        /// </summary>
        public static GUIStyle ButtonLeft
        {
            get
            {
                if (buttonLeft == null)
                {
                    buttonLeft = new GUIStyle("ButtonLeft");
                }
                return buttonLeft;
            }
        }

        /// <summary>
        /// Left button selected style.
        /// </summary>
        public static GUIStyle ButtonLeftSelected
        {
            get
            {
                if (buttonLeftSelected == null)
                {
                    buttonLeftSelected = new GUIStyle(ButtonLeft)
                    {
                        normal = new GUIStyle(ButtonLeft).onNormal
                    };
                }
                return buttonLeftSelected;
            }
        }

        /// <summary>
        /// Mid button style.
        /// </summary>
        public static GUIStyle ButtonMid
        {
            get
            {
                if (buttonMid == null)
                {
                    buttonMid = new GUIStyle("ButtonMid");
                }
                return buttonMid;
            }
        }

        /// <summary>
        /// Mid button selected style.
        /// </summary>
        public static GUIStyle ButtonMidSelected
        {
            get
            {
                if (buttonMidSelected == null)
                {
                    buttonMidSelected = new GUIStyle(ButtonMid)
                    {
                        normal = new GUIStyle(ButtonMid).onNormal
                    };
                }
                return buttonMidSelected;
            }
        }

        /// <summary>
        /// Right button style.
        /// </summary>
        public static GUIStyle ButtonRight
        {
            get
            {
                if (buttonRight == null)
                {
                    buttonRight = new GUIStyle("ButtonRight");
                }
                return buttonRight;
            }
        }

        /// <summary>
        /// Right button selected style.
        /// </summary>
        public static GUIStyle ButtonRightSelected
        {
            get
            {
                if (buttonRightSelected == null)
                {
                    buttonRightSelected = new GUIStyle(ButtonRight)
                    {
                        normal = new GUIStyle(ButtonRight).onNormal
                    };
                }
                return buttonRightSelected;
            }
        }

        /// <summary>
        /// Left button style.
        /// </summary>
        public static GUIStyle MiniButton
        {
            get
            {
                if (miniButton == null)
                {
                    miniButton = new GUIStyle(EditorStyles.miniButton);
                }
                return miniButton;
            }
        }

        /// <summary>
        /// Left button selected style.
        /// </summary>
        public static GUIStyle MiniButtonSelected
        {
            get
            {
                if (miniButtonSelected == null)
                {
                    miniButtonSelected = new GUIStyle(MiniButton)
                    {
                        normal = new GUIStyle(MiniButton).onNormal
                    };
                }
                return miniButtonSelected;
            }
        }

        /// <summary>
        /// Left button style.
        /// </summary>
        public static GUIStyle MiniButtonLeft
        {
            get
            {
                if (miniButtonLeft == null)
                {
                    miniButtonLeft = new GUIStyle(EditorStyles.miniButtonLeft);
                }
                return miniButtonLeft;
            }
        }

        /// <summary>
        /// Left button selected style.
        /// </summary>
        public static GUIStyle MiniButtonLeftSelected
        {
            get
            {
                if (miniButtonLeftSelected == null)
                {
                    miniButtonLeftSelected = new GUIStyle(MiniButtonLeft)
                    {
                        normal = new GUIStyle(MiniButtonLeft).onNormal
                    };
                }
                return miniButtonLeftSelected;
            }
        }

        /// <summary>
        /// Mid button style.
        /// </summary>
        public static GUIStyle MiniButtonMid
        {
            get
            {
                if (miniButtonMid == null)
                {
                    miniButtonMid = new GUIStyle(EditorStyles.miniButtonMid);
                }
                return miniButtonMid;
            }
        }

        /// <summary>
        /// Mid button selected style.
        /// </summary>
        public static GUIStyle MiniButtonMidSelected
        {
            get
            {
                if (miniButtonMidSelected == null)
                {
                    miniButtonMidSelected = new GUIStyle(MiniButtonMid)
                    {
                        normal = new GUIStyle(MiniButtonMid).onNormal
                    };
                }
                return miniButtonMidSelected;
            }
        }

        /// <summary>
        /// Right button style.
        /// </summary>
        public static GUIStyle MiniButtonRight
        {
            get
            {
                if (miniButtonRight == null)
                {
                    miniButtonRight = new GUIStyle(EditorStyles.miniButtonRight);
                }
                return miniButtonRight;
            }
        }

        /// <summary>
        /// Right button selected style.
        /// </summary>
        public static GUIStyle MiniButtonRightSelected
        {
            get
            {
                if (miniButtonRightSelected == null)
                {
                    miniButtonRightSelected = new GUIStyle(MiniButtonRight)
                    {
                        normal = new GUIStyle(MiniButtonRight).onNormal
                    };
                }
                return miniButtonRightSelected;
            }
        }

        /// <summary>
        /// Color field background style.
        /// </summary>
        public static GUIStyle ColorFieldBackground
        {
            get
            {
                if (colorFieldBackground == null)
                {
                    colorFieldBackground = new GUIStyle("ShurikenEffectBg");
                }
                return colorFieldBackground;
            }
        }

        /// <summary>
        /// Foldout style.
        /// </summary>
        public static GUIStyle Foldout
        {
            get
            {
                if (foldout == null)
                {
                    foldout = new GUIStyle(EditorStyles.foldout) { fixedWidth = 0, fixedHeight = 16, stretchHeight = false, stretchWidth = true };
                }
                return foldout;
            }
        }

        /// <summary>
        /// Icon button style.
        /// </summary>
        public static GUIStyle IconButton
        {
            get
            {
                if (iconButton == null)
                {
                    iconButton = new GUIStyle(GUIStyle.none) { padding = new RectOffset(1, 1, 1, 1), };
                }
                return iconButton;
            }
        }

        /// <summary>
        /// Label style.
        /// </summary>
        public static GUIStyle Label
        {
            get
            {
                if (label == null)
                {
                    label = new GUIStyle(EditorStyles.label) { margin = new RectOffset(0, 0, 0, 0) };
                }
                return label;
            }
        }

        /// <summary>
        /// White label style.
        /// </summary>
        public static GUIStyle WhiteLabel
        {
            get
            {
                if (whiteLabel == null)
                {
                    whiteLabel = new GUIStyle(EditorStyles.label)
                    {
                        margin = new RectOffset(0, 0, 0, 0)
                    };
                    whiteLabel.normal.textColor = Color.white;
                    whiteLabel.onNormal.textColor = Color.white;
                }

                return whiteLabel;
            }
        }

        /// <summary>
        /// Black label style.
        /// </summary>
        public static GUIStyle BlackLabel
        {
            get
            {
                if (blackLabel == null)
                {
                    blackLabel = new GUIStyle(EditorStyles.label) { margin = new RectOffset(0, 0, 0, 0) };
                    blackLabel.normal.textColor = Color.black;
                    blackLabel.onNormal.textColor = Color.black;
                }
                return blackLabel;
            }
        }

        /// <summary>
        /// Centered label style.
        /// </summary>
        public static GUIStyle LabelCentered
        {
            get
            {
                if (labelCentered == null)
                {
                    labelCentered = new GUIStyle(Label) { alignment = TextAnchor.MiddleCenter, margin = new RectOffset(0, 0, 0, 0) };
                }
                return labelCentered;
            }
        }

        /// <summary>
        /// Left aligned grey mini label style.
        /// </summary>
        public static GUIStyle LeftAlignedGreyMiniLabel
        {
            get
            {
                if (leftAlignedGreyMiniLabel == null)
                {
                    leftAlignedGreyMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Overflow, };
                }
                return leftAlignedGreyMiniLabel;
            }
        }

        /// <summary>
        /// Centered grey mini label
        /// </summary>
        public static GUIStyle CenteredGreyMiniLabel
        {
            get
            {
                if (centeredGreyMiniLabel == null)
                {
                    centeredGreyMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Overflow, };
                }
                return centeredGreyMiniLabel;
            }
        }

        /// <summary>
        /// Left right aligned white mini label style.
        /// </summary>
        public static GUIStyle LeftRightAlignedWhiteMiniLabel
        {
            get
            {
                if (leftRightAlignedWhiteMiniLabel == null)
                {
                    leftRightAlignedWhiteMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Overflow, normal = new GUIStyleState() { textColor = Color.white } };
                }
                return leftRightAlignedWhiteMiniLabel;
            }
        }

        /// <summary>
        /// List item style.
        /// </summary>
        public static GUIStyle ListItem
        {
            get
            {
                if (listItem == null)
                {
                    listItem = new GUIStyle(None) { padding = new RectOffset(0, 0, 3, 3) };
                }
                return listItem;
            }
        }

        /// <summary>
        /// Menu button background style.
        /// </summary>
        public static GUIStyle MenuButtonBackground
        {
            get
            {
                if (menuButtonBackground == null)
                {
                    menuButtonBackground = new GUIStyle() { margin = new RectOffset(0, 1, 0, 0), padding = new RectOffset(0, 0, 4, 4), border = new RectOffset(0, 0, 0, 0) };
                }
                return menuButtonBackground;
            }
        }

        /// <summary>
        /// No style.
        /// </summary>
        public static GUIStyle None
        {
            get
            {
                if (none == null)
                {
                    none = new GUIStyle() { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(0, 0, 0, 0), border = new RectOffset(0, 0, 0, 0) };
                }
                return none;
            }
        }

        /// <summary>
        /// Odin Editor Wrapper.
        /// </summary>
        public static GUIStyle OdinEditorWrapper
        {
            get
            {
                if (odinEditorWrapper == null)
                {
                    odinEditorWrapper = new GUIStyle() { padding = new RectOffset(4, 4, 0, 0) };
                }
                return odinEditorWrapper;
            }
        }

        /// <summary>
        /// Padding less box style.
        /// </summary>
        public static GUIStyle PaddingLessBox
        {
            get
            {
                if (paddingLessBox == null)
                {
                    paddingLessBox = new GUIStyle("box") { padding = new RectOffset(1, 1, 0, 0) };
                }
                return paddingLessBox;
            }
        }

        /// <summary>
        /// Property padding
        /// </summary>
        public static GUIStyle PropertyMessagePaddingTest
        {
            get
            {
                if (propertyMessagePaddingTest == null)
                {
                    propertyMessagePaddingTest = new GUIStyle() { padding = new RectOffset(3, 3, 0, 0) };
                }
                return propertyMessagePaddingTest;
            }
        }

        /// <summary>
        /// Property padding.
        /// </summary>
        public static GUIStyle PropertyPadding
        {
            get
            {
                if (propertyPadding == null)
                {
                    propertyPadding = new GUIStyle(GUIStyle.none) { padding = new RectOffset(0, 0, 0, 3), margin = new RectOffset(0, 0, 0, 0) };
                }
                return propertyPadding;
            }
        }

        /// <summary>
        /// Property margin.
        /// </summary>
        public static GUIStyle PropertyMargin
        {
            get
            {
                if (propertyMargin == null)
                {
                    propertyMargin = new GUIStyle(GUIStyle.none) { margin = new RectOffset(4, 0, 0, 0) };
                }
                return propertyMargin;
            }
        }

        /// <summary>
        /// Rich text label style.
        /// </summary>
        public static GUIStyle RichTextLabel
        {
            get
            {
                if (richTextLabel == null)
                {
                    richTextLabel = new GUIStyle(EditorStyles.label) { richText = true, };
                }
                return richTextLabel;
            }
        }

        /// <summary>
        /// Right aligned grey mini label style.
        /// </summary>
        public static GUIStyle RightAlignedGreyMiniLabel
        {
            get
            {
                if (rightAlignedGreyMiniLabel == null)
                {
                    rightAlignedGreyMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { alignment = TextAnchor.MiddleRight, clipping = TextClipping.Overflow, };
                }
                return rightAlignedGreyMiniLabel;
            }
        }

        /// <summary>
        /// Right aligned white mini label style.
        /// </summary>
        public static GUIStyle RightAlignedWhiteMiniLabel
        {
            get
            {
                if (rightAlignedWhiteMiniLabel == null)
                {
                    rightAlignedWhiteMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { alignment = TextAnchor.MiddleRight, clipping = TextClipping.Overflow, normal = new GUIStyleState() { textColor = Color.white } };
                }
                return rightAlignedWhiteMiniLabel;
            }
        }

        /// <summary>
        /// Section header style.
        /// </summary>
        public static GUIStyle SectionHeader
        {
            get
            {
                if (sectionHeader == null)
                {
                    sectionHeader = new GUIStyle(EditorStyles.largeLabel)
                    {
                        fontSize = 22,
                        margin = new RectOffset(0, 0, 5, 0),
                        fontStyle = FontStyle.Normal,
                        wordWrap = true,
                        font = EditorStyles.centeredGreyMiniLabel.font,
                        overflow = new RectOffset(0, 0, 0, 0),
                    };
                }
                return sectionHeader;
            }
        }

        /// <summary>
        /// Section header style.
        /// </summary>
        public static GUIStyle SectionHeaderCentered
        {
            get
            {
                if (sectionHeaderCentered == null)
                {
                    sectionHeaderCentered = new GUIStyle(SectionHeader)
                    {
                        alignment = TextAnchor.MiddleCenter,
                    };
                }
                return sectionHeaderCentered;
            }
        }

        /// <summary>
        /// Toggle group background style.
        /// </summary>
        public static GUIStyle ToggleGroupBackground
        {
            get
            {
                if (toggleGroupBackground == null)
                {
                    toggleGroupBackground = new GUIStyle(EditorStyles.helpBox) { overflow = new RectOffset(0, 0, 0, 0), margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(0, 0, 0, 0) };
                }
                return toggleGroupBackground;
            }
        }

        /// <summary>
        /// Toggle group checkbox style.
        /// </summary>
        public static GUIStyle ToggleGroupCheckbox
        {
            get
            {
                if (toggleGroupCheckbox == null)
                {
                    toggleGroupCheckbox = new GUIStyle("ShurikenCheckMark");
                }
                return toggleGroupCheckbox;
            }
        }

        /// <summary>
        /// Toggle group padding style.
        /// </summary>
        public static GUIStyle ToggleGroupPadding
        {
            get
            {
                if (toggleGroupPadding == null)
                {
                    toggleGroupPadding = new GUIStyle(GUIStyle.none) { padding = new RectOffset(5, 5, 5, 5) };
                }
                return toggleGroupPadding;
            }
        }

        /// <summary>
        /// Toggle group title background style.
        /// </summary>
        public static GUIStyle ToggleGroupTitleBg
        {
            get
            {
                if (toggleGroupTitleBg == null)
                {
                    toggleGroupTitleBg = new GUIStyle("ShurikenModuleTitle") { font = (new GUIStyle("Label")).font, border = new RectOffset(15, 7, 4, 4), fixedHeight = 22, contentOffset = new Vector2(20f, -2f), margin = new RectOffset(0, 0, 3, 0) };
                }
                return toggleGroupTitleBg;
            }
        }

        /// <summary>
        /// Toolbar background style.
        /// </summary>
        public static GUIStyle ToolbarBackground
        {
            get
            {
                if (toolbarBackground == null)
                {
                    //toolbarBackground = new GUIStyle("OL title") { fixedHeight = 0, fixedWidth = 0, stretchHeight = true, stretchWidth = true, padding = new RectOffset(0, 0, 0, 0), margin = new RectOffset(0, 0, 0, 0), overflow = new RectOffset(0, 0, 0, 0), };
                    toolbarBackground = new GUIStyle(EditorStyles.toolbar)
                    {
                        padding = new RectOffset(0, 1, 0, 0),
                        stretchHeight = true,
                        fixedHeight = 0,
                    };
                }
                return toolbarBackground;
            }
        }

        /// <summary>
        /// Toolbar button style.
        /// </summary>
        public static GUIStyle ToolbarButton
        {
            get
            {
                if (toolbarButton == null)
                {
                    //toolbarButton = new GUIStyle("OL Title TextRight") { stretchHeight = true, stretchWidth = false, fixedHeight = 0f, alignment = TextAnchor.MiddleCenter, font = EditorStyles.toolbarButton.font, fontSize = EditorStyles.toolbarButton.fontSize, fontStyle = EditorStyles.toolbarButton.fontStyle, overflow = new RectOffset(1, 0, 0, 0), };
                    toolbarButton = new GUIStyle(EditorStyles.toolbarButton)
                    {
                        fixedHeight = 0,
                        alignment = TextAnchor.MiddleCenter,
                        stretchHeight = true,
                        stretchWidth = false,
                    };
                }
                return toolbarButton;
            }
        }

        /// <summary>
        /// Toolbar button selected style.
        /// </summary>
        public static GUIStyle ToolbarButtonSelected
        {
            get
            {
                if (toolbarButtonSelected == null)
                {
                    toolbarButtonSelected = new GUIStyle(ToolbarButton)
                    {
                        normal = new GUIStyle(ToolbarButton).onNormal
                    };
                }

                return toolbarButtonSelected;
            }
        }

        /// <summary>
        /// Toolbar search cancel button style.
        /// </summary>
        public static GUIStyle ToolbarSeachCancelButton
        {
            get
            {
                if (toolbarSeachCancelButton == null)
                {
                    toolbarSeachCancelButton = GUI.skin.FindStyle("ToolbarSeachCancelButton");
                }
                return toolbarSeachCancelButton;
            }
        }

        /// <summary>
        /// Toolbar search field style.
        /// </summary>
        public static GUIStyle ToolbarSeachTextField
        {
            get
            {
                if (toolbarSeachTextField == null)
                {
                    toolbarSeachTextField = GUI.skin.FindStyle("ToolbarSeachTextField");
                }
                return toolbarSeachTextField;
            }
        }

        /// <summary>
        /// Toolbar tab style.
        /// </summary>
        public static GUIStyle ToolbarTab
        {
            get
            {
                if (toolbarTab == null)
                {
                    toolbarTab = new GUIStyle(EditorStyles.toolbarButton)
                    {
                        fixedHeight = 0,
                        stretchHeight = true,
                        stretchWidth = true,
                    };
                }

                return toolbarTab;
            }
        }

        /// <summary>
        /// Title style.
        /// </summary>
        public static GUIStyle Title
        {
            get
            {
                if (title == null)
                {
                    title = new GUIStyle(EditorStyles.label)
                    {
                    };
                }

                return title;
            }
        }

        /// <summary>
        /// Bold title style.
        /// </summary>
        public static GUIStyle BoldTitle
        {
            get
            {
                if (boldTitle == null)
                {
                    boldTitle = new GUIStyle(Title)
                    {
                        fontStyle = FontStyle.Bold,
                    };
                }

                return boldTitle;
            }
        }

        /// <summary>
        /// Centered bold title style.
        /// </summary>
        public static GUIStyle BoldTitleCentered
        {
            get
            {
                if (boldTitleCentered == null)
                {
                    boldTitleCentered = new GUIStyle(BoldTitle)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                }

                return boldTitleCentered;
            }
        }

        /// <summary>
        /// Right aligned bold title style.
        /// </summary>
        public static GUIStyle BoldTitleRight
        {
            get
            {
                if (boldTitleRight == null)
                {
                    boldTitleRight = new GUIStyle(BoldTitle)
                    {
                        alignment = TextAnchor.MiddleRight
                    };
                }

                return boldTitleRight;
            }
        }

        /// <summary>
        /// Centered title style.
        /// </summary>
        public static GUIStyle TitleCentered
        {
            get
            {
                if (titleCentered == null)
                {
                    titleCentered = new GUIStyle(Title)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                }

                return titleCentered;
            }
        }

        /// <summary>
        /// Right aligned title style.
        /// </summary>
        public static GUIStyle TitleRight
        {
            get
            {
                if (titleRight == null)
                {
                    titleRight = new GUIStyle(Title)
                    {
                        alignment = TextAnchor.MiddleRight
                    };
                }

                return titleRight;
            }
        }

        /// <summary>
        /// Subtitle style.
        /// </summary>
        public static GUIStyle Subtitle
        {
            get
            {
                if (subtitle == null)
                {
                    subtitle = new GUIStyle(Title)
                    {
                        font = GUI.skin.button.font,
                        fontSize = 10,
                        contentOffset = new Vector2(0, -3),
                        fixedHeight = 14,
                    };
                    var c = subtitle.normal.textColor;
                    c.a *= 0.7f;
                    subtitle.normal.textColor = c;
                }

                return subtitle;
            }
        }

        /// <summary>
        /// Centered sub-title style.
        /// </summary>
        public static GUIStyle SubtitleCentered
        {
            get
            {
                if (subtitleCentered == null)
                {
                    subtitleCentered = new GUIStyle(Subtitle)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                }

                return subtitleCentered;
            }
        }

        /// <summary>
        /// Right aligned sub-title style.
        /// </summary>
        public static GUIStyle SubtitleRight
        {
            get
            {
                if (subtitleRight == null)
                {
                    subtitleRight = new GUIStyle(Subtitle)
                    {
                        alignment = TextAnchor.MiddleRight
                    };
                }

                return subtitleRight;
            }
        }

        /// <summary>
        /// Message box style.
        /// </summary>
        public static GUIStyle MessageBox
        {
            get
            {
                if (messageBox == null)
                {
                    messageBox = new GUIStyle("HelpBox")
                    {
                        margin = new RectOffset(0, 0, 3, 2),
                        padding = new RectOffset(4, 4, 4, 4),
                        fontSize = 10,
                        richText = true
                    };
                }

                return messageBox;
            }
        }

        /// <summary>
        /// Detailed Message box style.
        /// </summary>
		public static GUIStyle DetailedMessageBox
        {
            get
            {
                if (detailedMessageBox == null)
                {
                    detailedMessageBox = new GUIStyle(MessageBox);
                    detailedMessageBox.padding.right += 18;
                }

                return detailedMessageBox;
            }
        }

        /// <summary>
        /// Multiline Label
        /// </summary>
        public static GUIStyle MultiLineLabel
        {
            get
            {
                if (multiLineLabel == null)
                {
                    multiLineLabel = new GUIStyle(EditorStyles.label)
                    {
                        richText = true,
                        stretchWidth = false,
                        wordWrap = true,
                        margin = new RectOffset(0, 0, 0, 0)
                    };
                }

                return multiLineLabel;
            }
        }

        /// <summary>
        /// Centered Text Field
        /// </summary>
        public static GUIStyle CenteredTextField
        {
            get
            {
                if (centeredTextField == null)
                {
                    centeredTextField = new GUIStyle(EditorStyles.textField)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                }

                return centeredTextField;
            }
        }
    }
}
#endif