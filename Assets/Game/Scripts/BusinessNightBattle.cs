using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BusinessNight
{
    public sealed class BusinessNightBattle : MonoBehaviour
    {
        public static BusinessNightBattle Instance { get; private set; }

        [Header("Battle Art")]
        [SerializeField] Sprite playerSprite;
        [SerializeField] Sprite enemySprite;
        [SerializeField] Sprite targetMarkerSprite;

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
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject panel = Panel("BattlePanel", canvasObject.transform, new Color(0.011f, 0.014f, 0.019f, 0.985f));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.06f, 0.06f);
            panelRect.anchorMax = new Vector2(0.94f, 0.94f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            Outline panelOutline = panel.AddComponent<Outline>();
            panelOutline.effectColor = new Color32(236, 219, 178, 255);
            panelOutline.effectDistance = new Vector2(2f, -2f);
            battleGroup = panel.AddComponent<CanvasGroup>();

            Panel("TopMist", panel.transform, new Color(0.058f, 0.082f, 0.105f, 0.54f), new Vector2(0.02f, 0.76f), new Vector2(0.98f, 0.94f));
            Panel("WarmBacklight", panel.transform, new Color(0.48f, 0.22f, 0.06f, 0.32f), new Vector2(0.58f, 0.44f), new Vector2(0.91f, 0.71f));
            Panel("CoolBacklight", panel.transform, new Color(0.05f, 0.26f, 0.36f, 0.28f), new Vector2(0.09f, 0.27f), new Vector2(0.43f, 0.56f));
            Panel("ArenaFloor", panel.transform, new Color(0.033f, 0.039f, 0.045f, 0.96f), new Vector2(0.04f, 0.25f), new Vector2(0.96f, 0.39f));
            Panel("ArenaGoldLine", panel.transform, new Color32(255, 185, 47, 220), new Vector2(0.04f, 0.385f), new Vector2(0.96f, 0.394f));
            Panel("ArenaBlueLine", panel.transform, new Color32(58, 131, 160, 190), new Vector2(0.05f, 0.268f), new Vector2(0.54f, 0.276f));

            Text title = Label("BattleTitle", panel.transform, "BUSINESS DUEL", 22, new Vector2(0.04f, 0.885f), new Vector2(0.96f, 0.965f), TextAnchor.MiddleCenter, new Color32(255, 197, 50, 255), FontStyle.Bold);
            Shadow titleShadow = title.gameObject.AddComponent<Shadow>();
            titleShadow.effectColor = new Color32(0, 0, 0, 220);
            titleShadow.effectDistance = new Vector2(3f, -3f);
            Label("BattleKicker", panel.transform, "TURN-BASED PAPERWORK COMBAT", 10, new Vector2(0.04f, 0.848f), new Vector2(0.96f, 0.89f), TextAnchor.MiddleCenter, new Color32(137, 167, 176, 255), FontStyle.Bold);

            BuildStatusPlate(panel.transform, false);
            BuildStatusPlate(panel.transform, true);

            CharacterCard("PlayerCard", panel.transform, playerSprite, new Vector2(0.12f, 0.29f), new Vector2(0.39f, 0.66f), new Color32(42, 71, 86, 255), false);
            CharacterCard("EnemyCard", panel.transform, enemySprite, new Vector2(0.61f, 0.42f), new Vector2(0.87f, 0.75f), new Color32(206, 113, 34, 255), true);

            GameObject logPanel = Panel("BattleLogPanel", panel.transform, new Color(0.023f, 0.031f, 0.043f, 0.96f), new Vector2(0.08f, 0.105f), new Vector2(0.92f, 0.235f));
            Outline logOutline = logPanel.AddComponent<Outline>();
            logOutline.effectColor = new Color32(236, 219, 178, 255);
            logOutline.effectDistance = new Vector2(2f, -2f);
            battleLog = Label("BattleLog", logPanel.transform, "", 19, new Vector2(0.035f, 0.08f), new Vector2(0.965f, 0.92f), TextAnchor.MiddleLeft, new Color32(250, 240, 205, 255));

            GameObject commandDock = Panel("CommandDock", panel.transform, new Color(0.006f, 0.011f, 0.018f, 0.86f), new Vector2(0.08f, 0.02f), new Vector2(0.92f, 0.085f));
            Outline commandOutline = commandDock.AddComponent<Outline>();
            commandOutline.effectColor = new Color32(74, 97, 111, 255);
            commandOutline.effectDistance = new Vector2(1f, -1f);
            attackButton = BattleButton("Attack", commandDock.transform, new Vector2(0.015f, 0.14f), new Vector2(0.235f, 0.86f), Attack, new Color32(178, 60, 43, 255));
            focusButton = BattleButton("Focus", commandDock.transform, new Vector2(0.265f, 0.14f), new Vector2(0.485f, 0.86f), Focus, new Color32(43, 122, 153, 255));
            itemButton = BattleButton("Item", commandDock.transform, new Vector2(0.515f, 0.14f), new Vector2(0.735f, 0.86f), Item, new Color32(219, 152, 34, 255));
            fleeButton = BattleButton("Back", commandDock.transform, new Vector2(0.765f, 0.14f), new Vector2(0.985f, 0.86f), Flee, new Color32(98, 107, 118, 255));
        }

        void BuildStatusPlate(Transform parent, bool enemy)
        {
            Vector2 plateMin = enemy ? new Vector2(0.55f, 0.72f) : new Vector2(0.09f, 0.635f);
            Vector2 plateMax = enemy ? new Vector2(0.91f, 0.84f) : new Vector2(0.45f, 0.755f);
            GameObject plate = Panel(enemy ? "EnemyStatusPlate" : "PlayerStatusPlate", parent, new Color(0.016f, 0.024f, 0.035f, 0.93f), plateMin, plateMax);
            Outline outline = plate.AddComponent<Outline>();
            outline.effectColor = enemy ? new Color32(255, 181, 57, 255) : new Color32(100, 151, 174, 255);
            outline.effectDistance = new Vector2(2f, -2f);

            Text nameLabel = Label(enemy ? "EnemyName" : "PlayerName", plate.transform, enemy ? "Opponent" : "Ari Vale", 20, new Vector2(0.04f, 0.48f), new Vector2(0.96f, 0.95f), enemy ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft, new Color32(245, 240, 218, 255), FontStyle.Bold);
            Text hpLabel = Label(enemy ? "EnemyHP" : "PlayerHP", plate.transform, enemy ? "HP 36/36" : "HP 42/42", 13, new Vector2(0.04f, 0.04f), new Vector2(0.23f, 0.44f), TextAnchor.MiddleLeft, new Color32(255, 229, 150, 255), FontStyle.Bold);
            RectTransform fill = HpBar(enemy ? "EnemyHpBar" : "PlayerHpBar", plate.transform, new Vector2(0.24f, 0.12f), new Vector2(0.96f, 0.38f), new Color32(197, 48, 42, 255), enemy ? new Color32(255, 190, 42, 255) : new Color32(46, 132, 166, 255));

            if (enemy)
            {
                enemyName = nameLabel;
                enemyHpText = hpLabel;
                enemyHpFill = fill;
            }
            else
            {
                playerName = nameLabel;
                playerHpText = hpLabel;
                playerHpFill = fill;
            }
        }

        void CharacterCard(string name, Transform parent, Sprite sprite, Vector2 min, Vector2 max, Color32 accent, bool enemy)
        {
            GameObject shadow = Panel(name + "Shadow", parent, new Color(0f, 0f, 0f, 0.36f), min + new Vector2(0.018f, -0.022f), max + new Vector2(0.018f, -0.022f));
            shadow.transform.SetAsFirstSibling();

            GameObject card = Panel(name, parent, enemy ? new Color(0.09f, 0.045f, 0.035f, 0.9f) : new Color(0.024f, 0.044f, 0.061f, 0.9f), min, max);
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = accent;
            outline.effectDistance = new Vector2(2f, -2f);

            Panel(name + "Glow", card.transform, new Color(accent.r / 255f, accent.g / 255f, accent.b / 255f, 0.2f), new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.38f));
            Panel(name + "Ground", card.transform, new Color(0.005f, 0.008f, 0.012f, 0.72f), new Vector2(0.07f, 0.05f), new Vector2(0.93f, 0.17f));

            if (enemy && targetMarkerSprite != null)
            {
                Image marker = SpriteImage("TargetMarker", card.transform, targetMarkerSprite, new Vector2(0.64f, 0.58f), new Vector2(1.03f, 0.97f));
                marker.color = new Color(1f, 0.7f, 0.18f, 0.55f);
            }

            if (sprite != null)
            {
                Image portrait = SpriteImage("Portrait", card.transform, sprite, enemy ? new Vector2(0.08f, 0.12f) : new Vector2(0.12f, 0.1f), enemy ? new Vector2(0.92f, 0.95f) : new Vector2(0.88f, 0.96f));
                portrait.preserveAspect = true;
            }
            else
            {
                Label("FallbackGlyph", card.transform, enemy ? "P" : "A", 72, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, new Color32(245, 240, 218, 255), FontStyle.Bold);
            }
        }

        RectTransform HpBar(string name, Transform parent, Vector2 min, Vector2 max, Color32 fillA, Color32 fillB)
        {
            GameObject frame = Panel(name, parent, new Color(0.005f, 0.008f, 0.012f, 1f), min, max);
            Outline outline = frame.AddComponent<Outline>();
            outline.effectColor = new Color32(230, 218, 183, 255);
            outline.effectDistance = new Vector2(1f, -1f);

            GameObject fill = Panel("Fill", frame.transform, fillA);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            SetRect(fillRect, new Vector2(0.018f, 0.16f), new Vector2(0.982f, 0.84f));
            GameObject shine = Panel("Shine", fill.transform, fillB);
            SetRect(shine.GetComponent<RectTransform>(), new Vector2(0f, 0.55f), new Vector2(1f, 1f));
            return fillRect;
        }

        Button BattleButton(string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction action, Color32 accent)
        {
            GameObject go = Panel(text + "Button", parent, new Color(0.025f, 0.04f, 0.075f, 1f));
            SetRect(go.GetComponent<RectTransform>(), anchorMin, anchorMax);
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color32(236, 219, 178, 255);
            outline.effectDistance = new Vector2(1f, -1f);
            Panel("Accent", go.transform, accent, new Vector2(0f, 0f), new Vector2(0.035f, 1f));
            Button button = go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color32(7, 15, 28, 255);
            colors.highlightedColor = new Color32(64, 84, 104, 255);
            colors.pressedColor = new Color32(201, 96, 37, 255);
            button.colors = colors;
            button.onClick.AddListener(action);
            Label("Text", go.transform, text.ToUpperInvariant(), 15, new Vector2(0.06f, 0f), Vector2.one, TextAnchor.MiddleCenter, new Color32(245, 240, 218, 255), FontStyle.Bold);
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

        GameObject Panel(string name, Transform parent, Color color, Vector2 min, Vector2 max)
        {
            GameObject go = Panel(name, parent, color);
            SetRect(go.GetComponent<RectTransform>(), min, max);
            return go;
        }

        Image SpriteImage(string name, Transform parent, Sprite sprite, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
            SetRect(image.rectTransform, min, max);
            return image;
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
