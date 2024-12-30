using System.Collections.Generic;

public abstract class HTNCompositeTask : HTNTask
{
    protected List<HTNTask> subtasks;
   
    public abstract List<HTNTask> PlanSubtasks(Frog frog, FrogState state);

    public List<HTNTask> GetSubtasks(Frog frog, FrogState state)
    {
        if(subtasks.Count == 0)
        {
            PlanSubtasks(frog, state);
            if (subtasks.Count == 0)
            {
                return null; 
            }
        }
        return subtasks;
    }

    public HTNCompositeTask()
    {
        subtasks = new List<HTNTask>();
    }
}

