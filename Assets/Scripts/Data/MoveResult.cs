namespace Game.Data
{
    public readonly struct MoveResult
    {
        public bool Success { get; }
        public int Remainder { get; }

        public MoveResult(bool success, int remainder)
        {
            Success = success;
            Remainder = remainder;
        }
    }
}
