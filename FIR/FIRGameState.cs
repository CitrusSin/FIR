using System;
using System.Threading.Tasks;

namespace FIR
{
    public class FIRGameState
    {
        public enum SlotState
        {
            EMPTY, BLACK, WHITE
        }

        public FIRGameState(int width, int height, FIRPlayer player1, FIRPlayer player2)
        {
            Width = width;
            Height = height;
            BlackPlayer = player1;
            WhitePlayer = player2;
            checkerBoard = new SlotState[width, height];
        }

        public virtual int Width { get; }
        public virtual int Height { get; }
        public FIRPlayer BlackPlayer { get; }
        public FIRPlayer WhitePlayer { get; }
        public bool HasEnded { get; protected set; } = false;

        private SlotState[,] checkerBoard;

        public SlotState this[Slot slot]
        {
            get
            {
                CheckSlotThrow(slot);
                return checkerBoard[slot.X, slot.Y];
            }
            protected set
            {
                CheckSlotThrow(slot);
                checkerBoard[slot.X, slot.Y] = value;
            }
        }

        public SlotState this[int x, int y]
        {
            get
            {
                Slot s = new Slot(x, y);
                return this[s];
            }
            protected set
            {
                Slot s = new Slot(x, y);
                this[s] = value;
            }
        }

        public SlotState GetPieceOfSlot(Slot slot)
        {
            return this[slot];
        }

        protected void SetStateOfSlot(SlotState state, Slot slot)
        {
            this[slot] = state;
        }

        public bool CheckSlot(Slot slot)
        {
            if (slot == null || slot.X >= Width || slot.Y >= Height || slot.X < 0 || slot.Y < 0)
            {
                return false;
            }
            return true;
        }

        private void CheckSlotThrow(Slot slot)
        {
            if (!CheckSlot(slot))
            {
                throw new ArgumentOutOfRangeException("slot", slot, "Slot outside the board");
            }
        }

        private bool CheckWinFromSlot(Slot slot)
        {
            SlotState state = this[slot];
            if (state == SlotState.EMPTY)
            {
                return false;
            }
            // Check at back slash line
            {
                int count = 1;
                Slot emu = slot.UpperLeft;
                while (CheckSlot(emu) && this[emu] == state)
                {
                    count++;
                    emu = emu.UpperLeft;
                }
                emu = slot.BottomRight;
                while (CheckSlot(emu) && this[emu] == state)
                {
                    count++;
                    emu = emu.BottomRight;
                }
                if (count >= 5)
                {
                    return true;
                }
            }
            // Check at vertical line
            {
                int count = 1;
                Slot emu = slot.Upper;
                while (CheckSlot(emu) && this[emu] == state)
                {
                    count++;
                    emu = emu.Upper;
                }
                emu = slot.Bottom;
                while (CheckSlot(emu) && this[emu] == state)
                {
                    count++;
                    emu = emu.Bottom;
                }
                if (count >= 5)
                {
                    return true;
                }
            }
            // Check at forward slash line
            {
                int count = 1;
                Slot emu = slot.UpperRight;
                while (CheckSlot(emu) && this[emu] == state)
                {
                    count++;
                    emu = emu.UpperRight;
                }
                emu = slot.BottomLeft;
                while (CheckSlot(emu) && this[emu] == state)
                {
                    count++;
                    emu = emu.BottomLeft;
                }
                if (count >= 5)
                {
                    return true;
                }
            }
            // Check at horizontal line
            {
                int count = 1;
                Slot emu = slot.Left;
                while (CheckSlot(emu) && this[emu] == state)
                {
                    count++;
                    emu = emu.Left;
                }
                emu = slot.Right;
                while (CheckSlot(emu) && this[emu] == state)
                {
                    count++;
                    emu = emu.Right;
                }
                if (count >= 5)
                {
                    return true;
                }
            }
            return false;
        }

        public void Run()
        {
            BlackPlayer.Ready(this);
            BlackPlayer.SetColor(SlotState.BLACK);
            WhitePlayer.Ready(this);
            WhitePlayer.SetColor(SlotState.WHITE);
            Slot lastSlot = BlackPlayer.GetFirstPiece(this);
            if (lastSlot != null)
            {
                this[lastSlot] = SlotState.BLACK;
            }
            while (true)
            {
                lastSlot = WhitePlayer.GetNextPiece(this, lastSlot);
                if (lastSlot != null && this[lastSlot] == SlotState.EMPTY)
                {
                    this[lastSlot] = SlotState.WHITE;
                    if (CheckWinFromSlot(lastSlot))
                    {
                        HasEnded = true;
                        WhitePlayer.Win(this);
                        BlackPlayer.Lose(this);
                        return;
                    }
                }
                if (HasEnded) return;
                lastSlot = BlackPlayer.GetNextPiece(this, lastSlot);
                if (lastSlot != null && this[lastSlot] == SlotState.EMPTY)
                {
                    this[lastSlot] = SlotState.BLACK;
                    if (CheckWinFromSlot(lastSlot))
                    {
                        HasEnded = true;
                        BlackPlayer.Win(this);
                        WhitePlayer.Lose(this);
                        return;
                    }
                }
                if (HasEnded) return;
            }
        }

        public void Terminate()
        {
            HasEnded = true;
            BlackPlayer.Win(this);
            WhitePlayer.Lose(this);
        }

        public async Task RunAsync()
        {
            await Task.Run(Run);
        }
    }
}
