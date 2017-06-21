using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Parent
{
	public Child a;
}
[System.Serializable]
public class Child
{
	public int n;

	public Child()
	{
	}

	public Child(Child c)
	{
		n = c.n;
	}

}

public class TestClass : MonoBehaviour {
	public Child c1;
	public Child c2;
	// Use this for initialization
	void Start () {
		c1 = new Child();
		c1.n = 2;
		c2 = new Child(c1);
		c2.n = 3;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
