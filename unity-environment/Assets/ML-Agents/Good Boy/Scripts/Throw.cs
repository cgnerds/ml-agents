using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throw : MonoBehaviour {

	[Header("GENERAL")]
	public bool canThrow;
	public Transform item; //item we want to throw
	public Transform returnPoint;
	//refs
	[HideInInspector]
	public Rigidbody itemRB; //rb ref
	[HideInInspector]
	public Collider itemCol; //col ref
	[HideInInspector]
	public DogAgent dogAgent; //dog in the game

	[Header("HOLDING ITEM SETTINGS")]
	public Vector3 holdingPos;
	public Vector3 holdingPosOffset;
	public float holdingItemTargetVelocity;
	public float holdingItemMaxVelocityChange;

	[Header("THROWING FORCE")]
	public float throwSpeed;
	public Vector3 throwDir;

	[Header("SOUND")]
	public List<AudioClip> throwSounds = new List <AudioClip>();


	AudioSource audioSourceSFX;
	Vector3 startingPos;
	Vector3 currentPos;
	Vector3 previousPos;

	
	bool currentlyTouching;
	// bool directionChosen;
	Touch currentTouch;
	bool usingTouchInput;
	bool usingMouseInput;
	Camera cam;

	// Use this for initialization
	void Awake () {
		cam = Camera.main;
		canThrow = true;
		dogAgent = FindObjectOfType<DogAgent>();
		itemRB = item.GetComponent<Rigidbody>();
		itemCol = item.GetComponent<Collider>();
		audioSourceSFX = gameObject.AddComponent<AudioSource>();
	}
	
	void StartSwipe()
	{
		startingPos = cam.ScreenToViewportPoint(Input.mousePosition) - new Vector3(0.5f, 0.5f, 0.0f);
		usingTouchInput = true;
		currentlyTouching = true;
		currentTouch = Input.GetTouch(0);
		if(!dogAgent.returningItem)
		{
			dogAgent.target = item;
		}
	}

	void StartMouseDrag()
	{
		startingPos = cam.ScreenToViewportPoint(Input.mousePosition) - new Vector3(0.5f, 0.5f, 0.0f);
		usingMouseInput = true;
		currentlyTouching = true;
		if(!dogAgent.returningItem)
		{
			dogAgent.target = item;
		}
	}

	void ThrowItem()
	{
		canThrow = false;
		audioSourceSFX.PlayOneShot(throwSounds[Random.Range( 0, throwSounds.Count)], .25f);
		itemRB.velocity *= .5f;
		throwDir = (currentPos - startingPos);
		var dir = cam.transform.TransformDirection(throwDir) + cam.transform.forward;
		dir.y = 0;
		itemRB.AddForce(dir * throwSpeed, ForceMode.VelocityChange);
		StartCoroutine(DelayedThrow());
	}

	//dog should wait a few seconds before it goes to pick up the item
	IEnumerator DelayedThrow()
	{
		float elapsed = 0;
		while(elapsed < 2)
		{
			elapsed += Time.deltaTime;
			yield return null;
		}
		StartCoroutine(dogAgent.GoGetItemGame());
	}

	void FixedUpdate()
	{
		if(currentlyTouching)
		{
			if(usingTouchInput)
			{
				currentTouch = Input.GetTouch(0);

				currentPos = cam.ScreenToViewportPoint(currentTouch.position) - new Vector3(0.5f, 0.5f, 0.0f);
			}
			if(usingMouseInput)
			{
				currentPos = cam.ScreenToViewportPoint(Input.mousePosition) - new Vector3(0.5f, 0.5f, 0.0f);
			}
			holdingPos = cam.transform.TransformPoint(holdingPosOffset + (currentPos * 2));
			Vector3 moveToPos = holdingPos - itemRB.position;  //cube needs to go to the standard Pos
			Vector3 velocityTarget = moveToPos * holdingItemTargetVelocity * Time.deltaTime; //not sure of the logic here, but it modifies velTarget
            itemRB.velocity = Vector3.MoveTowards(itemRB.velocity, velocityTarget, holdingItemMaxVelocityChange);
			previousPos = currentPos;
		}
	}

	void Update()
	{
		if(canThrow)
		{
			if (Input.touchCount > 0 && !currentlyTouching)
			{
				currentTouch = Input.GetTouch(0);
				if(currentTouch.phase == TouchPhase.Began)
				{
					StartSwipe();
				}
			}

			if(usingTouchInput && currentlyTouching)
			{
				currentTouch = Input.GetTouch(0);
				if(currentTouch.phase == TouchPhase.Ended)
				{
					currentlyTouching = false;
					ThrowItem();
				}
			}

			if (Input.GetMouseButtonDown(0) && !currentlyTouching)
			{
				StartMouseDrag();
			}

			if(usingMouseInput && currentlyTouching)
			{
				if(Input.GetMouseButtonUp(0))
				{
					currentlyTouching = false;
					ThrowItem();
				}
			}
		}
    }
}
