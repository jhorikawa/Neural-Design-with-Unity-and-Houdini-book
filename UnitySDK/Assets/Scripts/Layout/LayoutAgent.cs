using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using HoudiniEngineUnity;

public class LayoutAgent : Agent {
	private HEU_HoudiniAsset houdiniAsset;
	private int roomNum; // 部屋数
	private int unitNum; // 横、縦方向のタイルの数
	private float unitSize; // タイルのサイズ 
	private float roomSize; // 部屋の大きさ（タイルの数の二乗）
	private float previousDif = float.MaxValue; // 部屋の面積の最小差分
	private List<Vector2> initialOffsets = new List<Vector2>(); // 部屋の中心点の初期位置

	public void Init(int _roomNum){
		houdiniAsset = gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>() != null ? gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>()._houdiniAsset : null;

		// HoudiniのアセットからunitNumとunitSizeを取得する。
		HEU_ParameterAccessor.GetInt(houdiniAsset, "unit_num", out unitNum);
		HEU_ParameterAccessor.GetFloat(houdiniAsset, "unit_size", out unitSize);
		roomSize = unitNum * unitSize;

		// 部屋数の入力値をつかって初期化をする。
		UpdateRoomNum(_roomNum);

		// レイアウトのリセットを行う。
		ResetLayout(true);
	}

	// 観測
	public override void CollectObservations(){
		houdiniAsset = gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>() != null ? gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>()._houdiniAsset : null;

		// 部屋の最小面積と最大面積の差分をとって、それを観測にわたす。
		AddVectorObs(GetRoomAreaDifferences());

		// 各部屋の中心点（ボロノイの点）の位置を観測にわたす。
		for(int i=0; i<roomNum; i++){
			Vector2 pointPos = GetPointPos(i) / (roomSize/2f);
			AddVectorObs(pointPos);
		}

	}

	// エージェントのアクション
	public override void AgentAction(float[] vectorAction, string textAction){
		houdiniAsset = gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>() != null ? gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>()._houdiniAsset : null;
		
		// 部屋の数だけ、部屋の中心点（ボロノイの点）を移動し、
		// Houdiniのアセットにその値をパラメータとしてわたす。
		for(int i=0; i<roomNum; i++){
			float[] positions = {};
			HEU_ParameterAccessor.GetFloats(houdiniAsset, "offset" + i.ToString(), out positions);

			float xval, zval;
			
			xval = positions[0] + vectorAction[i*2] * roomSize * 0.1f;//vectorAction[i*2] * roomSize;
			zval = positions[2] + vectorAction[i*2 + 1] * roomSize * 0.1f;//vectorAction[i*2 + 1] * roomSize;
			xval = Mathf.Clamp(xval, -roomSize * 0.5f, roomSize * 0.5f);
			zval = Mathf.Clamp(zval, -roomSize * 0.5f, roomSize * 0.5f);

			
			HEU_ParameterAccessor.SetFloats(houdiniAsset, "offset" + i.ToString(), new float[]{xval, 0, zval});
		}
		houdiniAsset.RequestCook(true, false, true, true);
		
		// 部屋の最小面積と最大面積の差分を取得する。
		float areaDif = GetRoomAreaDifferences();
		Monitor.Log("Area Dif", areaDif);

		// 差分が小さい時は+1.0の報酬を与え、レイアウトをリセットする。
		if(areaDif <= 0.05f){
			AddReward(1.0f);
			Done();
		}

		// 差分が前のステップで計算した差分よりも小さい場合は+0.1の報酬を与える。
		if(areaDif < previousDif){
			AddReward(0.1f);
		}

		// 毎ステップ-0.05の報酬が与えられる（引かれる）。
		AddReward(-0.05f);

		// 最終の面積比を更新。
		previousDif = areaDif;
	}

	// エージェントのリセット
	public override void AgentReset(){
		// BrainのタイプがExternalの時だけレイアウトをリセットする。
		if(brain.brainType == BrainType.External){
			ResetLayout(false);
		}
	}

