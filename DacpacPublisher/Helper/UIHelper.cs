using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace DacpacPublisher.Helper
{
	/// <summary>
	/// Enhanced UI Helper with modern styling and improved theming
	/// </summary>
	public static class UIHelper
	{
		#region Enhanced Color Palette

		// Primary Colors
		public static readonly Color PrimaryBlue = Color.FromArgb(25, 118, 210);
		public static readonly Color PrimaryDark = Color.FromArgb(13, 71, 161);
		public static readonly Color PrimaryLight = Color.FromArgb(66, 165, 245);

		// Status Colorsa
		public static readonly Color SuccessGreen = Color.FromArgb(46, 125, 50);
		public static readonly Color WarningOrange = Color.FromArgb(255, 152, 0);
		public static readonly Color ErrorRed = Color.FromArgb(211, 47, 47);
		public static readonly Color InfoBlue = Color.FromArgb(2, 136, 209);

		// Neutral Colors
		public static readonly Color DarkGray = Color.FromArgb(66, 66, 66);
		public static readonly Color MediumGray = Color.FromArgb(117, 117, 117);
		public static readonly Color LightGray = Color.FromArgb(245, 245, 245);
		public static readonly Color Background = Color.FromArgb(250, 250, 250);
		public static readonly Color Surface = Color.White;
		public static readonly Color Border = Color.FromArgb(224, 224, 224);

		// Interactive Colors
		public static readonly Color HoverLight = Color.FromArgb(248, 252, 255);
		public static readonly Color FocusBlue = Color.FromArgb(227, 242, 253);
		public static readonly Color DisabledGray = Color.FromArgb(189, 189, 189);
		public static readonly Color ActiveGreen = Color.FromArgb(232, 245, 233);

		// Enhanced Colors for Modern UI
		public static readonly Color CardShadow = Color.FromArgb(30, 0, 0, 0);
		public static readonly Color Accent = Color.FromArgb(156, 39, 176);
		public static readonly Color SecondaryText = Color.FromArgb(117, 117, 117);

		#endregion
		// ADD this method to your UIHelper.cs file (in the Additional Control Styling region)

		#region Compatibility Methods for DACPAC Publisher

		/// <summary>
		/// Apply modern data grid styling (alias for StyleDataGridView)
		/// </summary>
		public static void ApplyModernDataGridStyle(DataGridView dataGridView)
		{
			StyleDataGridView(dataGridView);
		}

		/// <summary>
		/// Apply modern theme to control (alias for recursive application)
		/// </summary>
		public static void ApplyModernTheme(Control parentControl)
		{
			if (parentControl == null || IsDesignMode(parentControl)) return;

			ApplyThemeRecursive(parentControl);
		}

		/// <summary>
		/// Get hover color for buttons (compatibility method)
		/// </summary>
		public static Color GetHoverColor(Color baseColor)
		{
			return GetHoverColor(baseColor, ButtonStyle.Primary);
		}

		#endregion
		#region Typography System

		private static Font CreateSafeFont(string fontName, float size, FontStyle style)
		{
			try
			{
				return new Font(fontName, size, style);
			}
			catch
			{
				return new Font(FontFamily.GenericSansSerif, size, style);
			}
		}

		// Typography Scale
		public static readonly Font HeaderFont = CreateSafeFont("Segoe UI", 16F, FontStyle.Bold);
		public static readonly Font SubheaderFont = CreateSafeFont("Segoe UI", 12F, FontStyle.Bold);
		public static readonly Font BodyFont = CreateSafeFont("Segoe UI", 9F, FontStyle.Regular);
		public static readonly Font ButtonFont = CreateSafeFont("Segoe UI", 9F, FontStyle.Regular);
		public static readonly Font SmallFont = CreateSafeFont("Segoe UI", 8F, FontStyle.Regular);
		public static readonly Font MonospaceFont = CreateSafeFont("Consolas", 9F, FontStyle.Regular);
		public static readonly Font LargeButtonFont = CreateSafeFont("Segoe UI", 11F, FontStyle.Bold);
		public static readonly Font CaptionFont = CreateSafeFont("Segoe UI", 8F, FontStyle.Italic);

		#endregion

		#region Layout Constants

		public const int SpacingXSmall = 2;
		public const int SpacingSmall = 4;
		public const int Spacing = 8;
		public const int SpacingMedium = 12;
		public const int SpacingLarge = 16;
		public const int SpacingXLarge = 24;
		public const int SpacingXXLarge = 32;

		public const int BorderRadius = 4;
		public const int BorderRadiusLarge = 8;
		public const int ShadowOffset = 2;

		public const int ControlHeightSmall = 24;
		public const int ControlHeight = 32;
		public const int ControlHeightLarge = 40;
		public const int ButtonHeight = 35;
		public const int ButtonHeightLarge = 45;

		#endregion

		#region Enhanced Control Styling

		private static bool IsDesignMode(Control control)
		{
			return LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
				   control?.Site?.DesignMode == true;
		}

		/// <summary>
		/// Apply modern button styling with enhanced visual feedback
		/// </summary>
		public static void StyleButton(Button button, ButtonStyle style = ButtonStyle.Primary)
		{
			if (button == null || IsDesignMode(button)) return;

			// Base button properties
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderSize = 0;
			button.Font = ButtonFont;
			button.Cursor = Cursors.Hand;
			button.Height = ButtonHeight;
			button.Padding = new Padding(SpacingLarge, 0, SpacingLarge, 0);
			button.TextAlign = ContentAlignment.MiddleCenter;

			// Apply style-specific colors
			ApplyButtonStyle(button, style);

			// Add enhanced hover effects
			AddModernButtonEffects(button, style);

			// Add subtle rounded corners
			AddRoundedCorners(button, BorderRadius);
		}

		/// <summary>
		/// Style large action buttons
		/// </summary>
		public static void StyleLargeButton(Button button, ButtonStyle style = ButtonStyle.Primary)
		{
			StyleButton(button, style);
			button.Height = ButtonHeightLarge;
			button.Font = LargeButtonFont;
			button.Padding = new Padding(SpacingXLarge, 0, SpacingXLarge, 0);
			AddRoundedCorners(button, BorderRadiusLarge);
		}

		/// <summary>
		/// Apply modern group box styling
		/// </summary>
		public static void StyleGroupBox(GroupBox groupBox)
		{
			if (groupBox == null || IsDesignMode(groupBox)) return;

			groupBox.Font = SubheaderFont;
			groupBox.ForeColor = DarkGray;
			groupBox.BackColor = Surface;
			groupBox.Padding = new Padding(SpacingLarge);

			// Custom paint for modern appearance
			groupBox.Paint += (s, e) => DrawModernGroupBox(groupBox, e.Graphics);
		}

		/// <summary>
		/// Style text boxes with modern appearance
		/// </summary>
		public static void StyleTextBox(TextBox textBox)
		{
			if (textBox == null || IsDesignMode(textBox)) return;

			textBox.Font = BodyFont;
			textBox.BorderStyle = BorderStyle.FixedSingle;
			textBox.BackColor = Surface;
			textBox.Height = ControlHeight;
			textBox.Padding = new Padding(SpacingSmall);

			// Add focus effects
			AddTextBoxFocusEffects(textBox);
		}

		/// <summary>
		/// Style combo boxes with consistent appearance
		/// </summary>
		public static void StyleComboBox(ComboBox comboBox)
		{
			if (comboBox == null || IsDesignMode(comboBox)) return;

			comboBox.Font = BodyFont;
			comboBox.FlatStyle = FlatStyle.Flat;
			comboBox.BackColor = Surface;
			comboBox.Height = ControlHeight;
			comboBox.DropDownStyle = ComboBoxStyle.DropDownList;

			AddComboBoxEffects(comboBox);
		}

		/// <summary>
		/// Style checkboxes with modern appearance
		/// </summary>
		public static void StyleCheckBox(CheckBox checkBox)
		{
			if (checkBox == null || IsDesignMode(checkBox)) return;

			checkBox.Font = BodyFont;
			checkBox.ForeColor = DarkGray;
			checkBox.FlatStyle = FlatStyle.Flat;
			checkBox.UseVisualStyleBackColor = true;

			AddCheckBoxEffects(checkBox);
		}

		/// <summary>
		/// Style labels with consistent typography
		/// </summary>
		public static void StyleLabel(Label label, LabelStyle style = LabelStyle.Body)
		{
			if (label == null || IsDesignMode(label)) return;

			switch (style)
			{
				case LabelStyle.Header:
					label.Font = HeaderFont;
					label.ForeColor = DarkGray;
					break;
				case LabelStyle.Subheader:
					label.Font = SubheaderFont;
					label.ForeColor = DarkGray;
					break;
				case LabelStyle.Caption:
					label.Font = CaptionFont;
					label.ForeColor = SecondaryText;
					break;
				default:
					label.Font = BodyFont;
					label.ForeColor = DarkGray;
					break;
			}

			AddLabelEffects(label);
		}

		/// <summary>
		/// Apply comprehensive theming to entire form
		/// </summary>
		public static void ApplyModernTheme(Form form)
		{
			if (form == null || IsDesignMode(form)) return;

			form.BackColor = Background;
			form.Font = BodyFont;
			form.ForeColor = DarkGray;

			ApplyThemeRecursive(form);
		}

		#endregion

		#region Enhanced Visual Effects

		private static void ApplyButtonStyle(Button button, ButtonStyle style)
		{
			switch (style)
			{
				case ButtonStyle.Primary:
					button.BackColor = PrimaryBlue;
					button.ForeColor = Color.White;
					break;
				case ButtonStyle.Success:
					button.BackColor = SuccessGreen;
					button.ForeColor = Color.White;
					break;
				case ButtonStyle.Danger:
					button.BackColor = ErrorRed;
					button.ForeColor = Color.White;
					break;
				case ButtonStyle.Warning:
					button.BackColor = WarningOrange;
					button.ForeColor = Color.White;
					break;
				case ButtonStyle.Secondary:
					button.BackColor = Surface;
					button.ForeColor = DarkGray;
					button.FlatAppearance.BorderSize = 1;
					button.FlatAppearance.BorderColor = Border;
					break;
				case ButtonStyle.Ghost:
					button.BackColor = Color.Transparent;
					button.ForeColor = PrimaryBlue;
					button.FlatAppearance.BorderSize = 1;
					button.FlatAppearance.BorderColor = PrimaryBlue;
					break;
			}
		}

		private static void AddModernButtonEffects(Button button, ButtonStyle style)
		{
			var normalColor = button.BackColor;
			var hoverColor = GetHoverColor(normalColor, style);
			var pressedColor = GetPressedColor(normalColor, style);

			button.MouseEnter += (s, e) =>
			{
				if (button.Enabled)
				{
					button.BackColor = hoverColor;
					button.Cursor = Cursors.Hand;
				}
			};

			button.MouseLeave += (s, e) =>
			{
				if (button.Enabled)
					button.BackColor = normalColor;
			};

			button.MouseDown += (s, e) =>
			{
				if (button.Enabled)
					button.BackColor = pressedColor;
			};

			button.MouseUp += (s, e) =>
			{
				if (button.Enabled)
					button.BackColor = hoverColor;
			};

			button.EnabledChanged += (s, e) =>
			{
				button.BackColor = button.Enabled ? normalColor : DisabledGray;
				button.ForeColor = button.Enabled ?
					(style == ButtonStyle.Secondary || style == ButtonStyle.Ghost ? DarkGray : Color.White) :
					Color.Gray;
			};
		}

		private static void AddTextBoxFocusEffects(TextBox textBox)
		{
			textBox.Enter += (s, e) =>
			{
				textBox.BackColor = HoverLight;
				textBox.BorderStyle = BorderStyle.FixedSingle;
			};

			textBox.Leave += (s, e) =>
			{
				textBox.BackColor = Surface;
			};

			textBox.EnabledChanged += (s, e) =>
			{
				textBox.BackColor = textBox.Enabled ? Surface : LightGray;
				textBox.ForeColor = textBox.Enabled ? DarkGray : DisabledGray;
			};
		}

		private static void AddComboBoxEffects(ComboBox comboBox)
		{
			comboBox.EnabledChanged += (s, e) =>
			{
				comboBox.BackColor = comboBox.Enabled ? Surface : LightGray;
				comboBox.ForeColor = comboBox.Enabled ? DarkGray : DisabledGray;
			};

			comboBox.Enter += (s, e) =>
			{
				if (comboBox.Enabled)
					comboBox.BackColor = HoverLight;
			};

			comboBox.Leave += (s, e) =>
			{
				if (comboBox.Enabled)
					comboBox.BackColor = Surface;
			};
		}

		private static void AddCheckBoxEffects(CheckBox checkBox)
		{
			checkBox.EnabledChanged += (s, e) =>
			{
				checkBox.ForeColor = checkBox.Enabled ? DarkGray : DisabledGray;
			};

			checkBox.MouseEnter += (s, e) =>
			{
				if (checkBox.Enabled)
					checkBox.BackColor = HoverLight;
			};

			checkBox.MouseLeave += (s, e) =>
			{
				checkBox.BackColor = Color.Transparent;
			};
		}

		private static void AddLabelEffects(Label label)
		{
			label.EnabledChanged += (s, e) =>
			{
				label.ForeColor = label.Enabled ?
					(label.Font.Style.HasFlag(FontStyle.Bold) ? DarkGray : SecondaryText) :
					DisabledGray;
			};
		}

		private static Color GetHoverColor(Color baseColor, ButtonStyle style)
		{
			if (style == ButtonStyle.Secondary || style == ButtonStyle.Ghost)
				return LightGray;

			return ControlPaint.Light(baseColor, 0.1f);
		}

		private static Color GetPressedColor(Color baseColor, ButtonStyle style)
		{
			if (style == ButtonStyle.Secondary || style == ButtonStyle.Ghost)
				return Border;

			return ControlPaint.Dark(baseColor, 0.1f);
		}

		#endregion

		#region Drawing Methods

		private static void AddRoundedCorners(Control control, int radius)
		{
			if (control == null || IsDesignMode(control)) return;

			control.Paint += (s, e) =>
			{
				using (var path = GetRoundedRectPath(control.ClientRectangle, radius))
				{
					control.Region = new Region(path);
				}
			};
		}

		private static GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
		{
			var path = new GraphicsPath();
			var diameter = radius * 2;

			if (diameter > 0)
			{
				path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
				path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
				path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
				path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
			}
			else
			{
				path.AddRectangle(rect);
			}

			path.CloseFigure();
			return path;
		}

		private static void DrawModernGroupBox(GroupBox groupBox, Graphics g)
		{
			if (g == null) return;

			using (var textBrush = new SolidBrush(groupBox.ForeColor))
			using (var borderPen = new Pen(Border))
			{
				var textSize = g.MeasureString(groupBox.Text, groupBox.Font);
				var borderRect = new Rectangle(
					groupBox.ClientRectangle.X,
					groupBox.ClientRectangle.Y + (int)(textSize.Height / 2),
					groupBox.ClientRectangle.Width - 1,
					groupBox.ClientRectangle.Height - (int)(textSize.Height / 2) - 1);

				// Clear background
				g.Clear(groupBox.BackColor);

				// Draw text
				g.DrawString(groupBox.Text, groupBox.Font, textBrush, SpacingLarge, 0);

				// Draw modern border
				using (var path = GetRoundedRectPath(borderRect, BorderRadius))
				{
					g.DrawPath(borderPen, path);
				}

				// Create gap for text
				using (var backgroundBrush = new SolidBrush(groupBox.BackColor))
				{
					var textRect = new RectangleF(
						SpacingLarge - SpacingSmall,
						0,
						textSize.Width + SpacingSmall * 2,
						textSize.Height);
					g.FillRectangle(backgroundBrush, textRect);
				}
			}
		}

		#endregion



		#region Additional Control Styling

		public static void StyleTabControl(TabControl tabControl)
		{
			if (tabControl == null || IsDesignMode(tabControl)) return;

			tabControl.Font = SubheaderFont;
			tabControl.Appearance = TabAppearance.FlatButtons;
			tabControl.SizeMode = TabSizeMode.Fixed;
			tabControl.ItemSize = new Size(120, 40);

			foreach (TabPage tab in tabControl.TabPages)
			{
				tab.BackColor = Background;
				tab.ForeColor = DarkGray;
				tab.Font = BodyFont;
			}
		}

		public static void StyleDataGridView(DataGridView dgv)
		{
			if (dgv == null || IsDesignMode(dgv)) return;

			dgv.Font = BodyFont;
			dgv.BackgroundColor = Surface;
			dgv.BorderStyle = BorderStyle.None;
			dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
			dgv.DefaultCellStyle.BackColor = Surface;
			dgv.DefaultCellStyle.ForeColor = DarkGray;
			dgv.DefaultCellStyle.SelectionBackColor = PrimaryLight;
			dgv.DefaultCellStyle.SelectionForeColor = Color.White;
			dgv.ColumnHeadersDefaultCellStyle.BackColor = LightGray;
			dgv.ColumnHeadersDefaultCellStyle.ForeColor = DarkGray;
			dgv.ColumnHeadersDefaultCellStyle.Font = SubheaderFont;
			dgv.EnableHeadersVisualStyles = false;
			dgv.GridColor = Border;
			dgv.RowHeadersVisible = false;
			dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgv.AllowUserToAddRows = false;
			dgv.AllowUserToDeleteRows = false;
			dgv.ReadOnly = true;
		}

		#endregion

		#region Factory Methods

		/// <summary>
		/// Create a modern card panel with shadow effect
		/// </summary>
		public static Panel CreateCard(string title = null, int padding = SpacingLarge)
		{
			var card = new Panel
			{
				BackColor = Surface,
				Padding = new Padding(padding),
				BorderStyle = BorderStyle.None
			};

			if (!string.IsNullOrEmpty(title))
			{
				var titleLabel = new Label
				{
					Text = title,
					Font = SubheaderFont,
					ForeColor = DarkGray,
					AutoSize = true,
					Location = new Point(padding, padding),
					Tag = "styled"
				};
				card.Controls.Add(titleLabel);
			}

			// Add shadow effect
			if (!IsDesignMode(card))
			{
				card.Paint += (s, e) => DrawCardShadow(e.Graphics, card.ClientRectangle);
				AddRoundedCorners(card, BorderRadius);
			}

			return card;
		}

		/// <summary>
		/// Create a status label with appropriate styling
		/// </summary>
		public static Label CreateStatusLabel(string text, StatusType status)
		{
			var label = new Label
			{
				Text = $"{GetStatusIcon(status)} {text}",
				Font = BodyFont,
				AutoSize = true,
				Padding = new Padding(Spacing),
				BackColor = GetStatusBackColor(status),
				ForeColor = GetStatusForeColor(status),
				Tag = "styled"
			};

			if (!IsDesignMode(label))
				AddRoundedCorners(label, BorderRadius);

			return label;
		}

		/// <summary>
		/// Create a modern separator line
		/// </summary>
		public static Panel CreateSeparator(int height = 1)
		{
			return new Panel
			{
				Height = height,
				BackColor = Border,
				Dock = DockStyle.Top,
				Tag = "styled"
			};
		}

		private static void DrawCardShadow(Graphics g, Rectangle rect)
		{
			if (g == null) return;

			var shadowRect = new Rectangle(
				rect.X + ShadowOffset,
				rect.Y + ShadowOffset,
				rect.Width - ShadowOffset,
				rect.Height - ShadowOffset);

			using (var path = GetRoundedRectPath(shadowRect, BorderRadius))
			using (var brush = new PathGradientBrush(path))
			{
				brush.CenterColor = CardShadow;
				brush.SurroundColors = new[] { Color.Transparent };
				g.FillPath(brush, path);
			}
		}

		#endregion

		#region Status Helpers

		private static string GetStatusIcon(StatusType status) => status switch
		{
			StatusType.Success => "✅",
			StatusType.Warning => "⚠️",
			StatusType.Error => "❌",
			StatusType.Info => "ℹ️",
			StatusType.Processing => "⏳",
			_ => "•"
		};

		private static Color GetStatusBackColor(StatusType status) => status switch
		{
			StatusType.Success => Color.FromArgb(232, 245, 233),
			StatusType.Warning => Color.FromArgb(255, 243, 224),
			StatusType.Error => Color.FromArgb(255, 235, 238),
			StatusType.Info => Color.FromArgb(227, 242, 253),
			StatusType.Processing => Color.FromArgb(239, 235, 233),
			_ => LightGray
		};

		private static Color GetStatusForeColor(StatusType status) => status switch
		{
			StatusType.Success => Color.FromArgb(27, 94, 32),
			StatusType.Warning => Color.FromArgb(245, 124, 0),
			StatusType.Error => Color.FromArgb(198, 40, 40),
			StatusType.Info => Color.FromArgb(2, 136, 209),
			StatusType.Processing => Color.FromArgb(121, 85, 72),
			_ => DarkGray
		};
		// ADD these methods to your existing UIHelper.cs file

		#region DACPAC Publisher Specific Additions

		/// <summary>
		/// Style ListView controls for modern appearance
		/// </summary>
		public static void StyleListView(ListView listView)
		{
			if (listView == null || IsDesignMode(listView)) return;

			listView.Font = BodyFont;
			listView.BackColor = Surface;
			listView.ForeColor = DarkGray;
			listView.BorderStyle = BorderStyle.None;
			listView.View = View.List;
			listView.FullRowSelect = true;
			listView.GridLines = false;
			listView.HeaderStyle = ColumnHeaderStyle.None;
			listView.Tag = "styled";
		}

		/// <summary>
		/// Style CheckedListBox controls for modern appearance
		/// </summary>
		public static void StyleCheckedListBox(CheckedListBox checkedListBox)
		{
			if (checkedListBox == null || IsDesignMode(checkedListBox)) return;

			checkedListBox.Font = BodyFont;
			checkedListBox.BackColor = Surface;
			checkedListBox.ForeColor = DarkGray;
			checkedListBox.BorderStyle = BorderStyle.FixedSingle;
			checkedListBox.CheckOnClick = true;
			checkedListBox.IntegralHeight = false;
			checkedListBox.Tag = "styled";

			// Add hover effects for better UX
			checkedListBox.MouseMove += (s, e) =>
			{
				var index = checkedListBox.IndexFromPoint(e.Location);
				if (index >= 0 && index < checkedListBox.Items.Count)
				{
					checkedListBox.Cursor = Cursors.Hand;
				}
				else
				{
					checkedListBox.Cursor = Cursors.Default;
				}
			};
		}

		/// <summary>
		/// Style ProgressBar for modern appearance
		/// </summary>
		public static void StyleProgressBar(ProgressBar progressBar)
		{
			if (progressBar == null || IsDesignMode(progressBar)) return;

			progressBar.BackColor = LightGray;
			progressBar.ForeColor = PrimaryBlue;
			progressBar.Tag = "styled";
		}

		/// <summary>
		/// Style RichTextBox for modern appearance
		/// </summary>
		public static void StyleRichTextBox(RichTextBox richTextBox)
		{
			if (richTextBox == null || IsDesignMode(richTextBox)) return;

			richTextBox.Font = BodyFont;
			richTextBox.BackColor = Surface;
			richTextBox.ForeColor = DarkGray;
			richTextBox.BorderStyle = BorderStyle.None;
			richTextBox.Tag = "styled";
		}

		/// <summary>
		/// Create a configuration panel with proper styling
		/// </summary>
		public static Panel CreateConfigurationPanel()
		{
			var panel = new Panel
			{
				BackColor = Color.FromArgb(248, 249, 250),
				BorderStyle = BorderStyle.FixedSingle,
				Padding = new Padding(SpacingLarge),
				Tag = "styled"
			};

			if (!IsDesignMode(panel))
			{
				AddRoundedCorners(panel, BorderRadius);
			}

			return panel;
		}

		/// <summary>
		/// Create a validation results panel
		/// </summary>
		public static Panel CreateValidationPanel(bool isValid = true)
		{
			var panel = new Panel
			{
				BackColor = isValid ? ActiveGreen : Color.FromArgb(248, 215, 218),
				BorderStyle = BorderStyle.FixedSingle,
				Padding = new Padding(SpacingMedium),
				Tag = "styled"
			};

			if (!IsDesignMode(panel))
			{
				AddRoundedCorners(panel, BorderRadius);
			}

			return panel;
		}

		/// <summary>
		/// Apply smart button styling based on button text content
		/// </summary>
		public static void ApplySmartButtonStyling(Button button)
		{
			if (button == null || IsDesignMode(button)) return;

			var text = button.Text?.ToLower() ?? "";

			if (text.Contains("deploy") || text.Contains("🚀"))
			{
				StyleLargeButton(button, ButtonStyle.Danger);
			}
			else if (text.Contains("test") || text.Contains("browse") || text.Contains("🔌") || text.Contains("📁"))
			{
				StyleButton(button, ButtonStyle.Primary);
			}
			else if (text.Contains("refresh") || text.Contains("configure") || text.Contains("auto detect") ||
					 text.Contains("🔄") || text.Contains("⚙️"))
			{
				StyleButton(button, ButtonStyle.Success);
			}
			else if (text.Contains("save") || text.Contains("💾"))
			{
				StyleButton(button, ButtonStyle.Primary);
			}
			else if (text.Contains("load") || text.Contains("📂"))
			{
				StyleButton(button, ButtonStyle.Secondary);
			}
			else
			{
				StyleButton(button, ButtonStyle.Primary);
			}
		}

		/// <summary>
		/// Show notification toast (simplified version for WinForms)
		/// </summary>
		public static void ShowNotificationToast(Form parentForm, string message, StatusType type = StatusType.Info, int duration = 3000)
		{
			if (parentForm == null || IsDesignMode(parentForm)) return;

			try
			{
				var toast = new Form();
				toast.FormBorderStyle = FormBorderStyle.None;
				toast.StartPosition = FormStartPosition.Manual;
				toast.TopMost = true;
				toast.ShowInTaskbar = false;
				toast.BackColor = GetNotificationColor(type);
				toast.Size = new Size(300, 60);
				toast.Location = new Point(parentForm.Right - 320, parentForm.Top + 20);

				var label = new Label();
				label.Text = message;
				label.ForeColor = Color.White;
				label.Font = ButtonFont;
				label.Dock = DockStyle.Fill;
				label.TextAlign = ContentAlignment.MiddleCenter;
				toast.Controls.Add(label);

				toast.Show();

				// Auto-hide after duration
				var timer = new Timer();
				timer.Interval = duration;
				timer.Tick += (s, e) => {
					timer.Stop();
					timer.Dispose();
					toast.Close();
					toast.Dispose();
				};
				timer.Start();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error showing notification toast: {ex.Message}");
			}
		}

		/// <summary>
		/// Animate panel visibility with smooth transition
		/// </summary>
		public static void AnimatePanelVisibility(Panel panel, bool show, int duration = 300)
		{
			if (panel == null || IsDesignMode(panel)) return;

			try
			{
				if (show && !panel.Visible)
				{
					panel.Visible = true;
					panel.Height = 0;

					var targetHeight = panel.Tag as int? ?? 200; // Store original height in Tag
					var timer = new Timer();
					timer.Interval = 20;
					var steps = duration / timer.Interval;
					var currentStep = 0;
					var stepHeight = (double)targetHeight / steps;

					timer.Tick += (s, e) => {
						currentStep++;
						if (currentStep >= steps)
						{
							panel.Height = targetHeight;
							timer.Stop();
							timer.Dispose();
						}
						else
						{
							panel.Height = (int)(stepHeight * currentStep);
						}
					};

					timer.Start();
				}
				else if (!show && panel.Visible)
				{
					// Store current height before hiding
					if (panel.Tag == null)
						panel.Tag = panel.Height;

					var originalHeight = panel.Height;
					var timer = new Timer();
					timer.Interval = 20;
					var steps = duration / timer.Interval;
					var currentStep = 0;
					var stepHeight = (double)originalHeight / steps;

					timer.Tick += (s, e) => {
						currentStep++;
						if (currentStep >= steps)
						{
							panel.Visible = false;
							panel.Height = originalHeight; // Restore height for next show
							timer.Stop();
							timer.Dispose();
						}
						else
						{
							panel.Height = (int)(originalHeight - (stepHeight * currentStep));
						}
					};

					timer.Start();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error animating panel: {ex.Message}");
				// Fallback to immediate show/hide
				panel.Visible = show;
			}
		}

		/// <summary>
		/// Create a modern header panel with logo and title
		/// </summary>
		public static Panel CreateHeaderPanel(string title, string subtitle = null)
		{
			var header = new Panel
			{
				Dock = DockStyle.Top,
				Height = 60,
				BackColor = PrimaryBlue,
				Padding = new Padding(SpacingLarge),
				Tag = "styled"
			};

			// Logo placeholder
			var logo = new Panel
			{
				Size = new Size(40, 40),
				Location = new Point(SpacingLarge, 10),
				BackColor = Surface,
				BorderStyle = BorderStyle.FixedSingle
			};

			// Title
			var titleLabel = new Label
			{
				Text = title,
				ForeColor = Color.White,
				Font = HeaderFont,
				AutoSize = true,
				Location = new Point(70, 10),
				Tag = "styled"
			};

			// Subtitle
			if (!string.IsNullOrEmpty(subtitle))
			{
				var subtitleLabel = new Label
				{
					Text = subtitle,
					ForeColor = Color.White,
					Font = BodyFont,
					AutoSize = true,
					Location = new Point(70, 35),
					Tag = "styled"
				};
				header.Controls.Add(subtitleLabel);
			}

			header.Controls.AddRange(new Control[] { logo, titleLabel });
			return header;
		}

		// Helper method for notification colors
		private static Color GetNotificationColor(StatusType type)
		{
			return type switch
			{
				StatusType.Success => SuccessGreen,
				StatusType.Warning => WarningOrange,
				StatusType.Error => ErrorRed,
				StatusType.Processing => InfoBlue,
				_ => PrimaryBlue
			};
		}

		#endregion

		#region Enhanced Recursive Theme Application Update

		// REPLACE your existing ApplyThemeRecursive method with this enhanced version:
		private static void ApplyThemeRecursive(Control parent)
		{
			if (parent?.Controls == null) return;

			foreach (Control control in parent.Controls)
			{
				// Skip controls that are already styled
				if (control.Tag?.ToString() == "styled") continue;

				switch (control)
				{
					case Button btn:
						ApplySmartButtonStyling(btn);  // Use smart styling instead
						btn.Tag = "styled";
						break;
					case GroupBox grp:
						StyleGroupBox(grp);
						grp.Tag = "styled";
						break;
					case TextBox txt:
						StyleTextBox(txt);
						txt.Tag = "styled";
						break;
					case ComboBox cmb:
						StyleComboBox(cmb);
						cmb.Tag = "styled";
						break;
					case CheckBox chk:
						StyleCheckBox(chk);
						chk.Tag = "styled";
						break;
					case Label lbl when lbl.Tag?.ToString() != "styled":
						StyleLabel(lbl);
						lbl.Tag = "styled";
						break;
					case TabControl tab:
						StyleTabControl(tab);
						tab.Tag = "styled";
						break;
					case DataGridView dgv:
						StyleDataGridView(dgv);
						dgv.Tag = "styled";
						break;
					case ListView lv:  // ADD THIS
						StyleListView(lv);
						lv.Tag = "styled";
						break;
					case CheckedListBox clb:  // ADD THIS
						StyleCheckedListBox(clb);
						clb.Tag = "styled";
						break;
					case ProgressBar pb:  // ADD THIS
						StyleProgressBar(pb);
						pb.Tag = "styled";
						break;
					case RichTextBox rtb:  // ADD THIS
						StyleRichTextBox(rtb);
						rtb.Tag = "styled";
						break;
				}

				// Recursively apply to child controls
				if (control.HasChildren)
					ApplyThemeRecursive(control);
			}
		}

		#endregion
		#endregion

		#region Enums

		public enum StatusType
		{
			Success,
			Warning,
			Error,
			Info,
			Processing
		}

		public enum ButtonStyle
		{
			Primary,
			Secondary,
			Success,
			Danger,
			Warning,
			Ghost
		}

		public enum LabelStyle
		{
			Header,
			Subheader,
			Body,
			Caption
		}

		#endregion
	}
}