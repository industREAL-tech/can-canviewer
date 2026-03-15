using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace industREAL.CAN.CanViewer.Models
{
    public class DeviceTreeNode : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string CombinedName => string.IsNullOrEmpty(_combinedName) ? Name : _combinedName;
        private string? _combinedName;

        public Type type { get; set; }

        public bool isClass { get; set; }
        public ObservableCollection<DeviceTreeNode> Children { get; set; } = new();

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public DeviceTreeNode(string name)
        {
            Name = name;
        }

        public void SetCombined(string display)
        {
            _combinedName = display;
        }

        public DeviceTreeNode Clone()
        {
            var clone = new DeviceTreeNode(Name)
            {
                isClass = this.isClass,
                type = this.type
            };
            clone._combinedName = this._combinedName;
            return clone;
        }

        public static ObservableCollection<DeviceTreeNode> BuildTree(IEnumerable<string> paths)
        {
            var rootNodes = new ObservableCollection<DeviceTreeNode>();

            foreach (var path in paths)
            {
                var parts = path.Split('.');
                ObservableCollection<DeviceTreeNode> currentLevel = rootNodes;

                foreach (var part in parts)
                {
                    var existing = currentLevel.FirstOrDefault(n => n.Name == part);
                    if (existing == null)
                    {
                        existing = new DeviceTreeNode(part);
                        currentLevel.Add(existing);
                    }
                    currentLevel = existing.Children;
                }
            }

            return rootNodes;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
