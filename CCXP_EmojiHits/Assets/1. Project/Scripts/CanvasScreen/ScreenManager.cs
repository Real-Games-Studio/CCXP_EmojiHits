using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public static class ScreenManager
{
    public const string BroadcastAllScreensKeyword = "__ALL__";
    public static Action OnReset;
    public static Action<string> CallScreen;
    public static string currentScreenName;
    public static string NormalizeName(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public static bool NamesMatch(string a, string b)
    {
        return string.Equals(NormalizeName(a), NormalizeName(b), StringComparison.OrdinalIgnoreCase);
    }
    public static void SetCallScreen(string name)
    {
        string normalizedName = NormalizeName(name);

        if (string.IsNullOrEmpty(normalizedName))
        {
            Debug.LogWarning("[ScreenManager] Tentativa de chamar tela com nome vazio.");
            return;
        }

        CallScreen?.Invoke(normalizedName);
        currentScreenName = normalizedName;
    }

    public static void TurnOnCanvasGroup(CanvasGroup c)
    {
        c.alpha = 1;
        c.interactable = true;
        c.blocksRaycasts = true;
    }

    public static void TurnOffCanvasGroup(CanvasGroup c)
    {
        c.alpha = 0;
        c.interactable = false;
        c.blocksRaycasts = false;
    }
}
