﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using System.IO;
using System.ComponentModel;
using System.Windows.Data;
using System.Text.RegularExpressions;

namespace ShoefitterDX.ToolWindows
{
    public class DataBrowserItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get => this._isExpanded;
            set
            {
                this._isExpanded = value;
                this.RaisePropertyChanged(nameof(IsExpanded));
            }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                this._isSelected = value;
                this.RaisePropertyChanged(nameof(IsSelected));
            }
        }

        private ImageSource _iconSource = null;
        public ImageSource IconSource
        {
            get => this._iconSource;
            set
            {
                this._iconSource = value;
                this.RaisePropertyChanged(nameof(IconSource));
            }
        }

        private bool _isDirectory = false;
        public bool IsDirectory
        {
            get => this._isDirectory;
            set
            {
                this._isDirectory = value;
                this.RaisePropertyChanged(nameof(IsDirectory));
            }
        }

        private string _name = "";
        public string Name
        {
            get => this._name;
            set
            {
                this._name = value;
                this.RaisePropertyChanged(nameof(Name));
            }
        }

        private string _fullPath = "";
        public string FullPath
        {
            get => this._fullPath;
            set
            {
                this._fullPath = value;
                this.RaisePropertyChanged(nameof(FullPath));
            }
        }

        private FileType _type = null;
        public FileType Type
        {
            get => this._type;
            set
            {
                this._type = value;
                this.RaisePropertyChanged(nameof(Type));
            }
        }

        public ObservableCollection<DataBrowserItem> Children { get; } = new ObservableCollection<DataBrowserItem>();
        public CollectionViewSource SortedChildren { get; }

        public DataBrowserItem()
        {
            this.SortedChildren = new CollectionViewSource();
            this.SortedChildren.Source = this.Children;
            this.SortedChildren.SortDescriptions.Add(new SortDescription(nameof(IsDirectory), ListSortDirection.Descending));
            this.SortedChildren.SortDescriptions.Add(new SortDescription(nameof(Name), ListSortDirection.Ascending));
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Interaction logic for DataBrowser.xaml
    /// </summary>
    public partial class DataBrowser : UserControl
    {
        public ObservableCollection<DataBrowserItem> TreeItems { get; } = new ObservableCollection<DataBrowserItem>();
        private DataBrowserItem ProjectItem { get; } = new DataBrowserItem();
        public Context Context { get; }

        private List<Regex> IgnoreExpressions { get; } = new List<Regex>
        {
            new Regex(@".*\\\..*", RegexOptions.Compiled),
            new Regex(@"\\build\\?", RegexOptions.Compiled),
            new Regex(@"\\native\\?", RegexOptions.Compiled),
            new Regex(@"\\tools\\?", RegexOptions.Compiled)
        };

        public event EventHandler<DataBrowserItem> ItemDoubleClicked;

        public DataBrowser(Context context)
        {
            InitializeComponent();

            this.Context = context;
            this.TreeItems.Add(ProjectItem);

            this.Refresh();
        }

        public void Refresh()
        {
            SyncTreeNode(Context.ProjectDirectory, ProjectItem, true);
        }

        private void SyncTreeNode(string path, DataBrowserItem item, bool isDirectory)
        {
            item.Name = Path.GetFileName(Path.TrimEndingDirectorySeparator(path));
            item.IsDirectory = isDirectory;
            item.Type = FileTypes.DetermineType(path);
            item.IconSource = item.Type?.IconSource;
            item.FullPath = path;

            if (isDirectory)
            {
                List<string> children = new List<string>();
                foreach (string childPath in Directory.EnumerateFileSystemEntries(path))
                {
                    if (IgnoreExpressions.Any(expr => expr.IsMatch(childPath)))
                    {
                        continue;
                    }
                    string name = Path.GetFileName(Path.TrimEndingDirectorySeparator(childPath));
                    children.Add(name);
                    DataBrowserItem existing = item.Children.FirstOrDefault(child => child.Name == name);
                    if (existing == null)
                    {
                        existing = new DataBrowserItem();
                        item.Children.Add(existing);
                    }
                    SyncTreeNode(Path.Combine(path, name), existing, (File.GetAttributes(childPath) & FileAttributes.Directory) == FileAttributes.Directory);
                }

                for (int i = item.Children.Count - 1; i >= 0; i--)
                {
                    if (!children.Contains(item.Children[i].Name)) {
                        item.Children.RemoveAt(i);
                    }
                }
                item.SortedChildren.View.Refresh();
            }
            else
            {

            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.Refresh();
        }

        private void ItemTreeViewItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (e.Source is TreeViewItem treeItem && treeItem.Tag is DataBrowserItem item)
            {
                ItemDoubleClicked?.Invoke(this, item);
                if (item.Type != null && item.Type.EditorType != null)
                {
                    e.Handled = true;
                }
            }
        }
    }
}