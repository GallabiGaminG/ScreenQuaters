using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace ScreenQuarters;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        using var single = new Mutex(true, @"Local\ScreenQuarters-v1", out bool first);
        if (!first) { MessageBox.Show("ScreenQuarters zaten sistem tepsisinde çalışıyor."); return; }
        ApplicationConfiguration.Initialize();
        Application.Run(new QuarterContext());
    }
}

internal sealed class Options
{
    public bool SnapOnDrag { get; set; } = false;
    public bool BrowserFullscreen { get; set; } = true;
    public Dictionary<string, LayoutPreset> MonitorLayouts { get; set; } = new();
    public string ExcludedProcesses { get; set; } = "bandicam,obs64,steam,explorer";
}

internal sealed class ManagedWindow
{
    public uint ProcessId;
    public int Zone;
    public Native.Placement Original;
    public nint Monitor;
    public bool Browser;
    public Queue<long> Corrections = new();
}

internal sealed class QuarterContext : ApplicationContext
{
    readonly NotifyIcon tray;
    readonly HotkeyWindow receiver = new();
    readonly System.Windows.Forms.Timer timer = new() { Interval = 200 };
    readonly Dictionary<nint, ManagedWindow> windows = new();
    readonly Native.WinEventProc eventProc;
    readonly List<int> registered = new();
    readonly string optionsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ScreenQuarters", "settings.json");
    readonly ToolStripMenuItem modeItem;
    nint hook;
    Options options = new();
    bool working = true, mutating, closing;
    nint moving;

    public QuarterContext()
    {
        try { if (File.Exists(optionsPath)) options = JsonSerializer.Deserialize<Options>(File.ReadAllText(optionsPath)) ?? new(); }
        catch { MessageBox.Show("Ayar dosyası okunamadı. Varsayılan ayarlar kullanılıyor.", "ScreenQuarters"); }
        var menu = new ContextMenuStrip();
        modeItem = new ToolStripMenuItem("Çalışma modu: açık", null, (_, _) => ToggleMode());
        menu.Items.Add(modeItem);
        menu.Items.Add("Tüm pencereleri bırak", null, (_, _) => ReleaseAll());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Settings / Ayarlar", null, (_, _) => Settings());
        menu.Items.Add("About / Hakkında", null, (_, _) => MessageBox.Show(
            "ScreenQuarters 0.1.0 — Windows 10/11\n\nCtrl+Alt+1: Sol üst\nCtrl+Alt+2: Sağ üst\nCtrl+Alt+3: Sol alt\nCtrl+Alt+4: Sağ alt\nCtrl+Alt+0: Aktif pencereyi bırak\nCtrl+Alt+F12: Çalışma / tek ekran\n\nTarayıcıda önce bölgeye yerleştir, ardından F11 veya video tam ekranını aç.\nTarayıcı desteği deneyseldir. Gerçek sanal monitör oluşturmaz.\n\nYönetici yetkisi, sürücü ve ağ bağlantısı kullanmaz.", "About / Hakkında"));
        menu.Items.Add("Exit / Çıkış", null, (_, _) => ExitThread());
        tray = new NotifyIcon { Icon = SystemIcons.Application, Text = "ScreenQuarters 0.1.0 — Çalışma", ContextMenuStrip = menu, Visible = true };
        tray.DoubleClick += (_, _) => Settings();
        receiver.Pressed += Hotkey;
        var failures = new List<string>();
        for (int id = 1; id <= 11; id++)
        {
            uint key = id <= 9
                ? (uint)('0' + id)
                : id == 10 ? 0x30u : 0x7Bu;

            if (Native.RegisterHotKey(receiver.Handle, id, 0x4003, key))
            {
                registered.Add(id);
            }
            else
            {
                failures.Add(id <= 9
                    ? $"Ctrl+Alt+{id}"
                    : id == 10 ? "Ctrl+Alt+0" : "Ctrl+Alt+F12");
            }
        }
        eventProc = OnMoveEvent; // Keep delegate alive for the entire hook lifetime.
        hook = Native.SetWinEventHook(0xA, 0xB, 0, eventProc, 0, 0, 2);
        timer.Tick += (_, _) => Tick();
        timer.Start();
        if (failures.Count > 0) Notify("Kısayol başka uygulamada kullanılıyor: " + string.Join(", ", failures));
        else Notify("Hazır. Aktif pencere için Ctrl+Alt+1–4. Tek ekran: Ctrl+Alt+F12.");
        if (hook == 0) Notify("Sürükleme takibi başlatılamadı. Bölge kısayollarını kullanabilirsin.");
    }

