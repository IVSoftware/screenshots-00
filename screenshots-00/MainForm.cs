using System;
using System.Windows.Forms;

namespace screenshots_00
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Disposed += (sender, e) => SnapshotProviderForm.Dispose();
            buttonSingle.Click += async (sender, e) =>
            {
                var context = new ScreenshotCommandContext{ OpenEditor = true };
                SnapshotProviderForm.Execute(context);  // ICommand does not block and is not async.
                await context;                          // The task is awaited by virtue of the context awaiter.
                if (context.Path is string path && File.Exists(path))
                {                    
                    await ProcessFile(path);            // Now 'we' hold the lock on the context.
                }
                context.Release();                      // Release context for any 'other' awaiters of this context.
            };
            checkBoxAuto.CheckedChanged += async (sender, e) =>
            {
                if (checkBoxAuto.Checked) 
                {
                    var restartContext = new TimerCommandContext { TimerCommandMode = TimerCommandMode.Restart };
                    SnapshotProviderForm.Execute(restartContext);
                    await restartContext;
                    flowLayoutPanel.Controls.Clear();
                    while (checkBoxAuto.Checked)
                    {
                        var context = new ScreenshotCommandContext { OpenEditor = false }; // Different
                        SnapshotProviderForm.Execute(context);
                        await context;
                        if(context.Path is string path && File.Exists(path))
                        {
                            await ProcessFile(path);
                        }
                        context.Release();
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }
            };
            SnapshotProviderForm.Execute(new TimerCommandContext { TimerCommandMode = TimerCommandMode.Start });
        }

        private async Task ProcessFile(string fullPath)
        {
            await Task.Run(() =>
            {
                Bitmap scaled;
                using (var orig = Bitmap.FromFile(fullPath))
                {
                    scaled = new Bitmap(orig.Width / 4, orig.Height / 4);
                    using (Graphics graphics = Graphics.FromImage(scaled))
                    {
                        graphics.DrawImage(orig, 0, 0, scaled.Width, scaled.Height);
                        var pictureBox = new PictureBox
                        {
                            Size = scaled.Size,
                            Image = scaled,
                        };
                        BeginInvoke(() =>
                        {
                            flowLayoutPanel.Controls.Add(pictureBox);
                        });
                    }
                }
                // Process long running task.
                for (int i = 1; i <= 100; i++)
                {
                    BeginInvoke(() =>
                    {
                        progressBar.SetProgress(i);
                    });
                    Thread.Sleep(10); // Block this non-ui thread.
                }
            });
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SnapshotProviderForm.Show(this);
        }
        SnapshotProviderForm SnapshotProviderForm { get; } = new SnapshotProviderForm();
    }
    static partial class Extensions
    {
        public static void SetProgress(this ProgressBar progressBar, int value) 
        {
            progressBar.Value = value;
            progressBar.Visible = value != 0 && Math.Ceiling((double)value) < 100;
        }
    }
}
