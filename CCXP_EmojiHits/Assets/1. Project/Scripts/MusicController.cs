using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RealGames;
using UnityEngine;
using UnityEngine.Networking;

public static class MusicControllerEvents
{
    public static Action<String> OnUserAwnser; // acertou, errou, acabou
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
    public event Action OnDatabaseLoaded;

    private MusicData currentMusic;
    private int currentIndex = -1;
    private bool isDatabaseLoaded;
    private Coroutine databaseCoroutine;
    private Coroutine musicLoadCoroutine;
    private readonly Dictionary<string, Sprite> emojiCache = new();
    private readonly Dictionary<string, AudioClip> audioCache = new();
    private int lastRandomIndex = -1;

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

        int nextIndex = (currentIndex + 1) % catalog.Count;
        StartMusicLoad(nextIndex);
        return true;
    }

    public MusicData GetRandomMusic()
    {
        if (!isDatabaseLoaded || catalog.Count == 0)
        {
            return null;
        }

        if (catalog.Count == 1)
        {
            StartMusicLoad(0);
            return catalog[0];
        }

        int randomIndex;
        do
        {
            randomIndex = UnityEngine.Random.Range(0, catalog.Count);
        }
        while (randomIndex == lastRandomIndex);

        lastRandomIndex = randomIndex;
        StartMusicLoad(randomIndex);
        Debug.Log($"[MusicController] Musica aleatoria selecionada: {catalog[randomIndex].musicName}.");
        return catalog[randomIndex];
    }

    private void HandleUserAnswer(string _)
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

            catalog.Add(data);
            yield return new WaitForEndOfFrame();
        }



        isDatabaseLoaded = true;
        databaseCoroutine = null;
        OnDatabaseLoaded?.Invoke();

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

    private void StartMusicLoad(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= catalog.Count)
        {
            return;
        }

        if (musicLoadCoroutine != null)
        {
            StopCoroutine(musicLoadCoroutine);
        }

        musicLoadCoroutine = StartCoroutine(PrepareAndApplyMusic(targetIndex));
    }

    private IEnumerator PrepareAndApplyMusic(int index)
    {
        if (index < 0 || index >= catalog.Count)
        {
            musicLoadCoroutine = null;
            yield break;
        }

        MusicData data = catalog[index];
        yield return EnsureMusicAssets(data);

        if (!data.HasEmoji || !data.HasClip)
        {
            Debug.LogWarning($"[MusicController] Musica '{data.musicName}' ignorada devido a recursos faltantes.");
            musicLoadCoroutine = null;
            yield break;
        }

        currentIndex = index;
        currentMusic = data;
        OnMusicChanged?.Invoke(currentMusic);
        musicLoadCoroutine = null;
    }

    private IEnumerator EnsureMusicAssets(MusicData data)
    {
        if (data == null)
        {
            yield break;
        }

        if (!data.HasEmoji)
        {
            yield return LoadEmojiForData(data);
        }

        if (!data.HasClip)
        {
            yield return LoadAudioForData(data);
        }
    }

    private IEnumerator LoadEmojiForData(MusicData data)
    {
        if (data == null || string.IsNullOrEmpty(data.emojiFileName))
        {
            yield break;
        }

        if (emojiCache.TryGetValue(data.emojiFileName, out Sprite cached) && cached != null)
        {
            data.musicEmoji = cached;
            yield break;
        }

        bool isDone = false;

        var loader = new FileLoader(data.emojiFileName);
        loader.LoadSprite(sprite =>
        {
            if (sprite != null)
            {
                emojiCache[data.emojiFileName] = sprite;
            }

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
        if (data == null || string.IsNullOrEmpty(data.audioFileName))
        {
            yield break;
        }

        if (audioCache.TryGetValue(data.audioFileName, out AudioClip cached) && cached != null)
        {
            data.musicClip = cached;
            yield break;
        }

        bool isDone = false;

        var loader = new FileLoader(data.audioFileName);
        loader.LoadAudioClip(clip =>
        {
            if (clip != null)
            {
                audioCache[data.audioFileName] = clip;
            }

            data.musicClip = clip;
            isDone = true;
        });

        while (!isDone)
        {
            yield return null;
        }
    }
}
