
namespace screenshots_00
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Disposed += (sender, e) => SnapshotProviderForm.Dispose();
            button1.Click += (sender, e) => SnapshotProviderForm.Execute(new SnapshotCommandContext());
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SnapshotProviderForm.Show(this);
        }
        SnapshotProviderForm SnapshotProviderForm { get; } = new SnapshotProviderForm();
    }
    class SnapshotCommandContext
    {
        public bool OpenEditor { get; set; }
    }
}
