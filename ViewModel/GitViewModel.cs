using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LibGit2Sharp;
using Application = System.Windows.Application;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.Forms.MessageBox;
using Task = System.Threading.Tasks.Task;
using TextBox = System.Windows.Controls.TextBox;

namespace DbGitAddIn.ViewModel
{
    public class GitViewModel : ViewModelBase
    {
        #region PRIVATE FIELDS

        private static int countFile = 1;

        private static int countFolder = 1;

        private ICommand clone;

        private string localPath;

        private string remotePath;

        private ICommand reload;

        private string folderSelected;

        private bool pasteEnbaled;

        private ICommand copyFile;

        private ICommand pasteFile;

        private ICommand openFolder;

        private ICommand deleteFile;

        private ICommand lostFocus;

        private string copyPath;

        private ICommand checkout;

        private string commitMsg;

        private string commitMode;

        private bool btnEnabled;

        private ICommand createBranch;

        private ICommand commit;

        private static string username = "sichen.wang";

        private static string email = "email@xx.com";

        private string comment;

        private bool tabItemSelected;

        private readonly Comments<string> comments;

        private ICommand merge;

        private ICommand console;

        private MergeResult mergeResult;

        private ICommand pull;

        private ICommand stage;

        private ICommand revert;

        private ICommand newFile;
        #endregion

        #region PUBLIC PROPERTIES

        public ICommand NewFile
        {
            get
            {
                return newFile ?? (newFile = new RelayCommand(DoNewFile, p => true));
            }
        }

        public ICommand Revert
        {
            get { return revert ?? (revert = new RelayCommand(p => DoRevert(), p => true)); }
        }

        public ICommand Stage
        {
            get
            {
                return stage ?? (stage = new RelayCommand(p => DoStage(), p => true));
            }
        }

        public ICommand Pull
        {
            get
            {
                return pull ?? (pull = new RelayCommand(p => DoPull(), p => true));
            }
        }

        public MergeResult Result
        {
            set
            {
                mergeResult = value;
                if (mergeResult.Status == MergeStatus.Conflicts)
                    MessageBox.Show("Merge conflicts found");
            }
        }
        public ICommand Console
        {
            get
            {
                return console ?? (console = new RelayCommand(p => DoConsole(), p => true));
            }
        }

        private void DoConsole()
        {
            try
            {
                Process process = new Process
                {
                    StartInfo =
                    {
                        FileName = "cmd"
                    }
                };
                if (!string.IsNullOrEmpty(ClonedRepoPath))
                    process.StartInfo.WorkingDirectory = Path.GetDirectoryName(ClonedRepoPath) ?? throw new InvalidOperationException();
                process.Start();
            }
            catch (Exception)
            {
                MessageBox.Show($"Invalid path {ClonedRepoPath}");
            }

        }

        public GitViewModel()
        {
            comments = new Comments<string>();
            comments.AddComment += OnAddComment;
        }

        public ICommand Merge
        {
            get
            {
                return merge ?? (merge = new RelayCommand(DoMerge, p => true));
            }
        }

        public ICommand Commit
        {
            get
            {
                return commit ?? (commit = new RelayCommand(p => DoCommit(), p => true));
            }
        }

        public ICommand CreateBranch
        {
            get
            {
                return createBranch ?? (createBranch = new RelayCommand(p => DoCreateBranch(), p => true));
            }
        }

        public bool BtnEnabled
        {
            get => btnEnabled;
            set
            {
                btnEnabled = value;
                OnPropertyChanged("BtnEnabled");
            }
        }

        public string Comment
        {
            get => comment;
            set
            {
                comment = value;
                OnPropertyChanged("Comment");
            }
        }

        public bool TabItemSelected
        {
            get => tabItemSelected;
            set
            {
                tabItemSelected = value;
                if (tabItemSelected)
                {
                    DoGetChanges();
                    BtnEnabled = Changes.Any();
                }
                OnPropertyChanged("TabItemSelected");
            }
        }

        public string ClonedRepoPath { get; set; }

