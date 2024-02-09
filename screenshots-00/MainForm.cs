

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
                SnapshotProviderForm.Execute(context);
                await context;
                await ProcessFile(context.Path);
            };
        }

        private async Task ProcessFile(string fullPath)
        {
            await Task.Run(() =>
            {
                using (var orig = Bitmap.FromFile(fullPath))
                {
                    using (var scaled = new Bitmap(orig.Width / 4, orig.Height /4))
                    using (Graphics graphics = Graphics.FromImage(scaled))
                    {
                        graphics.DrawImage(orig, 0, 0, scaled.Width, scaled.Height);
                    }
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
}
