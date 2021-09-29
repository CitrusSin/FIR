using System;
using System.Windows.Forms;

namespace FIR
{
    public partial class NewGameDialog : Form
    {
        public enum PlayerType
        {
            PLAYER, MACHINE
        }
        public PlayerType Player1Type { get; private set; }
        public PlayerType Player2Type { get; private set; }

        public NewGameDialog()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    Player1Type = PlayerType.PLAYER;
                    break;
                case 1:
                    Player1Type = PlayerType.MACHINE;
                    break;
            }
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    Player2Type = PlayerType.PLAYER;
                    break;
                case 1:
                    Player2Type = PlayerType.MACHINE;
                    break;
            }
            Close();
        }

        private void NewGameDialog_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 1;
        }
    }
}
