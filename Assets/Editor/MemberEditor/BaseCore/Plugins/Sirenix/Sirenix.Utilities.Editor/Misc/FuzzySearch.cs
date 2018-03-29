#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="FuzzySearch.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
    using System;

    /// <summary>
    /// Compare strings and produce a distance score between them.
    /// </summary>
    public static class FuzzySearch
    {
        /// <summary>
        /// Determines whether if the source is within the search.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target string.</param>
        /// <param name="ignoreCase">Should the algorithm ignore letter case?.</param>
        /// <param name="abbreviation">Should the algorithm attempt to search on an abbreviation of the source?.</param>
        /// <param name="threshold">Threshold for what is considered to be within the search. 0 will return everything and 1 will only return exact matches.</param>
        /// <returns>True if the source is within the search. Otherwise false.</returns>
        public static bool Contains(ref string source, ref string target, float threshold = 0.8f, bool ignoreCase = true, bool abbreviation = true)
        {
            return FuzzySearch.Compare(ref source, ref target, ignoreCase, abbreviation) >= threshold;
        }

        /// <summary>
        /// Compares the target to the source and returns a distance score.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target string.</param>
        /// <param name="ignoreCase">Should the algorithm ignore letter case?.</param>
        /// <param name="abbreviation">Should the algorithm attempt to search on an abbreviation of the source?.</param>
        /// <returns>Distance score. 0 is no match, and 1 is exact match.</returns>
        public static float Compare(ref string source, ref string target, bool ignoreCase = true, bool abbreviation = true)
        {
            if (target.Length == 0) return 1f;
            if (source.Length == 0) return 0f;

            int stringPosition = 0;
            int stringBestMatches = 0;

            int abbreviationPosition = 0;
            int abbreviationBestMatches = 0;

            char prev = '\0';

            for (int i = 0; i < source.Length && stringPosition < target.Length && abbreviationPosition < target.Length; i++)
            {
                char c = source[i];

                // Search
                if (FuzzySearch.CompareCharacters(c, target[stringPosition], ignoreCase))
                {
                    stringPosition++;
                }
                else
                {
                    // Reset
                    stringBestMatches = Math.Max(stringPosition, stringBestMatches);
                    stringPosition = 0;
                }

                // Abbreviation
                if (abbreviation && char.IsLetter(c) && (!char.IsLetterOrDigit(prev) || char.IsUpper(c)))
                {
                    if (FuzzySearch.CompareCharacters(c, target[abbreviationPosition], true)) // Abbreviation always ignore case.
                    {
                        abbreviationPosition++;
                    }
                    else
                    {
                        // Reset
                        abbreviationBestMatches = Math.Max(abbreviationPosition, abbreviationBestMatches);
                        abbreviationPosition = 0;
                    }
                }

                prev = c;
            }

            stringBestMatches = Math.Max(stringPosition, stringBestMatches);
            abbreviationBestMatches = Math.Max(abbreviationPosition, abbreviationBestMatches);

            // Distance
            float distance = (float)stringBestMatches / (float)target.Length;
            if (abbreviation)
            {
                distance = Math.Max((float)abbreviationBestMatches / (float)target.Length, distance);
            }

            // Weight good matches higher, and bad matches lower
            return SmoothStep(0f, 1f, distance);
        }

        private static float SmoothStep(float a, float b, float t)
        {
            t = Math.Min(Math.Max((t - a) / (b - a), 0), 1);
            return t * t * (3.0f - 2.0f * t);
        }

        /// <summary>
        /// Compares the characters.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns></returns>
        private static bool CompareCharacters(char a, char b, bool ignoreCase)
        {
            if (ignoreCase)
            {
                return char.ToLowerInvariant(a) == char.ToLowerInvariant(b);
            }
            else
            {
                return char.IsLower(a) == char.IsLower(b) && char.ToLowerInvariant(a) == char.ToLowerInvariant(b);
            }
        }
    }
}
#endif