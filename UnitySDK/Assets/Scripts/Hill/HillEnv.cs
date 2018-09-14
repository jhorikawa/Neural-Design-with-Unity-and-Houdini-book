using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoudiniEngineUnity;

public class HillEnv : MonoBehaviour {
	public GameObject agentObject;//エージェントのオブジェクト
	public GameObject goalObject;//ゴールのオブジェクト
	public LayerMask goalLayerMask;//ゴールを配置するためのレイヤーマスク
	[HideInInspector]
	public float height;//丘の高さ
	[HideInInspector]
	public float radius;//丘の半径
	[HideInInspector]
	public float[] mountPos;//丘の中心の位置
	[HideInInspector]
	public float size;//地形のサイズ
	private HEU_HoudiniAsset houdiniAsset;//デジタルアセット

	void Start () {
		InitEnvironment(); //環境の初期化
		UpdateEnvironment(); //環境のアップデート
	}
	
	//環境の初期化
	private void InitEnvironment(){
		//デジタルアセットのsizeパラメータを取得し、size変数へ格納する
		houdiniAsset = gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>() != null ? gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>()._houdiniAsset : null;
		HEU_ParameterAccessor.GetFloat(houdiniAsset, "size", out size);

		//丘の中心位置を初期化
		mountPos = new float[]{0.0f, 0.0f};
	}

	//環境のアップデート
	public void UpdateEnvironment(){
		if(houdiniAsset != null){
			//デジタルアセットのパラメータにランダムな値を送って、ランダムな地形を作る
			float height = Random.Range(0.0f, 5.0f);
			HEU_ParameterAccessor.SetFloat(houdiniAsset, "height", height);

			float radius = Random.Range(3.0f, 10.0f);
			HEU_ParameterAccessor.SetFloat(houdiniAsset, "radius", radius);
			
			float[] mountPos = {Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)};
			HEU_ParameterAccessor.SetFloats(houdiniAsset, "mountain_pos", mountPos);

			houdiniAsset.RequestCook(true, false, true, true);

			//地形にMeshColliderを加える
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

			//地形の上のランダムな位置にゴールを配置する
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

			//エージェントの位置を更新する
			agentObject.GetComponent<HillAgent>().UpdateAgentPosition(true);
		}
	}
}
