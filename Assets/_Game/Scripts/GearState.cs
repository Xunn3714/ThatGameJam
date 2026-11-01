namespace SeedGame
{
    // Kept across scene loads; starting the first level begins a fresh run.
    public static class GearState
    {
        public static int Count { get; private set; }

        public static void Reset() => Count = 0;
        // Level 2 always begins with the two gears earned in level 1.
        // This also makes opening its scene directly in the Editor playable.
        public static void BeginSecondLevel() => Count = 2;
        public static void Collect() { if (Count < 4) Count++; }
    }
}
