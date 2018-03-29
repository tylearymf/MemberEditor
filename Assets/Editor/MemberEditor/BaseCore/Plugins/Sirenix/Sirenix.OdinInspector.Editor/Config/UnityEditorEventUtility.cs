#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UnityEditorEventUtility.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
    using UnityEditor;
    using System;
    using System.Reflection;

    /// <summary>
    /// Sometimes, an idiot overrides a delay action subscription to <see cref="EditorApplication.delayCall"/>,
    /// which can be done because the people at Unity didn't know what events were once upon a time.
    /// This method subscribes to a lot of different callbacks, in the hopes of catching at least one.
    /// </summary>
    public static class UnityEditorEventUtility
    {
        private static readonly EventInfo onProjectChangedEvent = typeof(EditorApplication).GetEvent("projectChanged");

        public static readonly bool HasOnProjectChanged = onProjectChangedEvent != null;

        public static event Action OnProjectChanged
        {
            add
            {
                if (onProjectChangedEvent != null)
                {
                    onProjectChangedEvent.AddEventHandler(null, value);
                }
                else throw new NotImplementedException("EditorApplication.projectChanged is not implemented in this version of Unity.");
            }
            remove
            {
                if (onProjectChangedEvent != null)
                {
                    onProjectChangedEvent.RemoveEventHandler(null, value);
                }
                else throw new NotImplementedException("EditorApplication.projectChanged is not implemented in this version of Unity.");
            }
        }


        /// <summary>
        /// Sometimes, an idiot overrides a delay action subscription to <see cref="EditorApplication.delayCall"/>,
        /// which can be done because the people at Unity didn't know what events were once upon a time.
        /// This method subscribes to a lot of different callbacks, in the hopes of catching at least one.
        /// </summary>
        public static void DelayAction(Action action)
        {
            bool executed = false;

            Action execute = null;

            EditorApplication.ProjectWindowItemCallback projectWindowItemOnGUI = (_, __) => execute();
            EditorApplication.HierarchyWindowItemCallback hierarchyWindowItemOnGUI = (_, __) => execute();
            EditorApplication.CallbackFunction update = () => execute();
            EditorApplication.CallbackFunction delayCall = () => execute();

            execute = () =>
            {
                if (!executed)
                {
                    try
                    {
                        action();
                    }
                    finally
                    {
                        executed = true;

                        EditorApplication.projectWindowItemOnGUI -= projectWindowItemOnGUI;
                        EditorApplication.hierarchyWindowItemOnGUI -= hierarchyWindowItemOnGUI;
                        EditorApplication.update -= update;
                        EditorApplication.delayCall -= delayCall;
                    }
                }
            };

            EditorApplication.projectWindowItemOnGUI += projectWindowItemOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += hierarchyWindowItemOnGUI;
            EditorApplication.update += update;
            EditorApplication.delayCall += delayCall;
        }
    }
}
#endif