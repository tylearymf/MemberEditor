#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EnsureOdinInspectorDefine.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities
{
    using System;
    using System.Linq;
    using UnityEditor;

    internal static class EnsureOdinInspectorDefine
    {
        private const string DEFINE = "ODIN_INSPECTOR";

        [InitializeOnLoadMethod]
        private static void AssureScriptingDefineSymbol()
        {
            var currentTarget = EditorUserBuildSettings.selectedBuildTargetGroup;

            if (currentTarget == BuildTargetGroup.Unknown)
            {
                return;
            }

            var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentTarget).Trim();
            var defines = definesString.Split(';');

            if (defines.Contains(DEFINE) == false)
            {
                if (definesString.EndsWith(";", StringComparison.InvariantCulture) == false)
                {
                    definesString += ";";
                }

                definesString += DEFINE;

                PlayerSettings.SetScriptingDefineSymbolsForGroup(currentTarget, definesString);
            }
        }
    }
}
#endif