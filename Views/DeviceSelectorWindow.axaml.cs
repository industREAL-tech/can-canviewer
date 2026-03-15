using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using industREAL.CAN.CanViewer.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace industREAL.CAN.CanViewer.Views;

public partial class DeviceSelectorWindow : Window
{

    private Point _dragStart;
    private DeviceTreeNode rootDevicesNode;

    public DeviceSelectorWindow()
    {
        InitializeComponent();

        this.Opened += (_, _) =>
        {
            var container = this.FindControl<Grid>("DeviceTreeContainer");

            var availableDevicesNode = new DeviceTreeNode("Available Devices");
            var possibleDevicesNode = new DeviceTreeNode("Other devices");

            var root = new ObservableCollection<DeviceTreeNode>
                {
                    availableDevicesNode,
                    possibleDevicesNode
                };

            var treeView = new TreeView
            {
                Background = new SolidColorBrush(Color.Parse("#1e1e1e")),
                BorderBrush = null,
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Cascadia Code, Consolas, Menlo, Monospace"),
                ItemsSource = root
            };


            treeView.ItemTemplate = new FuncTreeDataTemplate<DeviceTreeNode>(
                _ => true,
                (node, _) =>
                {
                    var stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    if (node.isClass)
                    {
                        var icon = new TextBlock
                        {
                            Text = "🔌",
                            FontSize = 16,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        stackPanel.Children.Add(icon);
                    }

                    var nameText = new TextBlock
                    {
                        [!TextBlock.TextProperty] = new Binding("CombinedName"),
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = Brushes.White
                    };

                    stackPanel.Children.Add(nameText);

                    var border = new Border
                    {
                        Padding = new Thickness(10, 4, 10, 4),
                        Background = Brushes.Transparent,
                        Child = stackPanel
                    };

                    DragDrop.SetAllowDrop(border, true);
                    border.AddHandler(DragDrop.DropEvent, async (s, e) => await OnDrop(s, e), RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
                    border.AddHandler(DragDrop.DragOverEvent, OnDragOver, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);

                    // Attach context menu to each node under rootDevicesNode
                    if (availableDevicesNode.Children.Contains(node))
                    {
                        var contextMenu = new ContextMenu
                        {
                            ItemsSource = new[]
                            {
                                    new MenuItem
                                    {
                                        Header = "Configuration",
                                        IsEnabled = true,// laborEngine.isDeviceHasConfigurationWindow(node),
                                        Command = new SimpleCommand(() =>
                                        {
                                            //Console.WriteLine($"⚙️ Open configuration for: {node.Name}");
                                            //var content = \\;// laborEngine.GetConfigurationWindow(node);
                                            //Show window
                                            //TODO: use -> content.DesiredSize
                                            //if(content != null) {
                                            //    var window = new Window
                                            //    {
                                            //        Title = $"Configuration - {node.Name}",
                                            //        Width = 600,
                                            //        Height = 400,
                                            //        Content = content,
                                            //        Icon = this.Icon,
                                            //        WindowStartupLocation = WindowStartupLocation.CenterOwner
                                            //    };
                                            //    window.ShowDialog(this); //await window.ShowDialog(this);
                                            //}
                                            //else
                                            //{
                                            //    Log.Warning("The device does not have a proper configuration form!");
                                            //}


                                        })
                                    },
                                    new MenuItem
                                    {
                                        Header = "Device specific",
                                        IsEnabled = true ,//laborEngine.isDeviceHasDeviceSpecificWindow(node),
                                        Command = new SimpleCommand(() =>
                                        {
                                            Console.WriteLine($"🔧 Device-specific logic for: {node.Name}");
                                            // Add device-specific logic here
                                        })
                                    },
                                    new MenuItem
                                    {
                                        Header = "Documentation",
                                        Command = new SimpleCommand(() =>
                                        {
                                            Console.WriteLine($"📄 Documentation for: {node.Name}");
                                            // Add logic to open documentation
                                        })
                                    },
                                    new MenuItem
                                    {
                                        Header = "Remove",
                                        Command = new SimpleCommand(() =>
                                        {
                                            //Log.Information($"Device removed in progress: {node.CombinedName}");
                                            rootDevicesNode.Children.Remove(node);
                                            //laborEngine.testDevices.Remove(node);
                                            //laborEngine.RemoveInstance(node);
                                            //Log.Information($"Device removed: {node.CombinedName}");
                                        })
                                    }
                            }
                        };

                        border.ContextMenu = contextMenu;
                    }

                    return border;
                },
                node => node.Children
            );

            treeView.PointerPressed += (sender, e) =>
            {
                _dragStart = e.GetPosition(treeView);
            };

            treeView.PointerMoved += async (sender, e) =>
            {
                var current = e.GetPosition(treeView);
                var dx = current.X - _dragStart.X;
                var dy = current.Y - _dragStart.Y;
                var distance = Math.Sqrt(dx * dx + dy * dy);

                var point = e.GetCurrentPoint(treeView);
                if (distance > 5 && point.Properties.IsLeftButtonPressed == true)
                {
                    if (e.Source is Control control && control.DataContext is DeviceTreeNode draggedNode && draggedNode.isClass)
                    {
                        var dragData = new DataObject();
                        dragData.Set("TreeNode", draggedNode);
                        await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy);
                    }
                }
            };

            container.Children.Add(treeView);

        }; }

        private async Task OnDrop(object? sender, DragEventArgs e)
    {
        if (e.Source is Control control && control.DataContext is DeviceTreeNode targetNode)
        {
            if (targetNode != rootDevicesNode)
            {
                Console.WriteLine($"❌ Drop rejected on: {targetNode.Name}");
                return;
            }

            if (e.Data.Get("TreeNode") is DeviceTreeNode droppedNode)
            {
                string baseName = droppedNode.Name;
                int next = rootDevicesNode.Children
                    .Where(n => n.Name.StartsWith(baseName))
                    .Select(n =>
                    {
                        var suffix = n.Name.Substring(baseName.Length);
                        return int.TryParse(suffix, out int num) ? num : 0;
                    })
                    .DefaultIfEmpty()
                    .Max() + 1;

                var suggestedName = $"{baseName}{next}";
                var existingNames = rootDevicesNode.Children.Select(n => n.Name);
                //var prompt = new NamePromptWindow(suggestedName, existingNames)
                //{
                //    Icon = this.Icon,
                //    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                //    ShowInTaskbar = false,
                //    Topmost = true
                //};

                //prompt.Activate();

                //var result = await prompt.ShowDialog<string?>(this);

                //if (!string.IsNullOrWhiteSpace(result))
                //{
                //    var cloned = droppedNode.Clone();
                //    cloned.Name = result;
                //    cloned.SetCombined($"{result} - Type: {baseName}");
                //    rootDevicesNode.Children.Add(cloned);
                //    laborEngine.testDevices.Add(cloned);
                //    if (laborEngine.CreateInstance(cloned))
                //    {
                //        Log.Information($"Device added: Name: {cloned.Name} - Type: {baseName} - Open configuration by right click");
                //    }
                //    else
                //    {
                //        rootDevicesNode.Children.Remove(cloned);
                //        laborEngine.testDevices.Remove(cloned);
                //    }
                //    //TODO: bugfix: node is not expanding
                //    rootDevicesNode.IsExpanded = true;

                //}
                //else
                //{
                //    Log.Information("Adding the device cancelled by user.");
                //}

                e.Handled = true;
            }
        }
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (e.Source is Control control && control.DataContext is DeviceTreeNode targetNode &&
            targetNode == rootDevicesNode &&
            e.Data.Contains("TreeNode"))
        {
            e.DragEffects = DragDropEffects.Copy;
            e.Handled = true;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
            e.Handled = true;
        }
    }

    private class SimpleCommand : ICommand
    {
        private readonly Action _execute;
        public event EventHandler? CanExecuteChanged;

        public SimpleCommand(Action execute) => _execute = execute;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _execute();
    }
}

