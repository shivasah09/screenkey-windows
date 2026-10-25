using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// ScreenKey: a native Windows on-screen keyboard, built entirely with Windows components.
// This window does not activate when clicked, so the previous application's caret
// remains active. Keystrokes are sent using the standard Windows SendInput API.

internal static class NativeKeyboard
{
    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_UNICODE = 0x0004;

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

    private static void Inject(ushort vk, ushort scan, uint flags)
    {
        INPUT input = new INPUT();
        input.type = INPUT_KEYBOARD;
        input.U.ki = new KEYBDINPUT();
        input.U.ki.wVk = vk;
        input.U.ki.wScan = scan;
        input.U.ki.dwFlags = flags;
        SendInput(1, ref input, Marshal.SizeOf(typeof(INPUT)));
    }

    public static void KeyDown(ushort vk)
    {
        uint extended = (vk == 0x25 || vk == 0x26 || vk == 0x27 || vk == 0x28 ||
                         vk == 0x2D || vk == 0x2E || vk == 0x24 || vk == 0x23 ||
                         vk == 0x21 || vk == 0x22 || vk == 0x5B) ? KEYEVENTF_EXTENDEDKEY : 0;
        Inject(vk, 0, extended);
    }

    public static void KeyUp(ushort vk)
    {
        uint extended = (vk == 0x25 || vk == 0x26 || vk == 0x27 || vk == 0x28 ||
                         vk == 0x2D || vk == 0x2E || vk == 0x24 || vk == 0x23 ||
                         vk == 0x21 || vk == 0x22 || vk == 0x5B) ? KEYEVENTF_EXTENDEDKEY : 0;
        Inject(vk, 0, extended | KEYEVENTF_KEYUP);
    }

    public static void TypeText(string value)
    {
        if (value == null) return;
        foreach (char c in value)
        {
            Inject(0, (ushort)c, KEYEVENTF_UNICODE);
            Inject(0, (ushort)c, KEYEVENTF_UNICODE | KEYEVENTF_KEYUP);
        }
    }
}

internal sealed class KeySpec
{
    public readonly string Code;
    public readonly string Normal;
    public readonly string Shifted;
    public readonly float Weight;
    public Button Button;

    public KeySpec(string code, string normal, string shifted, float weight)
    {
        Code = code;
        Normal = normal;
        Shifted = shifted;
        Weight = weight;
    }
}

public sealed class ScreenKeyForm : Form
{
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int WM_MOUSEACTIVATE = 0x0021;
    private const int MA_NOACTIVATE = 3;
    private const int WM_NCLBUTTONDOWN = 0x00A1;
    private const int HTCAPTION = 2;
    private const int HEADER_H = 55;
    private const int GAP = 5;

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

    private readonly Color background = Color.FromArgb(16, 22, 34);
    private readonly Color keyBackground = Color.FromArgb(36, 48, 66);
    private readonly Color keyHover = Color.FromArgb(57, 73, 96);
    private readonly Color specialBackground = Color.FromArgb(48, 60, 78);
    private readonly Color accent = Color.FromArgb(44, 139, 245);
    private readonly Color muted = Color.FromArgb(153, 168, 189);
    private readonly Color foreground = Color.FromArgb(240, 245, 252);

    private readonly List<List<KeySpec>> rows = new List<List<KeySpec>>();
    private readonly Dictionary<string, ushort> virtualKeys = new Dictionary<string, ushort>();
    private readonly Timer foregroundTimer;
    private readonly Panel header = new Panel();
    private readonly Label statusLabel = new Label();
    private readonly Button pinButton;
    private readonly Button opacityButton;
    private bool shift;
    private bool caps;
    private bool ctrl;
    private bool alt;
    private bool win;
    private IntPtr lastTarget = IntPtr.Zero;
    private int opacityStep = 0;
    private bool compact = false;

