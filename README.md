# Screenshot loop

There are many, many ways to go about this but when I want to do this kind of thing in my own app I find that implementing [ICommand](https://learn.microsoft.com/en-us/dotnet/api/system.windows.input.icommand?view=net-8.0#definition) in the child window that is the screenshot/service provider is a decent way of going about it. 

```
public partial class SnapshotProviderForm : Form, ICommand
{
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) { return true; }
    public void Execute(object? parameter) { ...}
}
```
___

The `ICommand.Execute(context)` method is _not_ going to be awaitable, but the context argument that gets passed into it can be. 

**Context passed as argument to ICommand.Execute**

```csharpclass AsyncCommandContext
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
enum TimerCommandMode { Toggle, Start, Stop, Restart }
class TimerCommandContext: AsyncCommandContext
{
    public TimerCommandMode TimerCommandMode { get; set; }
}

class ScreenshotCommandContext : AsyncCommandContext
{ 
    public bool OpenEditor { get; set; }
    public string? Path { get; internal set; }
}
```
___

You can [browse the full example](https://github.com/IVSoftware/screenshots-00.git) but basically the screenshot server will do what you ask it to do and release your context when it's done. To prove this out, the screenshot provider will be this top-level borderless form:
___

[![child window][1]][1]

___

The stand-alone behavior of this child form is to toggle a stopwatch when clicked, and when it's Control-clicked to capture a single screenshot and display it in a new instance of MS Paint and not return until MS Paint is closed by the user.  When the main form requests this action below, it means that if the user makes changes in Paint they will be reflected in the file that the main for subjects to the long-running processing.

___

**So for the part of the question**

>Call an async method (within a child form)...

here's is what that could look like on the client (Main Form) side:

```csharp
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
```

**and for the part of the question**

>from a thread from the main form

```csharp
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
```
___

**[Single] button test**

This shows two screen captures open in paint after clicking the [Single] button on the main form twice. MainForm continues to be responsive (and the stopwatch continues to increment) while waiting for the user to finish the edits and close the Paint window and when that happens it will process the resulting file and add it to the `FlowLayoutPanel` on the main form. 



[![screenshot][2]][2]

___

**[Auto} Loop Test**

Obviously, when the [Auto] loop is executing on the client side, we skip the part about opening Paint and waiting for user. Here we're just taking periodic screenshots and adding them to `FlowLayoutPanel after processing with long-running-task denoted by `ProgressBar`.

[![auto][3]][3]


  [1]: https://i.stack.imgur.com/Fwk9r.png
  [2]: https://i.stack.imgur.com/WnJ0P.png
  [3]: https://i.stack.imgur.com/fRqoH.png