using System;
using UnityEngine;

public static class MusicControllerEvents
{
    public static Action<bool> OnUserAwnser;
}

public class MusicController : MonoBehaviour
{
    public struct MusicData
    {
        public string musicName;
        public string musicAutor;
        public string musicLyric;
        public Sprite musicEmoji;
        public AudioClip musicClip;

        public MusicData(string name, string autor, string lyric, Sprite emoji, AudioClip clip)
        {
            musicName = name;
            musicAutor = autor;
            musicLyric = lyric; 
            musicEmoji = emoji;
            musicClip = clip;
        }

        public Sprite LoadEmoji()
        {
            return musicEmoji;
        }

        public AudioClip LoadClip()
        {
            return musicClip;
        }
    }


/*    public MusicData GetRandomMusic()
    {
        // Exemplo de dados de música

        MusicData music = new MusicData("Song Title", "Artist Name", exampleEmoji, exampleClip);
        return music;
    }
*/
}
