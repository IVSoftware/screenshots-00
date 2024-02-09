# Screenshot loop

There are many, many ways to go about this so this is just how I do this kind of thing in my own app. Let's say that at the heart of this we have a `ScreenshotProviderForm` and to demonstrate we'll make a periodic timer to update elapsed time. Clicking on `ScreenshotProviderForm` a.k.a. "child form" starts and stops the timer. If you [Control + click] it captures a screenshot of itself, saves it to a file, and opens the default editor (e.g. MS Paint) and _does not return from the async method _until MS Paint closes_. Nevertheless, while Paint remains open, the timer continues to run and you can even take another screenshot which will open in a new instance of paint and won't return until _that_ instance of paint closes.

___

Now, since it's a "provider" of something, we need an interface, and here is where I feel that a decent way of doing this is to implement [ICommand](https://learn.microsoft.com/en-us/dotnet/api/system.windows.input.icommand?view=net-8.0#definition). ICommand.Execute(context) is not going to be awaitable, but we'll make is that the context argument that we pass _is_. You can [browse the full example]() but basically the screenshot server will do what you ask it to do and release your context when it's done.

**Server**

```
public async void Execute(object? o)
{
    if( o is ScreenshotCommandContext contextSS)
    {
        // Capture the screenshot and release the context when finished.
        TakeScreenshot(contextSS);
    }
}

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
```

**Client**
```csharp
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
```

Strictly for demo purposes:
- When the [Single] button is clicked, the main form remains responsive while the provider captures a screenshot, opens the editor, and waits for user to close the editor. When that happens, main form will load the captured PNG file and do some long-running processing on it denoted by a progress bar.

- When the Auto option is active, do this on a loop, and of course in that context we would not want to open Paint and wait for it, however we still want to do long-running processing before requesting the next screenshot.

___

So to directly answer the question

>Call an async method (within a child form)...

here's is one approach:

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
