
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
            };
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SnapshotProviderForm.Show(this);
        }
        SnapshotProviderForm SnapshotProviderForm { get; } = new SnapshotProviderForm();
    }
}