    void Notify(string text) => tray.ShowBalloonTip(4000, "ScreenQuarters", text, ToolTipIcon.Info);
    void Hotkey(int id)
    {
        if (id == 11)
        {
            ToggleMode();
            return;
        }

        nint hwnd = Native.GetForegroundWindow();

        if (id == 10)
        {
            Release(hwnd);
            return;
        }

        if (!working)
        {
            Notify("Tek ekran modu açık. Ctrl+Alt+F12 ile çalışma moduna geç.");
            return;
        }

        Assign(hwnd, id - 1);
    }
    void ToggleMode()
    {
        working = !working;
        if (!working) ReleaseAll();
        modeItem.Text = "Çalışma modu: " + (working ? "açık" : "kapalı (tek ekran)");
        tray.Text = "ScreenQuarters 0.1.0 — " + (working ? "Çalışma" : "Tek ekran");
        Notify(working ? "Çalışma modu. Pencereleri bölgelere yeniden yerleştirebilirsin." : "Tek ekran modu. Bölge yönetimi durdu; F11 / oyun tam ekranı serbest.");
    }
    bool Eligible(nint hwnd, out uint pid, out bool browser)
    {
        pid = 0; browser = false;
        if (hwnd == 0 || !Native.IsWindowVisible(hwnd) || Native.GetAncestor(hwnd, 2) != hwnd || Native.GetWindow(hwnd, 4) != 0) return false;
        Native.GetWindowThreadProcessId(hwnd, out pid);
        if (pid == Environment.ProcessId) return false;
        try
        {
            using var process = Process.GetProcessById((int)pid);
            string name = process.ProcessName;
            if ((options.ExcludedProcesses ?? "").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Contains(name, StringComparer.OrdinalIgnoreCase)) return false;
            browser = new[] { "chrome", "msedge", "firefox", "brave", "opera", "vivaldi" }.Contains(name, StringComparer.OrdinalIgnoreCase);
            var cls = new StringBuilder(256); Native.GetClassName(hwnd, cls, cls.Capacity);
            return cls.ToString() is not "Progman" and not "WorkerW" and not "Shell_TrayWnd";
        }
        catch { return false; }
    }
    Native.Rect[] GetZones(nint monitorHandle)
    {
        var monitor = Native.MonitorInfo(monitorHandle);

        if (monitor is null)
            return Array.Empty<Native.Rect>();

        string? deviceName =
            Native.GetMonitorDeviceName(monitorHandle);

        var preset = LayoutPreset.Grid2x2;

        if (deviceName is not null &&
            options.MonitorLayouts is not null)
        {
            foreach (var entry in options.MonitorLayouts)
            {
                if (string.Equals(
                        entry.Key,
                        deviceName,
                        StringComparison.OrdinalIgnoreCase) &&
                    Enum.IsDefined(typeof(LayoutPreset), entry.Value))
                {
                    preset = entry.Value;
                    break;
                }
            }
        }

        return LayoutPresets.GetZones(monitor.Value.Work, preset);
    }

