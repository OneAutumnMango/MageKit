namespace BalancePatch.Randomiser
{
    public static class RandomiserHelpers
    {
        public static int HashSeed(string seed)
        {
            int hash = 0;
            foreach (char c in seed)
            {
                hash = (hash * 31 + c) & 0x7FFFFFFF;
            }
            return hash;
        }
    }
}
