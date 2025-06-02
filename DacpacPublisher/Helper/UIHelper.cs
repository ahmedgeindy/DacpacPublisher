using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DacpacPublisher.Helper
{
	public static class UIHelper
	{
		public enum StatusType
		{
			Success,
			Warning,
			Error,
			Info,
			Processing
		}

		// PROFESSIONAL COLOR PALETTE
		public static readonly Color ISTBlue = Color.FromArgb(25, 118, 210);
		public static readonly Color ISTDarkBlue = Color.FromArgb(13, 71, 161);
		public static readonly Color SuccessGreen = Color.FromArgb(46, 125, 50);
		public static readonly Color WarningOrange = Color.FromArgb(255, 152, 0);
		public static readonly Color DangerRed = Color.FromArgb(211, 47, 47);
		public static readonly Color NeutralGray = Color.FromArgb(97, 97, 97);
		public static readonly Color LightGray = Color.FromArgb(250, 250, 250);
		public static readonly Color CardBackground = Color.FromArgb(255, 255, 255);
		public static readonly Color DarkText = Color.FromArgb(66, 66, 66);
		public static readonly Color AccentBlue = Color.FromArgb(63, 81, 181);

		// PROFESSIONAL TYPOGRAPHY
		public static readonly Font HeaderFont = new Font("Segoe UI", 11F, FontStyle.Bold);
		public static readonly Font LabelFont = new Font("Segoe UI", 9F, FontStyle.Regular);
		public static readonly Font ButtonFont = new Font("Segoe UI", 9F, FontStyle.Regular);
		public static readonly Font TitleFont = new Font("Segoe UI", 14F, FontStyle.Bold);
		public static readonly Font SubtitleFont = new Font("Segoe UI", 10F, FontStyle.Regular);
		public static readonly Font MonospaceFont = new Font("Consolas", 9F, FontStyle.Regular);

		/// <summary>
		///     Applies modern flat button styling
		/// </summary>
		public static void StyleModernButton(Button button, Color backgroundColor, Color foregroundColor,
			string iconText = "")
		{
			button.BackColor = backgroundColor;
			button.ForeColor = foregroundColor;
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderSize = 0;
			button.Font = ButtonFont;
			button.Cursor = Cursors.Hand;
			button.UseVisualStyleBackColor = false;

			if (!string.IsNullOrEmpty(iconText)) button.Text = $"{iconText} {button.Text.Replace(iconText, "").Trim()}";

			// Add hover effects
			AddButtonHoverEffect(button, backgroundColor);
		}

		/// <summary>
		///     Adds sophisticated hover effects to buttons
		/// </summary>
		public static void AddButtonHoverEffect(Button button, Color originalColor)
		{
			var hoverColor = LightenColor(originalColor, 15);
			var pressedColor = DarkenColor(originalColor, 10);

			button.MouseEnter += (s, e) =>
			{
				button.BackColor = hoverColor;
				button.FlatAppearance.BorderSize = 1;
				button.FlatAppearance.BorderColor = DarkenColor(originalColor, 30);
			};

			button.MouseLeave += (s, e) =>
			{
				button.BackColor = originalColor;
				button.FlatAppearance.BorderSize = 0;
			};

			button.MouseDown += (s, e) => { button.BackColor = pressedColor; };

			button.MouseUp += (s, e) =>
			{
				button.BackColor = button.ClientRectangle.Contains(button.PointToClient(Cursor.Position))
					? hoverColor
					: originalColor;
			};
		}

		/// <summary>
		///     Styles a GroupBox with modern card appearance
		/// </summary>
		public static void StyleModernGroupBox(GroupBox groupBox, string iconText = "")
		{
			groupBox.Font = HeaderFont;
			groupBox.ForeColor = DarkText;
			groupBox.BackColor = CardBackground;
			groupBox.FlatStyle = FlatStyle.Flat;

			if (!string.IsNullOrEmpty(iconText))
				groupBox.Text = $"{iconText} {groupBox.Text.Replace(iconText, "").Trim()}";

			// Add subtle shadow effect
			groupBox.Paint += (s, e) =>
			{
				var rect = groupBox.ClientRectangle;
				rect.Inflate(-1, -1);
				using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
				{
					e.Graphics.DrawRectangle(pen, rect);
				}
			};
		}

		/// <summary>
		///     Applies modern styling to TextBox controls
		/// </summary>
		public static void StyleModernTextBox(TextBox textBox)
		{
			textBox.Font = LabelFont;
			textBox.BorderStyle = BorderStyle.FixedSingle;
			textBox.BackColor = Color.White;

			// Add focus effects
			textBox.Enter += (s, e) => { textBox.BackColor = Color.FromArgb(248, 252, 255); };

			textBox.Leave += (s, e) => { textBox.BackColor = Color.White; };
		}

		/// <summary>
		///     Applies modern styling to ComboBox controls
		/// </summary>
		public static void StyleModernComboBox(ComboBox comboBox)
		{
			comboBox.Font = LabelFont;
			comboBox.FlatStyle = FlatStyle.Flat;
			comboBox.BackColor = Color.White;
		}

		/// <summary>
		///     Creates a modern progress indicator
		/// </summary>
		public static void StyleModernProgressBar(ProgressBar progressBar)
		{
			progressBar.Style = ProgressBarStyle.Continuous;
			progressBar.BackColor = LightGray;
			progressBar.ForeColor = ISTBlue;
		}

		/// <summary>
		///     Applies modern styling to the entire form
		/// </summary>
		public static void ApplyModernStyling(Form form)
		{
			form.BackColor = LightGray;
			form.Font = LabelFont;

			ApplyModernStylingRecursive(form);
		}

		private static void ApplyModernStylingRecursive(Control parent)
		{
			foreach (Control control in parent.Controls)
			{
				switch (control)
				{
					case Button btn:
						if (btn.BackColor == SystemColors.Control) // Only style if not already styled
							StyleModernButton(btn, ISTBlue, Color.White);
						break;

					case GroupBox grp:
						StyleModernGroupBox(grp);
						break;

					case TextBox txt:
						StyleModernTextBox(txt);
						break;

					case ComboBox cmb:
						StyleModernComboBox(cmb);
						break;

					case ProgressBar prg:
						StyleModernProgressBar(prg);
						break;

					case Label lbl:
						if (lbl.Font == SystemFonts.DefaultFont)
						{
							lbl.Font = LabelFont;
							lbl.ForeColor = DarkText;
						}

						break;

					case CheckBox chk:
						chk.Font = LabelFont;
						chk.ForeColor = DarkText;
						break;
				}

				// Recursively apply to child controls
				if (control.HasChildren) ApplyModernStylingRecursive(control);
			}
		}

		/// <summary>
		///     Creates a modern gradient background
		/// </summary>
		public static void ApplyGradientBackground(Control control, Color startColor, Color endColor,
			LinearGradientMode mode = LinearGradientMode.Vertical)
		{
			control.Paint += (s, e) =>
			{
				using (var brush = new LinearGradientBrush(
						   control.ClientRectangle, startColor, endColor, mode))
				{
					e.Graphics.FillRectangle(brush, control.ClientRectangle);
				}
			};
		}

		/// <summary>
		///     Creates a modern card-style panel
		/// </summary>
		public static Panel CreateModernCard(int x, int y, int width, int height, string title = "")
		{
			var card = new Panel
			{
				Location = new Point(x, y),
				Size = new Size(width, height),
				BackColor = CardBackground,
				BorderStyle = BorderStyle.FixedSingle,
				Padding = new Padding(15)
			};

			if (!string.IsNullOrEmpty(title))
			{
				var titleLabel = new Label
				{
					Text = title,
					Font = HeaderFont,
					ForeColor = DarkText,
					AutoSize = true,
					Location = new Point(15, 15)
				};
				card.Controls.Add(titleLabel);
			}

			// Add subtle shadow effect
			card.Paint += (s, e) =>
			{
				var shadowRect = new Rectangle(2, 2, card.Width - 2, card.Height - 2);
				using (var shadowBrush = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
				{
					e.Graphics.FillRectangle(shadowBrush, shadowRect);
				}
			};

			return card;
		}

		/// <summary>
		///     Creates a modern status indicator
		/// </summary>
		public static Label CreateStatusIndicator(string text, StatusType status)
		{
			var indicator = new Label
			{
				Text = GetStatusIcon(status) + " " + text,
				Font = LabelFont,
				AutoSize = true,
				ForeColor = GetStatusColor(status),
				BackColor = GetStatusBackgroundColor(status),
				Padding = new Padding(8, 4, 8, 4),
				BorderStyle = BorderStyle.FixedSingle
			};

			return indicator;
		}

		private static string GetStatusIcon(StatusType status)
		{
			return status switch
			{
				StatusType.Success => "✅",
				StatusType.Warning => "⚠️",
				StatusType.Error => "❌",
				StatusType.Info => "ℹ️",
				StatusType.Processing => "🔄",
				_ => "•"
			};
		}

		private static Color GetStatusColor(StatusType status)
		{
			return status switch
			{
				StatusType.Success => Color.FromArgb(27, 94, 32),
				StatusType.Warning => Color.FromArgb(245, 124, 0),
				StatusType.Error => Color.FromArgb(198, 40, 40),
				StatusType.Info => Color.FromArgb(2, 136, 209),
				StatusType.Processing => Color.FromArgb(121, 85, 72),
				_ => DarkText
			};
		}

		private static Color GetStatusBackgroundColor(StatusType status)
		{
			return status switch
			{
				StatusType.Success => Color.FromArgb(232, 245, 233),
				StatusType.Warning => Color.FromArgb(255, 243, 224),
				StatusType.Error => Color.FromArgb(255, 235, 238),
				StatusType.Info => Color.FromArgb(227, 242, 253),
				StatusType.Processing => Color.FromArgb(239, 235, 233),
				_ => LightGray
			};
		}

		/// <summary>
		///     Lightens a color by the specified percentage
		/// </summary>
		public static Color LightenColor(Color color, int percentage)
		{
			var factor = 1.0f + percentage / 100.0f;
			return Color.FromArgb(
				color.A,
				Math.Min(255, (int)(color.R * factor)),
				Math.Min(255, (int)(color.G * factor)),
				Math.Min(255, (int)(color.B * factor))
			);
		}

		/// <summary>
		///     Darkens a color by the specified percentage
		/// </summary>
		public static Color DarkenColor(Color color, int percentage)
		{
			var factor = 1.0f - percentage / 100.0f;
			return Color.FromArgb(
				color.A,
				Math.Max(0, (int)(color.R * factor)),
				Math.Max(0, (int)(color.G * factor)),
				Math.Max(0, (int)(color.B * factor))
			);
		}

		/// <summary>
		///     Creates a modern tooltip with enhanced styling
		/// </summary>
		public static ToolTip CreateModernToolTip()
		{
			var toolTip = new ToolTip
			{
				BackColor = Color.FromArgb(55, 55, 55),
				ForeColor = Color.White,
				IsBalloon = false,
				OwnerDraw = true
			};

			toolTip.Draw += (s, e) =>
			{
				e.Graphics.FillRectangle(new SolidBrush(toolTip.BackColor), e.Bounds);
				e.Graphics.DrawString(e.ToolTipText, new Font("Segoe UI", 9F),
					new SolidBrush(toolTip.ForeColor), e.Bounds.Location);
			};

			return toolTip;
		}

		/// <summary>
		///     Applies modern styling to DataGridView
		/// </summary>
		public static void StyleModernDataGridView(DataGridView dgv)
		{
			dgv.BorderStyle = BorderStyle.FixedSingle;
			dgv.BackgroundColor = Color.White;
			dgv.GridColor = Color.FromArgb(230, 230, 230);
			dgv.Font = LabelFont;

			// Header styling
			dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
			dgv.ColumnHeadersDefaultCellStyle.ForeColor = DarkText;
			dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 220, 220);

			// Row styling
			dgv.DefaultCellStyle.BackColor = Color.White;
			dgv.DefaultCellStyle.ForeColor = DarkText;
			dgv.DefaultCellStyle.SelectionBackColor = ISTBlue;
			dgv.DefaultCellStyle.SelectionForeColor = Color.White;
			dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);

			// Remove default styling
			dgv.EnableHeadersVisualStyles = false;
			dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
			dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
		}

		/// <summary>
		///     Creates a modern loading overlay
		/// </summary>
		public static Panel CreateLoadingOverlay(Control parent, string message = "Loading...")
		{
			var overlay = new Panel
			{
				Size = parent.Size,
				Location = new Point(0, 0),
				BackColor = Color.FromArgb(180, 255, 255, 255),
				Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
			};

			var loadingLabel = new Label
			{
				Text = "🔄 " + message,
				Font = new Font("Segoe UI", 12F, FontStyle.Bold),
				ForeColor = DarkText,
				AutoSize = true,
				BackColor = Color.Transparent
			};

			// Center the loading label
			loadingLabel.Location = new Point(
				(overlay.Width - loadingLabel.Width) / 2,
				(overlay.Height - loadingLabel.Height) / 2
			);

			overlay.Controls.Add(loadingLabel);
			parent.Controls.Add(overlay);
			overlay.BringToFront();

			return overlay;
		}
	}
}