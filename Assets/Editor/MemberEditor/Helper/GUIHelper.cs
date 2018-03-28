namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public struct FuncInfo<T>
    {
        public T value { get; set; }
        public bool refresh { set; get; }
    }

    public struct FuncInfo1<T>
    {
        public T value { get; set; }
        public int index1 { get; set; }
        public int index2 { get; set; }
    }

    static public class GUIHelper
    {
        static public void ListField<T, Class>(IList<T> pSources, List<string> pLabelNames, bool pDisable, ref bool pShowIndex, Action<int, List<object>> pOnValueChange, BaseDrawer<Class> pDrawer, params Func<FuncInfo1<T>, FuncInfo<T>>[] pInputFields)
        {
            if (pSources == null || pInputFields.IsNullOrEmpty()) return;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !pDisable;
            EditorGUILayout.LabelField("size", GUILayout.Width(30));
            var tNewCount = EditorGUILayout.IntField(pSources.GetCountIgnoreNull(), GUILayout.MaxWidth(70));
            GUI.enabled = true;
            EditorGUILayout.LabelField("show index", GUILayout.Width(70));
            pShowIndex = EditorGUILayout.Toggle(pShowIndex, GUILayout.MaxWidth(100));

            GUI.enabled = !pDisable;
            if (GUILayout.Button("+"))
            {
                ++tNewCount;
            }
            if (GUILayout.Button("-"))
            {
                --tNewCount;
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            while (pSources.GetCountIgnoreNull() > 0 && tNewCount < pSources.GetCountIgnoreNull())
            {
                pSources.RemoveAt(pSources.GetCountIgnoreNull() - 1);
            }
            while (tNewCount > pSources.GetCountIgnoreNull())
            {
                pSources.Add(default(T));
            }

            var pFieldCount = pInputFields.GetCountIgnoreNull();
            GUI.enabled = !pDisable;
            bool tBreak = false;
            for (int i = 0, imax = pSources.GetCountIgnoreNull(); i < imax; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (pShowIndex)
                {
                    EditorGUILayout.LabelField(new GUIContent(i.ToString()), GUILayout.MaxWidth(20));
                }

                var tResults = new List<object>();
                for (int j = 0; j < pFieldCount; j++)
                {
                    if (!pLabelNames.IsNullOrEmpty() && j < pLabelNames.Count)
                    {
                        EditorGUILayout.LabelField(pLabelNames[j], GUILayout.Width(pLabelNames[j].GetLabelWidth(pDrawer)));
                    }

                    if (pInputFields[j] != null)
                    {
                        var tFuncInfo = pInputFields[j](new FuncInfo1<T>() { value = pSources[i], index1 = i, index2 = j });
                        if (tFuncInfo.refresh)
                        {
                            tBreak = true;
                            break;
                        }
                        tResults.Add(tFuncInfo.value);
                    }
                }

                if (tBreak) break;
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

        static public Vector3 Vector3Field<T>(Vector3 pVal, BaseDrawer<T> pDrawer, string pName = "")
        {
            EditorGUILayout.BeginHorizontal();
            if (!pName.IsNullOrEmpty())
            {
                EditorGUILayout.LabelField(pName, GUILayout.Width(pName.GetLabelWidth(pDrawer)));
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            EditorGUILayout.LabelField("X", GUILayout.Width(15));
            pVal.x = EditorGUILayout.FloatField(pVal.x);
            EditorGUILayout.LabelField("Y", GUILayout.Width(15));
            pVal.y = EditorGUILayout.FloatField(pVal.y);
            EditorGUILayout.LabelField("Z", GUILayout.Width(15));
            pVal.z = EditorGUILayout.FloatField(pVal.z);
            EditorGUILayout.EndHorizontal();

            return pVal;
        }

        static public Vector4 Vector4Field<T>(Vector4 pVal, BaseDrawer<T> pDrawer, string pName = "")
        {
            EditorGUILayout.BeginHorizontal();
            if (!pName.IsNullOrEmpty())
            {
                EditorGUILayout.LabelField(pName, GUILayout.Width(pName.GetLabelWidth(pDrawer)));
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            EditorGUILayout.LabelField("X", GUILayout.Width(15));
            pVal.x = EditorGUILayout.FloatField(pVal.x);
            EditorGUILayout.LabelField("Y", GUILayout.Width(15));
            pVal.y = EditorGUILayout.FloatField(pVal.y);
            EditorGUILayout.LabelField("Z", GUILayout.Width(15));
            pVal.z = EditorGUILayout.FloatField(pVal.z);
            EditorGUILayout.LabelField("W", GUILayout.Width(15));
            pVal.w = EditorGUILayout.FloatField(pVal.w);
            EditorGUILayout.EndHorizontal();

            return pVal;
        }

        static public Matrix4x4 Matrix4x4Field(Matrix4x4 pValue)
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