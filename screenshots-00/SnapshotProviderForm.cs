using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace screenshots_00
{

    public partial class SnapshotProviderForm : Form, ICommand
    {
        public SnapshotProviderForm()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            labelElapsedTime.Click += (sender, e) =>
            {
                // Control-click to take screenshot and open in editor.
                if (ModifierKeys == Keys.Control)
                {
                    _ = takeScreenshot(new ScreenshotCommandContext { OpenEditor = true });
                }
                else Execute(new TimerCommandContext());
            };
        }
        
        int _count = 0;

        /// <summary>
        ///  Capture Snapshot of form while elapsed time continues to run.
        /// </summary>
        /// <param name="openEditor"
        /// Show the screenshot in the default PNG editor e.g. Paint and wait for editor to close before returning.
        /// </param>
        /// <returns>
        /// The name of the saved file.
        /// </returns>
        private async Task takeScreenshot(ScreenshotCommandContext context)
        {
            using (Bitmap bmp = new Bitmap(Width, Height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(Location, new(), Size);
                }
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                Directory.CreateDirectory(folder);
                context.Path = Path.Combine(folder, $"Image{_count++:D2}.png");

                await Task.Run(()=>bmp.Save(context.Path, System.Drawing.Imaging.ImageFormat.Png));
                if (context.OpenEditor)
                {
                    if ( Process.Start(new ProcessStartInfo 
                    { 
                        UseShellExecute = true,
                        FileName = "mspaint.exe", 
                        Arguments = context.Path })
                    is Process process)
                    {
                        await process.WaitForExitAsync();
                    }
                }
            }
        }

        public bool CanExecute(object? o) => true;

        public async void Execute(object? o)
        {
            try
            {
                if (o is TimerCommandContext contextTT)
                {
                    switch (contextTT.TimerCommandMode)
                    {
                        case TimerCommandMode.Toggle:
                            if (_stopwatch.IsRunning) Execute(new TimerCommandContext { TimerCommandMode = TimerCommandMode.Stop });
                            else Execute(new TimerCommandContext { TimerCommandMode = TimerCommandMode.Start });
                            break;
                        case TimerCommandMode.Start:
                            if (!_stopwatch.IsRunning) _pollingTask = runPeriodicTimer();
                            break;
                        case TimerCommandMode.Stop:
                        case TimerCommandMode.Restart:
                            if (_cts is CancellationTokenSource cts && _pollingTask is Task task)
                            {
                                cts?.Cancel();
                                await task;
                                task.Dispose();
                            }
                            if (!_stopwatch.IsRunning) _pollingTask = runPeriodicTimer();
                            break;
                        default:
                            break;
                    }

                    if (contextTT.TimerCommandMode == TimerCommandMode.Toggle)
                    {
                        contextTT.TimerCommandMode =
                            _stopwatch.IsRunning ?
                                TimerCommandMode.Stop :
                                TimerCommandMode.Start;
                    }
                    switch (contextTT.TimerCommandMode)
                    {
                        case TimerCommandMode.Start:
                            break;
                        case TimerCommandMode.Stop:
                            break;
                    }
                }
                else if (o is ScreenshotCommandContext contextSS)
                {
                    await takeScreenshot(contextSS);
                }
            }
            finally
            {
                if (o is AsyncCommandContext contextAsync) contextAsync.Release();
            }
        }

        public new void Show(IWin32Window owner) 
        {
            if(owner is Form form) Location = new Point(form.Right + form.Padding.Right, form.Top);
            base.Show(owner);
            Execute(null);
        }
        
        Task? _pollingTask = null;
        private Stopwatch _stopwatch = new Stopwatch();
        CancellationTokenSource? _cts = null;

        public event EventHandler? CanExecuteChanged;
        private async Task runPeriodicTimer()
        {
            if (!_stopwatch.IsRunning)
            {
                try
                {
                    if (_cts != null)
                    {
                        _cts.Cancel();
                    }
                    _cts = new CancellationTokenSource();
                    var token = _cts.Token;
                    _stopwatch.Restart();
                    while (!token.IsCancellationRequested)
                    {
                        var elapsed = _stopwatch.Elapsed;
                            labelElapsedTime.Text =
                                elapsed < TimeSpan.FromSeconds(1) ?
                                elapsed.ToString(@"\0\0\:ffff") :
                                elapsed.ToString(@"hh\:mm\:ss");
                        await Task.Delay(TimeSpan.FromSeconds(0.1), token);
                    }
                }
                catch { }
                finally
                {
                    _stopwatch.Stop();
                }
            }
        }
    }
    class AsyncCommandContext
    {
        private SemaphoreSlim _busy { get; } = new SemaphoreSlim(0, 1);
        public TaskAwaiter GetAwaiter()
        {
            return _busy
            .WaitAsync()        // Do not use the Token here
            .GetAwaiter();
        }
        public ConfiguredTaskAwaitable ConfigureAwait(bool configureAwait)
        {
            return
                _busy
                .WaitAsync()    // Do not use the Token here, either.
                .ConfigureAwait(configureAwait);
        }
        public void Release()
        {
            // Make sure there's something to release.
            _busy.Wait(0);
            _busy.Release();
        }
    }
    enum TimerCommandMode { Toggle, Start, Stop,
        Restart
    }
    class TimerCommandContext: AsyncCommandContext
    {
        public TimerCommandMode TimerCommandMode { get; set; }
    }

    class ScreenshotCommandContext : AsyncCommandContext
    { 
        public bool OpenEditor { get; set; }
        public string? Path { get; internal set; }
    }
}
