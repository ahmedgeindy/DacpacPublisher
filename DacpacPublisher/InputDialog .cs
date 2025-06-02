using System;
using System.Windows.Forms;

namespace DacpacPublisher
{
	public partial class InputDialog : Form
	{
		public InputDialog()
		{
			InitializeComponent();
		}

		public InputDialog(string title, string prompt, string defaultValue)
		{
			InitializeComponent();

			Text = title;
			lblPrompt.Text = prompt;
			txtInput.Text = defaultValue;
			InputValue = defaultValue;
		}

		public string InputValue { get; private set; }

		private void btnOK_Click(object sender, EventArgs e)
		{
			InputValue = txtInput.Text;
			DialogResult = DialogResult.OK;
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}