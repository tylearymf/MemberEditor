using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    Test tt;
    public string a;
    string a1;
    public float b;
    public bool c;
    public List<string> abc;
    public List<TestInfo> abcd = new List<TestInfo>()
    {
        new TestInfo(){ a = "1", b = 2, c = true,},
        new TestInfo(){ a = "2", b = 3, c = false,},
        new TestInfo(){ a = "3", b = 4, c = true,},
        new TestInfo(){ a = "4", b = 5, c = false,},
    };
    public string d { get; private set; }
    public string d1 { get; private set; }
    public long e { set; get; }
    static bool f;
    static bool g { get; set; }
    public Test ttta { set; get; }
    public List<string> listaaa { set; get; }

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

    void AA(List<string> aa)
    {
        var a = string.Empty;
        foreach (var item in aa)
        {
            a += " " + item;
        }
        Debug.Log(a);
    }

    void AA2(List<NewBehaviourScript> aa)
    {

    }
    void AA1(string a, Vector3 bcdefgasdfsdf, float c, Transform d, long e, int f, List<string> absfasdfsafsa)
    {
        a = a + " " + bcdefgasdfsdf.ToString() + " " + c + " " + (d == null ? "null" : d.name) + " " + e;
        foreach (var item in absfasdfsafsa)
        {
            a += " " + item;
        }
        Debug.Log(a);
    }

    string B1(string a)
    {
        return a;
    }

    string B2(string a)
    {
        return a;
    }
}

public enum Test
{
    A = -1, B = 1, C = 3, D = 5, E = 999
}