	// レイアウトのリセット
	public void ResetLayout(bool isRandom){
		previousDif = float.MaxValue; // 最終の面積差分をリセット

		// 各部屋の中心点の位置をリセットする。
		for(int i=0; i<roomNum; i++){
			Vector2 offset = Vector2.zero;
			float x, y;
			// isRandomフラグがあるときはランダムな位置に、
			// ないときは一番最初に設定した点の位置に設定する。
			if(isRandom){
				offset = new Vector2(Random.Range(-roomSize * 0.5f, roomSize * 0.5f), Random.Range(-roomSize * 0.5f, roomSize * 0.5f));
			}else{
				offset = initialOffsets[i];
			}
			HEU_ParameterAccessor.SetFloats(houdiniAsset, "offset" + i.ToString(), new float[]{offset.x, 0, offset.y});
		}
		houdiniAsset.RequestCook(true, false, true, true);

		// isRandomフラグがあるときは、初期値としてinitialOffsetsに
		// 点の位置を登録する。
		if(isRandom){
			initialOffsets = new List<Vector2>();
			for(int i=0; i<roomNum; i++){
				Vector2 pos = GetPointPos(i);
				initialOffsets.Add(pos);
			}
		}
	}

	// 部屋数の更新
	private void UpdateRoomNum(int _roomNum){
		roomNum = _roomNum;

		// Houdiniのアセットの部屋数のパラメータを更新
		houdiniAsset = gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>() != null ? gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>()._houdiniAsset : null;
		HEU_ParameterAccessor.SetInt(houdiniAsset, "room_num", roomNum);
		houdiniAsset.RequestCook(true, false, true, true);
	}

	// 部屋の面積を取得
	public int GetRoomArea(int index){
		int area = 0;

		if(houdiniAsset != null){
			var atts = houdiniAsset.GetAttributesStores();

			if(atts.Count > 0){
				var attributesStore = atts[0];

				// PointのAttributeに入っている部屋毎の面積（タイル数）を取得する。
				// すべてのPointのAttributeに同じ値が入っているので、取得するのは1個目だけでいい。
				var vis = attributesStore.GetAttributeData("area" + (index+1).ToString());
				var values = vis._intValues;
				
				if(values.Length > 0){
					area = values[0];
				}
			}
		}

		return area;
	}

	// 部屋の中心点の取得
	public Vector2 GetPointPos(int index){
		Vector2 pos = Vector2.zero;

		if(houdiniAsset != null){
			var atts = houdiniAsset.GetAttributesStores();

			if(atts.Count > 0){
				// 部屋の中心点（ボロノイの点）の位置をPointのAttributeから取得する。
				var attributesStore = atts[0];

				var visX = attributesStore.GetAttributeData("posx" + index.ToString());
				var valuesX = visX._floatValues;
				
				if(valuesX.Length > 0){
					pos.x = valuesX[0];
				}

				var visY = attributesStore.GetAttributeData("posy" + index.ToString());
				var valuesY = visY._floatValues;
				
				if(valuesY.Length > 0){
					pos.y = valuesY[0];
				}
			}
		}

		return pos;
	}

	// 部屋の最小面積と最大面積の差分を取得
	private float GetRoomAreaDifferences(){
		// 各部屋の面積を調べ、最小面積と最大面積を取得した上で差分を計算する。
		// さらに部屋のサイズ（縦横のタイル数の二乗）で割ることで、
		// 値の範囲を0.0~1.0にする。
		houdiniAsset = gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>() != null ? gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>()._houdiniAsset : null;

		int minArea = 999;
		int maxArea = -1;
		for(int i=0; i<roomNum; i++){
			int areaI = GetRoomArea(i);
			if(areaI > maxArea){
				maxArea = areaI;
			}
			if(areaI < minArea){
				minArea = areaI;
			}
		}
		float dif = (maxArea - minArea) / (float)(unitNum * unitNum);

		return dif;
	}
}
