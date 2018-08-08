using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  MLAgents
{
    /// <summary>
    /// Used to store relevant information for acting and learning for each body part in agent.
    /// </summary>
    [System.Serializable]
    public class BodyPart
    {
        [Header("Body Part Info")] 
        public ConfigurableJoint joint;
        public Rigidbody rb;
        [HideInInspector]
        public Vector3 startingPos;
        [HideInInspector]
        public Quaternion startingRot;

        [Header("Ground & Target Contact")] 
        public GroundContact groundContact;
        // public TargetContact targetContact;

        [HideInInspector]
        public JointDriveController thisJDController;

        [Header("Current Joint Settings")] 
        [HideInInspector]
        public float currentStrength;
        public float currentXNormalizedRot;
        public float currentYNormalizedRot;
        public float currentZNormalizedRot;

        /// <summary>
        /// Reset body part to initial configuration.
        /// </summary>
        public void Reset()
        {
            rb.transform.position = startingPos;
            rb.transform.rotation = startingRot;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            if(groundContact)
            {
			    groundContact.touchingGround = false;
            }
            // if(targetContact)
            // {
			//     targetContact.touchingTarget = false;
            // }
        }

         /// <summary>
        /// Apply torque according to defined goal `x, y, z` angle and force `strength`.
        /// </summary>
        public void SetJointTargetRotation(float x, float y, float z)
        {
            x = (x + 1f) * 0.5f;
            y = (y + 1f) * 0.5f;
            z = (z + 1f) * 0.5f;

            var xRot = Mathf.Lerp(joint.lowAngularXLimit.limit, joint.highAngularXLimit.limit, x);
            var yRot = Mathf.Lerp(-joint.angularYLimit.limit, joint.angularYLimit.limit, y);
            var zRot = Mathf.Lerp(-joint.angularZLimit.limit, joint.angularZLimit.limit, z);

            currentXNormalizedRot = Mathf.InverseLerp(joint.lowAngularXLimit.limit, joint.highAngularXLimit.limit, xRot);
            currentYNormalizedRot = Mathf.InverseLerp(-joint.angularYLimit.limit, joint.angularYLimit.limit, yRot);
            currentZNormalizedRot = Mathf.InverseLerp(-joint.angularZLimit.limit, joint.angularZLimit.limit, zRot);

            joint.targetRotation = Quaternion.Euler(xRot, yRot, zRot);
        }



        public void SetJointStrength(float strength)
        {
            var rawVal = ((strength + 1f) * 0.5f) * thisJDController.maxJointForceLimit;
            var jd = new JointDrive
            {
                positionSpring = thisJDController.maxJointSpring,
                positionDamper = thisJDController.jointDampen,
                maximumForce = rawVal
            };
            joint.slerpDrive = jd;
            currentStrength = jd.maximumForce;
        }
    }

    public class JointDriveController : MonoBehaviour {

        //These settings are used when updating the JointDrive settings (the joint's strength)
        [Header("Joint Drive Settings")] 
        [Space(10)] 
        public float maxJointSpring;
        public float jointDampen;
        public float maxJointForceLimit;
        public Dictionary<Transform, BodyPart> bodyPartsDict = new Dictionary<Transform, BodyPart>();
        public List<BodyPart> bodyPartsList = new List<BodyPart>(); //to look at values in inspector, just for debugging


        /// <summary>
        /// Create BodyPart object and add it to dictionary.
        /// </summary>
        public void SetupBodyPart(Transform t)
        {
            BodyPart bp = new BodyPart
            {
                rb = t.GetComponent<Rigidbody>(),
                joint = t.GetComponent<ConfigurableJoint>(),
                startingPos = t.position,
                startingRot = t.rotation
            };
            bp.rb.maxAngularVelocity = 100;

            //add & setup the ground contact script
            bp.groundContact = t.GetComponent<GroundContact>();
            if(!bp.groundContact)
            {
                bp.groundContact = t.gameObject.AddComponent<GroundContact>();
                bp.groundContact.agent = gameObject.GetComponent<Agent>();
            }
            else
            {
                bp.groundContact.agent = gameObject.GetComponent<Agent>();
            }

            // //add & setup the target contact script
            // bp.targetContact = t.GetComponent<TargetContact>();
            // if(!bp.targetContact)
            // {
            //     bp.targetContact = t.gameObject.AddComponent<TargetContact>();
            // }

            bp.thisJDController = this;
            bodyPartsDict.Add(t, bp);
            bodyPartsList.Add(bp);
        }
    }
}