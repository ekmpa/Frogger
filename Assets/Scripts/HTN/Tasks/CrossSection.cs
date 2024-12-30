using System.Collections.Generic;
using UnityEngine;

public class CrossSection : HTNCompositeTask
{

    public override List<HTNTask> PlanSubtasks(Frog frog, FrogState state)
    {
        subtasks = new List<HTNTask>();
        for(int i=0; i<Mathf.Min(5, 10-state.Row); i++) 
        {
            subtasks.Add(HTNCompositeTask.CreateInstance<CrossLane>());
        }
        return subtasks;
    }   
}
