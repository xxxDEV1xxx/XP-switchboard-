using System;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;

namespace ButtonCommandBoard
{
    public class CommandBoard : Form
    {
        private Button[] buttons = new Button[16];
        private TextBox[] commandTextBoxes = new TextBox[16];
        private TextBox[] descriptionTextBoxes = new TextBox[16];
        private Label[] textBoxLabels = new Label[16];
        private Button plusButton;
        private Button minusButton;
        private Label pageNumberLabel;
        private TextBox pageDescriptionTextBox;
        private readonly string commandsFile = "commands.txt";
        private readonly string descriptionsFile = "descriptions.txt";
        private readonly string colorsFile = "colors.txt";
        private readonly string resetFlagFile = "reset.flag";
        private IntPtr keyboardHookId = IntPtr.Zero;
        private LowLevelKeyboardProc keyboardProc;
        private int currentPage = 1;
        private const int MaxPages = 5;

        public Color Button1BackColor = Color.FromArgb(195, 25, 21);
        public Color Button1ForeColor = Color.White;
        public Color Button2BackColor = Color.FromArgb(11, 16, 150);
        public Color Button2ForeColor = Color.White;
        public Color Button3BackColor = Color.FromArgb(11, 16, 150);
        public Color Button3ForeColor = Color.White;
        public Color Button4BackColor = Color.FromArgb(11, 16, 150);
        public Color Button4ForeColor = Color.White;
        public Color Button5BackColor = Color.FromArgb(11, 16, 150);
        public Color Button5ForeColor = Color.White;
        public Color Button6BackColor = Color.FromArgb(11, 16, 150);
        public Color Button6ForeColor = Color.White;
        public Color Button7BackColor = Color.FromArgb(11, 16, 150);
        public Color Button7ForeColor = Color.White;
        public Color Button8BackColor = Color.FromArgb(11, 16, 150);
        public Color Button8ForeColor = Color.White;
        public Color Button9BackColor = Color.FromArgb(11, 16, 150);
        public Color Button9ForeColor = Color.White;
        public Color Button10BackColor = Color.FromArgb(11, 16, 150);
        public Color Button10ForeColor = Color.White;
        public Color Button11BackColor = Color.FromArgb(11, 16, 150);
        public Color Button11ForeColor = Color.White;
        public Color Button12BackColor = Color.FromArgb(11, 16, 150);
        public Color Button12ForeColor = Color.White;
        public Color Button13BackColor = Color.FromArgb(11, 16, 150);
        public Color Button13ForeColor = Color.White;
        public Color Button14BackColor = Color.FromArgb(11, 16, 150);
        public Color Button14ForeColor = Color.White;
        public Color Button15BackColor = Color.FromArgb(11, 16, 150);
        public Color Button15ForeColor = Color.White;
        public Color Button16BackColor = Color.FromArgb(11, 16, 150);
        public Color Button16ForeColor = Color.White;
        public Color CommandTextBoxBackColor = Color.Black;
        public Color CommandTextBoxForeColor = Color.FromArgb(22, 181, 4);
        public Color DescriptionTextBoxBackColor = Color.Black;
        public Color DescriptionTextBoxForeColor = Color.FromArgb(22, 181, 4);
        public Color TextBoxLabelBackColor = Color.Black;
        public Color TextBoxLabelForeColor = Color.FromArgb(22, 181, 4);
        public Color PlusMinusEditButtonBackColor = Color.Black;
        public Color PlusMinusEditButtonForeColor = Color.FromArgb(22, 181, 4);
        public Color PageNumberLabelBackColor = Color.Black;
        public Color PageNumberLabelForeColor = Color.FromArgb(22, 181, 4);
        public Color PageDescriptionTextBoxBackColor = Color.Black;
        public Color PageDescriptionTextBoxForeColor = Color.FromArgb(22, 181, 4);
        public Color FormBackColor = Color.Black;
        public Color ControlBorderColor = Color.White;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        public CommandBoard()
        {
            this.Text = "Command Board";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(100, 100);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = FormBackColor;
            this.TopMost = true;

            LoadColors();
            InitializeControls();
            LoadCommands();
            LoadDescriptions();
            this.Resize += new EventHandler(Form_Resize);
            this.Closing += new CancelEventHandler(Form_Closing);
            this.Resize += new EventHandler(Form_WindowStateChanged);

            keyboardProc = new LowLevelKeyboardProc(HookCallback);
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardProc, GetModuleHandle(null), 0);
            }

