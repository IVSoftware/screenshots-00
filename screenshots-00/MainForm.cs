

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
                await ProcessFile(context.Path);        // Now we have a lock on the context.
                context.Release();                      // Release context for any 'other' awaiters of this context.
            };
            checkBoxAuto.CheckedChanged += async (sender, e) =>
            {
                if(checkBoxAuto.Checked) 
                {
                    flowLayoutPanel.Controls.Clear();
                    while (checkBoxAuto.Checked)
                    {
                        var context = new ScreenshotCommandContext { OpenEditor = false }; // Different
                        SnapshotProviderForm.Execute(context);
                        await context;
                        await ProcessFile(context.Path);
                        context.Release();
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }
            };
        }

        private async Task ProcessFile(string fullPath)
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
            await Task.Run(() =>
            {
            });
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SnapshotProviderForm.Show(this);
        }
        SnapshotProviderForm SnapshotProviderForm { get; } = new SnapshotProviderForm();
    }
}
