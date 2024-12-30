using System.Collections.Generic;

public class CrossLane : HTNCompositeTask
{
    public override List<HTNTask> PlanSubtasks(Frog frog, FrogState state)
    {
        subtasks = new List<HTNTask>();

        MoveForward mf = HTNSimpleTask.CreateInstance<MoveForward>();
        Retreat r = HTNSimpleTask.CreateInstance<Retreat>();
        HandleFly hf = HTNSimpleTask.CreateInstance<HandleFly>();
        DigestFly df = HTNSimpleTask.CreateInstance<DigestFly>();

        if(hf.Precondition(state))
        {
            subtasks.Add(hf);
            subtasks.Add(HTNCompositeTask.CreateInstance<CrossLane>());
            subtasks.Add(HTNCompositeTask.CreateInstance<CoolDown>());
        } 
        else if(mf.Precondition(state))
        {
            subtasks.Add(mf);
            subtasks.Add(HTNCompositeTask.CreateInstance<CoolDown>());
        }

        return subtasks;
    }

}

