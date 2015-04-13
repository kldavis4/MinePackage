using UnityEngine;
using System.Collections;

public class RotateAroundAxis : MonoBehaviour {
    public Vector3 Axis;
    public float Angle;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        transform.RotateAround(Axis, Angle);
	
	}
}