    protected override bool ShowWithoutActivation { get { return true; } }

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= WS_EX_NOACTIVATE;
            return cp;
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_MOUSEACTIVATE)
        {
            m.Result = new IntPtr(MA_NOACTIVATE);
            return;
        }
        base.WndProc(ref m);
    }

    public ScreenKeyForm()
    {
        Text = "ScreenKey — On-Screen Keyboard";
        BackColor = background;
        ForeColor = foreground;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        ShowInTaskbar = true;
        DoubleBuffered = true;
        MinimumSize = new Size(810, 350);
        Size = new Size(1125, 445);

        Screen screen = Screen.PrimaryScreen;
        Rectangle area = screen.WorkingArea;
        Width = Math.Min(Width, area.Width - 16);
        Height = Math.Min(Height, area.Height - 16);
        Location = new Point(area.Left + (area.Width - Width) / 2,
                             area.Bottom - Height - 10);

        header.BackColor = Color.FromArgb(22, 32, 48);
        header.SetBounds(0, 0, ClientSize.Width, HEADER_H);
        header.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        header.MouseDown += DragHeader;
        Controls.Add(header);

        Label iconLabel = new Label();
        iconLabel.Text = "⌨";
        iconLabel.ForeColor = accent;
        iconLabel.Font = new Font("Segoe UI Symbol", 20F, FontStyle.Bold);
        iconLabel.SetBounds(17, 8, 44, 39);
        iconLabel.MouseDown += DragHeader;
        header.Controls.Add(iconLabel);

        Label title = new Label();
        title.Text = "SCREENKEY";
        title.ForeColor = foreground;
        title.Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold);
        title.SetBounds(63, 8, 165, 24);
        title.MouseDown += DragHeader;
        header.Controls.Add(title);

        Label subtitle = new Label();
        subtitle.Text = "Click into an app, then type here";
        subtitle.ForeColor = muted;
        subtitle.Font = new Font("Segoe UI", 8F);
        subtitle.SetBounds(64, 31, 280, 18);
        subtitle.MouseDown += DragHeader;
        header.Controls.Add(subtitle);

        pinButton = HeaderButton("PIN", 48, delegate { TopMost = !TopMost; pinButton.Text = TopMost ? "PIN" : "UNPIN"; });
        opacityButton = HeaderButton("85%", 51, delegate { CycleOpacity(); });
        HeaderButton("SIZE", 55, delegate { ToggleSize(); });
        HeaderButton("—", 38, delegate { WindowState = FormWindowState.Minimized; });
        Button close = HeaderButton("×", 38, delegate { Close(); });
        close.BackColor = Color.FromArgb(130, 48, 65);
        close.MouseEnter += delegate { close.BackColor = Color.FromArgb(180, 55, 75); };
        close.MouseLeave += delegate { close.BackColor = Color.FromArgb(130, 48, 65); };
        opacityButton.Text = "100%";

        statusLabel.Text = "SHIFT / CTRL / ALT / WIN = tap once for a shortcut   •   CAPS = lock uppercase   •   Drag the top bar to move";
        statusLabel.ForeColor = muted;
        statusLabel.Font = new Font("Segoe UI", 8F);
        statusLabel.AutoEllipsis = true;
        statusLabel.SetBounds(17, ClientSize.Height - 23, ClientSize.Width - 32, 19);
        statusLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        Controls.Add(statusLabel);

        InitializeVirtualKeys();
        BuildKeyboard();
        Resize += delegate { LayoutKeys(); PositionHeaderButtons(); };
        LayoutKeys();
        PositionHeaderButtons();

        foregroundTimer = new Timer();
        foregroundTimer.Interval = 150;
        foregroundTimer.Tick += delegate { RememberTarget(); };
        foregroundTimer.Start();
        RememberTarget();
    }

    private readonly List<Button> headerButtons = new List<Button>();
    private readonly List<int> headerWidths = new List<int>();

    private Button HeaderButton(string text, int width, EventHandler action)
    {
        Button button = new Button();
        button.Text = text;
        button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Color.FromArgb(41, 54, 74);
        button.ForeColor = foreground;
        button.Cursor = Cursors.Hand;
        button.TabStop = false;
        button.Height = 31;
        button.Click += action;
        header.Controls.Add(button);
        headerButtons.Add(button);
        headerWidths.Add(width);
        return button;
    }

    private void PositionHeaderButtons()
    {
        int right = header.ClientSize.Width - 13;
        for (int i = headerButtons.Count - 1; i >= 0; i--)
        {
            int width = headerWidths[i];
            right -= width;
            headerButtons[i].SetBounds(right, 12, width, 31);
            right -= 5;
        }
    }

    private void DragHeader(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        ReleaseCapture();
        SendMessage(Handle, WM_NCLBUTTONDOWN, new IntPtr(HTCAPTION), IntPtr.Zero);
    }

    private void CycleOpacity()
    {
        opacityStep = (opacityStep + 1) % 3;
        double[] values = new double[] { 1.00, 0.87, 0.74 };
        Opacity = values[opacityStep];
        opacityButton.Text = ((int)(values[opacityStep] * 100)).ToString() + "%";
    }

    private void ToggleSize()
    {
        compact = !compact;
        Size = compact ? new Size(Math.Min(900, Screen.PrimaryScreen.WorkingArea.Width - 16), 360)
                       : new Size(Math.Min(1125, Screen.PrimaryScreen.WorkingArea.Width - 16), 445);
    }

    private void InitializeVirtualKeys()
    {
        virtualKeys["ESC"] = 0x1B;
        virtualKeys["DEL"] = 0x2E;
        virtualKeys["BACKSPACE"] = 0x08;
        virtualKeys["TAB"] = 0x09;
        virtualKeys["ENTER"] = 0x0D;
        virtualKeys["LEFT"] = 0x25;
        virtualKeys["UP"] = 0x26;
        virtualKeys["RIGHT"] = 0x27;
        virtualKeys["DOWN"] = 0x28;
        virtualKeys["HOME"] = 0x24;
        virtualKeys["END"] = 0x23;
        for (int i = 1; i <= 12; i++)
        {
            virtualKeys["F" + i.ToString()] = (ushort)(0x70 + i - 1);
        }
    }

    private KeySpec K(string code, string normal, string shifted, float units)
    {
        return new KeySpec(code, normal, shifted, units);
    }

    private KeySpec K(string code, string normal)
    {
        return K(code, normal, normal, 1F);
    }

    private KeySpec C(string letter)
    {
        return K(letter, letter.ToLowerInvariant(), letter.ToUpperInvariant(), 1F);
    }

    private void AddRow(params KeySpec[] keys)
    {
        List<KeySpec> row = new List<KeySpec>(keys);
        rows.Add(row);
        foreach (KeySpec key in row)
        {
            Button button = new Button();
            key.Button = button;
            button.Text = GetKeyLabel(key);
            button.Tag = key;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseDownBackColor = keyHover;
            button.FlatAppearance.MouseOverBackColor = keyHover;
            button.ForeColor = foreground;
            button.BackColor = IsSpecial(key) ? specialBackground : keyBackground;
            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;
            button.TabStop = false;
            button.Margin = new Padding(0);
            button.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Regular);
            button.Click += KeyClicked;
            button.MouseEnter += delegate(object s, EventArgs e) {
                Button b = (Button)s;
                KeySpec spec = (KeySpec)b.Tag;
                if (!IsSelected(spec)) b.BackColor = keyHover;
            };
            button.MouseLeave += delegate(object s, EventArgs e) {
                Button b = (Button)s;
                KeySpec spec = (KeySpec)b.Tag;
                b.BackColor = IsSelected(spec) ? accent : (IsSpecial(spec) ? specialBackground : keyBackground);
            };
            Controls.Add(button);
        }
    }

    private void BuildKeyboard()
    {
        AddRow(K("ESC", "Esc"), K("F1", "F1"), K("F2", "F2"), K("F3", "F3"),
               K("F4", "F4"), K("F5", "F5"), K("F6", "F6"), K("F7", "F7"),
               K("F8", "F8"), K("F9", "F9"), K("F10", "F10"), K("F11", "F11"),
               K("F12", "F12"), K("DEL", "Del", "Del", 1.1F));

        AddRow(K("GRAVE", "`", "~", 1F), K("1", "1", "!", 1F), K("2", "2", "@", 1F),
               K("3", "3", "#", 1F), K("4", "4", "$", 1F), K("5", "5", "%", 1F),
               K("6", "6", "^", 1F), K("7", "7", "&", 1F), K("8", "8", "*", 1F),
               K("9", "9", "(", 1F), K("0", "0", ")", 1F), K("MINUS", "-", "_", 1F),
               K("EQUAL", "=", "+", 1F), K("BACKSPACE", "⌫ Backspace", "⌫ Backspace", 2.15F));

        AddRow(K("TAB", "Tab", "Tab", 1.55F), C("Q"), C("W"), C("E"), C("R"),
               C("T"), C("Y"), C("U"), C("I"), C("O"), C("P"),
               K("LBRACKET", "[", "{", 1F), K("RBRACKET", "]", "}", 1F),
               K("BACKSLASH", "\\", "|", 1.55F));

        AddRow(K("CAPS", "Caps", "Caps", 1.9F), C("A"), C("S"), C("D"), C("F"),
               C("G"), C("H"), C("J"), C("K"), C("L"),
               K("SEMICOLON", ";", ":", 1F), K("APOSTROPHE", "'", "\"", 1F),
               K("ENTER", "↵ Enter", "↵ Enter", 2.25F));

        AddRow(K("SHIFT", "⇧ Shift", "⇧ Shift", 2.2F), C("Z"), C("X"), C("C"),
               C("V"), C("B"), C("N"), C("M"), K("COMMA", ",", "<", 1F),
               K("PERIOD", ".", ">", 1F), K("SLASH", "/", "?", 1F),
               K("SHIFT", "⇧ Shift", "⇧ Shift", 2.2F), K("UP", "↑", "↑", 1F));

        AddRow(K("CTRL", "Ctrl", "Ctrl", 1.4F), K("WIN", "Win", "Win", 1.25F),
               K("ALT", "Alt", "Alt", 1.25F), K("SPACE", "Space", "Space", 7.4F),
               K("ALT", "Alt", "Alt", 1.25F), K("CTRL", "Ctrl", "Ctrl", 1.4F),
               K("LEFT", "←"), K("DOWN", "↓"), K("RIGHT", "→"),
               K("HOME", "Home", "Home", 1.1F), K("END", "End", "End", 1.1F));
    }

    private bool IsSpecial(KeySpec spec)
    {
        return (spec.Code.Length > 1 && spec.Code != "GRAVE" && spec.Code != "MINUS" &&
                spec.Code != "EQUAL" && spec.Code != "LBRACKET" && spec.Code != "RBRACKET" &&
                spec.Code != "BACKSLASH" && spec.Code != "SEMICOLON" &&
                spec.Code != "APOSTROPHE" && spec.Code != "COMMA" &&
                spec.Code != "PERIOD" && spec.Code != "SLASH") ||
               spec.Code == "SPACE";
    }

    private bool IsSelected(KeySpec spec)
    {
        switch (spec.Code)
        {
            case "SHIFT": return shift;
            case "CAPS": return caps;
            case "CTRL": return ctrl;
            case "ALT": return alt;
            case "WIN": return win;
            default: return false;
        }
    }

    private string GetKeyLabel(KeySpec spec)
    {
        if (spec.Code.Length == 1 && char.IsLetter(spec.Code[0])) return spec.Code;
        if (spec.Code == "GRAVE" || spec.Code == "MINUS" || spec.Code == "EQUAL" ||
            spec.Code == "LBRACKET" || spec.Code == "RBRACKET" || spec.Code == "BACKSLASH" ||
            spec.Code == "SEMICOLON" || spec.Code == "APOSTROPHE" || spec.Code == "COMMA" ||
            spec.Code == "PERIOD" || spec.Code == "SLASH" ||
            (spec.Code.Length == 1 && char.IsDigit(spec.Code[0])))
        {
            return shift ? spec.Shifted : spec.Normal;
        }
        return spec.Normal;
    }

    private void UpdateKeys()
    {
        foreach (List<KeySpec> row in rows)
        {
            foreach (KeySpec key in row)
            {
                key.Button.Text = GetKeyLabel(key);
                key.Button.BackColor = IsSelected(key) ? accent :
                    (IsSpecial(key) ? specialBackground : keyBackground);
            }
        }
    }

    private void LayoutKeys()
    {
        int left = 13;
        int usableW = Math.Max(300, ClientSize.Width - left * 2);
        int top = HEADER_H + 9;
        int bottomReserved = 29;
        int usableH = Math.Max(240, ClientSize.Height - top - bottomReserved);
        int rowGap = 6;
        int keyH = (usableH - rowGap * (rows.Count - 1)) / rows.Count;

        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            List<KeySpec> row = rows[rowIndex];
            float totalWeight = 0;
            foreach (KeySpec key in row) totalWeight += key.Weight;
            int widthForKeys = usableW - GAP * (row.Count - 1);
            float unit = widthForKeys / totalWeight;
            int x = left;
            int y = top + rowIndex * (keyH + rowGap);

            for (int i = 0; i < row.Count; i++)
            {
                int w = i == row.Count - 1 ? left + usableW - x :
                    Math.Max(19, (int)Math.Round(unit * row[i].Weight));
                row[i].Button.SetBounds(x, y, w, keyH);
                x += w + GAP;
            }
        }
    }

    private void RememberTarget()
    {
        IntPtr active = NativeKeyboard.GetForegroundWindow();
        if (active == IntPtr.Zero) return;
        IntPtr root = NativeKeyboard.GetAncestor(active, 2);
        if (root != Handle && NativeKeyboard.IsWindow(active)) lastTarget = active;
    }

    private void KeepTargetFocused()
    {
        IntPtr active = NativeKeyboard.GetForegroundWindow();
        if ((active == Handle || NativeKeyboard.GetAncestor(active, 2) == Handle) &&
            lastTarget != IntPtr.Zero && NativeKeyboard.IsWindow(lastTarget))
        {
            NativeKeyboard.SetForegroundWindow(lastTarget);
        }
    }

    private void KeyClicked(object sender, EventArgs e)
    {
        KeySpec spec = (KeySpec)((Button)sender).Tag;
        string code = spec.Code;
        switch (code)
        {
            case "SHIFT": shift = !shift; UpdateKeys(); return;
            case "CAPS": caps = !caps; UpdateKeys(); return;
            case "CTRL": ctrl = !ctrl; UpdateKeys(); return;
            case "ALT": alt = !alt; UpdateKeys(); return;
            case "WIN": win = !win; UpdateKeys(); return;
        }

        KeepTargetFocused();

        ushort vk;
        if (virtualKeys.TryGetValue(code, out vk))
        {
            SendShortcutOrKey(vk);
        }
        else if (code == "SPACE")
        {
            if (ctrl || alt || win) SendShortcutOrKey(0x20);
            else NativeKeyboard.TypeText(" ");
        }
        else if (code.Length == 1 && char.IsLetterOrDigit(code[0]) && (ctrl || alt || win))
        {
            // Virtual key codes for Latin A-Z and 0-9 are their ASCII upper-case values.
            SendShortcutOrKey((ushort)char.ToUpperInvariant(code[0]));
        }
        else
        {
            bool upper = shift ^ caps;
            string text = code.Length == 1 && char.IsLetter(code[0]) ?
                (upper ? spec.Shifted : spec.Normal) :
                (shift ? spec.Shifted : spec.Normal);
            NativeKeyboard.TypeText(text);
        }

        // Shift, Ctrl, Alt, and Win are one-shot toggles; Caps Lock stays on.
        shift = false;
        ctrl = false;
        alt = false;
        win = false;
        UpdateKeys();
    }

    private void SendShortcutOrKey(ushort vk)
    {
        // Keep modifier key presses balanced, even if the foreground app rejects them.
        if (ctrl) NativeKeyboard.KeyDown(0x11);
        if (alt) NativeKeyboard.KeyDown(0x12);
        if (win) NativeKeyboard.KeyDown(0x5B);
        if (shift) NativeKeyboard.KeyDown(0x10);
        NativeKeyboard.KeyDown(vk);
        NativeKeyboard.KeyUp(vk);
        if (shift) NativeKeyboard.KeyUp(0x10);
        if (win) NativeKeyboard.KeyUp(0x5B);
        if (alt) NativeKeyboard.KeyUp(0x12);
        if (ctrl) NativeKeyboard.KeyUp(0x11);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (foregroundTimer != null) foregroundTimer.Dispose();
        }
        base.Dispose(disposing);
    }
}