        public string CommitMode
        {
            get => commitMode;
            set
            {
                commitMode = value;
                OnPropertyChanged("CommitMode");
            }
        }

        public string CommitMsg
        {
            get => commitMsg;
            set
            {
                commitMsg = value;
                OnPropertyChanged("CommitMsg");
            }

        }

        public ICommand Checkout
        {
            get
            {
                return checkout ?? (checkout = new RelayCommand(DoCheckout, p => true));
            }
        }

        public ICommand Clone
        {
            get
            {
                return clone ?? (clone = new RelayCommand(p => DoClone(), p => true));
            }
        }

        public ICommand LostFocus
        {
            get
            {
                return lostFocus ?? (lostFocus = new RelayCommand(DoLostFocus, p => true));
            }
        }

        public ICommand DeleteFile
        {
            get
            {
                return deleteFile ?? (deleteFile = new RelayCommand(DoDeleteFile, p => true));
            }
        }

        public ICommand Reload
        {
            get { return reload ?? (reload = new RelayCommand(p => DoLoad(), p => true)); }
        }

        public string LocalPath
        {
            get => localPath;
            set
            {
                localPath = value;
                OnPropertyChanged("LocalPath");
            }
        }

        public string RemotePath
        {
            get => remotePath;
            set
            {
                remotePath = value;
                OnPropertyChanged("RemotePath");
            }
        }

        public ObservableCollection<Folder> Folders { get; } = new ObservableCollection<Folder>();

        public ICommand OpenFolder
        {
            get { return openFolder ?? (openFolder = new RelayCommand(p => DoOpenFolder(), p => true)); }
        }

        public ICommand PasteFile
        {
            get { return pasteFile ?? (pasteFile = new RelayCommand(DoPasteFile, p => true)); }
        }

        public bool PasteEnbaled
        {
            get => pasteEnbaled;
            set
            {
                pasteEnbaled = value;
                OnPropertyChanged("PasteEnbaled");
            }
        }

        public ICommand CopyFile
        {
            get { return copyFile ?? (copyFile = new RelayCommand(DoCopyFile, p => true)); }
        }

        public string CopyPath
        {
            get => copyPath;
            set
            {
                copyPath = value;
                PasteEnbaled = !string.IsNullOrEmpty(copyPath);
                OnPropertyChanged("CopyPath");
            }
        }

        public ObservableCollection<string> Branches { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> LocalBranches { get; } = new ObservableCollection<string>();

        public ObservableCollection<Folder> Changes { get; } = new ObservableCollection<Folder>();

        #endregion

        #region PRIVATE METHODS

        private void DoCreateBranch()
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("New Branch Name", "Create Branch");
            if (!string.IsNullOrEmpty(input))
            {
                if (!string.IsNullOrEmpty(ClonedRepoPath))
                {
                    using (var repo = new Repository(ClonedRepoPath))
                    {
                        foreach (var branch in repo.Branches.Where(b => !b.IsRemote))
                        {
                            if (branch.FriendlyName == input)
                            {
                                MessageBox.Show("This name is already taken");
                                return;
                            }
                        }
                        var currentBranch = repo.Branches.First(b => b.IsCurrentRepositoryHead);
                        var newBranch = CreateNewBranch(input, currentBranch);
                        if (newBranch != null)
                            Commands.Checkout(repo, newBranch);
                        LoadBranches();
                    }
                }
                else
                {
                    MessageBox.Show("No repository found");
                }
            }
            else
                MessageBox.Show("Branch name can't be empty");
        }

        private Branch CreateNewBranch(string newName, Branch currentBranch)
        {
            if (!string.IsNullOrEmpty(ClonedRepoPath))
            {
                using (var repo = new Repository(ClonedRepoPath))
                {
                    var newBranch = repo.CreateBranch(newName, currentBranch.Tip);
                    var remote = repo.Network.Remotes["origin"];
                    repo.Branches.Update(newBranch,
                        b => b.Remote = remote.Name,
                        b => b.TrackedBranch = newBranch.CanonicalName,
                        b => b.UpstreamBranch = newBranch.CanonicalName);
                    return newBranch;
                }
            }
            return null;
        }

