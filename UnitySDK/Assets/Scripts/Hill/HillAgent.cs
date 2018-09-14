using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class HillAgent : Agent {
	public HillEnv hillEnv; //エージェントごとの環境をコントロールするHillEnv
	public float speed = 0.1f; //エージェントの動くスピード
	public LayerMask raycastLayerMask; //エージェント以外のレイヤー
	private CharacterController characterController; //キャラクタを動かすためのコントローラ
	private Vector3 prevPos; //事前のエージェントの初期位置
	private bool lastDiscoveryVal = false; //最後にエージェントがゴールを見つけたかどうか

	public void Start(){
		characterController = GetComponent<CharacterController>();
	}

	public void UpdateAgentPosition(bool random){
		
		if(random){
			//地形の上にランダムにキャラクタを配置する
			float size = hillEnv.size - 1f;
			float x = Random.Range(-size * 0.5f, size * 0.5f);
			float z = Random.Range(-size * 0.5f, size * 0.5f);
			transform.localPosition = new Vector3(x, 20f, z);
			prevPos = transform.localPosition;
		}else{
			//キャラクタを事前の初期位置に戻す
			transform.localPosition = prevPos;
		}
	}

	public override void CollectObservations(){
		//キャラクタからゴールに向けたベクトルを作る
		Vector3 dir = hillEnv.goalObject.transform.localPosition - transform.localPosition;
		dir = dir.normalized * 0.1f;

		//キャラクタの視界にゴールが入っているかをチェックする
		RaycastHit hit;
		bool didHit = Physics.Raycast(transform.position, dir, out hit, Mathf.Infinity, raycastLayerMask);
		bool visible = false;
		if(didHit){
			visible = hit.transform.gameObject.layer == LayerMask.NameToLayer("goal");
		}

		//ゴールが見えるか見えないかという情報を観察に加える
		AddVectorObs(visible);
		//キャラクタの位置を観察に加える
		AddVectorObs(transform.localPosition * 0.1f);

        if (visible){
			//ゴールが見えるとき、キャラクタからゴールへの視線の向きを観察に加える
			AddVectorObs(dir);
			lastDiscoveryVal = true;
        }else{
			//ゴールが見えないとき、ランダムな向きを観察に加える
			AddVectorObs(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized);
			lastDiscoveryVal = false;
		}
		
	}

	public override void AgentAction(float[] vectorAction, string textAction){
		float xMove = vectorAction[0]; //X方向に進む値
		float zMove = vectorAction[1]; //Z方向に進む値
		
		//キャラクタの移動
		characterController.Move(new Vector3(xMove, 0, zMove) * speed);
		
		//キャラクタの位置の上空から下にレイを放って、ヒットした地形の位置にキャラクタを移動する
		RaycastHit hit;
        if (Physics.Raycast(new Vector3(transform.position.x, 20f, transform.position.z), new Vector3(0,-1f,0), out hit, Mathf.Infinity, raycastLayerMask))
        {
            transform.position = hit.point + new Vector3(0,transform.localScale.y * 0.5f,0);
        }else{
			Vector3 curpos = transform.localPosition;
			transform.localPosition = new Vector3(curpos.x, transform.localScale.y * 0.5f, curpos.z);
		}


		float size = hillEnv.size;
		if(Vector3.Distance(hillEnv.goalObject.transform.localPosition, transform.localPosition) < 1f){
			//キャラクタがゴールにたどり着いた時に+1の報酬を与える
			AddReward(1.0f);
			Done();
			hillEnv.UpdateEnvironment();
		}else if(transform.localPosition.x > hillEnv.size * 0.5f || transform.localPosition.x < -hillEnv.size * 0.5f
			|| transform.localPosition.z > hillEnv.size * 0.5f || transform.localPosition.z < -hillEnv.size * 0.5f){
				//キャラクタが地形の外に出た時に-1の報酬を与える
				AddReward(-1.0f);
				Done();
		}else if(lastDiscoveryVal == false){
			//キャラクタがゴールを見つけられていないときに-0.05の報酬を与える
			AddReward(-0.05f);
		}else{
			//上記以外のとき-0.01の報酬を与える
			AddReward(-0.01f);
		}
	}

	public override void AgentReset(){
		//エージェントの位置をランダムにリセットする
		UpdateAgentPosition(true);
	}
}
