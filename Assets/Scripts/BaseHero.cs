using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHero : MonoBehaviour {
	public int id;
	public string name;
	public float hp;
	public float atk;
	public float def;
	public float spd;
	// Use this for initialization
	void Start () {
		
	}

	public void InitHero(int _id)
	{
		id = _id;
		
	}

	// Update is called once per frame
	void Update () {
		
	}
}
