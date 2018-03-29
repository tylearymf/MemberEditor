#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinInspectorValidationChecker.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
    using System.Collections.Generic;

    /// <summary>
    /// Use this class to collect all error and warning messages drawn by the GUI.
    /// </summary>
    /// <example>
    /// Usage:
    /// <code>
    /// PropertyTree myPropertyTree;
    ///
    /// OdinInspectorValidationChecker.BeginValidationCheck();
    /// GUIHelper.BeginDrawToNothing();
    /// InspectorUtilities.DrawPropertyTree(this.propertyTree, false, x => true); // Aggressively draws all properties even those hidden by attributes, foldouts, tabs etc...
    /// GUIHelper.EndDrawToNothing();
    /// OdinInspectorValidationChecker.EndValidationCheck();
    ///
    /// var warningMessages = OdinInspectorValidationChecker.WarningMessages;
    /// var errorMessages = OdinInspectorValidationChecker.ErrorMessages;
    ///
    /// </code>
    /// </example>
    /// <example>
    /// A few GUI methods in SirenixEditorGUI such as ErrorMessageBox() and WarningMessageBox() already
    /// registers error and warnings messages when a OdinInspectorValidationChecker session is running.
    /// However you can easily register your own warning and error messages as well.
    /// <code>
    /// if (OdinInspectorValidationChecker.IsRunningValidationCheck)
    /// {
    ///     OdinInspectorValidationChecker.LogError(message);
    ///     OdinInspectorValidationChecker.LogWarning(message);
    /// }
    /// </code>
    /// </example>
    public static class OdinInspectorValidationChecker
    {
        private static bool isRunningValidation;

        private static readonly HashSet<string> warningMessages = new HashSet<string>();

        private static readonly HashSet<string> errorMessages = new HashSet<string>();

        /// <summary>
        /// The warning messages gathered from the last validation check.
        /// </summary>
        public static readonly ImmutableHashSet<string> WarningMessages = new ImmutableHashSet<string>(warningMessages);

        /// <summary>
        /// The error messages gathered from the last validation check.
        /// </summary>
        public static readonly ImmutableHashSet<string> ErrorMessages = new ImmutableHashSet<string>(errorMessages);

        /// <summary>
        /// Begins a validation check.
        /// </summary>
        public static void BeginValidationCheck()
        {
            isRunningValidation = true;
            warningMessages.Clear();
            errorMessages.Clear();
        }

        /// <summary>
        /// Begins a validation check.
        /// </summary>
        public static void EndValidationCheck()
        {
            isRunningValidation = false;
        }

        /// <summary>
        /// Gets a value indicating whether or not a validation is running.
        /// </summary>
        public static bool IsRunningValidationCheck { get { return isRunningValidation; } }

        /// <summary>
        /// Logs an error if a validation check is currently running.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogError(string message)
        {
            if (isRunningValidation)
            {
                errorMessages.Add(message);
            }
        }

        /// <summary>
        /// Logs a warning if a validation check is currently running.
        /// </summary>
        public static void LogWarning(string message)
        {
            if (isRunningValidation)
            {
                warningMessages.Add(message);
            }
        }
    }
}
#endif