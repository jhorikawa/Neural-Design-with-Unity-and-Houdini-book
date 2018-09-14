using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class RockAgent : Agent {
	public RockEnv rockEnv;

	public int maxRocks {get; set;}
	public int maxFallenCount {get; set;}
	public float speed {get; set;}
	public float envSize{get; set;}
	private float createTrigger = 0;
	private float prevMaxHeight = 0;
	private float prevFillRatio = 0;
	private int fallenCount = 0;
	private float maxHeight = 0;
	private float fillRatio = 0;
	private float edgeDist = 0;
	

	// Use this for initialization

	public override void CollectObservations(){
		AddVectorObs(transform.localPosition / (envSize * 0.5f));

		AddVectorObs(rockEnv.GetHeightGridVals(out maxHeight, out fillRatio));
		AddVectorObs(maxHeight / 10f);
		AddVectorObs(fillRatio);
		AddVectorObs(rockEnv.ClosestDistanceToRocks(transform.localPosition)/envSize);
		//AddVectorObs(rockEnv.GetRocks().Length / (float)maxRocks);
		//AddVectorObs(rockEnv.GetMaxHeight() / RockAcademy.instance.envSize * 0.5f);
		// AddVectorObs(rockEnv.GetAverageHeight() / RockAcademy.instance.envSize * 0.5f);
	}

	public override void AgentAction(float[] vectorAction, string textAction){
		float x, z, trigger, rotx, roty, rotz;
		if(brain.brainType == BrainType.Player){
			x = Random.Range(-1f, 1f);
			z = Random.Range(-1f, 1f);
			rotx = Random.Range(0, 360f);
			roty = Random.Range(0, 360f);
			rotz = Random.Range(0, 360f);
			trigger = Random.Range(-1f, 1f);
		}else{
			x = vectorAction[0];
			z = vectorAction[1];
			rotx = Random.Range(0, 360f);
			roty = Random.Range(0, 360f);
			rotz = Random.Range(0, 360f);
			// rotx = vectorAction[2] * 180f;
			// roty = vectorAction[3] * 180f;
			// rotz = vectorAction[4] * 180f;
			trigger = vectorAction[2];//(vectorAction[5] + 1f) * 0.5f;
		}
		//createTrigger += trigger * 0.1f;

		Vector3 dir = new Vector3(x,0,z);
		dir *= speed;


		Vector3 curPos = transform.localPosition + dir;
		Vector3 newPos = curPos;
		if(Mathf.Abs(curPos.x) > envSize * 0.5f){
			newPos.x = Mathf.Abs(curPos.x) / curPos.x * envSize * 0.5f; 
		}
		if(Mathf.Abs(curPos.z) > envSize * 0.5f){
			newPos.z = Mathf.Abs(curPos.z) / curPos.z * envSize * 0.5f;
		}

		newPos.y = maxHeight + 1f;
		/*
		newPos.y = 100f;

		RaycastHit hit;
		if (Physics.Raycast(newPos, new Vector3(0, -1, 0), out hit, Mathf.Infinity))
        {
            float height = hit.point.y + 1f;
			newPos.y = height;
        }else{
			newPos.y = 1f;
		}
		*/

		transform.localPosition = newPos; 

		


		// Rewarding
		// AddReward(-0.01f);

		if(trigger > 0f && rockEnv.ClosestDistanceToRocks(newPos) > 1.5f){
			RockAcademy.instance.InstantiateRock(newPos, rockEnv.rockParent, new Vector3(rotx, roty, rotz));
			
			RaycastHit hit;
			if(Physics.Raycast(new Vector3(newPos.x, 100f, newPos.z),new Vector3(0, -1f, 0),out hit, float.PositiveInfinity, RockAcademy.instance.collideRockLayer)){
				float dist = 1f - hit.distance / 100f;
				AddReward(dist);
			}else{
				AddReward(-0.01f);
			}

			float edgeDist = Mathf.Min(Mathf.Abs(Mathf.Abs(newPos.x) - envSize * 0.5f), Mathf.Abs(Mathf.Abs(newPos.z) - envSize * 0.5f));
			if(edgeDist < 2f){
				AddReward(-0.02f);
			}else{
				float val = edgeDist / (envSize * 0.5f) * 0.01f;
				AddReward(val);
			}
		}

		if(rockEnv.DeleteFallenRocks() > 0){
			//AddReward(-0.1f);
			//Done();
			fallenCount++;
		}

		

		// if(maxHeight > prevMaxHeight){
		// 	AddReward(0.1f);
		// 	prevMaxHeight = maxHeight;
		// }

		// if(fillRatio > prevFillRatio){
		// 	AddReward(-0.01f);
		// 	prevFillRatio = fillRatio;
		// }

		// if(rockEnv.GetAverageHeight() > prevAveHeight){
		// 	AddReward(0.25f);
		// 	prevAveHeight = rockEnv.GetAverageHeight();
		// }

		// if(fallenCount > maxFallenCount){
		// 	// AddReward(-1f);
		// 	Done();
		// }

		if(rockEnv.GetRocks().Length >= maxRocks){
			Done();
		}
	}

	public override void AgentReset(){
		transform.localPosition = new Vector3(Random.Range(-10f, 10f), 20f, Random.Range(-10f, 10f));
		prevMaxHeight = 0;
		// prevAveHeight = 0;
		fallenCount = 0;
		prevFillRatio = 0;

		rockEnv.InitRocks();
	}


}
