using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using HoudiniEngineUnity;

public class NaviAcademy : Academy {
	public static NaviAcademy instance;
	/* 
	public GameObject controllerObject;
	public GameObject agentObject;
	public GameObject goalObject;
	public LayerMask goalLayerMask;

	[HideInInspector]
	public float height;
	[HideInInspector]
	public float radius;
	[HideInInspector]
	public float[] mountPos;
	[HideInInspector]
	public float size;// {get; set;}
	private HEU_HoudiniAsset houdiniAsset;
	*/
	public void Start(){
		//mountPos = new float[]{0.0f, 0.0f};
	}

	public override void InitializeAcademy()
	{
		instance = this;

		Brain brain = GetComponentInChildren<Brain>();
		NaviAgent[] naviAgents = GameObject.FindObjectsOfType<NaviAgent>();
		foreach(NaviAgent naviAgent in naviAgents){
			naviAgent.brain = brain;
		}

		// houdiniAsset = controllerObject.GetComponent<HEU_HoudiniAssetRoot>() != null ? controllerObject.GetComponent<HEU_HoudiniAssetRoot>()._houdiniAsset : null;

		// HEU_ParameterAccessor.GetFloat(houdiniAsset, "size", out size);

		//UpdateEnvironment();
	}

	public override void AcademyReset()
	{
		UpdateAllEnvironments();
	}

	public override void AcademyStep()
	{
		// if(prevPos != agentObject.transform.localPosition){
		// 	Vector3 pos = agentObject.transform.localPosition;
		// 	float[] agentpos = {-pos.x, pos.y, pos.z};
		// 	HEU_ParameterAccessor.SetFloats(houdiniAsset, "agent_pos", agentpos);
		// 	houdiniAsset.RequestCook(true, false, true, true);
		// 	prevPos = pos;
		// }

	}

	private void UpdateAllEnvironments(){
		NaviEnv[] naviEnvs = GameObject.FindObjectsOfType<NaviEnv>();

		foreach(NaviEnv naviEnv in naviEnvs){
			naviEnv.UpdateEnvironment();
		}
	}

	// private void UpdateEnvironment(){
		
	// 	if(houdiniAsset != null){

	// 		float height = Random.Range(0.0f, 5.0f);
	// 		HEU_ParameterAccessor.SetFloat(houdiniAsset, "height", height);

	// 		float radius = Random.Range(1.0f, 10.0f);
	// 		HEU_ParameterAccessor.SetFloat(houdiniAsset, "radius", radius);
			
	// 		float[] mountPos = {Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)};
	// 		HEU_ParameterAccessor.SetFloats(houdiniAsset, "mountain_pos", mountPos);

	// 		houdiniAsset.RequestCook(true, false, true, true);

	// 		// Add MeshCollider
	// 		MeshRenderer meshRenderer = controllerObject.GetComponentInChildren<MeshRenderer>();
	// 		MeshCollider collider = meshRenderer.GetComponent<MeshCollider>();
	// 		if(collider != null){
	// 			Destroy(collider);
	// 		}
	// 		meshRenderer.gameObject.AddComponent<MeshCollider>();

	// 		// Update Goal Position
	// 		goalObject.transform.localPosition = new Vector3(Random.Range(-size * 0.5f, size * 0.5f), 20, Random.Range(-size * 0.5f, size * 0.5f));
	// 		RaycastHit hit;
	// 		if (Physics.Raycast(new Vector3(goalObject.transform.localPosition.x, 20f, goalObject.transform.localPosition.z), new Vector3(0,-1f,0), out hit, Mathf.Infinity, goalLayerMask))
	// 		{
	// 			goalObject.transform.localPosition = hit.point + new Vector3(0,goalObject.transform.localScale.y * 0.5f,0);
	// 		}

	// 		agentObject.GetComponent<NaviAgent>().UpdateAgentPosition(true);
	// 	}
	// }
}
