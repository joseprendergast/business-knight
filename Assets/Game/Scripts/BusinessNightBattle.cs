using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BusinessNight
{
    public sealed class BusinessNightBattle : MonoBehaviour
    {
        public static BusinessNightBattle Instance { get; private set; }

        CanvasGroup battleGroup;
        Text playerName;
        Text enemyName;
        Text playerHpText;
        Text enemyHpText;
        Text battleLog;
        RectTransform playerHpFill;
        RectTransform enemyHpFill;
        Button attackButton;
        Button focusButton;
        Button itemButton;
        Button fleeButton;

        int playerHp;
        int enemyHp;
        bool active;
        bool busy;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildUi();
            HideImmediate();
        }

        public void StartBattle(string opponentName)
        {
            if (active)
                return;

            active = true;
            busy = false;
            playerHp = 42;
            enemyHp = 36;
            playerName.text = "Ari Vale";
            enemyName.text = opponentName;
            battleLog.text = $"{opponentName} blocks the corridor with a clipboard.";
            battleGroup.alpha = 1f;
            battleGroup.blocksRaycasts = true;
            battleGroup.interactable = true;
            SetAllInventoryVisible(false);
            Refresh();
        }

        public void Attack()
        {
            if (!CanAct())
                return;

            StartCoroutine(TurnRoutine("Audit Strike", 9, 6));
        }

        public void Focus()
        {
            if (!CanAct())
                return;

            StartCoroutine(TurnRoutine("Compliance Feint", 6, 4));
        }

        public void Item()
        {
            if (!CanAct())
                return;

            playerHp = Mathf.Min(42, playerHp + 8);
            battleLog.text = "Ari stamps a form in triplicate and regains 8 HP.";
            Refresh();
        }

        public void Flee()
        {
            if (!CanAct())
                return;

            battleLog.text = "Ari backs away from the paperwork duel.";
            EndBattle(false);
        }

        bool CanAct() => active && !busy && enemyHp > 0 && playerHp > 0;

        IEnumerator TurnRoutine(string attackName, int damage, int counterDamage)
        {
            busy = true;
            enemyHp = Mathf.Max(0, enemyHp - damage);
            battleLog.text = $"{attackName}! {enemyName.text} loses {damage} HP.";
            Refresh();
            yield return new WaitForSeconds(0.9f);

            if (enemyHp <= 0)
            {
                battleLog.text = $"{enemyName.text} yields and signs the corridor pass.";
                BusinessNightGlobals.Instance?.SetFlag("m_talkedToFirstCharacter");
                yield return new WaitForSeconds(1.0f);
                EndBattle(true);
                yield break;
            }

            playerHp = Mathf.Max(0, playerHp - counterDamage);
            battleLog.text = $"{enemyName.text} counters with Policy Citation. Ari loses {counterDamage} HP.";
            Refresh();
            yield return new WaitForSeconds(0.9f);

            if (playerHp <= 0)
            {
                battleLog.text = "Ari is defeated by process, not force.";
                yield return new WaitForSeconds(1.0f);
                EndBattle(false);
                yield break;
            }

            busy = false;
        }

        void EndBattle(bool won)
        {
            active = false;
            busy = false;
            HideImmediate();
            SetAllInventoryVisible(true);
            BusinessNightDialogue.Instance?.Say("Ari", won ? "That was legally a fight. I hate that it worked." : "I need a better argument before trying that again.");
        }

        void SetAllInventoryVisible(bool visible)
        {
            BusinessNightUi.Instance?.SetInventoryVisible(visible);
            foreach (BusinessNightUi ui in FindObjectsByType<BusinessNightUi>(FindObjectsSortMode.None))
                ui.SetInventoryVisible(visible);
        }

        void Refresh()
        {
            playerHpText.text = $"HP {playerHp}/42";
            enemyHpText.text = $"HP {enemyHp}/36";
            SetHpFill(playerHpFill, playerHp / 42f);
            SetHpFill(enemyHpFill, enemyHp / 36f);
        }

        void SetHpFill(RectTransform fill, float normalized)
        {
            if (fill == null)
                return;

            fill.anchorMax = new Vector2(Mathf.Clamp01(normalized), 1f);
        }

        void HideImmediate()
        {
            if (battleGroup == null)
                return;

            battleGroup.alpha = 0f;
            battleGroup.blocksRaycasts = false;
            battleGroup.interactable = false;
        }

        void BuildUi()
        {
            GameObject canvasObject = new GameObject("BattleCanvas");
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject panel = Panel("BattlePanel", canvasObject.transform, new Color(0.018f, 0.023f, 0.03f, 0.96f));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.08f, 0.08f);
            panelRect.anchorMax = new Vector2(0.92f, 0.92f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            Outline panelOutline = panel.AddComponent<Outline>();
            panelOutline.effectColor = new Color32(236, 219, 178, 255);
            panelOutline.effectDistance = new Vector2(2f, -2f);
            battleGroup = panel.AddComponent<CanvasGroup>();

            Label("BattleTitle", panel.transform, "BUSINESS DUEL", 28, new Vector2(0.08f, 0.86f), new Vector2(0.92f, 0.96f), TextAnchor.MiddleCenter, new Color32(255, 197, 50, 255), FontStyle.Bold);
            playerName = Label("PlayerName", panel.transform, "Ari Vale", 24, new Vector2(0.08f, 0.64f), new Vector2(0.42f, 0.72f), TextAnchor.MiddleLeft, new Color32(245, 240, 218, 255), FontStyle.Bold);
            enemyName = Label("EnemyName", panel.transform, "Opponent", 24, new Vector2(0.58f, 0.74f), new Vector2(0.92f, 0.82f), TextAnchor.MiddleRight, new Color32(245, 240, 218, 255), FontStyle.Bold);

            playerHpFill = HpBar("PlayerHpBar", panel.transform, new Vector2(0.08f, 0.58f), new Vector2(0.42f, 0.63f), new Color32(211, 52, 43, 255), new Color32(44, 122, 158, 255));
            enemyHpFill = HpBar("EnemyHpBar", panel.transform, new Vector2(0.58f, 0.68f), new Vector2(0.92f, 0.73f), new Color32(211, 52, 43, 255), new Color32(255, 190, 42, 255));
            playerHpText = Label("PlayerHP", panel.transform, "HP 42/42", 17, new Vector2(0.08f, 0.58f), new Vector2(0.42f, 0.63f), TextAnchor.MiddleCenter, new Color32(255, 248, 216, 255), FontStyle.Bold);
            enemyHpText = Label("EnemyHP", panel.transform, "HP 36/36", 17, new Vector2(0.58f, 0.68f), new Vector2(0.92f, 0.73f), TextAnchor.MiddleCenter, new Color32(255, 248, 216, 255), FontStyle.Bold);

            GameObject playerBlock = Panel("PlayerSpriteBlock", panel.transform, new Color(0.035f, 0.052f, 0.078f, 1f));
            SetRect(playerBlock.GetComponent<RectTransform>(), new Vector2(0.16f, 0.25f), new Vector2(0.34f, 0.5f));
            Outline playerOutline = playerBlock.AddComponent<Outline>();
            playerOutline.effectColor = new Color32(120, 150, 168, 255);
            playerOutline.effectDistance = new Vector2(2f, -2f);
            Label("AriSpriteGlyph", playerBlock.transform, "A", 80, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, new Color32(245, 240, 218, 255), FontStyle.Bold);
            Label("SwordGlyph", playerBlock.transform, "/", 62, new Vector2(0.62f, 0.08f), new Vector2(1f, 0.78f), TextAnchor.MiddleCenter, new Color32(255, 197, 50, 255), FontStyle.Bold);

            GameObject enemyBlock = Panel("EnemySpriteBlock", panel.transform, new Color(0.18f, 0.09f, 0.07f, 1f));
            SetRect(enemyBlock.GetComponent<RectTransform>(), new Vector2(0.66f, 0.42f), new Vector2(0.84f, 0.62f));
            Outline enemyOutline = enemyBlock.AddComponent<Outline>();
            enemyOutline.effectColor = new Color32(255, 184, 60, 255);
            enemyOutline.effectDistance = new Vector2(2f, -2f);
            Label("EnemySpriteGlyph", enemyBlock.transform, "P", 76, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, new Color32(255, 197, 50, 255), FontStyle.Bold);
            Label("AntlerGlyph", enemyBlock.transform, "Y", 42, new Vector2(0.05f, 0.56f), new Vector2(0.95f, 1f), TextAnchor.MiddleCenter, new Color32(184, 120, 51, 255), FontStyle.Bold);

            GameObject logPanel = Panel("BattleLogPanel", panel.transform, new Color(0.025f, 0.04f, 0.07f, 0.92f));
            SetRect(logPanel.GetComponent<RectTransform>(), new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.26f));
            Outline logOutline = logPanel.AddComponent<Outline>();
            logOutline.effectColor = new Color32(236, 219, 178, 255);
            logOutline.effectDistance = new Vector2(2f, -2f);
            battleLog = Label("BattleLog", logPanel.transform, "", 20, new Vector2(0.03f, 0.08f), new Vector2(0.97f, 0.92f));
            attackButton = BattleButton("Attack", panel.transform, new Vector2(0.08f, 0.02f), Attack);
            focusButton = BattleButton("Focus", panel.transform, new Vector2(0.31f, 0.02f), Focus);
            itemButton = BattleButton("Item", panel.transform, new Vector2(0.54f, 0.02f), Item);
            fleeButton = BattleButton("Back", panel.transform, new Vector2(0.77f, 0.02f), Flee);
        }

        RectTransform HpBar(string name, Transform parent, Vector2 min, Vector2 max, Color32 fillA, Color32 fillB)
        {
            GameObject frame = Panel(name, parent, new Color(0.005f, 0.008f, 0.012f, 1f));
            SetRect(frame.GetComponent<RectTransform>(), min, max);
            Outline outline = frame.AddComponent<Outline>();
            outline.effectColor = new Color32(236, 219, 178, 255);
            outline.effectDistance = new Vector2(2f, -2f);

            GameObject fill = Panel("Fill", frame.transform, fillA);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            SetRect(fillRect, new Vector2(0.015f, 0.18f), new Vector2(0.985f, 0.82f));
            GameObject shine = Panel("Shine", fill.transform, fillB);
            SetRect(shine.GetComponent<RectTransform>(), new Vector2(0f, 0.55f), new Vector2(1f, 1f));
            return fillRect;
        }

        Button BattleButton(string text, Transform parent, Vector2 anchorMin, UnityEngine.Events.UnityAction action)
        {
            GameObject go = Panel(text + "Button", parent, new Color(0.025f, 0.04f, 0.075f, 1f));
            SetRect(go.GetComponent<RectTransform>(), anchorMin, anchorMin + new Vector2(0.15f, 0.08f));
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color32(236, 219, 178, 255);
            outline.effectDistance = new Vector2(2f, -2f);
            Button button = go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color32(7, 15, 28, 255);
            colors.highlightedColor = new Color32(64, 84, 104, 255);
            colors.pressedColor = new Color32(201, 96, 37, 255);
            button.colors = colors;
            button.onClick.AddListener(action);
            Label("Text", go.transform, text.ToUpperInvariant(), 18, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, new Color32(245, 240, 218, 255), FontStyle.Bold);
            return button;
        }

        Text Label(string name, Transform parent, string text, int size, Vector2 min, Vector2 max, TextAnchor anchor = TextAnchor.MiddleLeft, Color32? color = null, FontStyle style = FontStyle.Normal)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text label = go.AddComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = size;
            label.fontStyle = style;
            label.color = color ?? new Color32(246, 237, 204, 255);
            label.alignment = anchor;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(label.rectTransform, min, max);
            return label;
        }

        GameObject Panel(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = color;
            return go;
        }

        void SetRect(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
