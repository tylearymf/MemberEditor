using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript6 : MonoBehaviour
{
    string a6;
    float b6;
    public bool c6;
    public string d6 { get; private set; }
    public long e6 { set; get; }
    static bool f6;
    static bool g6 { get; set; }

    void Start() { }

    void Update() { }

    string A(string a, Vector3 b, float c, Transform d, long e, int f)
    {
        Debug.Log(a + " " + b.ToString() + " " + c + " " + (d == null ? "null" : d.name) + " " + e);
        return string.Empty;
    }

    static void B(string b, string c)
    {
        Debug.LogError(b + " " + c);
    }
}
