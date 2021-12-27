namespace OpenGLBouncingDVD.Source.GameLoop
{
    static class GameTime
    {
        public static double DeltaTime { get; set; }
        public static double TotalElapsedSeconds { get; set; }
        public static float DeltaTimeF => (float)DeltaTime;
        public static float TotalElapsedSecondsF => (float)TotalElapsedSeconds;
    }
}
