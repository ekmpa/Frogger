

public class HandleFly : HTNSimpleTask
{
    public override bool Execute(Frog frog)
    {
        frog.state.FliesInBelly += 1;
        frog.state.FlyWithinReach = false;

        frog.TongueOut(); // Animate reaching the fly 
        
        return true; 
        
    }

    internal override FrogState Postcondition(FrogState state)
    {
        FrogState copy = state.DeepCopy();
        copy.FliesInBelly += 1; // Increment FIB
        copy.FlyWithinReach = false; // Fly no longer within reach
        return copy;
    }

    internal override bool Precondition(FrogState state)
    {
        return (state.FliesInBelly < 2 && state.FlyWithinReach && state.Row < 11);
    }
}



