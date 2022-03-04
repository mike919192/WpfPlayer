using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace WpfPlayer.Model
{
    [JsonObject]
    public class Folder : IEnumerable //IComparable<Folder>
    {
        //private ObservableCollection<Folder> subFolders;
        public ObservableCollection<string> files;
        private string folderName;
        private string parentDirectory;

        public string FolderName
        {
            get => folderName;
            set => folderName = value;
        }

        public string ParentDirectory
        {
            get => parentDirectory;
            set => parentDirectory = value;
        }

        public ObservableCollection<Folder> SubFolders { get; set; }

        public Folder(string directoryString)
        {
            SubFolders = new ObservableCollection<Folder>();
            files = new ObservableCollection<string>();
            
            int length = directoryString.LastIndexOf(Path.DirectorySeparatorChar);
            folderName = directoryString.Substring(length + 1);
            parentDirectory = directoryString.Substring(0, length);

            List<string> test = getFiles(directoryString, "*.mp3|*.flac", SearchOption.AllDirectories).ToList();
            test.Sort();
            for (int index = 0; index < test.Count; ++index)
            {
                test[index] = test[index].Substring(directoryString.Length + 1);
                Add(test[index].Split(Path.DirectorySeparatorChar));
            }
        }

        public Folder()
        {

        }

        private string[] getFiles(string SourceFolder, string Filter, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // ArrayList will hold all file names
            ArrayList alFiles = new ArrayList();

            // Create an array of filter string
            string[] MultipleFilters = Filter.Split('|');

            // for each filter find mathing file names
            foreach (string FileFilter in MultipleFilters)
            {
                // add found file names to array list
                alFiles.AddRange(Directory.GetFiles(SourceFolder, FileFilter, searchOption));
            }

            // returns string array of relevant file names
            return (string[])alFiles.ToArray(typeof(string));
        }

        private Folder(string FolderName, string Path)
        {
            SubFolders = new ObservableCollection<Folder>();
            files = new ObservableCollection<string>();
            folderName = FolderName;
            parentDirectory = Path;
        }

        public override string ToString() { return FolderName; }
        //public int CompareTo(Folder that)
        //{
        //    var test = string.Compare(this.FolderName, that.FolderName, true);

        //    return test;
        //}

        private void Add(string[] splitString)
        {
            if (splitString.Length == 1)
            {
                files.Add(splitString[0]);
            }
            else
            {
                if (SubFolders.ToList().FindIndex(folder => folder.folderName == splitString[0]) == -1)
                    SubFolders.Add(new Folder(splitString[0], Path.Combine(ParentDirectory, FolderName)));
                string[] array = ((IEnumerable<string>)splitString).Skip(1).Take(splitString.Length - 1).ToArray();
                SubFolders[SubFolders.ToList().FindIndex(folder => folder.FolderName == splitString[0])].Add(array);
            }
        }

        public List<string> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            List<string> filesOut = new List<string>();

            filesOut = files.Select(t => Path.Combine(parentDirectory, folderName, t)).ToList();

            if (searchOption == SearchOption.AllDirectories)
            {
                for (int i = 0; i < SubFolders.Count; i++)
                {
                    filesOut.AddRange(SubFolders[i].GetFiles(SearchOption.AllDirectories));
                }
            }

            return filesOut;
        }

        public IEnumerator GetEnumerator()
        {
            foreach (Folder val in SubFolders)
            {
                yield return val;
            }
        }
    }
}
