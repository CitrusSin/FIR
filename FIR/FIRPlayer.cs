namespace FIR
{
    public abstract class FIRPlayer
    {
        public virtual void SetColor(FIRGameState.SlotState color) { }
        public virtual void Ready(FIRGameState state) { }
        public abstract Slot GetFirstPiece(FIRGameState state);
        public abstract Slot GetNextPiece(FIRGameState state, Slot enemyLastPiece);
        public virtual void Win(FIRGameState state) { }
        public virtual void Lose(FIRGameState state) { }
    }
}
