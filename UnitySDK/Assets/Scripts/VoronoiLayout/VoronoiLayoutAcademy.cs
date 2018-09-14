using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using HoudiniEngineUnity;

public class VoronoiLayoutAcademy : Academy {

	public static VoronoiLayoutAcademy instance;

	public int roomNumber = 5;



	public void Update(){
		
	}
	public override void InitializeAcademy()
	{
		instance = this;

		Brain brain = GetComponentInChildren<Brain>();
		brain.brainParameters.vectorObservationSize = roomNumber * 2+1;
		brain.brainParameters.vectorActionSize[0] = roomNumber * 2;

		VoronoiLayoutAgent[] voronoiLayoutAgents = GameObject.FindObjectsOfType<VoronoiLayoutAgent>();
		foreach(VoronoiLayoutAgent voronoiLayoutAgent in voronoiLayoutAgents){
			voronoiLayoutAgent.Init(roomNumber);

		}
	}

	public override void AcademyReset()
	{
		VoronoiLayoutAgent[] voronoiLayoutAgents = GameObject.FindObjectsOfType<VoronoiLayoutAgent>();
		foreach(VoronoiLayoutAgent voronoiLayoutAgent in voronoiLayoutAgents){
			voronoiLayoutAgent.ResetLayout(true);
		}
	}

	public override void AcademyStep()
	{
		

	}
}
