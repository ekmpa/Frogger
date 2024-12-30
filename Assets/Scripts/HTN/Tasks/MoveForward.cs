
using UnityEngine;

public class MoveForward : HTNSimpleTask
{

    public Vector3 goal; 

    public override bool Execute(Frog frog)
    {
        goal = frog.ForwardChoice();

        if (goal == frog.transform.position)
        {
            return false; // No safe moves available -> fails
        }

        if (goal.z != frog.transform.position.z) // Diagonal moves 
        {
            // First move laterally (left or right)
            Vector3 lateralMove = new Vector3(
                frog.transform.position.x,
                frog.transform.position.y,
                goal.z 
            );

            frog.transform.position = lateralMove;
            
            // Trigger cooldown
            frog.LaunchCoolDown();
            return false; // Task is not yet complete - need to cross lane still 
        }

        // Update state 
        frog.transform.position = goal;
        frog.state.Row += 1;
        return true; 
    }


    internal override FrogState Postcondition(FrogState state)
    {
        // Copy to avoid corrupting the actual state value while planning 
        FrogState copy = state.DeepCopy();
        copy.Row += 1;
        return copy;
    }

    internal override bool Precondition(FrogState state)
    {
        return state.Row < 11;
    }
}
