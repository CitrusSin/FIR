using System.Threading;

namespace FIR
{
    public class FIRLocalUserPlayer : FIRPlayer
    {
        FIRGameState.SlotState SlotState;

        public FIRLocalUserPlayer(IFIRUserInterface ui)
        {
            UI = ui;
        }

        public IFIRUserInterface UI { get; }

        public override Slot GetFirstPiece(FIRGameState state)
        {
            UI.DrawGameState(state);
            UI.AllowOnceClick();
            while (!UI.HasResult())
            {
                Thread.Sleep(10);
                if (state.HasEnded)
                {
                    UI.CancelListening();
                    return null;
                }
            }
            return UI.GetLastClickSlot();
        }

        public override Slot GetNextPiece(FIRGameState state, Slot enemyLastPiece)
        {
            UI.DrawGameState(state, enemyLastPiece);
            UI.AllowOnceClick();
            while (!UI.HasResult())
            {
                Thread.Sleep(10);
                if (state.HasEnded)
                {
                    UI.CancelListening();
                    return null;
                }
            }
            return UI.GetLastClickSlot();
        }

        public override void Lose(FIRGameState state)
        {
            FIRGameState.SlotState winner = FIRGameState.SlotState.EMPTY;
            switch (SlotState)
            {
                case FIRGameState.SlotState.WHITE:
                    winner = FIRGameState.SlotState.BLACK;
                    break;
                case FIRGameState.SlotState.BLACK:
                    winner = FIRGameState.SlotState.WHITE;
                    break;
            }
            UI.Finish(state, winner);
        }

        public override void SetColor(FIRGameState.SlotState color)
        {
            SlotState = color;
        }

        public override void Win(FIRGameState state)
        {
            UI.Finish(state, SlotState);
        }
    }
}
