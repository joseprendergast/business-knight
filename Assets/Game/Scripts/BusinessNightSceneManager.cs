using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BusinessNight
{
    public sealed class BusinessNightSceneManager : MonoBehaviour
    {
        public static BusinessNightSceneManager Instance { get; private set; }

        [SerializeField] float defaultFadeSeconds = 0.28f;

        bool changingScene;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            BusinessNightGlobals.Instance?.VisitScene(SceneManager.GetActiveScene().name);
        }

        public void NewGame()
        {
            BusinessNightGlobals.Instance?.ResetProgress();
            BusinessNightGlobals.Instance?.SetFlag("m_gameStarted");
            ChangeScene("RoomPrototypeA", BusinessNightTransitionType.FadeToBlack);
        }

        public void Continue()
        {
            BusinessNightSaveData data = BusinessNightSaveSystem.Load();
            if (data == null)
            {
                NewGame();
                return;
            }

            BusinessNightGlobals.Instance?.Restore(data);
            ChangeScene(data.currentScene, BusinessNightTransitionType.FadeToBlack);
        }

        public void ChangeScene(string sceneId, BusinessNightTransitionType transition = BusinessNightTransitionType.FadeToBlack)
        {
            if (changingScene || string.IsNullOrWhiteSpace(sceneId))
                return;

            StartCoroutine(ChangeSceneRoutine(sceneId, transition));
        }

        IEnumerator ChangeSceneRoutine(string sceneId, BusinessNightTransitionType transition)
        {
            changingScene = true;

            if (transition != BusinessNightTransitionType.HardCut)
                yield return BusinessNightUi.Instance?.FadeOut(defaultFadeSeconds);

            SceneManager.LoadScene(sceneId, LoadSceneMode.Single);
            yield return null;

            BusinessNightGlobals.Instance?.VisitScene(sceneId);
            BusinessNightUi.Instance?.ForceClearFade();

            if (transition != BusinessNightTransitionType.HardCut)
                yield return BusinessNightUi.Instance?.FadeIn(defaultFadeSeconds);

            BusinessNightSaveSystem.Save(BusinessNightGlobals.Instance);
            changingScene = false;
        }
    }
}
