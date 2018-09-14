using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using HoudiniEngineUnity;

public class RockAcademy : Academy {
	public static RockAcademy instance;
	public GameObject houdiniAssetRoot;
	public GameObject rockEnvPrefab;
	public LayerMask collideRockLayer;
	public LayerMask uncollideRockLayer;
	public int envNum = 1;
	public int heightGridNum = 32;
	private float envSize = 20f;
	public int maxRocks = 100;
	public int maxFallenCount = 30;
	public float speed = 1.0f;
	private HEU_HoudiniAsset houdiniAsset;
	private GameObject rockObject;


	// Use this for initialization
	public override void InitializeAcademy()
	{
		instance = this;

		InitBrain();
		InitRockObject();
		InitRockEnvs();
	}

	public override void AcademyReset()
	{
	}

	public override void AcademyStep()
	{
	}

	private void InitBrain(){
		Brain brain = GetComponentInChildren<Brain>();

		brain.brainParameters.vectorObservationSize = 6 + heightGridNum * heightGridNum;
	}

	private void InitRockEnvs(){
		int totalCount = 0;
		int gridNum = (int)Mathf.Ceil(Mathf.Sqrt(envNum));
		for(int i =0; i<gridNum; i++){
			for(int n=0; n<gridNum; n++){
				InitRockEnv(i, n);

				totalCount++;
				if(totalCount >= envNum){
					return;
				}
			}
		}
	}

	private void InitRockEnv(int i, int n){
		GameObject rockEnvObject = (GameObject)Instantiate(rockEnvPrefab);
		rockEnvObject.transform.localPosition = new Vector3((envSize+5f) * i, 0, (envSize+5f) * n);

		RockEnv rockEnv = rockEnvObject.GetComponent<RockEnv>();
		rockEnv.Init(GetComponentInChildren<Brain>(), envSize + Random.Range(-1f, 1f) * 4f, maxRocks, maxFallenCount, speed, heightGridNum);
	}

	private void InitRockObject(){
		houdiniAsset = houdiniAssetRoot.GetComponentInChildren<HEU_HoudiniAssetRoot>() != null ? houdiniAssetRoot.GetComponentInChildren<HEU_HoudiniAssetRoot>()._houdiniAsset : null;
		rockObject = houdiniAssetRoot.GetComponentInChildren<MeshFilter>().gameObject;

		MeshCollider meshCollider = rockObject.AddComponent<MeshCollider>();
		meshCollider.convex = true;

		Rigidbody rigidbody = rockObject.AddComponent<Rigidbody>();
		rockObject.SetActive(false);

		
	}

	public GameObject InstantiateRock(Vector3 pos, Transform parent, Vector3 rot){
		GameObject instance = (GameObject)Instantiate(rockObject);
		Rock rock = instance.AddComponent<Rock>();
		instance.transform.SetParent(parent);
		instance.transform.localPosition = pos;
		instance.transform.localEulerAngles = rot;
		instance.GetComponent<Rigidbody>().velocity = new Vector3(0, -0.2f, 0);
		instance.layer = LayerMask.NameToLayer("uncollideRock");
		instance.SetActive(true);

		return instance;
	}
}
