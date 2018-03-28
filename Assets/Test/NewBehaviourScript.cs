using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    Test tt;
    string a;
    float b;
    public bool c;
    public List<string> abc;
    public List<NewBehaviourScript> abcd;

    public string d { get; private set; }
    public long e { set; get; }
    static bool f;
    static bool g { get; set; }
    public Test ttta { set; get; }

    void Start() { }

    void Update() { }

    string A(string a, Vector3 bcdefgasdfsdf, float c, Transform d, long e, int f)
    {
        Debug.Log(a + " " + bcdefgasdfsdf.ToString() + " " + c + " " + (d == null ? "null" : d.name) + " " + e);
        return string.Empty;
    }

    static void B(string b, string c, Test ttt)
    {
        Debug.LogError(b + " " + c + " " + ttt.ToString());
    }

}

public enum Test
{
    A = -1, B = 1, C = 3, D = 5, E = 999
}