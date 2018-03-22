#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorConfig.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
    using UnityEditor;
    [InitializeOnLoad]
    internal static class CustomEditorLoader
    {
        static CustomEditorLoader()
        {
            if (InspectorConfig.HasInstanceLoaded)
            {
                InspectorConfig.Instance.UpdateOdinEditors();
            }
            else
            {
                EditorApplication.delayCall += () => InspectorConfig.Instance.UpdateOdinEditors();
            }
        }
    }
}
#endif