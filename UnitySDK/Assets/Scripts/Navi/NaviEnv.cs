using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using HoudiniEngineUnity;

public class NaviEnv : MonoBehaviour {
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
	public float size;
	private HEU_HoudiniAsset houdiniAsset;

	// Use this for initialization
	void Start () {
		houdiniAsset = gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>() != null ? gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>()._houdiniAsset : null;
		HEU_ParameterAccessor.GetFloat(houdiniAsset, "size", out size);

		mountPos = new float[]{0.0f, 0.0f};

		UpdateEnvironment();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public float GetVisibility(){
		float visibility = 0;
		if(houdiniAsset != null){
			var atts = houdiniAsset.GetAttributesStores();
			
			if(atts.Count > 0){
				var attributesStore = atts[0];

				var vis = attributesStore.GetAttributeData("visibility");
				var values = vis._floatValues;
				
				if(values.Length > 0){
					visibility = values[0];
				}
			}
		}

		return visibility;
	}

	public void UpdateEnvironment(){
		
		if(houdiniAsset != null){

			float height = Random.Range(0.0f, 5.0f);
			HEU_ParameterAccessor.SetFloat(houdiniAsset, "height", height);

			float radius = Random.Range(3.0f, 10.0f);
			HEU_ParameterAccessor.SetFloat(houdiniAsset, "radius", radius);
			
			float[] mountPos = {Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)};
			HEU_ParameterAccessor.SetFloats(houdiniAsset, "mountain_pos", mountPos);

			houdiniAsset.RequestCook(true, false, true, true);

			// Add MeshCollider
			MeshCollider[] meshColliders = gameObject.GetComponentsInChildren<MeshCollider>();
			foreach(MeshCollider meshCollider in meshColliders){
				Destroy(meshCollider);
			}
			MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
			foreach(MeshRenderer meshRenderer in meshRenderers){
				if(meshRenderer.enabled){
					meshRenderer.gameObject.AddComponent<MeshCollider>();
				}
			}

			// Update Goal Position
			float planeSize = size - 1f;
			goalObject.transform.localPosition = new Vector3(Random.Range(-planeSize * 0.5f, planeSize * 0.5f), 20, Random.Range(-planeSize * 0.5f, planeSize * 0.5f));
			RaycastHit hit;
			if (Physics.Raycast(goalObject.transform.position, new Vector3(0,-1f,0), out hit, Mathf.Infinity, goalLayerMask))
			{
				goalObject.transform.position = hit.point + new Vector3(0,goalObject.transform.localScale.y * 0.5f,0);
			}else{
				Vector3 curpos = goalObject.transform.localPosition;
				goalObject.transform.position = new Vector3(curpos.x, goalObject.transform.localScale.y * 0.5f, curpos.z);
			}

			agentObject.GetComponent<NaviAgent>().UpdateAgentPosition(true);
		}
	}
}
