using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bandit : MonoBehaviour {
	public Material Gold;
	public Material Silver;
	public Material Bronze;
	private MeshRenderer mesh;
	private Material reset;

	// Use this for initialization
	void Start () {
		mesh = GetComponent<MeshRenderer>();
		reset = mesh.material;
	}

	public int PullArm(int arm)
	{
		var reward = 0;
		switch(arm)
		{
			case 1:
			    mesh.material = Gold;
				reward = 3;
				break;
			case 2:
			    mesh.material = Bronze;
				reward = 1;
				break;
			case 3:
			    mesh.material = Bronze;
				reward = 1;
				break;
			case 4:
			    mesh.material = Silver;
				reward = 2;
				break;
		}

		return reward;
	}

	public void Reset()
	{
		mesh.material = reset;
	}
}
