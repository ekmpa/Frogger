
public abstract class HTNSimpleTask : HTNTask
{
    public abstract bool Execute(Frog frog);
    internal abstract bool Precondition(FrogState state);
    internal abstract FrogState Postcondition(FrogState state);
}