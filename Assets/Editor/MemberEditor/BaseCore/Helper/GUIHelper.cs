namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    static public class GUIHelper
    {
        static public void ListField<T, Drawer>(IList<T> pSources, bool pDisable, IListDrawerInfo<Drawer> pInfo, Action<int, T> pOnValueChange, Func<T> pDefaultVal, Func<FuncInfo<T, Drawer>, FuncInfo<T, Drawer>> pDrawer, Action pNewList = null)
        {
            if (pSources == null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("list is null", GUILayout.Width(100));
                if (GUILayout.Button("New List", GUILayout.MinWidth(100), GUILayout.ExpandWidth(true)))
                {
                    if (pNewList != null) pNewList();
                }
                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !pDisable;
            EditorGUILayout.LabelField("size", GUILayout.Width(30));
            var tNewCount = EditorGUILayout.IntField(pSources.GetCountIgnoreNull());
            GUI.enabled = true;
            EditorGUILayout.LabelField("show index", GUILayout.Width(70));
            pInfo.ShowIndex = EditorGUILayout.Toggle(pInfo.ShowIndex);

            GUI.enabled = !pDisable;
            if (GUILayout.Button("+", ConstHelper.GetButtonStyle(ButtonSizeType.Normal)))
            {
                ++tNewCount;
            }
            if (GUILayout.Button("-", ConstHelper.GetButtonStyle(ButtonSizeType.Normal)))
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
                pSources.Add(pDefaultVal());
            }

            GUI.enabled = !pDisable;
            bool tBreak = false;
            for (int i = 0, imax = pSources.GetCountIgnoreNull(); i < imax; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (pInfo.ShowIndex)
                {
                    GUI.enabled = true;
                    EditorGUILayout.LabelField(new GUIContent(i.ToString()), GUILayout.MaxWidth(20));
                    GUI.enabled = !pDisable;
                }

                T tResult = default(T);
                if (pDrawer != null)
                {
                    var tFuncInfo = pDrawer(new FuncInfo<T, Drawer>() { value = pSources[i], verIndex = i, drawerInfo = pInfo });
                    if (tFuncInfo.refreshImmediate)
                    {
                        tBreak = true;
                        GUI.FocusControl("");
                        break;
                    }
                    tResult = tFuncInfo.value;
                }

                if (tBreak) break;
                if (GUI.changed)
                {
                    if (pOnValueChange != null)
                    {
                        pOnValueChange(i, tResult);
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