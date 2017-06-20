using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ParentList
{
	public ChildRent a;
}
[System.Serializable]
public class ChildRent
{
	public int n;
}

public class TestClass : MonoBehaviour {
	public ParentList pl1;
	public ParentList pl2;
	// Use this for initialization
	void Start () {
		pl1.a.n=2;
		pl2.a  = new ChildRent();
		pl2.a = pl1.a;
		pl2.a.n=1;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
