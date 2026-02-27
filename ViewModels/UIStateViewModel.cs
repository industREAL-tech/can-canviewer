using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using industREAL.CAN.CanViewer.Views;
using System.Threading.Tasks;

namespace industREAL.CAN.CanViewer.ViewModels
{
    public partial class UIStateViewModel : ObservableObject
    {
        private DeviceSelectorWindow? _dialog;
        private bool _dialogOpen;


        [ObservableProperty]
        private bool connected;

        [ObservableProperty]
        private string statusText = "Ready";

        [ObservableProperty]
        private int rxCount;

        [ObservableProperty]
        private int txCount;

        [ObservableProperty]
        private double updateRateHz;

        public string ConnectionLabel => Connected ? "Connected" : "Disconnected";

        // === Commands for the toolbar buttons ===
        public IRelayCommand ConnectCommand { get; }
        public IRelayCommand DisconnectCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand SaveCommand { get; }

        public UIStateViewModel()
        {
            ConnectCommand = new RelayCommand(OnConnect);
            DisconnectCommand = new RelayCommand(OnDisconnect);
            RefreshCommand = new RelayCommand(OnRefresh);
            SaveCommand = new RelayCommand(OnSave);
        }

        private void OnConnect()
        {
            Connected = true;
            StatusText = "Connected to CAN gateway.";
        }

        private void OnDisconnect()
        {
            Connected = false;
            StatusText = "Disconnected.";
        }

        private void OnRefresh()
        {
            StatusText = "Refreshing data…";
            RxCount += 1;
        }

        private void OnSave()
        {
            StatusText = "Configuration saved.";
        }


        [RelayCommand]
        private async Task OpenDeviceSelectorDialog(Window owner)
        {
            if (owner is null) return;

            // If a dialog is already open, just bring it front
            if (_dialogOpen)
            {
                _dialog?.Activate();
                return;
            }

            _dialogOpen = true;
            _dialog = new DeviceSelectorWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            // When it closes, clear state
            _dialog.Closed += (_, __) =>
            {
                _dialogOpen = false;
                _dialog = null;
            };

            // Modal: blocks interaction with owner until the dialog closes
            await _dialog.ShowDialog(owner);

            // Safety (in case Closed wasn’t fired for some reason)
            _dialogOpen = false;
            _dialog = null;
        }
    }
}
