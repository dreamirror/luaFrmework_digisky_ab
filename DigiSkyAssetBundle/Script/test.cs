using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class test : MonoBehaviour {

    // Use this for initialization
    
    public int a = 1;
	void Start () {
        Random rd = new Random();
        Debug.Log("test=="+ rd.ToString());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
