using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using HoudiniEngineUnity;

public class VoronoiLayoutAgent : Agent {
	private HEU_HoudiniAsset houdiniAsset;
	private int roomNum;
	private int unitNum;
	private float unitSize;
	private float roomSize;
	private float previousDif = float.MaxValue;
	private List<Vector2> initialOffsets = new List<Vector2>();

	public void Init(int _roomNum){
		houdiniAsset = gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>() != null ? gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>()._houdiniAsset : null;

		HEU_ParameterAccessor.GetInt(houdiniAsset, "unit_num", out unitNum);
		HEU_ParameterAccessor.GetFloat(houdiniAsset, "unit_size", out unitSize);
		roomSize = unitNum * unitSize;

		UpdateRoomNum(_roomNum);

		ResetLayout(true);

	}

	public override void CollectObservations(){
		houdiniAsset = gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>() != null ? gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>()._houdiniAsset : null;

		AddVectorObs(GetRoomAreaDifferences());

		for(int i=0; i<roomNum; i++){

			Vector2 pointPos = GetPointPos(i) / (roomSize/2f);

			AddVectorObs(pointPos);

			//int roomArea = GetRoomArea(i);
			//float area = roomArea / (float)(unitNum * unitNum);

			//AddVectorObs(area);
		}

	}

	public override void AgentAction(float[] vectorAction, string textAction){
		houdiniAsset = gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>() != null ? gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>()._houdiniAsset : null;
		
		if(brain.brainType != BrainType.Player){
			
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
		}else{
			//if(Input.GetKeyUp(KeyCode.Space)){
				for(int i=0; i<roomNum; i++){
					float[] positions = {};
					HEU_ParameterAccessor.GetFloats(houdiniAsset, "offset" + i.ToString(), out positions);

					float xval = positions[0] + Random.Range(-1f, 1f) * roomSize * 0.1f;
					float zval = positions[2] + Random.Range(-1f, 1f) * roomSize * 0.1f;
					xval = Mathf.Clamp(xval, -roomSize * 0.5f, roomSize * 0.5f);
					zval = Mathf.Clamp(zval, -roomSize * 0.5f, roomSize * 0.5f);

					HEU_ParameterAccessor.SetFloats(houdiniAsset, "offset" + i.ToString(), new float[]{xval, 0, zval});
				}
				houdiniAsset.RequestCook(true, false, true, true);

				//float ard = GetRoomAreaDifferences();
				//Monitor.Log("Area Dif", ard);
				
			//}
		}

		
		float areaDif = GetRoomAreaDifferences();
		Monitor.Log("Area Dif", areaDif);

		if(areaDif <= 0.05f){
			AddReward(1.0f);
			Done();
		}


		if(areaDif < previousDif){
			AddReward(0.1f);
		}

		AddReward(-0.05f);
		/* 
		float rewardDif = (0.15f-areaDif) * 0.1f;
		//AddReward(rewardDif);

		if(areaDif <= 0.05f){
			AddReward(1.0f);
			Done();
		}else{
			AddReward(rewardDif);
		}
		*/

		previousDif = areaDif;
	}

	public override void AgentReset(){
		if(brain.brainType == BrainType.External){
			ResetLayout(false);
		}
	}

	public void ResetLayout(bool isRandom){

		previousDif = float.MaxValue;
		//HEU_ParameterAccessor.SetInt(houdiniAsset, "seed", Random.Range(0, 10000));
		for(int i=0; i<roomNum; i++){
			Vector2 offset = Vector2.zero;
			float x, y;
			if(isRandom){
				offset = new Vector2(Random.Range(-roomSize * 0.5f, roomSize * 0.5f), Random.Range(-roomSize * 0.5f, roomSize * 0.5f));
			}else{
				offset = initialOffsets[i];
			}
			HEU_ParameterAccessor.SetFloats(houdiniAsset, "offset" + i.ToString(), new float[]{offset.x, 0, offset.y});
		}
		houdiniAsset.RequestCook(true, false, true, true);

		if(isRandom){
			initialOffsets = new List<Vector2>();
			for(int i=0; i<roomNum; i++){
				Vector2 pos = GetPointPos(i);
				initialOffsets.Add(pos);
			}
		}
	}

	private void UpdateRoomNum(int _roomNum){
		roomNum = _roomNum;

		houdiniAsset = gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>() != null ? gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>()._houdiniAsset : null;
		HEU_ParameterAccessor.SetInt(houdiniAsset, "room_num", roomNum);
		houdiniAsset.RequestCook(true, false, true, true);
	}



	public int GetRoomArea(int index){
		int area = 0;

		if(houdiniAsset != null){
			var atts = houdiniAsset.GetAttributesStores();

			if(atts.Count > 0){
				var attributesStore = atts[0];

				var vis = attributesStore.GetAttributeData("area" + (index+1).ToString());
				var values = vis._intValues;
				
				if(values.Length > 0){
					area = values[0];
				}
			}
		}

		return area;
	}

	public Vector2 GetPointPos(int index){
		Vector2 pos = Vector2.zero;

		if(houdiniAsset != null){
			var atts = houdiniAsset.GetAttributesStores();

			if(atts.Count > 0){
				var attributesStore = atts[0];

				var visX = attributesStore.GetAttributeData("posx" + index.ToString());
				var valuesX = visX._floatValues;
				
				if(valuesX.Length > 0){
					pos.x = valuesX[0];// / (roomSize/2f);
				}

				var visY = attributesStore.GetAttributeData("posy" + index.ToString());
				var valuesY = visY._floatValues;
				
				if(valuesY.Length > 0){
					pos.y = valuesY[0];// / (roomSize/2f);
				}
			}
		}

		return pos;
	}

	private float GetRoomAreaDifferences(){
		houdiniAsset = gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>() != null ? gameObject.GetComponentInChildren<HEU_HoudiniAssetRoot>()._houdiniAsset : null;

		int totalAreaDif = 0;
		int count = 0;
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
