using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class RockEnv : MonoBehaviour {
	public RockAgent rockAgent; // RockEnvと紐づいたエージェント
	public GameObject floorObject; // 床オブジェクト
	public Transform rockParent; // 岩のインスタンを格納する場所s
	private int heightGridNum; // 計測グリッドの分割数

	// RockEnvの初期化
	public void Init(Brain brain, float envSize, int maxRocks, float speed, int _heightGridNum){
		// RockEnvに紐づいたエージェントや床のサイズなどの設定を行う。
		floorObject.transform.localScale = new Vector3(envSize, envSize, envSize + 5f);
		rockAgent.brain = brain;
		rockAgent.maxRocks = maxRocks;
		rockAgent.speed = speed;
		rockAgent.envSize = envSize;
		heightGridNum = _heightGridNum;
	}

	// 床から落ちた岩を削除
	public int DeleteFallenRocks(){
		int fallenCount = 0;
		Rigidbody[] rocks = GetRocks();
		foreach(Rigidbody rock in rocks){
			if(rock.transform.localPosition.y < 0){
				Destroy(rock.gameObject);
				fallenCount++;
			}
		}
		// いくつ落ちたかを返す。
		return fallenCount;
	}

	// 環境にある岩を全て削除
	public void InitRocks(){
		Rigidbody[] rocks = GetRocks();
		foreach(Rigidbody rock in rocks){
			Destroy(rock.gameObject);
		}
	}

	// 環境にある岩を配列として取得
	public Rigidbody[] GetRocks(){
		Rigidbody[] rocks = rockParent.GetComponentsInChildren<Rigidbody>();
		return rocks;
	}

	// 岩のレイヤーを張り替え
	public void SetRockCollidables(){
		// 環境にある岩の中で、動きがほぼ止まっている岩に対してはcollideRock、
		// まだ動いている岩に対してはuncollideRockというレイヤーをつける。
		// uncollideRockがついた岩はRaycastから無視されるようにするため。
		// （動いている岩はRaycastから無視されるように）
		Rigidbody[] rocks = GetRocks();
		foreach(Rigidbody rock in rocks){
			if(rock.velocity.magnitude > 0.025f){
				rock.gameObject.layer = LayerMask.NameToLayer("uncollideRock");
			}else{
				rock.gameObject.layer = LayerMask.NameToLayer("collideRock");
			}
		}
	}

	// キャラクタと岩との最短距離を取得
	public float ClosestDistanceToRocks(Vector3 pos){
		Rigidbody[] rocks = GetRocks();
		float minDist = float.PositiveInfinity;
		foreach(Rigidbody rock in rocks){
			if(rock.velocity.magnitude > 0.05f){
				float dist = Vector3.Distance(pos, rock.transform.localPosition);
				if(minDist > dist){
					minDist = dist;
				}
			}
		}
		minDist = Mathf.Clamp(minDist, 0, rockAgent.envSize);
		return minDist;
	}

	// 計測グリッドの計測結果を取得
	public List<float> GetHeightGridVals(out float maxHeight, out float fillRatio){
		// 環境にある岩のRaycast用のレイヤーを更新する。
		SetRockCollidables(); 

		// 変数の初期化をする。
		List<float> gridVals = new List<float>();
		float envSize = rockAgent.envSize - 0.1f;
		maxHeight = -1f;
		
		// 計測グリッドの各点の位置で、上部から下に向けてRaycastを放ち、
		// 岩とぶつかったときのその高さを取得する。
		// uncollideRockのレイヤーがついた岩は無視する。
		// また、グリッドの点の中でどれだけ岩にぶつかったかという情報を
		// 岩の床に対する平面占有率として計算する。
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