            LayoutControls();
        }

        private void LoadColors()
        {
            try
            {
                if (File.Exists(colorsFile))
                {
                    string[] colorLines = File.ReadAllLines(colorsFile);
                    foreach (string line in colorLines)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length != 2) continue;
                        string[] rgb = parts[1].Split(',');
                        if (rgb.Length != 3) continue;
                        int r, g, b;
                        if (!int.TryParse(rgb[0], out r) || !int.TryParse(rgb[1], out g) || !int.TryParse(rgb[2], out b)) continue;
                        Color color = Color.FromArgb(r, g, b);
                        switch (parts[0].Trim())
                        {
                            case "Button1BackColor": Button1BackColor = color; break;
                            case "Button1ForeColor": Button1ForeColor = color; break;
                            case "Button2BackColor": Button2BackColor = color; break;
                            case "Button2ForeColor": Button2ForeColor = color; break;
                            case "Button3BackColor": Button3BackColor = color; break;
                            case "Button3ForeColor": Button3ForeColor = color; break;
                            case "Button4BackColor": Button4BackColor = color; break;
                            case "Button4ForeColor": Button4ForeColor = color; break;
                            case "Button5BackColor": Button5BackColor = color; break;
                            case "Button5ForeColor": Button5ForeColor = color; break;
                            case "Button6BackColor": Button6BackColor = color; break;
                            case "Button6ForeColor": Button6ForeColor = color; break;
                            case "Button7BackColor": Button7BackColor = color; break;
                            case "Button7ForeColor": Button7ForeColor = color; break;
                            case "Button8BackColor": Button8BackColor = color; break;
                            case "Button8ForeColor": Button8BackColor = color; break;
                            case "Button9BackColor": Button9BackColor = color; break;
                            case "Button9ForeColor": Button9ForeColor = color; break;
                            case "Button10BackColor": Button10BackColor = color; break;
                            case "Button10ForeColor": Button10ForeColor = color; break;
                            case "Button11BackColor": Button11BackColor = color; break;
                            case "Button11ForeColor": Button11ForeColor = color; break;
                            case "Button12BackColor": Button12BackColor = color; break;
                            case "Button12ForeColor": Button12ForeColor = color; break;
                            case "Button13BackColor": Button13BackColor = color; break;
                            case "Button13ForeColor": Button13ForeColor = color; break;
                            case "Button14BackColor": Button14BackColor = color; break;
                            case "Button14ForeColor": Button14ForeColor = color; break;
                            case "Button15BackColor": Button15BackColor = color; break;
                            case "Button15ForeColor": Button15ForeColor = color; break;
                            case "Button16BackColor": Button16BackColor = color; break;
                            case "Button16ForeColor": Button16ForeColor = color; break;
                            case "CommandTextBoxBackColor": CommandTextBoxBackColor = color; break;
                            case "CommandTextBoxForeColor": CommandTextBoxForeColor = color; break;
                            case "DescriptionTextBoxBackColor": DescriptionTextBoxBackColor = color; break;
                            case "DescriptionTextBoxForeColor": DescriptionTextBoxForeColor = color; break;
                            case "TextBoxLabelBackColor": TextBoxLabelBackColor = color; break;
                            case "TextBoxLabelForeColor": TextBoxLabelForeColor = color; break;
                            case "PlusMinusEditButtonBackColor": PlusMinusEditButtonBackColor = color; break;
                            case "PlusMinusEditButtonForeColor": PlusMinusEditButtonForeColor = color; break;
                            case "PageNumberLabelBackColor": PageNumberLabelBackColor = color; break;
                            case "PageNumberLabelForeColor": PageNumberLabelForeColor = color; break;
                            case "PageDescriptionTextBoxBackColor": PageDescriptionTextBoxBackColor = color; break;
                            case "PageDescriptionTextBoxForeColor": PageDescriptionTextBoxForeColor = color; break;
                            case "FormBackColor": FormBackColor = color; break;
                            case "ControlBorderColor": ControlBorderColor = color; break;
                        }
                    }
                    UpdateControlColors();
                }
            }
            catch
            {
                // Silent fail
            }
        }

        private void SaveColors()
        {
            try
            {
                string[] colorLines = new string[]
                {
                    "Button1BackColor=" + Button1BackColor.R + "," + Button1BackColor.G + "," + Button1BackColor.B,
                    "Button1ForeColor=" + Button1ForeColor.R + "," + Button1ForeColor.G + "," + Button1ForeColor.B,
                    "Button2BackColor=" + Button2BackColor.R + "," + Button2BackColor.G + "," + Button2BackColor.B,
                    "Button2ForeColor=" + Button2ForeColor.R + "," + Button2ForeColor.G + "," + Button2ForeColor.B,
                    "Button3BackColor=" + Button3BackColor.R + "," + Button3BackColor.G + "," + Button3BackColor.B,
                    "Button3ForeColor=" + Button3ForeColor.R + "," + Button3ForeColor.G + "," + Button3ForeColor.B,
                    "Button4BackColor=" + Button4BackColor.R + "," + Button4BackColor.G + "," + Button4BackColor.B,
                    "Button4ForeColor=" + Button4ForeColor.R + "," + Button4ForeColor.G + "," + Button4ForeColor.B,
                    "Button5BackColor=" + Button5BackColor.R + "," + Button5BackColor.G + "," + Button5BackColor.B,
                    "Button5ForeColor=" + Button5ForeColor.R + "," + Button5ForeColor.G + "," + Button5ForeColor.B,
                    "Button6BackColor=" + Button6BackColor.R + "," + Button6BackColor.G + "," + Button6BackColor.B,
                    "Button6ForeColor=" + Button6ForeColor.R + "," + Button6ForeColor.G + "," + Button6ForeColor.B,
                    "Button7BackColor=" + Button7BackColor.R + "," + Button7BackColor.G + "," + Button7BackColor.B,
                    "Button7ForeColor=" + Button7ForeColor.R + "," + Button7ForeColor.G + "," + Button7ForeColor.B,
                    "Button8BackColor=" + Button8BackColor.R + "," + Button8BackColor.G + "," + Button8BackColor.B,
                    "Button8ForeColor=" + Button8ForeColor.R + "," + Button8ForeColor.G + "," + Button8ForeColor.B,
                    "Button9BackColor=" + Button9BackColor.R + "," + Button9BackColor.G + "," + Button9BackColor.B,
                    "Button9ForeColor=" + Button9ForeColor.R + "," + Button9ForeColor.G + "," + Button9ForeColor.B,
                    "Button10BackColor=" + Button10BackColor.R + "," + Button10BackColor.G + "," + Button10BackColor.B,
                    "Button10ForeColor=" + Button10ForeColor.R + "," + Button10ForeColor.G + "," + Button10ForeColor.B,
                    "Button11BackColor=" + Button11BackColor.R + "," + Button11BackColor.G + "," + Button11BackColor.B,
                    "Button11ForeColor=" + Button11ForeColor.R + "," + Button11ForeColor.G + "," + Button11ForeColor.B,
                    "Button12BackColor=" + Button12BackColor.R + "," + Button12BackColor.G + "," + Button12BackColor.B,
                    "Button12ForeColor=" + Button12ForeColor.R + "," + Button12ForeColor.G + "," + Button12ForeColor.B,
                    "Button13BackColor=" + Button13BackColor.R + "," + Button13BackColor.G + "," + Button13BackColor.B,
                    "Button13ForeColor=" + Button13ForeColor.R + "," + Button13ForeColor.G + "," + Button13ForeColor.B,
                    "Button14BackColor=" + Button14BackColor.R + "," + Button14BackColor.G + "," + Button14BackColor.B,
                    "Button14ForeColor=" + Button14ForeColor.R + "," + Button14ForeColor.G + "," + Button14ForeColor.B,
                    "Button15BackColor=" + Button15BackColor.R + "," + Button15BackColor.G + "," + Button15BackColor.B,
                    "Button15ForeColor=" + Button15ForeColor.R + "," + Button15ForeColor.G + "," + Button15ForeColor.B,
                    "Button16BackColor=" + Button16BackColor.R + "," + Button16BackColor.G + "," + Button16BackColor.B,
                    "Button16ForeColor=" + Button16ForeColor.R + "," + Button16ForeColor.G + "," + Button16ForeColor.B,
                    "CommandTextBoxBackColor=" + CommandTextBoxBackColor.R + "," + CommandTextBoxBackColor.G + "," + CommandTextBoxBackColor.B,
                    "CommandTextBoxForeColor=" + CommandTextBoxForeColor.R + "," + CommandTextBoxForeColor.G + "," + CommandTextBoxForeColor.B,
                    "DescriptionTextBoxBackColor=" + DescriptionTextBoxBackColor.R + "," + DescriptionTextBoxBackColor.G + "," + DescriptionTextBoxBackColor.B,
                    "DescriptionTextBoxForeColor=" + DescriptionTextBoxForeColor.R + "," + DescriptionTextBoxForeColor.G + "," + DescriptionTextBoxForeColor.B,
                    "TextBoxLabelBackColor=" + TextBoxLabelBackColor.R + "," + TextBoxLabelBackColor.G + "," + TextBoxLabelBackColor.B,
                    "TextBoxLabelForeColor=" + TextBoxLabelForeColor.R + "," + TextBoxLabelForeColor.G + "," + TextBoxLabelForeColor.B,
                    "PlusMinusEditButtonBackColor=" + PlusMinusEditButtonBackColor.R + "," + PlusMinusEditButtonBackColor.G + "," + PlusMinusEditButtonBackColor.B,
                    "PlusMinusEditButtonForeColor=" + PlusMinusEditButtonForeColor.R + "," + PlusMinusEditButtonForeColor.G + "," + PlusMinusEditButtonForeColor.B,
                    "PageNumberLabelBackColor=" + PageNumberLabelBackColor.R + "," + PageNumberLabelBackColor.G + "," + PageNumberLabelBackColor.B,
                    "PageNumberLabelForeColor=" + PageNumberLabelForeColor.R + "," + PageNumberLabelForeColor.G + "," + PageNumberLabelForeColor.B,
                    "PageDescriptionTextBoxBackColor=" + PageDescriptionTextBoxBackColor.R + "," + PageDescriptionTextBoxBackColor.G + "," + PageDescriptionTextBoxBackColor.B,
                    "PageDescriptionTextBoxForeColor=" + PageDescriptionTextBoxForeColor.R + "," + PageDescriptionTextBoxForeColor.G + "," + PageDescriptionTextBoxForeColor.B,
                    "FormBackColor=" + FormBackColor.R + "," + FormBackColor.G + "," + FormBackColor.B,
                    "ControlBorderColor=" + ControlBorderColor.R + "," + ControlBorderColor.G + "," + ControlBorderColor.B
                };
                File.WriteAllLines(colorsFile, colorLines);
            }
            catch
            {
                // Silent fail
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                if (key >= Keys.F1 && key <= Keys.F12)
                {
                    int index = key - Keys.F1;
                    if (index < 12)
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new MethodInvoker(HookCallbackInvoke));
                        }
                        else
                        {
                            buttons[index].PerformClick();
                        }
                        return new IntPtr(1);
                    }
                }
            }
            return CallNextHookEx(keyboardHookId, nCode, wParam, lParam);
        }

        private void HookCallbackInvoke()
        {
            int index = 0; // Simplified for F1; adjust for full F1–F12 support
            buttons[index].PerformClick();
        }

        private void InitializeControls()
        {
            string defaultCommand = "ipconfig /all";
            Color[] buttonBackColors = new Color[]
            {
                Button1BackColor, Button2BackColor, Button3BackColor, Button4BackColor,
                Button5BackColor, Button6BackColor, Button7BackColor, Button8BackColor,
                Button9BackColor, Button10BackColor, Button11BackColor, Button12BackColor,
                Button13BackColor, Button14BackColor, Button15BackColor, Button16BackColor
            };
            Color[] buttonForeColors = new Color[]
            {
                Button1ForeColor, Button2ForeColor, Button3ForeColor, Button4ForeColor,
                Button5ForeColor, Button6ForeColor, Button7ForeColor, Button8ForeColor,
                Button9ForeColor, Button10ForeColor, Button11ForeColor, Button12ForeColor,
                Button13ForeColor, Button14ForeColor, Button15ForeColor, Button16ForeColor
            };

            for (int i = 0; i < 16; i++)
            {
                int buttonNumber = (currentPage - 1) * 16 + i + 1;
                Button btn = new Button();
                btn.Text = Convert.ToString(buttonNumber);
                btn.Tag = i;
                btn.FlatStyle = FlatStyle.Standard;
                btn.BackColor = buttonBackColors[i];
                btn.ForeColor = buttonForeColors[i];
                btn.Font = new Font("Arial", 10, FontStyle.Bold);
                buttons[i] = btn;
                buttons[i].Click += new EventHandler(Button_Click);
                this.Controls.Add(buttons[i]);

                TextBox cmdTextBox = new TextBox();
                cmdTextBox.Text = defaultCommand;
                cmdTextBox.Font = new Font("Arial", 10);
                cmdTextBox.Width = 200;
                cmdTextBox.BackColor = CommandTextBoxBackColor;
                cmdTextBox.ForeColor = CommandTextBoxForeColor;
                cmdTextBox.BorderStyle = BorderStyle.FixedSingle;
                commandTextBoxes[i] = cmdTextBox;
                commandTextBoxes[i].Paint += new PaintEventHandler(CommandTextBox_Paint);
                this.Controls.Add(commandTextBoxes[i]);

                TextBox descTextBox = new TextBox();
                descTextBox.Text = Convert.ToString(buttonNumber);
                descTextBox.Font = new Font("Arial", 10);
                descTextBox.Width = 200;
                descTextBox.BackColor = DescriptionTextBoxBackColor;
                descTextBox.ForeColor = DescriptionTextBoxForeColor;
                descTextBox.BorderStyle = BorderStyle.FixedSingle;
                descriptionTextBoxes[i] = descTextBox;
                descriptionTextBoxes[i].TextChanged += new EventHandler(DescriptionTextBox_TextChanged);
                descriptionTextBoxes[i].Paint += new PaintEventHandler(DescriptionTextBox_Paint);
                this.Controls.Add(descriptionTextBoxes[i]);

                Label label = new Label();
                label.Text = Convert.ToString(buttonNumber);
                label.Font = new Font("Arial", 10, FontStyle.Bold);
                label.AutoSize = true;
                label.BackColor = TextBoxLabelBackColor;
                label.ForeColor = TextBoxLabelForeColor;
                textBoxLabels[i] = label;
                this.Controls.Add(textBoxLabels[i]);
            }

            minusButton = new Button();
            minusButton.Text = "-";
            minusButton.Size = new Size(30, 30);
            minusButton.FlatStyle = FlatStyle.Standard;
            minusButton.BackColor = PlusMinusEditButtonBackColor;
            minusButton.ForeColor = PlusMinusEditButtonForeColor;
            minusButton.Font = new Font("Arial", 12, FontStyle.Bold);
            minusButton.Click += new EventHandler(MinusButton_Click);
            this.Controls.Add(minusButton);

            plusButton = new Button();
            plusButton.Text = "+";
            plusButton.Size = new Size(30, 30);
            plusButton.FlatStyle = FlatStyle.Standard;
            plusButton.BackColor = PlusMinusEditButtonBackColor;
            plusButton.ForeColor = PlusMinusEditButtonForeColor;
            plusButton.Font = new Font("Arial", 12, FontStyle.Bold);
            plusButton.Click += new EventHandler(PlusButton_Click);
            this.Controls.Add(plusButton);

            pageNumberLabel = new Label();
            pageNumberLabel.Text = Convert.ToString(currentPage);
            pageNumberLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            pageNumberLabel.AutoSize = true;
            pageNumberLabel.BackColor = PageNumberLabelBackColor;
            pageNumberLabel.ForeColor = PageNumberLabelForeColor;
            this.Controls.Add(pageNumberLabel);

            pageDescriptionTextBox = new TextBox();
            pageDescriptionTextBox.Text = "Page " + Convert.ToString(currentPage) + " Description";
            pageDescriptionTextBox.Font = new Font("Arial", 10);
            pageDescriptionTextBox.Width = 200;
            pageDescriptionTextBox.BackColor = PageDescriptionTextBoxBackColor;
            pageDescriptionTextBox.ForeColor = PageDescriptionTextBoxForeColor;
            pageDescriptionTextBox.BorderStyle = BorderStyle.FixedSingle;
            pageDescriptionTextBox.Paint += new PaintEventHandler(PageDescriptionTextBox_Paint);
            this.Controls.Add(pageDescriptionTextBox);
        }

        private void DescriptionTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            for (int i = 0; i < 16; i++)
            {
                if (descriptionTextBoxes[i] == tb)
                {
                    UpdateButtonTextFromDescription(i);
                    break;
                }
            }
        }

        private void CommandTextBox_Paint(object sender, PaintEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            using (Pen pen = new Pen(ControlBorderColor, 2))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, tb.Width - 1, tb.Height - 1);
            }
        }

        private void DescriptionTextBox_Paint(object sender, PaintEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            using (Pen pen = new Pen(ControlBorderColor, 2))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, tb.Width - 1, tb.Height - 1);
            }
        }

        private void PageDescriptionTextBox_Paint(object sender, PaintEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            using (Pen pen = new Pen(ControlBorderColor, 2))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, tb.Width - 1, tb.Height - 1);
            }
        }

        private void UpdateControlColors()
        {
            this.BackColor = FormBackColor;
            for (int i = 0; i < 16; i++)
            {
                buttons[i].BackColor = new Color[] {
                    Button1BackColor, Button2BackColor, Button3BackColor, Button4BackColor,
                    Button5BackColor, Button6BackColor, Button7BackColor, Button8BackColor,
                    Button9BackColor, Button10BackColor, Button11BackColor, Button12BackColor,
                    Button13BackColor, Button14BackColor, Button15BackColor, Button16BackColor
                }[i];
                buttons[i].ForeColor = new Color[] {
                    Button1ForeColor, Button2ForeColor, Button3ForeColor, Button4ForeColor,
                    Button5ForeColor, Button6ForeColor, Button7ForeColor, Button8ForeColor,
                    Button9ForeColor, Button10ForeColor, Button11ForeColor, Button12ForeColor,
                    Button13ForeColor, Button14ForeColor, Button15ForeColor, Button16ForeColor
                }[i];
                commandTextBoxes[i].BackColor = CommandTextBoxBackColor;
                commandTextBoxes[i].ForeColor = CommandTextBoxForeColor;
                commandTextBoxes[i].Invalidate();
                descriptionTextBoxes[i].BackColor = DescriptionTextBoxBackColor;
                descriptionTextBoxes[i].ForeColor = DescriptionTextBoxForeColor;
                descriptionTextBoxes[i].Invalidate();
                textBoxLabels[i].BackColor = TextBoxLabelBackColor;
                textBoxLabels[i].ForeColor = TextBoxLabelForeColor;
            }
            minusButton.BackColor = PlusMinusEditButtonBackColor;
            minusButton.ForeColor = PlusMinusEditButtonForeColor;
            plusButton.BackColor = PlusMinusEditButtonBackColor;
            plusButton.ForeColor = PlusMinusEditButtonForeColor;
            pageNumberLabel.BackColor = PageNumberLabelBackColor;
            pageNumberLabel.ForeColor = PageNumberLabelForeColor;
            pageDescriptionTextBox.BackColor = PageDescriptionTextBoxBackColor;
            pageDescriptionTextBox.ForeColor = PageDescriptionTextBoxForeColor;
            pageDescriptionTextBox.Invalidate();
        }

        private void PlusButton_Click(object sender, EventArgs e)
        {
            if (currentPage < MaxPages)
            {
                SaveCurrentPageData();
                currentPage++;
                UpdatePage();
            }
        }

        private void MinusButton_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                SaveCurrentPageData();
                currentPage--;
                UpdatePage();
            }
        }

        private void UpdatePage()
        {
            pageNumberLabel.Text = Convert.ToString(currentPage);
            pageDescriptionTextBox.Text = "Page " + Convert.ToString(currentPage) + " Description";

            for (int i = 0; i < 16; i++)
            {
                int buttonNumber = (currentPage - 1) * 16 + i + 1;
                textBoxLabels[i].Text = Convert.ToString(buttonNumber);
                commandTextBoxes[i].Text = "ipconfig /all";
                descriptionTextBoxes[i].Text = Convert.ToString(buttonNumber);
                UpdateButtonTextFromDescription(i);
            }

            LoadCommands();
            LoadDescriptions();
            LayoutControls();
        }

        private void SaveCurrentPageData()
        {
            try
            {
                string[] allCommands = File.Exists(commandsFile) ? File.ReadAllLines(commandsFile) : new string[MaxPages * 16];
                string[] allDescriptions = File.Exists(descriptionsFile) ? File.ReadAllLines(descriptionsFile) : new string[MaxPages * 16 + MaxPages];

                if (allCommands.Length < MaxPages * 16)
                {
                    string[] temp = new string[MaxPages * 16];
                    Array.Copy(allCommands, temp, allCommands.Length);
                    allCommands = temp;
                }
                if (allDescriptions.Length < MaxPages * 16 + MaxPages)
                {
                    string[] temp = new string[MaxPages * 16 + MaxPages];
                    Array.Copy(allDescriptions, temp, allDescriptions.Length);
                    allDescriptions = temp;
                }

                for (int i = 0; i < 16; i++)
                {
                    int index = (currentPage - 1) * 16 + i;
                    allCommands[index] = commandTextBoxes[i].Text;
                    allDescriptions[index] = descriptionTextBoxes[i].Text;
                }
                allDescriptions[MaxPages * 16 + currentPage - 1] = pageDescriptionTextBox.Text;

                File.WriteAllLines(commandsFile, allCommands);
                File.WriteAllLines(descriptionsFile, allDescriptions);
            }
            catch
            {
                // Silent fail
            }
        }

        private void LoadCommands()
        {
            try
            {
                if (File.Exists(resetFlagFile) || !File.Exists(commandsFile))
                {
                    for (int i = 0; i < 16; i++)
                    {
                        commandTextBoxes[i].Text = "ipconfig /all";
                    }
                    if (File.Exists(resetFlagFile))
                    {
                        File.Delete(resetFlagFile);
                    }
                    return;
                }

                string[] commands = File.ReadAllLines(commandsFile);
                for (int i = 0; i < 16; i++)
                {
                    int index = (currentPage - 1) * 16 + i;
                    if (index < commands.Length)
                    {
                        commandTextBoxes[i].Text = commands[index];
                    }
                }
            }
            catch
            {
                // Silent fail
            }
        }

        private void LoadDescriptions()
        {
            try
            {
                if (File.Exists(descriptionsFile))
                {
                    string[] descriptions = File.ReadAllLines(descriptionsFile);
                    for (int i = 0; i < 16; i++)
                    {
                        int index = (currentPage - 1) * 16 + i;
                        if (index < descriptions.Length && !string.IsNullOrEmpty(descriptions[index]))
                        {
                            descriptionTextBoxes[i].Text = descriptions[index];
                        }
                        else
                        {
                            descriptionTextBoxes[i].Text = Convert.ToString((currentPage - 1) * 16 + i + 1);
                        }
                        UpdateButtonTextFromDescription(i);
                    }
                    int pageDescIndex = MaxPages * 16 + currentPage - 1;
                    if (pageDescIndex < descriptions.Length)
                    {
                        pageDescriptionTextBox.Text = descriptions[pageDescIndex];
                    }
                }
                else
                {
                    for (int i = 0; i < 16; i++)
                    {
                        descriptionTextBoxes[i].Text = Convert.ToString((currentPage - 1) * 16 + i + 1);
                        UpdateButtonTextFromDescription(i);
                    }
                }
            }
            catch
            {
                for (int i = 0; i < 16; i++)
                {
                    descriptionTextBoxes[i].Text = Convert.ToString((currentPage - 1) * 16 + i + 1);
                    UpdateButtonTextFromDescription(i);
                }
            }
        }

        private void UpdateButtonTextFromDescription(int index)
        {
            if (index >= 0 && index < 16)
            {
                string description = descriptionTextBoxes[index].Text;
                buttons[index].Text = string.IsNullOrEmpty(description)
                    ? Convert.ToString((currentPage - 1) * 16 + index + 1)
                    : description;
            }
        }

        private void Form_Closing(object sender, CancelEventArgs e)
        {
            SaveCurrentPageData();
            SaveColors();

            try
            {
                if (keyboardHookId != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(keyboardHookId);
                }
            }
            catch
            {
                // Silent fail
            }
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            LayoutControls();
        }

        private void Form_WindowStateChanged(object sender, EventArgs e)
        {
            this.TopMost = (this.WindowState != FormWindowState.Minimized);
        }

        private void LayoutControls()
        {
            int margin = 7;
            int gridSize = 4;
            int buttonSize = 70;
            int buttonSpacing = 10;
            int commandWidth = 200;
            int descriptionWidth = 200;
            int textBoxHeight = 30;
            int labelWidth = 30;
            int commandSpacing = 10;

            int totalGridWidth = gridSize * buttonSize + buttonSpacing;

            int startX = margin;
            int startY = margin;

            for (int i = 0; i < 16; i++)
            {
                int row = i / gridSize;
                int col = i % gridSize;

                buttons[i].Size = new Size(buttonSize, buttonSize);
                buttons[i].Location = new Point(
                    startX + col * (buttonSize + buttonSpacing),
                    startY + row * (buttonSize + buttonSpacing)
                );
            }

            for (int i = 0; i < 16; i++)
            {
                textBoxLabels[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing + 25,
                    startY + i * textBoxHeight + (textBoxHeight - textBoxLabels[i].Height) / 2
                );

                commandTextBoxes[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing + labelWidth + 20,
                    startY + i * textBoxHeight
                );
                commandTextBoxes[i].Size = new Size(commandWidth, textBoxHeight);

                descriptionTextBoxes[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing + labelWidth + commandWidth + commandSpacing + 20,
                    startY + i * textBoxHeight
                );
                descriptionTextBoxes[i].Size = new Size(descriptionWidth, textBoxHeight);
            }

            int gridBottom = startY + gridSize * (buttonSize + buttonSpacing) - buttonSpacing;
            minusButton.Location = new Point(startX, gridBottom + 10);
            plusButton.Location = new Point(startX + 40, gridBottom + 10);

            pageNumberLabel.Location = new Point(startX + 80, gridBottom + 15);
            pageDescriptionTextBox.Location = new Point(startX + 80 + pageNumberLabel.Width + 5, gridBottom + 10);
            pageDescriptionTextBox.Size = new Size(160, textBoxHeight);
        }

        private string output = "";
        private string error = "";

        private void OutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            if (args.Data != null) output += args.Data + Environment.NewLine;
        }

        private void ErrorDataReceived(object sender, DataReceivedEventArgs args)
        {
            if (args.Data != null) error += args.Data + Environment.NewLine;
        }

        private void ShowOutputMessageBox(string result, int index)
        {
            MessageBox.Show(result, "Output of Command " + Convert.ToString((currentPage - 1) * 16 + index + 1),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowErrorMessageBox(string message)
        {
            MessageBox.Show("Error: " + message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int index = (int)btn.Tag;
            string cmd = commandTextBoxes[index].Text.Trim();

            if (string.IsNullOrEmpty(cmd))
            {
                MessageBox.Show("Please enter a command in the TextBox.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ThreadPool.QueueUserWorkItem(new WaitCallback(RunCommand), new object[] { cmd, index });
        }

        private void RunCommand(object state)
        {
            object[] args = (object[])state;
            string cmd = (string)args[0];
            int index = (int)args[1];

            try
            {
                string resolvedCmd = ResolveCommand(cmd);
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/C " + resolvedCmd;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                using (Process process = new Process())
                {
                    process.StartInfo = psi;
                    output = "";
                    error = "";
                    process.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceived);
                    process.ErrorDataReceived += new DataReceivedEventHandler(ErrorDataReceived);

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();

                    string result = string.IsNullOrEmpty(error) ? output : "Error: " + error;

                    if (!string.IsNullOrEmpty(result))
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new ShowOutputDelegate(ShowOutputMessageBox), new object[] { result, index });
                        }
                        else
                        {
                            ShowOutputMessageBox(result, index);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new ShowErrorDelegate(ShowErrorMessageBox), new object[] { ex.Message });
                }
                else
                {
                    ShowErrorMessageBox(ex.Message);
                }
            }
        }

        private delegate void ShowOutputDelegate(string result, int index);
        private delegate void ShowErrorDelegate(string message);

        private string ResolveCommand(string cmd)
        {
            if (string.IsNullOrEmpty(cmd))
                return cmd;

            string[] parts = cmd.Split(new char[] { ' ' }, 2);
            string executable = parts[0];
            string arguments = parts.Length > 1 ? parts[1] : "";

            if (File.Exists(executable))
                return cmd;

            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            string[] paths = pathEnv != null ? pathEnv.Split(';') : new string[0];
            foreach (string path in paths)
            {
                string fullPath = Path.Combine(path, executable);
                if (File.Exists(fullPath))
                    return arguments.Length > 0 ? "\"" + fullPath + "\" " + arguments : fullPath;
                fullPath = Path.Combine(path, executable + ".exe");
                if (File.Exists(fullPath))
                    return arguments.Length > 0 ? "\"" + fullPath + "\" " + arguments : fullPath;
            }

            return cmd;
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new CommandBoard());
        }
    }
}