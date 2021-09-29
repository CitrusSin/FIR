namespace FIR
{
    public interface IFIRUserInterface
    {
        void AllowOnceClick();
        void CancelListening();
        bool HasResult();
        Slot GetLastClickSlot();
        void DrawGameState(FIRGameState state, Slot lastSlot = null);
        void Finish(FIRGameState state, FIRGameState.SlotState winner);
    }
}
