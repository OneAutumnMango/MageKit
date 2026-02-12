namespace MageKit.Boosted
{
    public static class Upgrades
    {
        public readonly struct Tier(float rate, float up, float down)
        {
            public float Rate { get; } = rate;
            public float Up { get; } = up;
            public float Down { get; } = down;

            public bool Equals(Tier tier) => tier.Rate == Rate;
            public override bool Equals(object obj) => obj is Tier tier && Equals(tier);
            public override int GetHashCode() => 0;
        }

        public static readonly Tier Common =    new(1.00f, 0.25f, -0.10f);
        public static readonly Tier Rare =      new(0.25f, 0.50f, -0.20f);
        public static readonly Tier Legendary = new(0.05f, 0.75f, -0.30f);
        public static readonly Tier[] AllTiers = [Common, Rare, Legendary];

        public static Tier GetRandom()
        {
            double roll = Plugin.Random.NextDouble();
            if (roll < Legendary.Rate)
                return Legendary;
            else if (roll < Rare.Rate)
                return Rare;
            else
                return Common;
        }
    }
}
