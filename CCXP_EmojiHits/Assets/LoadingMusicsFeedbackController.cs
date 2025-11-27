using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class LoadingMusicsFeedbackController : MonoBehaviour
{
    public UnityEvent onLoadingCompleted;

    private Coroutine subscriptionCoroutine;
    private bool isSubscribed;

    void OnEnable()
    {
        subscriptionCoroutine = StartCoroutine(SubscribeWhenControllerReady());
    }

    void OnDisable()
    {
        if (subscriptionCoroutine != null)
        {
            StopCoroutine(subscriptionCoroutine);
            subscriptionCoroutine = null;
        }

        if (isSubscribed && MusicController.Instance != null)
        {
            MusicController.Instance.OnDatabaseLoaded -= HandleDatabaseLoaded;
        }
        isSubscribed = false;
    }

    private void HandleDatabaseLoaded()
    {
        Debug.Log("[LoadingMusicsFeedbackController] Database de músicas carregado.");
        onLoadingCompleted?.Invoke();
    }

    private IEnumerator SubscribeWhenControllerReady()
    {
        while (MusicController.Instance == null)
        {
            yield return null;
        }

        MusicController.Instance.OnDatabaseLoaded += HandleDatabaseLoaded;
        isSubscribed = true;

        if (MusicController.Instance.IsReady)
        {
            HandleDatabaseLoaded();
        }
    }
}
