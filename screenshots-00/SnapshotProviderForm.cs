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
                    TakeScreenshot(new ScreenshotCommandContext { OpenEditor = true });
                }
                else Execute(new ToggleTimerCommandContext());
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
        private async void TakeScreenshot(ScreenshotCommandContext context)
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
                    if (Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = context.Path }) is Process process)
                    {
                        if (InvokeRequired)
                        { }
                        await process.WaitForExitAsync();
                    }
                }
                context.Release();
            }
        }

        public bool CanExecute(object? o) => true;

        public async void Execute(object? o)
        {
            if (o is ToggleTimerCommandContext contextTT)
            {
                if (_stopwatch.IsRunning)
                {
                    if (_cts is CancellationTokenSource cts && _pollingTask is Task task)
                    {
                        cts?.Cancel();
                        await task;
                        task.Dispose();
                    }
                }
                else
                {
                    _pollingTask = runPeriodicTimer();
                }
            }
            else if(o is ScreenshotCommandContext contextSS)
            {
                TakeScreenshot(contextSS);
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
    class ToggleTimerCommandContext: AsyncCommandContext {  }
    class ScreenshotCommandContext : AsyncCommandContext { public bool OpenEditor { get; set; }
        public string Path { get; internal set; }
    }
}
