using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript4 : MonoBehaviour
{

    string a;
    float b;
    public bool c;

    public string d { get; private set; }
    public long e { set; get; }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    string A(string a, Vector3 b, float c, Transform d, long e, int f)
    {
        Debug.Log(a + " " + b.ToString() + " " + c + " " + (d == null ? "null" : d.name) + " " + e);
        return string.Empty;
    }

    static void B(string b ,string c)
    {
        Debug.LogError(b + " " + c);
    }

    static bool abc;
    static bool abcd { get; set; }
}