        private void DoCheckout(object args)
        {
            var name = (string)args;
            if (name.Contains("*")) return;
            if (string.IsNullOrEmpty(ClonedRepoPath)) return;
            using (var repo = new Repository(ClonedRepoPath))
            {
                Branch newBranch = null;
                if (name.Contains("origin"))
                {
                    var shortname = name.Split('/').Last();
                    if (repo.Branches.Where(b => !b.IsRemote).Select(b => b.FriendlyName).Contains(shortname))
                    {
                        newBranch = repo.Branches[shortname];
                    }
                    else
                    {
                        var branch = repo.Branches[name];
                        if (branch == null)
                            MessageBox.Show($"{name} doesn't exist at server");
                        else
                            newBranch = CreateNewBranch(shortname, branch);
                    }
                }
                else
                    newBranch = repo.Branches[name];

                if (newBranch != null)
                    Commands.Checkout(repo, newBranch);
            }
            LoadBranches();
            DoLoad();
        }

        private void DoLostFocus(object sender)
        {
            var textBox = (TextBox)sender;
            var textBlock = (TextBlock)VisualTreeHelper.GetChild(textBox.Parent, 0);
            var path = textBlock.Tag.ToString();
            var newName = textBox.Text;
            textBox.Visibility = Visibility.Hidden;
            textBlock.Visibility = Visibility.Visible;

            if (Directory.Exists(textBlock.Tag.ToString()))
            {
                var folder = Folders[0].FindFolder(path, out ObservableCollection<Folder> folders);
                if (folder != null)
                {
                    folder.Name = newName;
                    folder.Path = $"{Path.GetDirectoryName(path)}\\{newName}";
                    var index = folders.IndexOf(folder);
                    folders.Remove(folder);
                    folders.Insert(index, folder);
                    Directory.Move(path, folder.Path);
                }
            }
            else
            {
                var fichier = Folders[0].FindFile(path, out Folder folder);
                if (fichier != null)
                {
                    fichier.Name = newName;
                    fichier.Path = $"{Path.GetDirectoryName(fichier.Path)}\\{newName}";
                    var index = folder.Fichiers.IndexOf(fichier);
                    folder.Fichiers.Remove(fichier);
                    folder.Fichiers.Insert(index, fichier);
                    File.Move(path, fichier.Path);
                }
            }
        }

        private void OnAddComment(Queue<string> queue)
        {
            while (queue.Any())
            {
                Comment = queue.Dequeue();
                Task.Delay(10).Wait();
            }
        }

        private void DoRevert()
        {
            if (string.IsNullOrEmpty(ClonedRepoPath)) return;
            var res = MessageBox.Show("This operation will undo all of uncommitted changes", "ALERT",
                MessageBoxButtons.OKCancel);
            if (res == DialogResult.OK || res == DialogResult.Yes)
            {
                using (var repo = new Repository(ClonedRepoPath))
                {
                    var files = repo.RetrieveStatus().Select(f => f.FilePath).ToList();
                    var options = new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force };
                    repo.CheckoutPaths(repo.Head.FriendlyName, files, options);
                    TabItemSelected = true;
                }
            }

        }

        private void DoPasteFile(object sender)
        {
            if (!string.IsNullOrEmpty(CopyPath))
            {
                var item = (MenuItem)sender;
                if (((ContextMenu)item.Parent)?.PlacementTarget is TextBlock textBlock)
                {
                    var path = textBlock.Tag.ToString();
                    if (Directory.Exists(path))
                    {

                        var f = Folders[0].FindFolder(path);
                        if (f != null)
                        {
                            var copyExtention = Path.GetExtension(CopyPath);
                            var name = $"{Path.GetFileNameWithoutExtension(CopyPath)}_copy{copyExtention}";
                            var tmpPath = $"{path}\\{name}";
                            path = File.Exists(tmpPath) ? $"{path}\\{Path.GetFileNameWithoutExtension(tmpPath)}_copy{copyExtention}" : tmpPath;
                            var content = File.ReadAllText(CopyPath);
                            File.WriteAllText(path, content);
                            f.Fichiers.Add(new Fichier { Path = path, Name = name });
                        }
                    }
                }
                CopyPath = "";
            }
        }

