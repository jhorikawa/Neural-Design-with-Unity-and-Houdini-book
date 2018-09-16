using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class LayoutAcademy : Academy {
	public static LayoutAcademy instance; // Singleton
	public int roomNumber = 5; // 部屋数

	public override void InitializeAcademy()
	{
		instance = this;

		// BrainのVector Observation Sizeと、Vector Action Sizeを
		// 部屋の数に応じて指定する。
		Brain brain = GetComponentInChildren<Brain>();
		brain.brainParameters.vectorObservationSize = roomNumber * 2+1;
		brain.brainParameters.vectorActionSize[0] = roomNumber * 2;

		// シーン内にあるすべてのLayoutAgentコンポーネントを取得し、
		// すべて指定した部屋数で初期化する。
		LayoutAgent[] layoutAgents = GameObject.FindObjectsOfType<LayoutAgent>();
		foreach(LayoutAgent layoutAgent in layoutAgents){
			layoutAgent.Init(roomNumber);
		}
	}

	public override void AcademyReset()
	{
		// 全てのLayoutAgentをリセット
		LayoutAgent[] layoutAgents = GameObject.FindObjectsOfType<LayoutAgent>();
		foreach(LayoutAgent layoutAgent in layoutAgents){
			layoutAgent.ResetLayout(true);
		}
	}

	public override void AcademyStep()
	{
	}
}
