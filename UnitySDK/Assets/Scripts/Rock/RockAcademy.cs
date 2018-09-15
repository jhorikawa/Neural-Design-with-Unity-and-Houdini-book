using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using HoudiniEngineUnity;

public class RockAcademy : Academy {
	public static RockAcademy instance;
	public GameObject houdiniAssetRoot;
	public GameObject rockEnvPrefab; //RockEnvのプレファブ
	public LayerMask collideRockLayer;
	public LayerMask uncollideRockLayer;
	public int envNum = 1; // 環境の数
	public int heightGridNum = 32; //計測グリッドの分解数
	private float envSize = 20f; // 環境のサイズ
	public int maxRocks = 100; // 環境ごとの岩の最大数
	public float speed = 1.0f; // キャラクタの移動スピード

	private HEU_HoudiniAsset houdiniAsset;
	private GameObject rockObject; // 岩のインスタンスのオリジナル

	public override void InitializeAcademy()
	{
		instance = this; // Singleton作成

		InitBrain(); // Brainの初期化
		InitRockObject(); //岩形状の初期化
		InitRockEnvs(); //環境の初期化
	}

	public override void AcademyReset()
	{
	}

	public override void AcademyStep()
	{
	}

	// Brainの初期化
	private void InitBrain(){
		// 計測グリッドの数だけVector Observationのサイズを変更する。
		Brain brain = GetComponentInChildren<Brain>();
		brain.brainParameters.vectorObservationSize = 6 + heightGridNum * heightGridNum;
	}

	// 環境の初期化
	private void InitRockEnvs(){
		// envNumで指定した環境の数だけ環境を作る。
		int totalCount = 0;
		int gridNum = (int)Mathf.Ceil(Mathf.Sqrt(envNum));
		for(int i =0; i<gridNum; i++){
			for(int n=0; n<gridNum; n++){
				InitRockEnv(i, n); // RockEnvの初期化

				totalCount++;
				if(totalCount >= envNum){
					return;
				}
			}
		}
	}

	// RockEnvの初期化
	private void InitRockEnv(int i, int n){
		// RockEnvプレファブからインスタンスを作り、グリッド状に配列する。
		GameObject rockEnvObject = (GameObject)Instantiate(rockEnvPrefab);
		rockEnvObject.transform.localPosition = new Vector3((envSize+5f) * i, 0, (envSize+5f) * n);

		// RockEnvコンポーネントを初期化する。
		RockEnv rockEnv = rockEnvObject.GetComponent<RockEnv>();
		rockEnv.Init(GetComponentInChildren<Brain>(), envSize + Random.Range(-1f, 1f) * 4f, maxRocks, speed, heightGridNum);
	}

	// 岩形状の初期化
	private void InitRockObject(){
		// Houdiniのデジタルアセットのメッシュオブジェクトを取得する。
		houdiniAsset = houdiniAssetRoot.GetComponentInChildren<HEU_HoudiniAssetRoot>() != null ? houdiniAssetRoot.GetComponentInChildren<HEU_HoudiniAssetRoot>()._houdiniAsset : null;
		rockObject = houdiniAssetRoot.GetComponentInChildren<MeshFilter>().gameObject;

		// 岩のオブジェクトにMeshColliderを加える。
		MeshCollider meshCollider = rockObject.AddComponent<MeshCollider>();
		meshCollider.convex = true;

		// 岩のオブジェクトにRigidbodyを加えて、Activeをオフにしておく。
		Rigidbody rigidbody = rockObject.AddComponent<Rigidbody>();
		rockObject.SetActive(false);
	}

	// 岩形状のインスタンス化
	public GameObject InstantiateRock(Vector3 pos, Transform parent, Vector3 rot){
		// 岩をrockObjectからインスタンス化して作り、初期値を設置する。
		GameObject instance = (GameObject)Instantiate(rockObject);
		instance.transform.SetParent(parent);
		instance.transform.localPosition = pos;
		instance.transform.localEulerAngles = rot;
		instance.GetComponent<Rigidbody>().velocity = new Vector3(0, -0.2f, 0);
		// 作った瞬間はRaycastに反応して欲しくないのでLayerをuncollideRockに設定しておく。
		instance.layer = LayerMask.NameToLayer("uncollideRock");
		instance.SetActive(true);

		return instance;
	}
}
