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
        private Button[] buttons = new Button[160];
        private TextBox[] commandTextBoxes = new TextBox[160];
        private TextBox[] descriptionTextBoxes = new TextBox[160];
        private Label[] textBoxLabels = new Label[160];
        private Button plusButton;
        private Button minusButton;
        private Label pageNumberLabel;
        private TextBox pageDescriptionTextBox;
        private readonly string commandsFile = "commands.txt";
        private readonly string descriptionsFile = "descriptions.txt";
        private readonly string resetFlagFile = "reset.flag";
        private IntPtr keyboardHookId = IntPtr.Zero;
        private LowLevelKeyboardProc keyboardProc;
        private int currentPage = 1;
        private const int MaxPages = 10;
        private readonly Color ButtonBackColor = Color.FromArgb(11, 16, 150);
        private readonly Color ButtonForeColor = Color.White;
        private readonly Color TextBoxForeColor = Color.FromArgb(22, 181, 4);
        private readonly Color TextBoxBackColor = Color.Black;
        private readonly Color FormBackColor = Color.Black;
        private readonly Color ControlBorderColor = Color.White;

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

            InitializeControls();
            LoadCommands();
            LoadDescriptions();

            // Start in full-screen mode
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Bounds = Screen.PrimaryScreen.Bounds;

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

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                if (key == Keys.Escape)
                {
                    if (this.WindowState == FormWindowState.Maximized)
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new MethodInvoker(RestoreWindow));
                        }
                        else
                        {
                            RestoreWindow();
                        }
                    }
                    return new IntPtr(1);
                }

                if (key >= Keys.F1 && key <= Keys.F12)
                {
                    int index = (currentPage - 1) * 16 + (key - Keys.F1);
                    if (index < 160 && (key - Keys.F1) < 12)
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new MethodInvoker(delegate { TriggerButtonClick(index); }));
                        }
                        else
                        {
                            TriggerButtonClick(index);
                        }
                        return new IntPtr(1);
                    }
                }
            }
            return CallNextHookEx(keyboardHookId, nCode, wParam, lParam);
        }

        private void RestoreWindow()
        {
            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            LayoutControls();
        }

        private void TriggerButtonClick(int index)
        {
            buttons[index].PerformClick();
        }

        private void InitializeControls()
        {
            string defaultCommand = "ipconfig /all";

            for (int i = 0; i < 160; i++)
            {
                int buttonNumber = i + 1;
                Button btn = new Button();
                btn.Text = Convert.ToString(buttonNumber);
                btn.Tag = i;
                btn.FlatStyle = FlatStyle.Standard;
                btn.BackColor = ButtonBackColor;
                btn.ForeColor = ButtonForeColor;
                btn.Font = new Font("Arial", 10, FontStyle.Bold);
                btn.ImageAlign = ContentAlignment.MiddleCenter;
                buttons[i] = btn;
                buttons[i].Click += new EventHandler(Button_Click);
                this.Controls.Add(buttons[i]);

                TextBox cmdTextBox = new TextBox();
                cmdTextBox.Text = defaultCommand;
                cmdTextBox.Font = new Font("Arial", 10);
                cmdTextBox.Width = 200;
                cmdTextBox.BackColor = TextBoxBackColor;
                cmdTextBox.ForeColor = TextBoxForeColor;
                cmdTextBox.BorderStyle = BorderStyle.FixedSingle;
                commandTextBoxes[i] = cmdTextBox;
                commandTextBoxes[i].Paint += new PaintEventHandler(CommandTextBox_Paint);
                this.Controls.Add(cmdTextBox);

                TextBox descTextBox = new TextBox();
                descTextBox.Text = Convert.ToString(buttonNumber);
                descTextBox.Font = new Font("Arial", 10);
                descTextBox.Width = 200;
                descTextBox.BackColor = TextBoxBackColor;
                descTextBox.ForeColor = TextBoxForeColor;
                descTextBox.BorderStyle = BorderStyle.FixedSingle;
                descriptionTextBoxes[i] = descTextBox;
                descriptionTextBoxes[i].TextChanged += new EventHandler(DescriptionTextBox_TextChanged);
                descriptionTextBoxes[i].Paint += new PaintEventHandler(DescriptionTextBox_Paint);
                this.Controls.Add(descTextBox);

                Label label = new Label();
                label.Text = Convert.ToString(buttonNumber);
                label.Font = new Font("Arial", 10, FontStyle.Bold);
                label.AutoSize = true;
                label.BackColor = TextBoxBackColor;
                label.ForeColor = TextBoxForeColor;
                textBoxLabels[i] = label;
                this.Controls.Add(label);
            }

            minusButton = new Button();
            minusButton.Text = "-";
            minusButton.Size = new Size(30, 30);
            minusButton.FlatStyle = FlatStyle.Standard;
            minusButton.BackColor = TextBoxBackColor;
            minusButton.ForeColor = TextBoxForeColor;
            minusButton.Font = new Font("Arial", 12, FontStyle.Bold);
            minusButton.Click += new EventHandler(MinusButton_Click);
            this.Controls.Add(minusButton);

            plusButton = new Button();
            plusButton.Text = "+";
            plusButton.Size = new Size(30, 30);
            plusButton.FlatStyle = FlatStyle.Standard;
            plusButton.BackColor = TextBoxBackColor;
            plusButton.ForeColor = TextBoxForeColor;
            plusButton.Font = new Font("Arial", 12, FontStyle.Bold);
            plusButton.Click += new EventHandler(PlusButton_Click);
            this.Controls.Add(plusButton);

            pageNumberLabel = new Label();
            pageNumberLabel.Text = Convert.ToString(currentPage);
            pageNumberLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            pageNumberLabel.AutoSize = true;
            pageNumberLabel.BackColor = TextBoxBackColor;
            pageNumberLabel.ForeColor = TextBoxForeColor;
            this.Controls.Add(pageNumberLabel);

            pageDescriptionTextBox = new TextBox();
            pageDescriptionTextBox.Text = "Page " + Convert.ToString(currentPage) + " Description";
            pageDescriptionTextBox.Font = new Font("Arial", 10);
            pageDescriptionTextBox.Width = 200;
            pageDescriptionTextBox.BackColor = TextBoxBackColor;
            pageDescriptionTextBox.ForeColor = TextBoxForeColor;
            pageDescriptionTextBox.BorderStyle = BorderStyle.FixedSingle;
            pageDescriptionTextBox.Paint += new PaintEventHandler(PageDescriptionTextBox_Paint);
            this.Controls.Add(pageDescriptionTextBox);
        }

        private void DescriptionTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            for (int i = 0; i < 160; i++)
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

            int startIndex = (currentPage - 1) * 16;
            for (int i = 0; i < 16; i++)
            {
                int buttonNumber = startIndex + i + 1;
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

                int startIndex = (currentPage - 1) * 16;
                for (int i = 0; i < 16; i++)
                {
                    int index = startIndex + i;
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
                int startIndex = (currentPage - 1) * 16;
                for (int i = 0; i < 16; i++)
                {
                    int index = startIndex + i;
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
                    int startIndex = (currentPage - 1) * 16;
                    for (int i = 0; i < 16; i++)
                    {
                        int index = startIndex + i;
                        if (index < descriptions.Length && !string.IsNullOrEmpty(descriptions[index]))
                        {
                            descriptionTextBoxes[i].Text = descriptions[index];
                        }
                        else
                        {
                            descriptionTextBoxes[i].Text = Convert.ToString(index + 1);
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
                    int startIndex = (currentPage - 1) * 16;
                    for (int i = 0; i < 16; i++)
                    {
                        descriptionTextBoxes[i].Text = Convert.ToString(startIndex + i + 1);
                        UpdateButtonTextFromDescription(i);
                    }
                }
            }
            catch
            {
                int startIndex = (currentPage - 1) * 16;
                for (int i = 0; i < 16; i++)
                {
                    descriptionTextBoxes[i].Text = Convert.ToString(startIndex + i + 1);
                    UpdateButtonTextFromDescription(i);
                }
            }
        }

        private void UpdateButtonTextFromDescription(int index)
        {
            if (index < 0 || index >= 160) return;

            string description = descriptionTextBoxes[index].Text;
            Button btn = buttons[index];

            // Check if description is a valid image file path
            if (!string.IsNullOrEmpty(description) && File.Exists(description))
            {
                string ext = Path.GetExtension(description).ToLower();
                if (ext == ".gif" || ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp")
                {
                    try
                    {
                        using (Image img = Image.FromFile(description))
                        {
                            btn.Image = new Bitmap(img); // Create a copy to avoid file locking
                            btn.Text = "";
                            btn.ImageAlign = ContentAlignment.MiddleCenter;
                            return;
                        }
                    }
                    catch
                    {
                        // Fall back to text if image loading fails
                    }
                }
            }

            // Use text if no valid image
            btn.Image = null; // Clear any previous image
            btn.Text = string.IsNullOrEmpty(description) ? Convert.ToString(index + 1) : description;
        }

        private void Form_Closing(object sender, CancelEventArgs e)
        {
            SaveCurrentPageData();

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
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.Bounds = Screen.PrimaryScreen.Bounds;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }
            this.TopMost = (this.WindowState != FormWindowState.Minimized);
            LayoutControls();
        }

        private void LayoutControls()
        {
            int margin = 7;
            int buttonSpacing = 10;
            int textBoxHeight = 30;
            int commandWidth = 200;
            int descriptionWidth = 200;
            int labelWidth = 30;
            int commandSpacing = 10;

            if (this.WindowState == FormWindowState.Maximized)
            {
                for (int i = 0; i < 160; i++)
                {
                    commandTextBoxes[i].Visible = false;
                    descriptionTextBoxes[i].Visible = false;
                    textBoxLabels[i].Visible = false;
                    buttons[i].Visible = true;
                }
                minusButton.Visible = false;
                plusButton.Visible = false;
                pageNumberLabel.Visible = false;
                pageDescriptionTextBox.Visible = false;

                int gridCols = 16;
                int gridRows = 10;
                int clientWidth = this.ClientSize.Width;
                int clientHeight = this.ClientSize.Height;

                int availableWidth = clientWidth - 2 * margin - (gridCols - 1) * buttonSpacing;
                int availableHeight = clientHeight - 2 * margin - (gridRows - 1) * buttonSpacing;
                int buttonWidth = availableWidth / gridCols;
                int buttonHeight = availableHeight / gridRows;
                buttonWidth = Math.Max(buttonWidth, 30);
                buttonHeight = Math.Max(buttonHeight, 30);

                for (int i = 0; i < 160; i++)
                {
                    int row = i / gridCols;
                    int col = i % gridCols;
                    buttons[i].Size = new Size(buttonWidth, buttonHeight);
                    buttons[i].Location = new Point(
                        margin + col * (buttonWidth + buttonSpacing),
                        margin + row * (buttonHeight + buttonSpacing)
                    );
                    buttons[i].Font = new Font("Arial", Math.Max(8, Math.Min(buttonWidth, buttonHeight) / 8), FontStyle.Bold);
                }
            }
            else
            {
                int gridSize = 4;
                int buttonSize = 70;
                int totalGridWidth = gridSize * buttonSize + buttonSpacing;

                int startX = margin;
                int startY = margin;
                int startIndex = (currentPage - 1) * 16;

                for (int i = 0; i < 160; i++)
                {
                    buttons[i].Visible = (i >= startIndex && i < startIndex + 16);
                    commandTextBoxes[i].Visible = (i >= startIndex && i < startIndex + 16);
                    descriptionTextBoxes[i].Visible = (i >= startIndex && i < startIndex + 16);
                    textBoxLabels[i].Visible = (i >= startIndex && i < startIndex + 16);

                    if (buttons[i].Visible)
                    {
                        int localIndex = i - startIndex;
                        int row = localIndex / gridSize;
                        int col = localIndex % gridSize;

                        buttons[i].Size = new Size(buttonSize, buttonSize);
                        buttons[i].Location = new Point(
                            startX + col * (buttonSize + buttonSpacing),
                            startY + row * (buttonSize + buttonSpacing)
                        );
                        buttons[i].Font = new Font("Arial", 10, FontStyle.Bold);

                        textBoxLabels[i].Location = new Point(
                            startX + totalGridWidth + buttonSpacing + 25,
                            startY + localIndex * textBoxHeight + (textBoxHeight - textBoxLabels[i].Height) / 2
                        );

                        commandTextBoxes[i].Location = new Point(
                            startX + totalGridWidth + buttonSpacing + labelWidth + 20,
                            startY + localIndex * textBoxHeight
                        );
                        commandTextBoxes[i].Size = new Size(commandWidth, textBoxHeight);

                        descriptionTextBoxes[i].Location = new Point(
                            startX + totalGridWidth + buttonSpacing + labelWidth + commandWidth + commandSpacing + 20,
                            startY + localIndex * textBoxHeight
                        );
                        descriptionTextBoxes[i].Size = new Size(descriptionWidth, textBoxHeight);
                    }
                }

                int gridBottom = startY + gridSize * (buttonSize + buttonSpacing) - buttonSpacing;
                minusButton.Visible = true;
                minusButton.Location = new Point(startX, gridBottom + 10);
                plusButton.Visible = true;
                plusButton.Location = new Point(startX + 40, gridBottom + 10);
                pageNumberLabel.Visible = true;
                pageNumberLabel.Location = new Point(startX + 80, gridBottom + 15);
                pageDescriptionTextBox.Visible = true;
                pageDescriptionTextBox.Location = new Point(startX + 80 + pageNumberLabel.Width + 5, gridBottom + 10);
                pageDescriptionTextBox.Size = new Size(160, textBoxHeight);
            }
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
            MessageBox.Show(result, "Output of Command " + Convert.ToString(index + 1),
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
