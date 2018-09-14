using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class HillAcademy : Academy {
	public static HillAcademy instance;

	public override void InitializeAcademy()
	{
		instance = this;

		//シーンにある全てのHillAgentにAcademyの子階層にあるBrainを設定する。
		Brain brain = GetComponentInChildren<Brain>();
		HillAgent[] hillAgents = GameObject.FindObjectsOfType<HillAgent>();
		foreach(HillAgent hillAgent in hillAgents){
			hillAgent.brain = brain;
		}
	}

	public override void AcademyReset()
	{
		//Academyのリセット時に全ての環境をリセットする。
		UpdateAllEnvironments();
	}

	public override void AcademyStep()
	{

	}

	//シーン内にある全てのHillEnvのUpdateEnvironment関数（環境のリセットをする関数）を呼ぶ。
	private void UpdateAllEnvironments(){
		HillEnv[] hillEnvs = GameObject.FindObjectsOfType<HillEnv>();

		foreach(HillEnv hillEnv in hillEnvs){
			hillEnv.UpdateEnvironment();
		}
	}
}
