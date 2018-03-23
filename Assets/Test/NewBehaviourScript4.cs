using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript4 : MonoBehaviour
{
    string a4;
    float b4;
    public bool c4;
    public string d4 { get; private set; }
    public long e4 { set; get; }
    static bool f4;
    static bool g4 { get; set; }

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
