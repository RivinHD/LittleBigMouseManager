using System;
using System.Drawing;
using System.Windows.Forms;

namespace LittleBigMouseManager
{
    // From: https://stackoverflow.com/a/6560533
    internal static class CustomMessageBox
    {
        public static DialogResult Show(string Text, string Title, eDialogButtons Buttons, Icon Icon)
        {
            MessageForm message = new MessageForm();
            message.Text = Title;

            message.Icon = Icon;
            message.lblText.Text = Text;

            switch (Buttons)
            {
                case eDialogButtons.OK:
                    message.btnYes.Visible = false;
                    message.btnNo.Visible = false;
                    message.btnCancel.Visible = false;
                    message.btnOK.Location = message.btnCancel.Location;
                    break;
                case eDialogButtons.OKCancel:
                    message.btnYes.Visible = false;
                    message.btnNo.Visible = false;
                    break;
                case eDialogButtons.YesNo:
                    message.btnOK.Visible = false;
                    message.btnCancel.Visible = false;
                    message.btnYes.Location = message.btnOK.Location;
                    message.btnNo.Location = message.btnCancel.Location;
                    break;
                case eDialogButtons.YesNoCancel:
                    message.btnOK.Visible = false;
                    break;
            }

            if (message.lblText.Height > 64)
                message.Height = (message.lblText.Top + message.lblText.Height) + 78;

            return (message.ShowDialog());
        }

        public enum eDialogButtons { OK, OKCancel, YesNo, YesNoCancel }
    }
}
