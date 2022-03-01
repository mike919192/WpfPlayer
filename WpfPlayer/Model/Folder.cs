using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WpfPlayer.Model
{
    public class Folder : IComparable<Folder>, IEnumerable
    {
        //private ObservableCollection<Folder> subFolders;
        private ObservableCollection<string> files;
        private string folderName;
        private string path;

        public string FolderName => folderName;

        public string Path => path;

        public ObservableCollection<Folder> subFolders { get; set; }

        public Folder(string directoryString)
        {
            subFolders = new ObservableCollection<Folder>();
            files = new ObservableCollection<string>();
            string[] files1 = Directory.GetFiles(directoryString, "*.mp3", SearchOption.AllDirectories);
            string[] files2 = Directory.GetFiles(directoryString, "*.flac", SearchOption.AllDirectories);
            string[] strArray = new string[files1.Length + files2.Length];
            files1.CopyTo(strArray, 0);
            files2.CopyTo(strArray, files1.Length);
            int length = directoryString.LastIndexOf('\\');
            folderName = directoryString.Substring(length + 1);
            path = directoryString.Substring(0, length);
            var test = strArray.ToList();
            test.Sort();
            for (int index = 0; index < test.Count; ++index)
            {
                test[index] = test[index].Substring(directoryString.Length + 1);
                Add(test[index].Split('\\'));
            }
        }

        private Folder(string FolderName, string Path)
        {
            subFolders = new ObservableCollection<Folder>();
            files = new ObservableCollection<string>();
            folderName = FolderName;
            path = Path;
        }

        public override string ToString() { return FolderName; }
        public int CompareTo(Folder that)
        {
            var test = string.Compare(this.FolderName, that.FolderName, true);

            return test;
        }

        private void Add(string[] splitString)
        {
            if (splitString.Length == 1)
            {
                files.Add(splitString[0]);
            }
            else
            {
                if (subFolders.ToList().FindIndex(folder => folder.folderName == splitString[0]) == -1)
                    subFolders.Add(new Folder(splitString[0], Path + "\\" + FolderName));
                string[] array = ((IEnumerable<string>)splitString).Skip(1).Take(splitString.Length - 1).ToArray();
                subFolders[subFolders.ToList().FindIndex(folder => folder.FolderName == splitString[0])].Add(array);
            }
        }

        //public TreeNode GetTreeNode(TreeNode newNode)
        //{
        //    for (int index = 0; index < this.subFolders.Count; ++index)
        //    {
        //        newNode.Nodes.Add(this.subFolders[index].FolderName);
        //        this.subFolders[index].GetTreeNode(newNode.Nodes[index]);
        //    }
        //    return newNode;
        //}

        //public ListViewItem[] GetListView()
        //{
        //    ListViewItem[] listView = new ListViewItem[this.files.Count];
        //    for (int index = 0; index < this.files.Count; ++index)
        //    {
        //        listView[index] = new ListViewItem(this.files[index]);
        //        listView[index].SubItems.Add(File.GetLastWriteTime(this.Path + "\\" + this.FolderName + "\\" + this.files[index]).ToString());
        //    }
        //    return listView;
        //}

        //public Folder GetFolderFromTreeNode(TreeNode searchNode) => this.subFolders[this.subFolders.ToList().FindIndex((Predicate<Folder>)(folder => searchNode.Text == folder.FolderName))];

        //public string GetFolderFromListItem(ListViewItem searchItem) => this.files[this.files.ToList().FindIndex((Predicate<string>)(filename => searchItem.Text == filename))];

        public IEnumerator GetEnumerator()
        {
            foreach (Folder val in subFolders)
            {
                yield return val;
            }
        }
    }
}
