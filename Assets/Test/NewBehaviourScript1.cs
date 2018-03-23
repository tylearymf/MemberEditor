using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript1 : MonoBehaviour
{
    string a1;
    float b1;
    public bool c1;
    public string d1 { get; private set; }
    public long e1 { set; get; }
    static bool f1;
    static bool g1 { get; set; }

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
