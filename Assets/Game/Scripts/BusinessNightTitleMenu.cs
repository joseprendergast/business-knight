using UnityEngine;

namespace BusinessNight
{
    public sealed class BusinessNightTitleMenu : MonoBehaviour
    {
        [SerializeField] RectTransform newGameButton;
        [SerializeField] RectTransform continueButton;
        [SerializeField] RectTransform loadButton;
        [SerializeField] CanvasGroup menuGroup;
        [SerializeField] GameObject pressAnyButtonPrompt;

        Camera uiCamera;
        bool menuOpen;

        void Awake()
        {
            ShowCover();
        }

        public void NewGame()
        {
            gameObject.SetActive(false);
            BusinessNightSceneManager.Instance?.NewGame();
        }

        public void Continue()
        {
            gameObject.SetActive(false);
            BusinessNightSceneManager.Instance?.Continue();
        }

        public void LoadGame()
        {
            Continue();
        }

        void Update()
        {
            if (!menuOpen)
            {
                if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
                    ShowMenu();

                return;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.N))
            {
                NewGame();
                return;
            }

            if (!Input.GetMouseButtonDown(0))
                return;

            Vector2 normalized = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
            if (normalized.y > 0.54f && normalized.y < 0.68f)
            {
                NewGame();
                return;
            }

            Vector2 pointer = Input.mousePosition;
            if (IsPointerInside(newGameButton, pointer))
                NewGame();
            else if (IsPointerInside(continueButton, pointer))
                Continue();
            else if (IsPointerInside(loadButton, pointer))
                LoadGame();
        }

        void ShowCover()
        {
            menuOpen = false;
            if (menuGroup != null)
            {
                menuGroup.alpha = 0f;
                menuGroup.interactable = false;
                menuGroup.blocksRaycasts = false;
            }

            if (pressAnyButtonPrompt != null)
                pressAnyButtonPrompt.SetActive(true);
        }

        void ShowMenu()
        {
            menuOpen = true;
            if (menuGroup != null)
            {
                menuGroup.alpha = 1f;
                menuGroup.interactable = true;
                menuGroup.blocksRaycasts = true;
            }

            if (pressAnyButtonPrompt != null)
                pressAnyButtonPrompt.SetActive(false);
        }

        bool IsPointerInside(RectTransform target, Vector2 pointer)
        {
            return target != null && RectTransformUtility.RectangleContainsScreenPoint(target, pointer, uiCamera);
        }
    }
}
