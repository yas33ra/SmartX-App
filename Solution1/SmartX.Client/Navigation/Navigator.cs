// Owns one instance of each screen, so moving between them never loses state
namespace SmartX.Client.Navigation
{
    public static class Navigator
    {
        private static readonly Dictionary<AppModule, Form> _forms = new();
        private static Form? _current;

        // Called once from Program.cs: opens the menu and runs the app
        public static void Start()
        {
            var menu = GetOrCreate(AppModule.MainMenu);
            _current = menu;
            Application.Run(menu);
        }

        public static void NavigateTo(AppModule module)
        {
            var target = GetOrCreate(module);

            if (_current == target)
            {
                return;
            }

            if (_current is not null)
            {
                // open the next screen where the last one was, at the same size
                target.StartPosition = FormStartPosition.Manual;
                target.WindowState = _current.WindowState;

                if (_current.WindowState == FormWindowState.Normal)
                {
                    target.Location = _current.Location;
                    target.Size = _current.Size;
                }
            }

            target.Show();
            target.Activate();
            _current?.Hide();
            _current = target;
        }

        private static Form GetOrCreate(AppModule module)
        {
            if (_forms.TryGetValue(module, out var existing) && !existing.IsDisposed)
            {
                return existing;
            }

            Form form = module switch
            {
                AppModule.MainMenu => new MainMenuForm(),
                AppModule.Ingestion => new IngestionForm(),
                AppModule.CommandStream => new CommandStreamForm(),
                _ => throw new ArgumentOutOfRangeException(nameof(module))
            };

            // The X button on any screen closes the whole app, so nothing stays hidden in memory
            form.FormClosed += (s, e) => Application.Exit();

            _forms[module] = form;
            return form;
        }
    }
}