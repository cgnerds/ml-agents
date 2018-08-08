using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;


public class DogAgent : Agent {

    [Header("General")] 
    public Transform target; //stick
    public Transform spawnArea; //object in scene. will be used to spawn item during training
    public Transform mouthPosition; //this needs to be set in the inspector

    //these items should be set in the inspector
    [Header("Body Parts")] 
    public Transform body;
    public Transform leg0_upper;
    public Transform leg1_upper;
    public Transform leg2_upper;
    public Transform leg3_upper;
    public Transform leg0_lower;
    public Transform leg1_lower;
    public Transform leg2_lower;
    public Transform leg3_lower;

    [Header("Rewards")] 
	public float movingTowardsDot; //used for rewards
    public bool rewardMovingTowardsTarget; //agent should move towards target
    public bool rewardUseTimePenalty; //hurry up

    [Header("Body Rotation")] 
    public float maxTurnSpeed;
    public ForceMode turningForceMode;

    [Header("Fetch State")] 
    public bool runningToItem;
    public bool returningItem;

    [Header("Sounds")] 
	public List<AudioClip> barkSounds = new List <AudioClip>();
	AudioSource audioSourceSFX;



    bool isNewDecisionStep;
    int currentDecisionStep;
    Throw throwController;
    JointDriveController jdController;
    Bounds spawnAreaBounds;
	Vector3 dirToTarget;

    // void InitializeAgent()
    void Awake()
    {
        runningToItem = false;
        returningItem = false;
        audioSourceSFX = body.gameObject.AddComponent<AudioSource>();
        audioSourceSFX.spatialBlend = .75f;
        audioSourceSFX.minDistance = .7f;
        audioSourceSFX.maxDistance = 5;
        throwController = FindObjectOfType<Throw>();
        if(brain.brainType == BrainType.External)//we are training
		{
            target = throwController.item;
            SpawnItemTraining();
		}
		else if(brain.brainType == BrainType.Internal || brain.brainType == BrainType.Player)//we are doing inference
		{
            target = throwController.returnPoint;
            StartCoroutine(BarkBarkGame());
		}

        //Joint Drive
        jdController = GetComponent<JointDriveController>();
        //Setup each body part
        jdController.SetupBodyPart(body);
        jdController.SetupBodyPart(leg0_upper);
        jdController.SetupBodyPart(leg0_lower);
        jdController.SetupBodyPart(leg1_upper);
        jdController.SetupBodyPart(leg1_lower);
        jdController.SetupBodyPart(leg2_upper);
        jdController.SetupBodyPart(leg2_lower);
        jdController.SetupBodyPart(leg3_upper);
        jdController.SetupBodyPart(leg3_lower);

        currentDecisionStep = 1;
        spawnAreaBounds = spawnArea.GetComponent<Collider>().bounds;
    }



    /// <summary>
    /// Use the ground's bounds to pick a random spawn position.
    /// </summary>
    public void SpawnItemTraining()
    {
        Vector3 randomSpawnPos = Vector3.zero;
        float randomPosX = Random.Range(-spawnAreaBounds.extents.x, spawnAreaBounds.extents.x);
        float randomPosZ = Random.Range(-spawnAreaBounds.extents.z, spawnAreaBounds.extents.z);
        target.position = spawnArea.transform.position + new Vector3(randomPosX, 1f, randomPosZ);
    }

	/// <summary>
    /// Agent touched the target
    /// </summary>
	public void TouchedTargetTraining()
	{
		AddReward(1); //good boy
		SpawnItemTraining();
		Done();
	}


    public void PickUpItemGame()
    {
        throwController.itemCol.enabled = false;
        throwController.itemRB.isKinematic = true;
        throwController.item.position = mouthPosition.position;
        throwController.item.rotation = mouthPosition.rotation;
        throwController.item.SetParent(mouthPosition);

        target = throwController.returnPoint;
        returningItem = true;
    }

    public void DropItemGame()
    {
        if(throwController)
        {
            throwController.itemRB.isKinematic = false;
            throwController.item.parent = null;
            throwController.itemCol.enabled = true;
        }
    }

    /// <summary>
    /// Add relevant information on each body part to observations.
    /// </summary>
    public void CollectObservationBodyPart(BodyPart bp)
    {
        var rb = bp.rb;
        AddVectorObs(bp.groundContact.touchingGround ? 1 : 0); // Is this bp touching the ground
        if(bp.rb.transform != body)
        {
            AddVectorObs(bp.currentXNormalizedRot);
            AddVectorObs(bp.currentYNormalizedRot);
            AddVectorObs(bp.currentZNormalizedRot);
            AddVectorObs(bp.currentStrength/jdController.maxJointForceLimit);
        }
    }

    public override void CollectObservations()
    {
        AddVectorObs(dirToTarget.normalized);
        AddVectorObs(body.localPosition);
        AddVectorObs(jdController.bodyPartsDict[body].rb.velocity);
        AddVectorObs(jdController.bodyPartsDict[body].rb.angularVelocity);
        AddVectorObs(body.forward); //the capsule is rotated so this is local forward
        AddVectorObs(body.up); //the capsule is rotated so this is local forward
        foreach (var bodyPart in jdController.bodyPartsDict.Values)
        {
            CollectObservationBodyPart(bodyPart);
        }
    }

