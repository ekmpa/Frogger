public class CoolDown : HTNSimpleTask
{
    public override bool Execute(Frog frog)
    {
        frog.LaunchCoolDown();
        return true;
    }

    internal override FrogState Postcondition(FrogState state)
    {
        return state;
    }

    internal override bool Precondition(FrogState state)
    {
        return true;
    }
}
