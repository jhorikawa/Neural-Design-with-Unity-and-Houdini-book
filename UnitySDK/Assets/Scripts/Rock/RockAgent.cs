using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class RockAgent : Agent {
	public RockEnv rockEnv; // エージェントと紐づいていあるRockEnv
	public int maxRocks {get; set;} 
	public float speed {get; set;}
	public float envSize{get; set;}
	private float maxHeight = 0; // 環境の中の岩の中での最大の高さ
	private float fillRatio = 0; // 環境の中での岩の平面占有率
	private float edgeDist = 0; // キャラクタと床のエッジとの距離
	
	// 観測
	public override void CollectObservations(){
		// キャラクタの位置を観測値として追加。
		AddVectorObs(transform.localPosition / (envSize * 0.5f));
		// 計測グリッドの各点の位置での岩の高さのリストを観測値として追加。
		AddVectorObs(rockEnv.GetHeightGridVals(out maxHeight, out fillRatio));
		// 全部の岩の中での最大高さを観測値として追加。
		AddVectorObs(maxHeight / 10f);
		// 環境における岩の平面占有率を観測値として追加。
		AddVectorObs(fillRatio);
		// キャラクタと、キャラクタに一番近い岩との距離を観測値に追加。
		AddVectorObs(rockEnv.ClosestDistanceToRocks(transform.localPosition)/envSize);
	}

	// エージェントのアクション
	public override void AgentAction(float[] vectorAction, string textAction){
		float x, z, trigger, rotx, roty, rotz;
		if(brain.brainType == BrainType.Player){
			// BrainのタイプがPlayerの時は移動値と岩の配置のタイミングをランダムにする。
			x = Random.Range(-1f, 1f);
			z = Random.Range(-1f, 1f);
			trigger = Random.Range(-1f, 1f);
		}else{
			// BrainのタイプがPlayer以外の時にvectorActionの値を変数に振り分ける。
			x = vectorAction[0]; // X方向への移動値
			z = vectorAction[1]; // Y方向への移動値
			trigger = vectorAction[2]; // 岩を配置するタイミング
		}
		// 岩を配置するときの岩の初期回転値はランダムにする。
		rotx = Random.Range(0, 360f); 
		roty = Random.Range(0, 360f);
		rotz = Random.Range(0, 360f);

		// XとZの値から移動ベクトルを作る。
		Vector3 dir = new Vector3(x,0,z);
		dir *= speed;

		// キャラクタの位置を更新する。
		Vector3 curPos = transform.localPosition + dir;
		Vector3 newPos = curPos;
		if(Mathf.Abs(curPos.x) > envSize * 0.5f){
			newPos.x = Mathf.Abs(curPos.x) / curPos.x * envSize * 0.5f; 
		}
		if(Mathf.Abs(curPos.z) > envSize * 0.5f){
			newPos.z = Mathf.Abs(curPos.z) / curPos.z * envSize * 0.5f;
		}
		// キャラクタの高さ方向の位置は全部の岩の高さの最大の高さよりも高い値にする。
		newPos.y = maxHeight + 1f; 
		transform.localPosition = newPos; 
	
		// キャラクタの位置と環境に配置された岩が近すぎないとき、
		// かつtriggerによる岩が配置されるタイミングのときに報酬を与える。
		if(trigger > 0f && rockEnv.ClosestDistanceToRocks(newPos) > 1.5f){
			// 岩のインスタンスを作って環境に配置する。
			RockAcademy.instance.InstantiateRock(newPos, rockEnv.rockParent, new Vector3(rotx, roty, rotz));
			
			// キャラクタ上部から下の方向にRaycastを放ち、
			// 岩とぶつかったかどうかで報酬を与える。
			RaycastHit hit;
			if(Physics.Raycast(new Vector3(newPos.x, 100f, newPos.z),new Vector3(0, -1f, 0),out hit, float.PositiveInfinity, RockAcademy.instance.collideRockLayer)){
				float dist = 1f - hit.distance / 100f;
				AddReward(dist);
			}else{
				AddReward(-0.01f);
			}

			// 配置した時のキャラクタの位置と、
			// 床のエッジとの距離に応じて報酬を与える。
			float edgeDist = Mathf.Min(Mathf.Abs(Mathf.Abs(newPos.x) - envSize * 0.5f), Mathf.Abs(Mathf.Abs(newPos.z) - envSize * 0.5f));
			if(edgeDist < 2f){
				AddReward(-0.02f);
			}else{
				float val = edgeDist / (envSize * 0.5f) * 0.01f;
				AddReward(val);
			}
		}

		// 床から落ちた岩を削除する。
		rockEnv.DeleteFallenRocks();

		// 環境にある岩の数が設定した最大の岩の数を超えたらエージェントをリセットする。
		if(rockEnv.GetRocks().Length >= maxRocks){
			Done();
		}
	}

	// エージェントのリセット
	public override void AgentReset(){
		// キャラクタの位置をランダムに設定する。
		transform.localPosition = new Vector3(Random.Range(-10f, 10f), 20f, Random.Range(-10f, 10f));
		// 環境にある全ての岩を削除する。
		rockEnv.InitRocks();
	}


}
