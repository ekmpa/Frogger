
public class DigestFly : HTNSimpleTask
{
    public override bool Execute(Frog frog)
    {
        if (frog.isLeaping) return false; // Prevent re-execution during the leap

        frog.PerformLeap();
        return true; 
    }

    internal override bool Precondition(FrogState state)
    {
        // Must have at least one fly to digest
        return state.FliesInBelly > 0;
    }

    internal override FrogState Postcondition(FrogState state)
    {
        // Update state
        return new FrogState(state.FliesInBelly - 1, state.FlyWithinReach, state.Row);
    }

    

    
}
