using System.Collections.Generic;

public class CrossHighway : HTNCompositeTask
{
    public override List<HTNTask> PlanSubtasks(Frog frog, FrogState state)
    {
        if(frog.state.Row < 5)
        {
            subtasks = new List<HTNTask>
            {
                HTNCompositeTask.CreateInstance<CrossSection>(), // Go to REST
                HTNCompositeTask.CreateInstance<CrossSection>() // Go to GOAL 
            };
        } else if (frog.state.Row < 10)
        {
            subtasks = new List<HTNTask>
            {
                HTNCompositeTask.CreateInstance<CrossSection>(), // Go to GOAL
            };
        } else
        {
            subtasks = new List<HTNTask>(); // Done
        }
        return subtasks;
    }


}
