using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public static class InputSystemEventSystemFix
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        FixLoadedEventSystems();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FixLoadedEventSystems();
    }

    private static void FixLoadedEventSystems()
    {
        EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < eventSystems.Length; i++)
        {
            EventSystem eventSystem = eventSystems[i];
            if (eventSystem == null)
            {
                continue;
            }

            bool hasInputSystemModule = eventSystem.GetComponent<InputSystemUIInputModule>() != null;
            StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();

            if (!hasInputSystemModule)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }

            if (legacyModule != null)
            {
                Object.Destroy(legacyModule);
            }
        }
    }
}
