using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript3 : MonoBehaviour
{
    string a3;
    float b3;
    public bool c3;
    public string d3 { get; private set; }
    public long e3 { set; get; }
    static bool f3;
    static bool g3 { get; set; }

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