        private void DoCopyFile(object sender)
        {
            var item = (MenuItem)sender;
            if (((ContextMenu)item.Parent)?.PlacementTarget is TextBlock textBlock)
                CopyPath = textBlock.Tag.ToString();
        }

        private void DoNewFile(object sender)
        {
            var item = (MenuItem)((MenuItem)sender).Parent;
            if (((ContextMenu)item.Parent)?.PlacementTarget is TextBlock textBlock)
            {
                var path = textBlock.Tag.ToString();
                var folder = Folders[0].FindFolder(path);
                if (((MenuItem)sender).Header.ToString() == "New File")
                {
                    var fichier = new Fichier
                    {
                        Name = $"NewFile{countFile}.sql",
                        Path = $"{path}\\NewFile{countFile}.sql"
                    };
                    folder.Fichiers.Add(fichier);
                    File.Create(fichier.Path);
                    countFile++;
                }
                else
                {
                    var f = new Folder($"NewFolder{countFolder}", $"{path}\\NewFolder{countFolder}");
                    folder.Folders.Add(f);
                    Directory.CreateDirectory(f.Path);
                    countFolder++;
                }
            }
        }

        private void DoClone()
        {
            try
            {
                ClonedRepoPath = Repository.Clone(RemotePath, LocalPath);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void DoDeleteFile(object sender)
        {
            var item = (MenuItem)sender;
            if (((ContextMenu)item.Parent)?.PlacementTarget is TextBlock textBlock)
            {
                var path = textBlock.Tag.ToString();
                var fichier = Folders[0].FindFile(path, out Folder folder);
                File.Delete(path);
                folder.Fichiers.Remove(fichier);
            }
        }

        private void DoLoad()
        {
            if (string.IsNullOrEmpty(folderSelected)) return;
            Folders.Clear();
            var f = new Folder((new DirectoryInfo(folderSelected)).Name, folderSelected);
            LoadFolder(f, folderSelected);
            Folders.Add(f);
        }

        private void LoadFolder(Folder folder, string path)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                var fichier = new Fichier { Path = file, Name = Path.GetFileName(file) };
                folder.Fichiers.Add(fichier);
            }
            foreach (var directory in Directory.GetDirectories(path))
            {
                var f = new Folder((new DirectoryInfo(directory)).Name, directory);
                folder.Folders.Add(f);
            }
            foreach (var f in folder.Folders)
            {
                LoadFolder(f, f.Path);
            }
        }

