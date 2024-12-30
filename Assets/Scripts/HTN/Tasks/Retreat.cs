using UnityEngine;


public class Retreat : HTNSimpleTask
{
    public Vector3 goal;

    public override bool Execute(Frog frog)
    {
        goal = frog.BackwardChoice();

        if (goal == frog.transform.position)
        {
            return false; // No safe moves available -> fails
        }

        if (goal.z != frog.transform.position.z)
        {
            // First move laterally (L/R)
            Vector3 lateralMove = new Vector3(
                frog.transform.position.x,
                frog.transform.position.y,
                goal.z 
            );

            // Execute lateral move
            frog.transform.position = lateralMove;

            // Trigger cooldown
            frog.LaunchCoolDown();
            return false; 
        }

        frog.transform.position = goal;
        frog.state.Row -= 1;
        return true; 
    }


    internal override FrogState Postcondition(FrogState state)
    {
        FrogState copy = state.DeepCopy();
        copy.Row -= 1;
        return copy;
    }

    internal override bool Precondition(FrogState state)
    {
        return state.Row < 10 && state.Row > 0;
    }
}
