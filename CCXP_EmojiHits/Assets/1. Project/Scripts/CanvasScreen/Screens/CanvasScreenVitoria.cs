using TMPro;
using UnityEngine;

public class CanvasScreenVitoria : CanvasScreen
{

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void TurnOn()
    {
        base.TurnOn();
    }

    public override void TurnOff()
    {
        base.TurnOff();
    }

    private void HandleMusicChanged(MusicController.MusicData music)
    {
        if (IsOn())
        {
        }
    }

/*
    private void ApplyMusicData(MusicController.MusicData music)
    {
        if (musicLyricText != null)
        {
            musicLyricText.text = string.IsNullOrEmpty(music.musicLyric)
                ? "Letra indisponivel."
                : music.musicLyric;
        }

        if (audioSource != null)
        {
            audioSource.clip = music.musicClip;
            if (audioSource.clip != null)
            {
                audioSource.Play();
            }
        }
    }
    */
}
