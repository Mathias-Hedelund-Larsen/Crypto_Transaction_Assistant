public static class IntExtensions 
{
    public static int Pow(this int source, int powerOf)
    {
        int origSourceVal = source;

        for (int i = 0; i < powerOf - 1; i++)
        {
            source *= origSourceVal;
        }

        return source;
    }
}
