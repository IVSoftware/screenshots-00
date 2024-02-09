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
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            labelElapsedTime.Click += (sender, e) =>
            {
                if (ModifierKeys == Keys.Control) _ = TakeSnapshotAsync();
                else Execute(null);
            };
        }

        private async Task TakeSnapshotAsync()
        {
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
