//------------------------------------------------------------------------------
// <copyright file="GitFormControl.xaml.cs" company="LabSichen">
//     Copyright (c) LabSichen.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Data;
using System.Data.SqlClient;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using System.Windows;
using System.Windows.Controls;
using DbGitAddIn.ViewModel;
using EnvDTE;
using EnvDTE80;
using LibGit2Sharp;
using Microsoft.SqlServer.Management.Smo.RegSvrEnum;

namespace DbGitAddIn
{


    /// <summary>
    /// Interaction logic for GitFormControl.
    /// </summary>
    public partial class GitFormControl : UserControl
    {
        //private Process process;
        private TreeViewItem treeViewItemSelected;
        /// <summary>
        /// Initializes a new instance of the <see cref="GitFormControl"/> class.
        /// </summary>
        public GitFormControl()
        {
            this.InitializeComponent();
        }

        private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var path = ((TextBlock)sender).Tag.ToString();
                if (GitFormCommand.Provider != null)
                {
                    var dte = (DTE2)GitFormCommand.Provider.GetService(typeof(DTE));
                    dte.ItemOperations.OpenFile(path);
                }
            }
        }
        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                treeViewItem.IsSelected = true;
                e.Handled = true;
            }
        }

        private TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var contextMenu = ((TextBlock)sender).ContextMenu;
            if (contextMenu != null)
                contextMenu.Visibility = Visibility.Visible;
        }

        private void OnRenameClick(object sender, RoutedEventArgs e)
        {
            var textBlock = (TextBlock)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget;
            var parent = ((TextBlock)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).Parent;
            var textBox = (TextBox)VisualTreeHelper.GetChild(parent, 1);
            textBlock.Visibility = Visibility.Hidden;
            textBox.Visibility = Visibility.Visible;
        }

        //private void OnTabItemSelected(object sender, RoutedEventArgs e)
        //{
        //    var dataContext = (GitViewModel)this.DataContext;
        //    using (var repo = new Repository(dataContext.ClonedRepoPath))
        //    {
        //        var status = repo.RetrieveStatus();
               
        //        dataContext.BtnEnabled = !status.IsDirty;
        //        repo.Network.Push(repo.Branches["xx"]);
        //        //repo.Config.
        //    }
        //}
    }
}