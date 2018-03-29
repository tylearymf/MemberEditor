#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DelayedGUIPainter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
    using UnityEngine;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public class DelayedGUIDrawer
    {
        private bool ignoreInput = true;
        private RenderTexture prevActiveRenderTexture;
        private RenderTexture tooltipRenderTexture;
        private Vector2 offset;
        private bool drawGUI;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void Begin(float width, float height, bool drawGUI = false)
        {
            this.Begin(new Vector2(width, height), drawGUI);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void Begin(Vector2 size, bool drawGUI = false)
        {
            if (Event.current.type == EventType.Repaint)
            {
                this.offset = GUIClipInfo.TopRect.position;
            }
            this.drawGUI = drawGUI;

            if (this.drawGUI)
            {
                return;
            }

            if (this.ignoreInput)
            {
                GUIHelper.BeginIgnoreInput();
            }

            GUILayout.BeginArea(new Rect(GUIClipInfo.VisibleRect.position, size), SirenixGUIStyles.None);

            if (Event.current.type == EventType.Repaint)
            {
                if (this.tooltipRenderTexture != null)
                {
                    RenderTexture.ReleaseTemporary(this.tooltipRenderTexture);
                }

                var texSize = GUIHelper.CurrentWindow.position.size + new Vector2(GUIHelper.CurrentWindowBorderSize.horizontal, GUIHelper.CurrentWindowBorderSize.vertical);
                this.prevActiveRenderTexture = RenderTexture.active;
                this.tooltipRenderTexture = RenderTexture.GetTemporary((int)texSize.x, (int)texSize.y, 0);
                this.tooltipRenderTexture.filterMode = FilterMode.Point;
                this.tooltipRenderTexture.wrapMode = TextureWrapMode.Clamp;

                RenderTexture.active = this.tooltipRenderTexture;
                GL.Clear(false, true, new Color(1, 1, 1, 0));
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void End()
        {
            if (this.drawGUI)
            {
                return;
            }

            if (Event.current.type == EventType.Repaint)
            {
                RenderTexture.active = this.prevActiveRenderTexture;
            }

            GUILayout.EndArea();

            if (this.ignoreInput)
            {
                GUIHelper.EndIgnoreInput();
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void Draw(Vector2 position)
        {
            //var orig = position;
            //var o = position + Vector2.down * 100;
            if (this.tooltipRenderTexture != null && Event.current.type == EventType.Repaint)
            {
                var border = GUIHelper.CurrentWindowBorderSize;
                position.x -= border.left;
                position.y -= border.top;
                position -= this.offset;
                GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                var rect = new Rect(position.x, position.y, this.tooltipRenderTexture.width, this.tooltipRenderTexture.height);
                GUI.DrawTexture(rect, this.tooltipRenderTexture);
                GL.sRGBWrite = false;
                // Alternative method which is alot more responsive, but it has some working properly with GUIClip.
                // position -= this.offset + GUIClipInfo.VisibleRect.position - GUIClipInfo.TopRect.position;
                // position.x = position.x / this.tooltipRenderTexture.width;
                // position.y = position.y / this.tooltipRenderTexture.height;
                // MaterialUtilities.CaptureGUITexture.SetVector("_Offset", new Vector2(-position.x, position.y));
                // Graphics.Blit(this.tooltipRenderTexture, RenderTexture.active, MaterialUtilities.CaptureGUITexture);
            }
        }
    }
}
#endif