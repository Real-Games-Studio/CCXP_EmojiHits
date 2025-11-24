using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Networking;


namespace RealGames
{
    [System.Serializable]
    public class FileLoader
    {
        // Caminho base padrao definido como Application.streamingAssetsPath para compatibilidade em tempo de execucao
        public string path = Path.Combine(Application.streamingAssetsPath, "Files");
        public string name; // Nome do arquivo com extensao

        public FileLoader() { }

        public FileLoader(string name)
        {
            this.name = name;
            this.path = Path.Combine(Application.streamingAssetsPath, "Files");
            Debug.Log($"[FileLoader] streamingAssetsPath: {Application.streamingAssetsPath}");
            Debug.Log($"[FileLoader] Resource path: {this.path}");
        }

        // ...existing code...
        // Helper to log and normalize file paths
        private string GetNormalizedFilePath(string subfolder = null)
        {
            // Use Application.streamingAssetsPath for runtime builds
            string basePath = Application.streamingAssetsPath;
            string filePath = subfolder != null
                ? Path.Combine(basePath, "Files", subfolder, name)
                : Path.Combine(basePath, "Files", name);

            // Normalize to forward slashes for UnityWebRequest and logging
            string normalized = filePath.Replace("\\", "/");
            Debug.Log($"[FileLoader] Resolved file path: {normalized}");
            return normalized;
        }

        public void LoadSprite(Action<Sprite> onComplete)
        {
            string filePath = GetNormalizedFilePath();
            Debug.Log($"[FileLoader] Checking sprite file: {filePath}");

#if UNITY_ANDROID && !UNITY_EDITOR
                    // On Android, StreamingAssets is inside APK, use UnityWebRequest
                    string url = filePath;
                    UnityWebRequest www = UnityWebRequest.Get(url);
                    www.SendWebRequest().completed += _ =>
                    {
                        if (www.result == UnityWebRequest.Result.Success)
                        {
                            byte[] fileData = www.downloadHandler.data;
                            Texture2D texture = new Texture2D(2, 2);
                            if (texture.LoadImage(fileData))
                            {
                                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                                onComplete?.Invoke(sprite);
                            }
                            else
                            {
                                Debug.LogError($"Failed to load sprite: {name}");
                                onComplete?.Invoke(null);
                            }
                        }
                        else
                        {
                            Debug.LogError($"Sprite file not found (Android): {filePath}");
                            onComplete?.Invoke(null);
                        }
                    };
#else
            if (System.IO.File.Exists(filePath))
            {
                byte[] fileData = System.IO.File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(fileData))
                {
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    onComplete?.Invoke(sprite);
                }
                else
                {
                    Debug.LogError($"Failed to load sprite: {name}");
                    onComplete?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"Sprite file not found: {filePath}");
                onComplete?.Invoke(null);
            }
#endif
        }

        public void LoadTexture2D(Action<Texture2D> onComplete)
        {
            string filePath = GetNormalizedFilePath();
            Debug.Log($"[FileLoader] Checking texture file: {filePath}");

#if UNITY_ANDROID && !UNITY_EDITOR
                    string url = filePath;
                    UnityWebRequest www = UnityWebRequest.Get(url);
                    www.SendWebRequest().completed += _ =>
                    {
                        if (www.result == UnityWebRequest.Result.Success)
                        {
                            byte[] fileData = www.downloadHandler.data;
                            Texture2D texture = new Texture2D(2, 2);
                            if (texture.LoadImage(fileData))
                            {
                                onComplete?.Invoke(texture);
                            }
                            else
                            {
                                Debug.LogError($"Failed to load texture: {name}");
                                onComplete?.Invoke(null);
                            }
                        }
                        else
                        {
                            Debug.LogError($"Texture file not found (Android): {filePath}");
                            onComplete?.Invoke(null);
                        }
                    };
#else
            if (System.IO.File.Exists(filePath))
            {
                byte[] fileData = System.IO.File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(fileData))
                {
                    onComplete?.Invoke(texture);
                }
                else
                {
                    Debug.LogError($"Failed to load texture: {name}");
                    onComplete?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"Texture file not found: {filePath}");
                onComplete?.Invoke(null);
            }
#endif
        }

        public void LoadAudioClip(Action<AudioClip> onComplete)
        {
            string filePath = GetNormalizedFilePath("Audio");
            Debug.Log($"[FileLoader] Checking audio file: {filePath}");

#if UNITY_ANDROID && !UNITY_EDITOR
                    string url = filePath;
#else
            string url = "file:///" + filePath;
#endif
            AudioType audioType = ResolveAudioType(name);

            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
            if (www.downloadHandler is DownloadHandlerAudioClip audioHandler)
            {
                audioHandler.streamAudio = false;
#if UNITY_2018_1_OR_NEWER
                audioHandler.compressed = false;
#endif
            }
            www.SendWebRequest().completed += _ =>
            {
                AudioClip clip = null;
                string errorMessage = null;

                if (www.result == UnityWebRequest.Result.Success)
                {
                    clip = DownloadHandlerAudioClip.GetContent(www);
                    if (clip != null)
                    {
                        clip.name = Path.GetFileNameWithoutExtension(name);
                    }
                    else
                    {
                        errorMessage = "UnityWebRequest returned null clip.";
                    }
                }
                else
                {
                    errorMessage = www.error;
                }

                if (clip == null && audioType == AudioType.UNKNOWN)
                {
                    // Retry with explicit MPEG as fallback for MP3-like files.
                    string retryUrl = url;
                    UnityWebRequest retryRequest = UnityWebRequestMultimedia.GetAudioClip(retryUrl, AudioType.MPEG);
                    retryRequest.SendWebRequest().completed += __ =>
                    {
                        AudioClip retryClip = null;
                        if (retryRequest.result == UnityWebRequest.Result.Success)
                        {
                            retryClip = DownloadHandlerAudioClip.GetContent(retryRequest);
                            if (retryClip != null)
                            {
                                retryClip.name = Path.GetFileNameWithoutExtension(name);
                                onComplete?.Invoke(retryClip);
                            }
                            else
                            {
                                Debug.LogError($"Failed to load audio on retry: {name}, clip remains null.");
                                onComplete?.Invoke(null);
                            }
                        }
                        else
                        {
                            Debug.LogError($"Failed to load audio on retry: {name}, Error: {retryRequest.error}");
                            onComplete?.Invoke(null);
                        }

                        retryRequest.Dispose();
                    };
                }
                else
                {
                    if (clip != null)
                    {
                        onComplete?.Invoke(clip);
                    }
                    else
                    {
                        Debug.LogError($"Failed to load audio: {name}, Error: {errorMessage}");
                        onComplete?.Invoke(null);
                    }
                }

                www.Dispose();
            };
        }

        public void LoadVideoClip(VideoPlayer videoPlayer, Action onComplete)
        {
            string filePath = GetNormalizedFilePath();
            Debug.Log($"[FileLoader] Checking video file: {filePath}");

#if UNITY_ANDROID && !UNITY_EDITOR
                    videoPlayer.url = filePath;
#else
            videoPlayer.url = "file:///" + filePath;
#endif
            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += _ => onComplete?.Invoke();
        }

        private static AudioType ResolveAudioType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return AudioType.UNKNOWN;
            }

            string extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            switch (extension)
            {
                case ".wav":
                    return AudioType.WAV;
                case ".mp3":
                    return AudioType.MPEG;
                case ".ogg":
                    return AudioType.OGGVORBIS;
                case ".aif":
                case ".aiff":
                    return AudioType.AIFF;
                default:
                    return AudioType.UNKNOWN;
            }
        }
    }
}
