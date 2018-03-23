using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript5 : MonoBehaviour
{
    string a5;
    float b5;
    public bool c5;
    public string d5 { get; private set; }
    public long e5 { set; get; }
    static bool f5;
    static bool g5 { get; set; }

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