    bool TryGetTarget(ManagedWindow state, out Native.Rect target)
    {
        var zones = GetZones(state.Monitor);

        if (state.Zone < 0 || state.Zone >= zones.Length)
        {
            target = default;
            return false;
        }

        target = zones[state.Zone];
        return true;
    }
    void Assign(
    nint hwnd,
    int zone,
    nint destinationMonitor = default)
    {
        if (!Eligible(hwnd, out uint pid, out bool browser))
            return;

        nint monitor = destinationMonitor != 0
            ? destinationMonitor
            : Native.MonitorFromWindow(hwnd, 2);

        var zones = GetZones(monitor);

        if (zone < 0 || zone >= zones.Length)
        {
            Notify($"Bu ekranın düzeninde {zones.Length} bölge var.");
            return;
        }

        if (!windows.TryGetValue(hwnd, out var state))
        {
            var original = Native.Placement.Create();

            if (!Native.GetWindowPlacement(hwnd, ref original))
            {
                Notify("Pencerenin mevcut konumu okunamadı.");
                return;
            }

            state = new ManagedWindow
            {
                Original = original,
                ProcessId = pid,
                Browser = browser
            };

            windows.Add(hwnd, state);
        }

        state.Zone = zone;
        state.Monitor = monitor;
        state.Corrections.Clear();

        if (!Place(hwnd, state))
        {
            Release(hwnd);
            Notify("Pencere taşınamadı. Uygulama boyutlandırmayı reddediyor olabilir.");
        }
    }
    static bool Alive(nint hwnd, ManagedWindow state)
    {
        Native.GetWindowThreadProcessId(hwnd, out uint pid);
        return Native.IsWindow(hwnd) && pid == state.ProcessId;
    }
    bool Fullscreen(nint hwnd, ManagedWindow state)
    {
        if (!state.Browser || !options.BrowserFullscreen) return false;
        return (Native.Style(hwnd) & 0x00C00000L) == 0;
    }
    bool Place(nint hwnd, ManagedWindow state)
    {
        var monitor = Native.MonitorInfo(state.Monitor);
        if (monitor is null) return false;
        if (!TryGetTarget(state, out var target))
            return false;
        mutating = true;
        try
        {
            if (Native.IsZoomed(hwnd)) Native.ShowWindow(hwnd, 9);
            // Compensate invisible resize borders, preserving the visible zone boundary.
            Native.GetWindowRect(hwnd, out var rect);
            if (Native.DwmGetWindowAttribute(hwnd, 9, out var frame, Marshal.SizeOf<Native.Rect>()) == 0)
                target = new Native.Rect { Left = target.Left - (frame.Left - rect.Left), Top = target.Top - (frame.Top - rect.Top), Right = target.Right + (rect.Right - frame.Right), Bottom = target.Bottom + (rect.Bottom - frame.Bottom) };
            return Native.SetWindowPos(hwnd, 0, target.Left, target.Top, target.Right - target.Left, target.Bottom - target.Top, 0x14);
        }
        finally { mutating = false; }
    }
    void Tick()
    {
        if (!working || closing || mutating) return;
        foreach (var pair in windows.ToArray())
        {
            var hwnd = pair.Key; var state = pair.Value;
            if (!Alive(hwnd, state)) { windows.Remove(hwnd); continue; }
            if (moving == hwnd || Native.IsIconic(hwnd) || !Native.IsWindowVisible(hwnd)) continue;
            var monitor = Native.MonitorInfo(state.Monitor);
            if (monitor is null) { Release(hwnd); continue; }
            bool needs = Native.IsZoomed(hwnd);
            if (Fullscreen(hwnd, state))
            {
                Native.GetWindowRect(hwnd, out var rect);
                if (!TryGetTarget(state, out var target))
                {
                    Release(hwnd);
                    continue;
                }
                needs |= Math.Abs(rect.Left - target.Left) > 12 || Math.Abs(rect.Top - target.Top) > 12 || Math.Abs(rect.Right - target.Right) > 12 || Math.Abs(rect.Bottom - target.Bottom) > 12;
            }
            if (!needs) continue;
            long now = Environment.TickCount64;
            while (state.Corrections.TryPeek(out long t) && now - t > 2500) state.Corrections.Dequeue();
            if (state.Corrections.Count >= 4)
            {
                Release(hwnd);
                Notify("Bu pencere bölge boyutunu sürekli geri alıyor. Yönetim bırakıldı; F11'i kapatıp yeniden deneyebilirsin.");
                continue;
            }
            state.Corrections.Enqueue(now);
            if (!Place(hwnd, state)) { Release(hwnd); Notify("Pencere bölgeye taşınamadığı için bırakıldı."); }
        }
    }
    void OnMoveEvent(
        nint hookHandle,
        uint evt,
        nint hwnd,
        int objectId,
        int childId,
        uint thread,
        uint time)
    {
        if (closing || mutating || hwnd == 0 || objectId != 0)
            return;

        if (evt == 0xA)
        {
            moving = hwnd;
            return;
        }

        moving = 0;

        if (!working ||
            (!options.SnapOnDrag && !windows.ContainsKey(hwnd)))
            return;

        if ((Native.Style(hwnd) & 0x00040000L) == 0)
            return;

        if (!Native.GetCursorPos(out var cursor))
            return;

        nint destination = Native.MonitorFromPoint(cursor, 2);
        var zones = GetZones(destination);

        for (int zone = 0; zone < zones.Length; zone++)
        {
            var rect = zones[zone];

            if (cursor.X >= rect.Left &&
                cursor.X < rect.Right &&
                cursor.Y >= rect.Top &&
                cursor.Y < rect.Bottom)
            {
                Assign(hwnd, zone, destination);
                return;
            }
        }
    }
    void Release(nint hwnd)
    {
        if (!windows.Remove(hwnd, out var state) || !Alive(hwnd, state)) return;
        mutating = true;
        try
        {
            if (Fullscreen(hwnd, state))
            {
                var m = Native.MonitorInfo(Native.MonitorFromWindow(hwnd, 2));
                if (m is not null) { var r = m.Value.Bounds; Native.SetWindowPos(hwnd, 0, r.Left, r.Top, r.Right-r.Left, r.Bottom-r.Top, 0x14); }
            }
            else { var original = state.Original; Native.SetWindowPlacement(hwnd, ref original); }
        }
        finally { mutating = false; }
    }
    void ReleaseAll() { foreach (var hwnd in windows.Keys.ToArray()) Release(hwnd); }
    void Settings()
    {
        using var form = new Form
        {
            Text = "ScreenQuarters — Settings",
            ClientSize = new Size(620, 440),
            StartPosition = FormStartPosition.CenterScreen,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            AutoScaleMode = AutoScaleMode.Dpi
        };

        var drag = new CheckBox
        {
            Text = "Sürükleyip bırakınca otomatik bölgeye yerleştir",
            Checked = options.SnapOnDrag,
            Location = new Point(20, 20),
            AutoSize = true
        };

        var browser = new CheckBox
        {
            Text = "Tarayıcı tam ekranını bölgede tut (deneysel)",
            Checked = options.BrowserFullscreen,
            Location = new Point(20, 55),
            AutoSize = true
        };

        var excludeLabel = new Label
        {
            Text = "Hariç tutulan işlem adları (.exe olmadan, virgülle ayır):",
            Location = new Point(20, 95),
            AutoSize = true
        };

        var exclude = new TextBox
        {
            Text = options.ExcludedProcesses,
            Location = new Point(20, 120),
            Width = 575
        };

        var monitorLabel = new Label
        {
            Text = "Monitör:",
            Location = new Point(20, 165),
            AutoSize = true
        };

        var monitorBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(20, 190),
            Width = 575
        };

