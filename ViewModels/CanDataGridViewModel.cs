using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using industREAL.CAN.CORE;
using industREAL.CAN.CanViewer.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace industREAL.CAN.CanViewer.ViewModels
{
    public partial class CanDataGridViewModel : ObservableObject
    {
        // Items for both DataGrids
        public ObservableCollection<CanFrameModel> Frames { get; }

        // Background/grid flyout commands (used in GridContextFlyout)
        public ICommand AddNewCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        // Row flyout commands (used in RowContextFlyout)
        public ICommand AddCommand { get; }                 // "Standard _Menu Item"
        public IRelayCommand<CanFrameModel> Submenu1Command { get; }
        public IRelayCommand<CanFrameModel> Submenu2Command { get; }

        public UIStateViewModel Status { get; } = new();

        private uint _nextFrameId = 3;

        public CanDataGridViewModel()
        {
            Frames = Seed();

            // Grid/background
            AddNewCommand = new RelayCommand(AddNew);
            PasteCommand = new RelayCommand(PasteFromClipboard);
            OpenSettingsCommand = new RelayCommand(OpenSettings);

            // Row menu
            AddCommand = new RelayCommand<object?>(OnStandardRow); // receives CommandParameter (row)
            Submenu1Command = new RelayCommand<CanFrameModel>(OnSubmenu1);
            Submenu2Command = new RelayCommand<CanFrameModel>(OnSubmenu2);
        }

        private static ObservableCollection<CanFrameModel> Seed()
        {
            var a = new CanFrame(CanDLC.DLC_8)
            {
                _canID = 0x18FF50E5,
                _flags = CanFlags.ExtendedId | CanFlags.BrsOn | CanFlags.FdCanFormat,
                _frameID = 1,
                _channel = CanInterface.A
            };
            Array.Copy(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF }, a._data, 8);

            var b = new CanFrame(CanDLC.DLC_4)
            {
                _canID = 0x123,
                _flags = CanFlags.None,
                _frameID = 2,
                _channel = CanInterface.B
            };
            Array.Copy(new byte[] { 0x10, 0x20, 0x30, 0x40 }, b._data, 4);

            var am = new CanFrameModel(a);
            var ab = new CanFrameModel(b);

            return new ObservableCollection<CanFrameModel> { am, ab };
        }

        // === Grid flyout handlers ===
        private void AddNew()
        {
            var f = new CanFrame(CanDLC.DLC_8)
            {
                _canID = 0x700,
                _flags = CanFlags.None,
                _frameID = _nextFrameId++,
                _channel = CanInterface.A
            };
            Frames.Add(new CanFrameModel(f));
        }

        private void PasteFromClipboard()
        {
            // Hook real clipboard later. Demo item:
            var f = new CanFrame(CanDLC.DLC_4)
            {
                _canID = 0x321,
                _flags = CanFlags.ExtendedId,
                _frameID = _nextFrameId++,
                _channel = CanInterface.B
            };
            Array.Copy(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, f._data, 4);
            Frames.Add(new CanFrameModel(f));
        }

        private void OpenSettings()
        {
            // TODO: open a dialog / set a state
        }

        // === Row flyout handlers ===
        private void OnStandardRow(object? param)
        {
            var row = param as CanFrameModel;
            if (row is null) return;
            row.canFrame._frameID = _nextFrameId++;    // simple visible effect
        }

        private void OnSubmenu1(CanFrameModel? row)
        {
            if (row is null) return;
            // Example: trim to 4 bytes if longer
            if ((byte)row.canFrame._dataLength > 4)
            {
                var tmp = new byte[4];
                Array.Copy(row.canFrame._data, tmp, 4);
                row.canFrame._dataLength = CanDLC.DLC_4;
                Array.Copy(tmp, row.canFrame._data, 4);
            }
        }

        private void OnSubmenu2(CanFrameModel? row)
        {
            if (row is null) return;
            // Example: set FDCAN + BRS flags
            row.canFrame._flags |= (CanFlags.FdCanFormat | CanFlags.BrsOn);
        }
    }
}
