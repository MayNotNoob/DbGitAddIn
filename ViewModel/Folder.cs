using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace DbGitAddIn.ViewModel
{
    public class Folder
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ObservableCollection<Folder> Folders { get; set; }
        public ObservableCollection<Fichier> Fichiers { get; set; }

        public IList Children => new CompositeCollection
        {
            new CollectionContainer() {Collection = Folders},
            new CollectionContainer() {Collection = Fichiers}
        };

        public Folder(string name, string path)
        {
            Name = name;
            Path = path;
            Folders = new ObservableCollection<Folder>();
            Fichiers = new ObservableCollection<Fichier>();
        }
        
        public Folder FindFolder(string path, out ObservableCollection<Folder> folders)
        {
            var stack = new Stack<ObservableCollection<Folder>>();
            if (Path == path)
            {
                folders = null;
                return this;
            }

            if (Folders.Any())
            {
                stack.Push(Folders);
            }

            while (stack.Any())
            {
                var fs = stack.Pop();
                foreach (var folder in fs)
                {
                    if (folder.Path == path)
                    {
                        folders = fs;
                        return folder;
                    }

                    if (folder.Folders.Any())
                    {
                        stack.Push(folder.Folders);
                    }
                }
            }

            folders = null;
            return null;
        }

        public Fichier FindFile(string path, out Folder folder)
        {
            foreach (var fichier in Fichiers)
            {
                if (fichier.Path == path)
                {
                    folder = this;
                    return fichier;
                }
            }

            var stack = new Stack<Folder>();

            foreach (var f in Folders)
            {
                stack.Push(f);
            }

            while (stack.Any())
            {
                var f = stack.Pop();
                foreach (var fichier in f.Fichiers)
                {
                    if (fichier.Path == path)
                    {
                        folder = f;
                        return fichier;
                    }
                }

                foreach (var ff in f.Folders)
                {
                    stack.Push(ff);
                }
            }

            folder = null;
            return null;
        }

        public Folder FindFolder(string path)
        {
            if (Path == path) return this;
           var stack=new Stack<Folder>();
            foreach (var folder in Folders)
            {
                stack.Push(folder);
            }

            while (stack.Any())
            {
                var f = stack.Pop();
                if (f.Path == path) return f;
                foreach (var folder in f.Folders)
                {
                    stack.Push(folder);
                }
            }
            return null;
        }
    }

    public class Fichier
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
