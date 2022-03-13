using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WpfPlayer.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    class Playlist
    {
        //private List<string> _playlistItems = new List<string>();
        [JsonProperty]
        public List<string> PlaylistItems = new List<string>();
    //    {
    //        get => _playlistItems;
    //        set
    //        {
    //            _playlistItems = value;
    //            _playlistPosition = 0;

    //            if (_shuffleState == ShuffleEnum.Off)
    //            {
    //                _playOrder = Enumerable.Range(0, _playlistItems.Count).ToList();
    //}
    //            else
    //            {
    //                _playOrder = shuffleList(Enumerable.Range(0, _playlistItems.Count).ToList());
    //            }
    //        }
    //    }

        //[OnDeserialized]
        //private void OnDeserialized(StreamingContext context)
        //{
        //    //need to do this so that the code in the setter executes
        //    PlaylistItems = _playlistItems;
        //}

        private List<int> _playOrder = new List<int>();

        private int _playlistPosition = 0;
        public int PlaylistPosition
        {
            get => _playlistPosition;
            set
            {
                _playlistPosition = value;
            }
        }
        private ShuffleEnum _shuffleState;

        public ShuffleEnum ShuffleState
        {
            get => _shuffleState;
            set
            {
                _shuffleState = value;
                if (_shuffleState == ShuffleEnum.Off)
                {
                    if (PlaylistItems.Count > 0)
                    {
                        _playlistPosition = _playOrder[_playlistPosition];
                        _playOrder = Enumerable.Range(0, PlaylistItems.Count).ToList();
                    }
                }
                else
                {
                    if (PlaylistItems.Count > 0)
                    {
                        _playOrder = shuffleList(_playOrder[_playlistPosition], Enumerable.Range(0, PlaylistItems.Count).ToList());
                        _playlistPosition = 0;
                    }
                }
            }
        }

        public string CurrentSong
        {
            get
            {
                return PlaylistItems[_playOrder[_playlistPosition]];
            }
        }

        public string NextSong
        {
            get
            {
                if (_playlistPosition + 1 < PlaylistItems.Count)
                {
                    return PlaylistItems[_playOrder[_playlistPosition + 1]];
                }
                else
                {
                    return null;
                }
            }
        }

        public Playlist()
        {

        }

        public Playlist(List<string> itemsIn, ShuffleEnum shuffle)
        {
            PlaylistItems = itemsIn;
            InitShuffle(shuffle);
            
        }

        public void InitShuffle(ShuffleEnum shuffle)
        {
            _shuffleState = shuffle;
            if (_shuffleState == ShuffleEnum.Off)
            {
                if (PlaylistItems.Count > 0)
                {
                    _playlistPosition = _playOrder[_playlistPosition];
                    _playOrder = Enumerable.Range(0, PlaylistItems.Count).ToList();
                }
            }
            else
            {
                if (PlaylistItems.Count > 0)
                {
                    _playOrder = shuffleList(Enumerable.Range(0, PlaylistItems.Count).ToList());
                    _playlistPosition = 0;
                }
            }
        }

        public void PlaylistFinished()
        {
            _playlistPosition = 0;
        }

        public void SongFinished()
        {
            _playlistPosition++;
        }

        public void ClearPlaylist()
        {
            PlaylistItems.Clear();
            _playlistPosition = 0;
        }

        public void Shuffle(int firstTrack)
        {
            _playlistPosition = 0;
            _playOrder = shuffleList(firstTrack, Enumerable.Range(0, PlaylistItems.Count).ToList());
        }
        public void ShufflePlaylist(int firstTrack)
        {
            _playlistPosition = 0;
            _playOrder = shuffleList(firstTrack, Enumerable.Range(0, PlaylistItems.Count).ToList());
        }

        private List<int> shuffleList(List<int> listIn)
        {
            List<int> listOut = new List<int>();

            Random rnd = new Random();

            while (listIn.Count > 0)
            {
                int index = rnd.Next(listIn.Count);
                listOut.Add(listIn[index]);
                listIn.RemoveAt(index);
            }

            return listOut;
        }

        private List<int> shuffleList(int firstElement, List<int> listIn)
        {
            List<int> listOut = new List<int>();

            int firstIndex = listIn.FindIndex(t => t == firstElement);
            listOut.Add(listIn[firstIndex]);
            listIn.RemoveAt(firstIndex);

            Random rnd = new Random();

            while (listIn.Count > 0)
            {
                int index = rnd.Next(listIn.Count);
                listOut.Add(listIn[index]);
                listIn.RemoveAt(index);
            }

            return listOut;
        }
    }

    public enum ShuffleEnum
    {
        Off, On
    }
}