        var presetLabel = new Label
        {
            Text = "Bu monitörün düzeni:",
            Location = new Point(20, 235),
            AutoSize = true
        };

        var presetBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(20, 260),
            Width = 575
        };

        var screens = Screen.AllScreens;
        var presets = Enum.GetValues<LayoutPreset>();

        // Formdaki değişiklikler yalnızca Kaydet'e basınca uygulanır.
        var layouts = new Dictionary<string, LayoutPreset>(
            options.MonitorLayouts ?? new());

        foreach (var screen in screens)
        {
            string primary = screen.Primary ? " — Ana ekran" : "";

            monitorBox.Items.Add(
                $"{screen.DeviceName} — " +
                $"{screen.Bounds.Width} × {screen.Bounds.Height}" +
                primary);
        }

        foreach (var preset in presets)
            presetBox.Items.Add(LayoutPresets.GetName(preset));

        bool loadingPreset = false;

        monitorBox.SelectedIndexChanged += (_, _) =>
        {
            int index = monitorBox.SelectedIndex;
            if (index < 0) return;

            string device = screens[index].DeviceName;

            var selected = layouts.TryGetValue(device, out var saved)
                ? saved
                : LayoutPreset.Grid2x2;

            int presetIndex = Array.IndexOf(presets, selected);

            loadingPreset = true;
            presetBox.SelectedIndex = presetIndex >= 0 ? presetIndex : 0;
            loadingPreset = false;
        };

        presetBox.SelectedIndexChanged += (_, _) =>
        {
            if (loadingPreset ||
                monitorBox.SelectedIndex < 0 ||
                presetBox.SelectedIndex < 0)
                return;

            string device = screens[monitorBox.SelectedIndex].DeviceName;
            layouts[device] = presets[presetBox.SelectedIndex];
        };

        var note = new Label
        {
            Text = "Her monitöre ayrı düzen seçebilirsin.\n" +
                   "Tek ekran modu: Ctrl+Alt+F12.",
            Location = new Point(20, 310),
            Size = new Size(575, 55)
        };

        var save = new Button
        {
            Text = "Kaydet",
            Location = new Point(485, 390),
            Width = 110
        };

        save.Click += (_, _) =>
        {
            var next = new Options
            {
                SnapOnDrag = drag.Checked,
                BrowserFullscreen = browser.Checked,
                ExcludedProcesses = exclude.Text,
                MonitorLayouts = layouts
            };

            try
            {
                Directory.CreateDirectory(
                    Path.GetDirectoryName(optionsPath)!);

                File.WriteAllText(
                    optionsPath + ".tmp",
                    JsonSerializer.Serialize(
                        next,
                        new JsonSerializerOptions { WriteIndented = true }),
                    Encoding.UTF8);

                File.Move(optionsPath + ".tmp", optionsPath, true);

                ReleaseAll();
                options = next;
                form.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ayarlar kaydedilemedi: " + ex.Message);
            }
        };

        form.Controls.AddRange(new Control[]
        {
        drag, browser, excludeLabel, exclude,
        monitorLabel, monitorBox, presetLabel, presetBox,
        note, save
        });

        form.AcceptButton = save;

        if (screens.Length > 0)
            monitorBox.SelectedIndex = 0;

        form.ShowDialog();
    }
    protected override void ExitThreadCore()
    {
        closing = true; timer.Stop();
        if (hook != 0) Native.UnhookWinEvent(hook);
        ReleaseAll();
        foreach (int id in registered) Native.UnregisterHotKey(receiver.Handle, id);
        receiver.DestroyHandle(); timer.Dispose(); tray.Visible = false; tray.ContextMenuStrip?.Dispose(); tray.Dispose();
        base.ExitThreadCore();
    }
}

internal sealed class HotkeyWindow : NativeWindow
{
    public event Action<int>? Pressed;
    public HotkeyWindow() => CreateHandle(new CreateParams { Caption = "ScreenQuarters hotkeys", Parent = new nint(-3) });
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 0x312) Pressed?.Invoke(m.WParam.ToInt32());
        base.WndProc(ref m);
    }
}

internal static class Geometry
{
    public static Native.Rect Zone(Native.Rect work, int zone)
    {
        int midX = work.Left + (work.Right - work.Left) / 2;
        int midY = work.Top + (work.Bottom - work.Top) / 2;
        return new Native.Rect { Left = (zone & 1) == 0 ? work.Left : midX, Right = (zone & 1) == 0 ? midX : work.Right, Top = zone < 2 ? work.Top : midY, Bottom = zone < 2 ? midY : work.Bottom };
    }
}
