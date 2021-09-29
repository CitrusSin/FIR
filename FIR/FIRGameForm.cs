using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FIR
{
    public partial class FIRGameForm : Form, IFIRUserInterface
    {
        FIRGameState state;
        Slot lastSlot;
        Task gameTask;
        string status = "";
        int BoardWidth = 15, BoardHeight = 15;

        public FIRGameForm()
        {
            InitializeComponent();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void NewGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewGame();
        }

        private void FIRGameForm_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.BurlyWood);
            var rect = new Rectangle(
                ClientRectangle.X,
                ClientRectangle.Y + menuStrip1.Height,
                ClientRectangle.Width,
                ClientRectangle.Height - menuStrip1.Height);
            float widthSpacing = rect.Width / (float)(BoardWidth + 1), heightSpacing = rect.Height / (float)(BoardHeight + 1);
            for (int x = 1; x <= BoardWidth; x++)
            {
                float xpos = rect.Left + (x * widthSpacing);
                g.DrawLine(Pens.Black, xpos, rect.Top + heightSpacing, xpos, rect.Top + (BoardWidth * heightSpacing));
            }
            for (int y = 1; y <= BoardHeight; y++)
            {
                float ypos = rect.Top + (y * heightSpacing);
                g.DrawLine(Pens.Black, rect.Left + widthSpacing, ypos, rect.Left + (BoardHeight * widthSpacing), ypos);
            }
            if (state != null)
            {
                for (int x = 0; x < state.Width; x++)
                {
                    for (int y = 0; y < state.Height; y++)
                    {
                        float radius = Math.Min(widthSpacing, heightSpacing) * 0.4f;
                        float pixelX = (x + 1) * widthSpacing;
                        float pixelY = (y + 1) * heightSpacing + menuStrip1.Height;
                        var piece = state[x, y];
                        switch (piece)
                        {
                            case FIRGameState.SlotState.BLACK:
                                g.FillEllipse(Brushes.Black, pixelX - radius, pixelY - radius, 2 * radius, 2 * radius);
                                break;
                            case FIRGameState.SlotState.WHITE:
                                g.FillEllipse(Brushes.White, pixelX - radius, pixelY - radius, 2 * radius, 2 * radius);
                                g.DrawEllipse(Pens.Black, pixelX - radius, pixelY - radius, 2 * radius, 2 * radius);
                                break;
                        }
                    }
                }
                if (lastSlot != null)
                {
                    float pixelX = (lastSlot.X + 1) * widthSpacing, pixelY = (lastSlot.Y + 1) * heightSpacing + menuStrip1.Height;
                    float radius = Math.Min(widthSpacing, heightSpacing) * 0.4f;
                    g.DrawRectangle(Pens.Red, pixelX - radius, pixelY - radius, radius * 2, radius * 2);
                }
                if (status != null)
                {
                    Font statusFont = new Font(FontFamily.GenericSansSerif, 15);
                    SizeF size = g.MeasureString(status, statusFont);
                    g.DrawString(status, statusFont, Brushes.Black, (rect.Width / 2f) - (size.Width / 2f), menuStrip1.Height + 1f);
                }
            }
        }

        private void FIRGameForm_Resize(object sender, EventArgs e)
        {
            Refresh();
        }

        private void NewGame()
        {
            if (state != null)
            {
                state.Terminate();
            }
            status = "";
            var dialog = new NewGameDialog();
            dialog.ShowDialog(this);
            FIRPlayer player1 = null, player2 = null;
            switch (dialog.Player1Type)
            {
                case NewGameDialog.PlayerType.PLAYER:
                    player1 = new FIRLocalUserPlayer(this);
                    break;
                case NewGameDialog.PlayerType.MACHINE:
                    player1 = new FIRMachinePlayer();
                    break;
            }
            switch (dialog.Player2Type)
            {
                case NewGameDialog.PlayerType.PLAYER:
                    player2 = new FIRLocalUserPlayer(this);
                    break;
                case NewGameDialog.PlayerType.MACHINE:
                    player2 = new FIRMachinePlayer();
                    break;
            }
            state = new FIRGameState(BoardWidth, BoardHeight, player1, player2);
            gameTask = state.RunAsync();
        }

        private void FIRGameForm_MouseClick(object sender, MouseEventArgs e)
        {
            lock (statusAccessLock)
            {
                if (isListening)
                {
                    status = "等待另外一方落子";
                    isListening = false;
                    hasResultNotCommited = true;
                    var rect = new Rectangle(
                        ClientRectangle.X,
                        ClientRectangle.Y + menuStrip1.Height,
                        ClientRectangle.Width,
                        ClientRectangle.Height - menuStrip1.Height);
                    float widthSpacing = rect.Width / (float)(BoardWidth + 1), heightSpacing = rect.Height / (float)(BoardHeight + 1);
                    Point p = new Point(e.X, e.Y - menuStrip1.Height);
                    int x = (int)Math.Round(p.X / widthSpacing);
                    int y = (int)Math.Round(p.Y / heightSpacing);
                    lastResult = new Slot(x - 1, y - 1);
                    Refresh();
                }
            }
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog(this);
        }

        void IFIRUserInterface.Finish(FIRGameState state, FIRGameState.SlotState winner)
        {
            switch (winner)
            {
                case FIRGameState.SlotState.EMPTY:
                    status = "程序发生错误";
                    break;
                case FIRGameState.SlotState.BLACK:
                    status = "黑方胜利！";
                    break;
                case FIRGameState.SlotState.WHITE:
                    status = "白方胜利！";
                    break;
            }
            isListening = false;
            Invoke(new Action(Refresh));
        }

        void IFIRUserInterface.AllowOnceClick()
        {
            status = "到你下子了";
            lock (statusAccessLock)
            {
                isListening = true;
            }
            Invoke(new Action(Refresh));
        }

        private readonly object statusAccessLock = new object();
        private bool isListening = false;
        private bool hasResultNotCommited = false;
        private Slot lastResult;

        bool IFIRUserInterface.HasResult()
        {
            bool ret;
            lock (statusAccessLock)
            {
                ret = hasResultNotCommited;
            }
            return ret;
        }

        Slot IFIRUserInterface.GetLastClickSlot()
        {
            lock (statusAccessLock)
            {
                if (hasResultNotCommited)
                {
                    hasResultNotCommited = false;
                    return lastResult;
                }
            }
            return null;
        }

        void IFIRUserInterface.DrawGameState(FIRGameState state, Slot lastSlot)
        {
            this.lastSlot = lastSlot;
            Invoke(new Action(Refresh));
        }

        void IFIRUserInterface.CancelListening()
        {
            lock (statusAccessLock)
            {
                isListening = false;
                hasResultNotCommited = false;
            }
        }
    }
}
