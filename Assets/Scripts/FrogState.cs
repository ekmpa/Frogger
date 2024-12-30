
public class FrogState
{
    public int FliesInBelly { get; set; }
    public bool FlyWithinReach { get; set; }
    public int Row { get; set; }

    public FrogState(int fliesInBelly = 0, bool flyWithinReach = false, int row = 0)
    {
        FliesInBelly = fliesInBelly;
        FlyWithinReach = flyWithinReach;
        Row = row;
    }

    public override string ToString()
    {
        return $"FliesInBelly: {FliesInBelly}, FlyWithinReach: {FlyWithinReach}, Row: {Row}";
    }

    public FrogState DeepCopy()
    {
        return new FrogState(FliesInBelly, FlyWithinReach, Row);
    }
}

