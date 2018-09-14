using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class NaviAgent : Agent {
	public NaviEnv naviEnv;
	public float speed = 0.1f;
	//public float searchDist = 5f;
	public LayerMask raycastLayerMask;
	private CharacterController characterController;
	private Vector3 prevPos;
	private float lastDiscoveryVal = 0f;

	public void Start(){
		characterController = GetComponent<CharacterController>();

	}

	public void UpdateAgentPosition(bool random){
		if(random){
			float size = naviEnv.size - 1f;
			float x = Random.Range(-size * 0.5f, size * 0.5f);
			float z = Random.Range(-size * 0.5f, size * 0.5f);
			transform.localPosition = new Vector3(x, 20f, z);
			prevPos = transform.localPosition;
		}else{
			transform.localPosition = prevPos;
		}
	}
	public override void CollectObservations(){
		AddVectorObs(naviEnv.GetVisibility());
		AddVectorObs(transform.localPosition * 0.1f);

		Vector3 dir = naviEnv.goalObject.transform.localPosition - transform.localPosition;
		dir = dir.normalized * 0.1f;
		//AddVectorObs(dir);
		
		RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, Mathf.Infinity, raycastLayerMask))
        {
			//float dist = dir.magnitude;
			//float hitdist = hit.distance;

			if(hit.transform.gameObject.layer == 15){//} && hitdist < searchDist){
				//float val = Mathf.Clamp(hitdist, 0, searchDist);
				//val /= searchDist;
				//val = 1f - val;
				//dir.Normalize();
				//AddVectorObs(val * dir);
				AddVectorObs(dir);
				lastDiscoveryVal = 1f;
			}else{
				AddVectorObs(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized);
				lastDiscoveryVal = 0;
			}
        }else{
			AddVectorObs(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized);
			lastDiscoveryVal = 0;
		}
		
	}

	public override void AgentAction(float[] vectorAction, string textAction){
		float xMove = vectorAction[0];
		float yMove = vectorAction[1];
		
		characterController.Move(new Vector3(xMove, 0, yMove) * speed);
		
		RaycastHit hit;
        if (Physics.Raycast(new Vector3(transform.position.x, 20f, transform.position.z), new Vector3(0,-1f,0), out hit, Mathf.Infinity, raycastLayerMask))
        {
            transform.position = hit.point + new Vector3(0,transform.localScale.y * 0.5f,0);
        }else{
			Vector3 curpos = transform.localPosition;
			transform.localPosition = new Vector3(curpos.x, transform.localScale.y * 0.5f, curpos.z);
		}

		float size = naviEnv.size;
		//print(size);
		if(Vector3.Distance(naviEnv.goalObject.transform.localPosition, transform.localPosition) < 1f){
			AddReward(1.0f);
			Done();

			//if(brain.brainType == BrainType.Internal){
				naviEnv.UpdateEnvironment();
			//}
		}else if(transform.localPosition.x > naviEnv.size * 0.5f || transform.localPosition.x < -naviEnv.size * 0.5f
			|| transform.localPosition.z > naviEnv.size * 0.5f || transform.localPosition.z < -naviEnv.size * 0.5f){
				AddReward(-1.0f);
				Done();
		}else if(lastDiscoveryVal == 0){
			AddReward(-0.05f);
		}else{
			AddReward(-0.01f);
		}
	}

	public override void AgentReset(){
		UpdateAgentPosition(true);
	}
}
