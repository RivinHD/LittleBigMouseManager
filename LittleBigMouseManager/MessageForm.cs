using System;
using System.Windows.Forms;

namespace LittleBigMouseManager
{
    // From: https://stackoverflow.com/a/6560533
    internal partial class MessageForm : Form
    {
        internal MessageForm()
        {
            InitializeComponent();
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void MessageForm_Load(object sender, EventArgs e)
        {

        }

        private void MessageForm_Load_1(object sender, EventArgs e)
        {

        }
    }
}
