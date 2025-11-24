using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RealGames;
using UnityEngine;
using UnityEngine.Networking;

public static class MusicControllerEvents
{
    public static Action<bool> OnUserAwnser;
}

public class MusicController : MonoBehaviour
{
    [Serializable]
    private class MusicDatabaseFile
    {
        public MusicEntry[] musicas;
    }

    [Serializable]
    private class MusicEntry
    {
        public string musica;
        public string autor;
        public string letra;
        public string arquivoImagemEmoji;
        public string arquivoMusica;
    }

    [Serializable]
    public class MusicData
    {
        public string musicName;
        public string musicAutor;
        [TextArea]
        public string musicLyric;
        public string emojiFileName;
        public string audioFileName;
        [NonSerialized]
        public Sprite musicEmoji;
        [NonSerialized]
        public AudioClip musicClip;

        public bool HasEmoji => musicEmoji != null;
        public bool HasClip => musicClip != null;
    }

    public static MusicController Instance { get; private set; }

    [SerializeField]
    private string databaseRelativePath = "Files/Data/music_database.json";

    [SerializeField]
    private List<MusicData> catalog = new List<MusicData>();

    public IReadOnlyList<MusicData> Catalog => catalog;
    public bool IsReady => isDatabaseLoaded;
    public bool HasCurrentMusic => currentMusic != null;
    public MusicData CurrentMusic => currentMusic;

    public event Action<MusicData> OnMusicChanged;

    private MusicData currentMusic;
    private int currentIndex = -1;
    private bool isDatabaseLoaded;
    private Coroutine databaseCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        MusicControllerEvents.OnUserAwnser += HandleUserAnswer;
    }

    private void OnDisable()
    {
        MusicControllerEvents.OnUserAwnser -= HandleUserAnswer;
    }

    private void Start()
    {
        if (databaseCoroutine == null)
        {
            databaseCoroutine = StartCoroutine(LoadDatabaseRoutine());
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool TryGetCurrentMusic(out MusicData music)
    {
        music = currentMusic;
        return music != null;
    }

    public bool TryAdvanceToNextMusic()
    {
        if (!isDatabaseLoaded || catalog.Count == 0)
        {
            return false;
        }

        currentIndex = (currentIndex + 1) % catalog.Count;
        currentMusic = catalog[currentIndex];
        OnMusicChanged?.Invoke(currentMusic);
        return true;
    }

    public MusicData GetRandomMusic()
    {
        if (!isDatabaseLoaded || catalog.Count == 0)
        {
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, catalog.Count);
        Debug.Log($"[MusicController] Musica aleatoria selecionada: {catalog[randomIndex].musicName}.");
        return catalog[randomIndex];
    }

    private void HandleUserAnswer(bool _)
    {
        // Hook reservado para logica futura de pontuacao ou feedback.
    }

    private IEnumerator LoadDatabaseRoutine()
    {
        string fullPath = ResolveDatabasePath();
        string json;

#if UNITY_ANDROID && !UNITY_EDITOR
        using (UnityWebRequest request = UnityWebRequest.Get(fullPath))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[MusicController] Falha ao carregar base de musicas: {request.error} ({fullPath})");
                yield break;
            }

            json = request.downloadHandler.text;
        }
#else
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"[MusicController] Base de musicas nao encontrada em {fullPath}");
            yield break;
        }

        json = File.ReadAllText(fullPath);
#endif

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogError("[MusicController] Base de musicas vazia.");
            yield break;
        }

        MusicDatabaseFile database;
        try
        {
            database = JsonUtility.FromJson<MusicDatabaseFile>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MusicController] Erro ao interpretar base de musicas: {ex.Message}");
            yield break;
        }

        if (database?.musicas == null || database.musicas.Length == 0)
        {
            Debug.LogWarning("[MusicController] Nenhuma musica encontrada na base.");
            yield break;
        }

        foreach (MusicEntry entry in database.musicas)
        {
            if (entry == null)
            {
                continue;
            }

            string name = entry.musica?.Trim();
            string audioFile = entry.arquivoMusica?.Trim();
            string emojiFile = entry.arquivoImagemEmoji?.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(audioFile) || string.IsNullOrEmpty(emojiFile))
            {
                Debug.LogWarning("[MusicController] Registro de musica ignorado por dados obrigatorios ausentes.");
                continue;
            }

            MusicData data = new MusicData
            {
                musicName = name,
                musicAutor = entry.autor?.Trim() ?? string.Empty,
                musicLyric = entry.letra ?? string.Empty,
                audioFileName = audioFile,
                emojiFileName = emojiFile
            };

            yield return LoadEmojiForData(data);
            if (!data.HasEmoji)
            {
                Debug.LogWarning($"[MusicController] Emoji '{emojiFile}' nao encontrado para '{name}'. Musica ignorada.");
                continue;
            }

            yield return LoadAudioForData(data);
            if (!data.HasClip)
            {
                Debug.LogWarning($"[MusicController] Audio '{audioFile}' nao encontrado para '{name}'. Musica ignorada.");
                continue;
            }

            catalog.Add(data);
        }

        isDatabaseLoaded = true;
        databaseCoroutine = null;

        if (catalog.Count == 0)
        {
            Debug.LogWarning("[MusicController] Nenhuma musica valida foi carregada.");
        }
        else if (currentMusic == null)
        {
            TryAdvanceToNextMusic();
        }
    }

    private string ResolveDatabasePath()
    {
        if (Path.IsPathRooted(databaseRelativePath))
        {
            return databaseRelativePath.Replace("\\", "/");
        }

        string combined = Path.Combine(Application.streamingAssetsPath, databaseRelativePath);
        return combined.Replace("\\", "/");
    }

    private IEnumerator LoadEmojiForData(MusicData data)
    {
        bool isDone = false;

        var loader = new FileLoader(data.emojiFileName);
        loader.LoadSprite(sprite =>
        {
            data.musicEmoji = sprite;
            isDone = true;
        });

        while (!isDone)
        {
            yield return null;
        }
    }

    private IEnumerator LoadAudioForData(MusicData data)
    {
        bool isDone = false;

        var loader = new FileLoader(data.audioFileName);
        loader.LoadAudioClip(clip =>
        {
            data.musicClip = clip;
            isDone = true;
        });

        while (!isDone)
        {
            yield return null;
        }
    }
}