        private void DoOpenFolder()
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == true)
            {
                if (folderSelected != dialog.SelectedPath)
                {
                    folderSelected = dialog.SelectedPath;
                    ClonedRepoPath = $"{folderSelected}\\.git";
                    DoLoad();
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)LoadBranches);
                }
            }
        }

        private void DoMerge(object obj)
        {
            var name = (string)obj;
            if (string.IsNullOrEmpty(name)) return;
            if (string.IsNullOrEmpty(ClonedRepoPath)) return;
            using (var repo = new Repository(ClonedRepoPath))
            {
                var branch = repo.Branches[name];
                var signature = new Signature(username, email, DateTimeOffset.Now);
                this.Result = repo.Merge(branch, signature);
                DoLoad();
                comments.Add("Merge finished");
            }
        }

        private void DoPull()
        {
            if (string.IsNullOrEmpty(ClonedRepoPath)) return;
            using (var repo = new Repository(ClonedRepoPath))
            {
                Result = PullChanges(repo);
            }
        }

        private void LoadBranches()
        {
            try
            {
                using (var repo = new Repository(ClonedRepoPath))
                {
                    BranchCollection branches = repo.Branches;
                    Branches.Clear();
                    LocalBranches.Clear();
                    foreach (Branch branch in branches)
                    {
                        Branches.Add(branch.IsCurrentRepositoryHead ? $"*{branch.FriendlyName}" : branch.FriendlyName);
                        if (!branch.IsRemote && !branch.IsCurrentRepositoryHead) LocalBranches.Add(branch.FriendlyName);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void DoCommit()
        {
            try
            {
                Task.Run(() =>
                {
                    if (string.IsNullOrEmpty(ClonedRepoPath)) return;
                    if (string.IsNullOrEmpty(CommitMode)) return;
                    using (var repo = new Repository(ClonedRepoPath))
                    {
                        var files = GetChangedFiles(repo).ToList();
                        if (!files.Any()) return;
                        StageChanges(files, repo);
                        CommitChanges(repo); //at least the mode is commit
                        if (CommitMode.Contains("sync")) //if mode is commit and sync
                            this.Result = PullChanges(repo);
                        if (CommitMode.Contains("and")) //if mode is commit and push
                            PushChanges(repo);
                    }
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }

        private void DoGetChanges()
        {
            if (string.IsNullOrEmpty(ClonedRepoPath)) return;
            Changes.Clear();
            using (var repo = new Repository(ClonedRepoPath))
            {
                var status = repo.RetrieveStatus();
                foreach (StatusEntry entry in status)
                {
                    if (!IsStatusExisted(entry.State.ToString(), Changes, out Folder f))
                    {
                        var st = entry.State.ToString();
                        f = new Folder(st, st);
                        Changes.Add(f);
                    }
                    var fichier = new Fichier
                    {
                        Name = Path.GetFileName(entry.FilePath),
                        Path = entry.FilePath
                    };
                    f.Fichiers.Add(fichier);
                }
            }
        }

        private void DoStage()
        {
            if (string.IsNullOrEmpty(ClonedRepoPath)) return;
            using (var repo = new Repository(ClonedRepoPath))
            {
                var files = repo.RetrieveStatus().Select(s => s.FilePath).ToList();
                if (files.Any())
                    StageChanges(files, repo);
                else
                {
                    comments.Add("Nothing to stage");
                }
            }
        }

        private bool IsStatusExisted(string status, ObservableCollection<Folder> folders, out Folder f)
        {
            if (!folders.Any())
            {
                f = null;
                return false;
            }
            foreach (var folder in folders)
            {
                if (folder.Name != status) continue;
                f = folder;
                return true;
            }

            f = null;
            return false;
        }

        private IEnumerable<string> GetChangedFiles(Repository repo)
        {
            RepositoryStatus status = repo.RetrieveStatus();
            return status.Select(f => f.FilePath);
        }

        private void StageChanges(IEnumerable<string> files, Repository repo)
        {
            Commands.Stage(repo, files);
            comments.Add("Stage Succeed");
        }

        private void CommitChanges(Repository repo)
        {
            var signature = new Signature(username, email, DateTimeOffset.Now);
            CommitMsg = CommitMsg ?? "";
            repo.Commit(CommitMsg, signature, signature);
            comments.Add("Commit Succeed");
        }

        private void PushChanges(Repository repo)
        {
            var options = new PushOptions
            {
                CredentialsProvider =
                   (url, usernameFromUrl, types) => GetUsernamePasswordCredentials()
            };
            var branch = repo.Branches.First(b => b.IsCurrentRepositoryHead);
            repo.Network.Push(branch, options);
            comments.Add("Push Succeed");
        }

        private MergeResult PullChanges(Repository repo)
        {
            var options = new PullOptions
            {
                FetchOptions = new FetchOptions
                {
                    CredentialsProvider =
                        (url, usernameFromUrl, types) => GetUsernamePasswordCredentials()
                }
            };

            // User information to create a merge commit
            var signature = new Signature(username, email, DateTimeOffset.Now);
            var result = Commands.Pull(repo, signature, options);
            comments.Add("Pull Succeed");
            return result;
        }

        private UsernamePasswordCredentials GetUsernamePasswordCredentials()
        {
            return new UsernamePasswordCredentials
            {
                Username = "GitOnly",
                Password = "yabmabh2r5eoyyghssw4n57qxfqsrydrhjm7swyrp6j26izjsvcq"
            };
        }
        #endregion PRIVATE METHODS
    }
}