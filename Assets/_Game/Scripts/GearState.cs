namespace SeedGame
{
    // Kept across scene loads; starting the first level begins a fresh run.
    public static class GearState
    {
        public static int Count { get; private set; }

        public static void Reset() => Count = 0;
        public static void Collect() { if (Count < 4) Count++; }
    }
}
