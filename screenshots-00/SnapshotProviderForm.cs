using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
            labelElapsedTime.Click += async(sender, e) =>
            {
                if (ModifierKeys == Keys.Control) await TakeSnapshotAsync();
                else Execute(null);
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
        public async Task<string> TakeSnapshotAsync(bool openEditor = true)
        {
            using (Bitmap bmp = new Bitmap(Width, Height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(Location, new(), Size);
                }
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                Directory.CreateDirectory(folder);
                var path = Path.Combine(folder, $"Image{_count++:D2}.png");
                await Task.Run(()=>bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png));
                if (Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = path }) is Process process)
                {
                    await process.WaitForExitAsync();
                }
                return path;
            }
        }

        public bool CanExecute(object? parameter) => true;

        public async void Execute(object? parameter)
        {
            if(_stopwatch.IsRunning)
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
}
