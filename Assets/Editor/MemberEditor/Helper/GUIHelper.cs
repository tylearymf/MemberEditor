namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    static public class GUIHelper
    {
        static public void DrawerListItem<T>(List<T> pSources, List<string> pLabelNames, Rect pRect, bool pDisable, ref bool pShowIndex, int pIntervalWidth, Action<int, List<object>> pOnValueChange, params Func<Rect, T, object>[] pInputFields) where T : new()
        {
            if (pSources.IsNullOrEmpty() || pLabelNames.IsNullOrEmpty() || pInputFields.IsNullOrEmpty() || (pLabelNames.GetCountIgnoreNull() != pInputFields.GetCountIgnoreNull())) return;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !pDisable;
            EditorGUILayout.LabelField("size", GUILayout.Width(30));
            var tNewCount = EditorGUILayout.IntField(pSources.Count, GUILayout.MaxWidth(70));
            GUI.enabled = true;
            EditorGUILayout.LabelField("show index", GUILayout.Width(70));
            pShowIndex = EditorGUILayout.Toggle(pShowIndex, GUILayout.MaxWidth(100));
            EditorGUILayout.EndHorizontal();
            while (tNewCount < pSources.Count)
            {
                pSources.RemoveAt(pSources.Count - 1);
            }
            while (tNewCount > pSources.Count)
            {
                pSources.Add(new T());
            }

            var pFieldCount = pLabelNames.Count;
            var tSingleItemWidth = (pRect.size.x - pIntervalWidth * (pFieldCount - 1)) / pFieldCount;
            GUI.enabled = !pDisable;
            for (int i = 0, imax = pSources.Count; i < imax; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (pShowIndex)
                {
                    EditorGUILayout.LabelField(new GUIContent(i.ToString()), GUILayout.MaxWidth(20));
                }

                List<object> tResults = new List<object>();
                for (int j = 0; j < pFieldCount; j++)
                {
                    if (!pLabelNames.IsNullOrEmpty() && j < pLabelNames.Count)
                    {
                        EditorGUILayout.LabelField(pLabelNames[j], GUILayout.Width(pLabelNames[j].GetLabelWidth()));
                    }

                    if (pInputFields[j] != null)
                    {
                        tResults.Add(pInputFields[j](Rect.zero, pSources[i]));
                    }
                }

                if (GUI.changed)
                {
                    if (pOnValueChange != null)
                    {
                        pOnValueChange(i, tResults);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            GUI.enabled = true;
            EditorGUILayout.EndVertical();
        }

        static public Vector3 DrawerVector3(Vector3 pVal, string pName)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(pName, GUILayout.Width(pName.GetLabelWidth() + 5));
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("X", GUILayout.Width(15));
            pVal.x = EditorGUILayout.FloatField(pVal.x);
            EditorGUILayout.LabelField("Y", GUILayout.Width(15));
            pVal.y = EditorGUILayout.FloatField(pVal.y);
            EditorGUILayout.LabelField("Z", GUILayout.Width(15));
            pVal.z = EditorGUILayout.FloatField(pVal.z);
            EditorGUILayout.EndHorizontal();

            return pVal;
        }

        static public Matrix4x4 DrawerMatrix4x4(Matrix4x4 pValue)
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal("Box");
                {
                    EditorGUILayout.LabelField(string.Empty, GUILayout.MaxWidth(50), GUILayout.ExpandWidth(true));
                    EditorGUILayout.LabelField("0", GUILayout.MaxWidth(50), GUILayout.ExpandWidth(true));
                    EditorGUILayout.LabelField("1", GUILayout.MaxWidth(50), GUILayout.ExpandWidth(true));
                    EditorGUILayout.LabelField("2", GUILayout.MaxWidth(50), GUILayout.ExpandWidth(true));
                    EditorGUILayout.LabelField("3", GUILayout.MaxWidth(50), GUILayout.ExpandWidth(true));
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.BeginVertical("Box", GUILayout.Width(20));
                    {
                        EditorGUILayout.LabelField("0", GUILayout.MaxWidth(20), GUILayout.ExpandWidth(true));
                        EditorGUILayout.LabelField("1", GUILayout.MaxWidth(20), GUILayout.ExpandWidth(true));
                        EditorGUILayout.LabelField("2", GUILayout.MaxWidth(20), GUILayout.ExpandWidth(true));
                        EditorGUILayout.LabelField("3", GUILayout.MaxWidth(20), GUILayout.ExpandWidth(true));
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.Space();
                        for (int row = 0; row < 4; row++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            for (int cell = 0; cell < 4; cell++)
                            {
                                pValue[row, cell] = EditorGUILayout.FloatField(pValue[row, cell], GUILayout.MaxWidth(50), GUILayout.ExpandWidth(true));
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            return pValue;
        }
    }
}