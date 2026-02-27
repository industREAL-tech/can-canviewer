using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.TextMate;
using industREAL.CAN.CanViewer.Models;
using industREAL.CAN.CanViewer.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using TextMateSharp.Grammars;
using TextMateSharp.Themes;

namespace industREAL.CAN.CanViewer
{
    public partial class MainWindow : Window
    {
        #region VARIABLES
        /* DataGrid related */
        private bool _wired;      // wired once
        private bool _syncing;    // reentrancy guard

        /* Script editor related */
        private CompletionWindow? completionWindow;
        private TextEditor? _editor;
        private List<string> autocompleteSuggestions;

        private CanDataGridViewModel canRxDataViewModel;
        #endregion

        #region CONSTRUCTOR
        public MainWindow()
        {
            InitializeComponent();

            var app = Application.Current;
            app.RequestedThemeVariant = ThemeVariant.Dark;

            canRxDataViewModel        = new CanDataGridViewModel();
            DataContext               = canRxDataViewModel;
            _editor                   = this.FindControl<TextEditor>("Editor");

            this.Opened += (_, __) =>
            {
                EnsureGridsAndSetup();

                //#region ScriptEditorRelated
                //if (_editor == null)
                //    return;

                //var options = new TextMateSharp.Grammars.RegistryOptions(ThemeName.SolarizedDark);
                //var tm = _editor.InstallTextMate(options);

                //// Load theme (IRawTheme) and apply it
                //IRawTheme rawTheme = options.LoadTheme(ThemeName.SolarizedDark);
                //tm.SetTheme(rawTheme); // returns ITheme

                //// Set a grammar that exists
                //var scope = options.GetScopeByLanguageId("javascript")
                //           ?? options.GetScopeByLanguageId("csharp")
                //           ?? options.GetScopeByLanguageId("json");
                //if (scope is not null)
                //    tm.SetGrammar(scope);

                ////(Optional)ensure readability regardless of theme
                //_editor.Background = Avalonia.Media.Brushes.Transparent; // or a solid dark brush
                //_editor.Foreground = Avalonia.Media.Brushes.White;
                ////_editor.CaretBrush = Avalonia.Media.Brushes.White;

                //// Test text
                //_editor.Text = "// Hello TextMate + AvaloniaEdit\nfunction foo() { return 42; }\n";

                ////autocompleteSuggestions = engine.GetAllAutoCompleteList();
                //autocompleteSuggestions = new List<string>();
                //autocompleteSuggestions.Add("Kutya");
                //autocompleteSuggestions.Add("Cica");

                //// Then add text
                //_editor.Text = "//This is the entry function\r\nfunction " + "(){\r\n\n}\r\n\n\n\n//Generated Events\r\n";

                //_editor.TextArea.KeyDown += (_, e) =>
                //{
                //    if (e.Key == Avalonia.Input.Key.Space && e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Control))
                //    {
                //        ShowCompletion();
                //        e.Handled = true;
                //    }
                //};

                //_editor.TextArea.TextEntering += (_, e) =>
                //{
                //    if (completionWindow == null)
                //        return;

                //    if (e.Text.Length > 0 && !char.IsLetterOrDigit(e.Text[0]))
                //        completionWindow.CompletionList.RequestInsertion(e);
                //};

                //#endregion ScriptEditorRelated
            };

            // If your grids live inside a tab, also watch tab switches
            // (Requires: <TabControl x:Name="MainTabs" ...> in XAML)
            if (this.FindControl<TabControl>("MainTabs") is { } tabs)
                tabs.SelectionChanged += (_, __) => EnsureGridsAndSetup();
        }
        #endregion

        //private TextEditor? _editor;
        //private ITextMate? _tm;

        private void Editor_OnAttached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            var _eeditor = (TextEditor)sender!;



            // TextMate wiring
            var options = new TextMateSharp.Grammars.RegistryOptions(ThemeName.DarkPlus);
            var _tm = _eeditor.InstallTextMate(options);

            IRawTheme rawTheme = options.LoadTheme(ThemeName.DarkPlus);
            _tm.SetTheme(rawTheme);

            var scope = options.GetScopeByLanguageId("javascript");
            if (scope is not null)
                _tm.SetGrammar(scope);

            // Sample text
            _editor.Text = "// Hello from TextMate + AvaloniaEdit\nfunction foo() { return 42; }\n";
        }

        #region PRIVATE_METHODS

        private void ShowCompletion()
        {
            completionWindow = new CompletionWindow(_editor.TextArea);
            var data = completionWindow.CompletionList.CompletionData;

            foreach (var suggestion in autocompleteSuggestions)
                data.Add(new CompletionData(suggestion));

            completionWindow.Show();
            completionWindow.Closed += (_, _) => completionWindow = null;
        }

        private void EnsureGridsAndSetup()
        {
            var canRxGrid = this.FindControl<DataGrid>("CanRxGrid"); // MASTER
            var canTxGrid = this.FindControl<DataGrid>("CanTxGrid"); // FOLLOWER

            if (canRxGrid is null || canTxGrid is null)
            {
                // Tab content may not be realized yet—retry next UI tick
                Dispatcher.UIThread.Post(EnsureGridsAndSetup, DispatcherPriority.Background);
                return;
            }

            if (_wired) return;
            _wired = true;

            // Initial sync AFTER first render so ActualWidth is valid
            Dispatcher.UIThread.Post(() => CopyActualWidths(canRxGrid, canTxGrid), DispatcherPriority.Render);

            // Hook master changes ONLY (avoid bi-directional ping-pong)
            HookMasterColumnChanges(canRxGrid, canTxGrid);

            // Optional: when the master grid overall size changes (window/splitter),
            // resync all follower column widths to current ActualWidth snapshot.
            canRxGrid.PropertyChanged += (_, e) =>
            {
                if (e.Property == BoundsProperty) // overall size change
                {
                    Dispatcher.UIThread.Post(() => CopyActualWidths(canRxGrid, canTxGrid), DispatcherPriority.Render);
                }
            };
        }

        /// <summary>
        /// Mirrors master column width changes (header drag or layout) to follower as pixel widths.
        /// </summary>
        private void HookMasterColumnChanges(DataGrid master, DataGrid follower)
        {
            int n = Math.Min(master.Columns.Count, follower.Columns.Count);

            for (int i = 0; i < n; i++)
            {
                var mCol = master.Columns[i];
                var fCol = follower.Columns[i];

                // React to width changes only
                mCol.PropertyChanged += (_, e) =>
                {
                    if (_syncing) return;

                    if (e.Property == DataGridColumn.WidthProperty)
                    {
                        _syncing = true;
                        try
                        {
                            double px = mCol.ActualWidth;
                            const double eps = 0.5; // ignore tiny jitter
                            if (Math.Abs(fCol.ActualWidth - px) > eps)
                            {
                                // Lock follower to exact pixel width to avoid reflow feedback
                                fCol.Width = new DataGridLength(px, DataGridLengthUnitType.Pixel);
                            }
                        }
                        finally { _syncing = false; }
                    }
                };
            }
        }

        /// <summary>
        /// One-shot copy: master ActualWidth -> follower pixel width.
        /// Call this after render or on master size changes.
        /// </summary>
        private static void CopyActualWidths(DataGrid from, DataGrid to)
        {
            int n = Math.Min(from.Columns.Count, to.Columns.Count);
            for (int i = 0; i < n; i++)
            {
                var src = from.Columns[i];
                var dst = to.Columns[i];
                dst.Width = new DataGridLength(src.ActualWidth, DataGridLengthUnitType.Pixel);
            }
        }
        #endregion
    }
}