    //We only need to change the joint settings based on decision freq.
    public void IncrementDecisionTimer()
    {
        if(currentDecisionStep == this.agentParameters.numberOfActionsBetweenDecisions || this.agentParameters.numberOfActionsBetweenDecisions == 1)
        {
            currentDecisionStep = 1;
            isNewDecisionStep = true;
        }
        else
        {
            currentDecisionStep ++;
            isNewDecisionStep = false;
        }
    }


    //The speed of body rotation is being controlled by the neural net
    void RotateBody(float act)
    {
        float speed = Mathf.Lerp(0, maxTurnSpeed, Mathf.Clamp(act, 0, 1));
        Vector3 rotDir = dirToTarget; 
        rotDir.y = 0;
        jdController.bodyPartsDict[body].rb.AddForceAtPosition(rotDir.normalized * speed * Time.deltaTime, body.forward, turningForceMode); //tug on the front
        jdController.bodyPartsDict[body].rb.AddForceAtPosition(-rotDir.normalized * speed * Time.deltaTime, -body.forward, turningForceMode); //tug on the back
    }

    public IEnumerator BarkBarkGame()
    {       
        while(true)
        {
            if(!returningItem)
            {
                audioSourceSFX.PlayOneShot(barkSounds[Random.Range( 0, barkSounds.Count)], 1);
            }
            yield return new WaitForSeconds(Random.Range(1, 10));
        }
    }


    //Only Used in Game Mode. Triggered when the player throws the item.
    public IEnumerator GoGetItemGame()
    {   
        //GO GET THE STICK
        target = throwController.item;
        runningToItem = true;

        //WHEN WE'RE IN RANGE
        while(dirToTarget.sqrMagnitude > 1f) //wait until we are close
        {
            yield return null;
        }
        PickUpItemGame();
        runningToItem = false;

        //RETURN THE STICK
        target = throwController.returnPoint;
        returningItem = true;
        yield return null; //wait a tic

        //WHEN WE'RE IN RANGE
        while(dirToTarget.sqrMagnitude > 1f) //wait until we are close
        {
            yield return null;
        }
        DropItemGame();
        returningItem = false;
        throwController.canThrow = true;
    }


	public override void AgentAction(float[] vectorAction, string textAction)
    {
        dirToTarget = target.position - jdController.bodyPartsDict[body].rb.position;
        if(brain.brainType == BrainType.External)//we are training
		{
            if(!IsDone() && dirToTarget.magnitude < 1f)
            {
                TouchedTargetTraining();
            }
		}

        if(isNewDecisionStep)
        {
            var bpDict = jdController.bodyPartsDict;
            int i = -1;

            bpDict[leg0_upper].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[leg1_upper].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[leg2_upper].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[leg3_upper].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[leg0_lower].SetJointTargetRotation(vectorAction[++i], 0, 0);
            bpDict[leg1_lower].SetJointTargetRotation(vectorAction[++i], 0, 0);
            bpDict[leg2_lower].SetJointTargetRotation(vectorAction[++i], 0, 0);
            bpDict[leg3_lower].SetJointTargetRotation(vectorAction[++i], 0, 0);

            //update joint drive settings
            bpDict[leg0_upper].SetJointStrength(vectorAction[++i]);
            bpDict[leg1_upper].SetJointStrength(vectorAction[++i]);
            bpDict[leg2_upper].SetJointStrength(vectorAction[++i]);
            bpDict[leg3_upper].SetJointStrength(vectorAction[++i]);
            bpDict[leg0_lower].SetJointStrength(vectorAction[++i]);
            bpDict[leg1_lower].SetJointStrength(vectorAction[++i]);
            bpDict[leg2_lower].SetJointStrength(vectorAction[++i]);
            bpDict[leg3_lower].SetJointStrength(vectorAction[++i]);

            RotateBody(vectorAction[++i]); 
        }

        var bodyRotationPenalty = -.001f * vectorAction[20]; //rotation strength.
        AddReward(bodyRotationPenalty);

        // Set reward for this step according to mixture of the following elements.
        if(rewardMovingTowardsTarget){RewardFunctionMovingTowards();}
        if(rewardUseTimePenalty){RewardFunctionTimePenalty();}
        IncrementDecisionTimer();
    }
	
    //Reward moving towards target & Penalize moving away from target.
    void RewardFunctionMovingTowards()
    {
		movingTowardsDot = Vector3.Dot(jdController.bodyPartsDict[body].rb.velocity, dirToTarget.normalized); 
        AddReward(0.01f * movingTowardsDot);
    }

    //Time penalty - HURRY UP
    void RewardFunctionTimePenalty()
    {
        AddReward(- 0.001f);  //0.001f chosen by experimentation.
    }
    

	/// <summary>
    /// Loop over body parts and reset them to initial conditions.
    /// </summary>
    public override void AgentReset()
    {
        // foreach (var bodyPart in jdController.bodyPartsDict.Values)
        // {
        //     bodyPart.Reset();
        // }
        currentDecisionStep = 1;
        isNewDecisionStep = true;
    }
}