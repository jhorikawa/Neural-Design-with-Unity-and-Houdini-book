using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class RockEnv : MonoBehaviour {
	public RockAgent rockAgent;
	public GameObject floorObject;
	public Transform rockParent;
	private int heightGridNum;


	public void Init(Brain brain, float envSize, int maxRocks, int maxFallenCount, float speed, int _heightGridNum){
		floorObject.transform.localScale = new Vector3(envSize, envSize, envSize + 5f);
		rockAgent.brain = brain;
		rockAgent.maxRocks = maxRocks;
		rockAgent.maxFallenCount = maxFallenCount;
		rockAgent.speed = speed;
		rockAgent.envSize = envSize;
		heightGridNum = _heightGridNum;
	}

	// public bool IsReadyToDrop(){
	// 	Rock[] rocks = GetRocks();
	// 	foreach(Rock rock in rocks){
	// 		Rigidbody rigidbody = rock.GetComponent<Rigidbody>();
	// 		if(rigidbody.velocity.magnitude > 0.05f){
	// 			return false;
	// 		}
	// 	}
	// 	return true;
	// }

	// public float GetMaxHeight(){
	// 	Rock[] rocks = GetRocks();
	// 	float maxHeight = 0;
	// 	foreach(Rock rock in rocks){
	// 		Rigidbody rigidbody = rock.GetComponent<Rigidbody>();
	// 		if(rigidbody.velocity.magnitude < 0.05f){
	// 			if(maxHeight < rock.transform.localPosition.y){
	// 				maxHeight = rock.transform.localPosition.y;
	// 			}
	// 		}
	// 	}
	// 	return maxHeight;
	// }

	// public float GetAverageHeight(){
	// 	Rock[] rocks = GetRocks();
	// 	float aveHeight = 0;
	// 	int count = 0;
	// 	foreach(Rock rock in rocks){
	// 		Rigidbody rigidbody = rock.GetComponent<Rigidbody>();
	// 		if(rigidbody.velocity.magnitude < 0.01f){
	// 			aveHeight += rock.transform.localPosition.y;
	// 			count++;
	// 		}
	// 	}
	// 	if(count > 0){
	// 		aveHeight /= (float)count;
	// 	}
		
	// 	return aveHeight;
	// }

	public int DeleteFallenRocks(){
		int fallenCount = 0;

		Rock[] rocks = GetRocks();
		foreach(Rock rock in rocks){
			if(rock.transform.localPosition.y < 0){
				Destroy(rock.gameObject);
				fallenCount++;
			}
		}

		return fallenCount;
	}

	public void InitRocks(){
		Rock[] rocks = GetRocks();
		foreach(Rock rock in rocks){
			Destroy(rock.gameObject);
		}
	}

	public Rock[] GetRocks(){
		Rock[] rocks = rockParent.GetComponentsInChildren<Rock>();

		return rocks;
	}

	public void SetRockCollidables(){
		Rock[] rocks = GetRocks();
		foreach(Rock rock in rocks){
			Rigidbody rigidbody = rock.GetComponent<Rigidbody>();
			if(rigidbody.velocity.magnitude > 0.025f){
				rock.gameObject.layer = LayerMask.NameToLayer("uncollideRock");
			}else{
				rock.gameObject.layer = LayerMask.NameToLayer("collideRock");
			}
		}
	}

	public float ClosestDistanceToRocks(Vector3 pos){
		Rock[] rocks = GetRocks();
		float minDist = float.PositiveInfinity;
		foreach(Rock rock in rocks){
			if(rock.GetComponent<Rigidbody>().velocity.magnitude > 0.05f){
				float dist = Vector3.Distance(pos, rock.transform.localPosition);
				if(minDist > dist){
					minDist = dist;
				}
			}
		}

		minDist = Mathf.Clamp(minDist, 0, rockAgent.envSize);

		return minDist;
	}

	public List<float> GetHeightGridVals(out float maxHeight, out float fillRatio){
		SetRockCollidables();

		List<float> gridVals = new List<float>();

		float envSize = rockAgent.envSize - 0.1f;
		maxHeight = -1f;
		
		int fillCount = 0;
		for(int i=0; i<heightGridNum; i++){
			float x = -envSize * 0.5f + envSize / (float)(heightGridNum-1) * i;
			
			for(int n=0; n<heightGridNum; n++){
				float z = -envSize * 0.5f + envSize / (float)(heightGridNum-1) * n;

				Vector3 gridPos = new Vector3(x, 100f, z);

				float val = 0;
				RaycastHit hit;
				
				if(Physics.Raycast(gridPos, new Vector3(0, -1f, 0), out hit, float.PositiveInfinity, RockAcademy.instance.collideRockLayer)){
					val = 100f - hit.distance;
				}

				if(maxHeight < val){
					maxHeight = val;
				}

				if(val > 0.01f){
					fillCount++;
				}

				gridVals.Add(val);

			}
		}
		fillRatio = fillCount / (float)gridVals.Count;

		return gridVals;
	}
}
