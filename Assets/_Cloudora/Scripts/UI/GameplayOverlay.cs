using System;
using UnityEngine;
using UnityEngine.UI;

namespace Cloudora.UI
{
    public enum RestartCost { Free, Life, Ad, NoLives }

    public sealed class GameplayOverlay : MonoBehaviour
    {
        private Button _undoButton;
        private Button _shuffleButton;
        private Button _extraButton;
        private GameObject _completePanel;
        private Text _levelLabel;
        private Text _tutorialLabel;
        private Text _livesLabel;
        private Text _adHintLabel;
        private Button _restartButton;
        private Text _undoBadge;
        private Text _extraBadge;
        private Text _shuffleBadge;
        private Text _restartBadge;
        private GameObject _blockingPanel;
        private float _pulse;

        private void Update()
        {
            if (_tutorialLabel == null || string.IsNullOrEmpty(_tutorialLabel.text)) return;
            _pulse += Time.unscaledDeltaTime * 4f;
            _tutorialLabel.transform.localScale = Vector3.one * (1f + Mathf.Sin(_pulse) * 0.035f);
        }

        public static GameplayOverlay Create(Canvas canvas, Action onRestart, Action onUndo, Action onContinue, Action onWorld, Action onExtraCloud, Action onSafeShuffle, Action onMenu)
        {
            var root = new GameObject("GameplayOverlay", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
            SafeAreaFitter.Attach(root);

            var overlay = root.AddComponent<GameplayOverlay>();
            overlay._levelLabel = CreateTopLabel(root.transform, "Level 1", new Vector2(0f, -70f), 42);
            overlay._livesLabel = CreateLivesLabel(root.transform);
            // Sadece bir rozet AD durumundayken görünür; AD'nin anlamını açıklar.
            overlay._adHintLabel = CreateTopLabel(root.transform, "<color=#F29A2E><b>AD</b></color>  Watch an ad to refill", new Vector2(0f, -125f), 26);
            overlay._adHintLabel.gameObject.SetActive(false);
            overlay._tutorialLabel = CreateTopLabel(root.transform, string.Empty, new Vector2(0f, -180f), 30);

            // Alt butonlar: 3 sütun x 2 sıra, eşit boyut ve aralık.
            overlay._restartButton = CreateButton(root.transform, "Restart", GridPosition(0, 1), onRestart);
            overlay._undoButton = CreateButton(root.transform, "Undo", GridPosition(1, 1), onUndo);
            CreateButton(root.transform, "World", GridPosition(2, 1), onWorld);
            overlay._extraButton = CreateButton(root.transform, "Extra", GridPosition(0, 0), onExtraCloud, SecondaryButtonColor);
            overlay._shuffleButton = CreateButton(root.transform, "Shuffle", GridPosition(1, 0), onSafeShuffle, SecondaryButtonColor);
            CreateButton(root.transform, "Menu", GridPosition(2, 0), onMenu, SecondaryButtonColor);
            overlay._restartBadge = CreateBadge(overlay._restartButton.transform);
            overlay._undoBadge = CreateBadge(overlay._undoButton.transform);
            overlay._extraBadge = CreateBadge(overlay._extraButton.transform);
            overlay._shuffleBadge = CreateBadge(overlay._shuffleButton.transform);
            overlay._completePanel = CreateCompletePanel(root.transform, onContinue);
            overlay._blockingPanel = CreateBlockingPanel(root.transform);
            overlay._completePanel.SetActive(false);
            overlay._blockingPanel.SetActive(false);
            return overlay;
        }

        public void SetUndoAvailable(bool available) => _undoButton.interactable = available;
        public void SetShuffleAvailable(bool available) => _shuffleButton.interactable = available;

        // Limit dolunca soluk görünür ama basılabilir kalır (basınca "Sky Is Full" mesajı çıkar).
        public void SetExtraAvailable(bool available)
        {
            if (!_extraButton.TryGetComponent(out CanvasGroup group)) group = _extraButton.gameObject.AddComponent<CanvasGroup>();
            group.alpha = available ? 1f : 0.45f;
        }
        public void ShowComplete(bool show) => _completePanel.SetActive(show);
        public void SetLevelInfo(int level, string world) => _levelLabel.text = $"{ToTitle(world)}  •  Level {level}";
        public void SetTutorialCue(string cue) => _tutorialLabel.text = string.IsNullOrWhiteSpace(cue) ? string.Empty : cue;
        public void SetLives(int lives, string countdown) => _livesLabel.text = lives >= 5 ? "5" : $"{lives}  {countdown}";
        // Köşe rozetleri: eldeki ücretsiz hak; hak 0 ve reklam varsa turuncu "AD".
        public void SetBoosters(int undo, int extra, int shuffle, bool adAvailable)
        {
            bool anyAd = SetChargeBadge(_undoBadge, undo, adAvailable);
            anyAd |= SetChargeBadge(_extraBadge, extra, adAvailable);
            anyAd |= SetChargeBadge(_shuffleBadge, shuffle, adAvailable);
            _boosterAd = anyAd;
            RefreshAdHint();
        }

        // Restart rozeti: bedavaysa gizli, can gidecekse kalp + "-1", can yoksa "AD" (reklam varsa) veya kalp + "0".
        public void SetRestartCost(RestartCost cost)
        {
            if (cost == _restartCost) return;
            _restartCost = cost;
            switch (cost)
            {
                case RestartCost.Life: SetBadge(_restartBadge, "-1", LifeBadgeColor, true); break;
                case RestartCost.Ad: SetBadge(_restartBadge, "AD", AdBadgeColor); break;
                case RestartCost.NoLives: SetBadge(_restartBadge, "0", LifeBadgeColor, true); break;
                default: SetBadge(_restartBadge, null, LifeBadgeColor); break;
            }
            RefreshAdHint();
        }

        private bool _boosterAd;
        private RestartCost _restartCost = (RestartCost)(-1);

        private void RefreshAdHint() => _adHintLabel.gameObject.SetActive(_boosterAd || _restartCost == RestartCost.Ad);

        private static bool SetChargeBadge(Text badge, int charges, bool adAvailable)
        {
            bool ad = charges <= 0 && adAvailable;
            SetBadge(badge, ad ? "AD" : charges.ToString(), ad ? AdBadgeColor : Color.white);
            return ad;
        }

        // heart: rozetin solunda kalp ikonu (Restart'ın can rozeti için).
        private static void SetBadge(Text badge, string value, Color background, bool heart = false)
        {
            GameObject go = badge.transform.parent.gameObject;
            go.SetActive(!string.IsNullOrEmpty(value));
            if (string.IsNullOrEmpty(value)) return;
            badge.text = value;
            go.GetComponent<Image>().color = background;
            badge.color = background == Color.white ? new Color(0.11f, 0.16f, 0.21f) : Color.white;
            go.transform.Find("Heart").gameObject.SetActive(heart);
            float iconSpace = heart ? BadgeHeartSize + 4f : 0f;
            badge.rectTransform.offsetMin = new Vector2(heart ? 12f + iconSpace : 0f, 0f);
            badge.rectTransform.offsetMax = new Vector2(heart ? -12f : 0f, 0f);
            float width = heart ? 24f + iconSpace + badge.preferredWidth : Mathf.Max(BadgeHeight, badge.preferredWidth + 22f);
            ((RectTransform)go.transform).sizeDelta = new Vector2(width, BadgeHeight);
        }

        // Sağ üstte can: kalp ikonu + sayı / geri sayım.
        private static Text CreateLivesLabel(Transform parent)
        {
            Text text = CreateTopLabel(parent, "5", new Vector2(420f, -70f), 34);
            text.alignment = TextAnchor.MiddleLeft;
            text.rectTransform.sizeDelta = new Vector2(240f, 60f);
            Image heart = CreateHeart(text.transform, 34f);
            RectTransform rect = heart.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(-24f, 0f);
            heart.color = new Color(0.88f, 0.32f, 0.42f);
            return text;
        }

        private static Image CreateHeart(Transform parent, float size)
        {
            var go = new GameObject("Heart", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.sprite = CloudoraVisuals.Heart;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.rectTransform.sizeDelta = new Vector2(size, size);
            return image;
        }
        public void ShowBlock(string title, string detail)
        {
            _blockingPanel.SetActive(true);
            Text[] labels = _blockingPanel.GetComponentsInChildren<Text>();
            labels[0].text = title;
            labels[1].text = detail;
        }
        public void HideBlock() => _blockingPanel.SetActive(false);

        private static Text CreateTopLabel(Transform parent, string value, Vector2 position, int size)
        {
            Text text = CreateText(parent, value, size, TextAnchor.MiddleCenter);
            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(880f, 60f);
            rect.anchoredPosition = position;
            text.color = new Color(0.15f, 0.22f, 0.29f);
            return text;
        }

        private static string ToTitle(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "Cloudora"
                : System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.Replace('-', ' '));
        }

        private static readonly Color AdBadgeColor = new Color(0.95f, 0.60f, 0.18f);
        private static readonly Color LifeBadgeColor = new Color(0.88f, 0.32f, 0.42f);
        private const float BadgeHeight = 38f;
        private const float BadgeHeartSize = 22f;

        // Butonun sağ üst köşesine taşan küçük hap şeklinde rozet.
        private static Text CreateBadge(Transform button)
        {
            var go = new GameObject("Badge", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(button, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(-10f, -2f);
            rect.sizeDelta = new Vector2(BadgeHeight, BadgeHeight);
            Image image = go.GetComponent<Image>();
            image.sprite = CloudoraVisuals.Pill;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = CloudoraVisuals.PillRadius * 2f / BadgeHeight;
            image.raycastTarget = false;
            Shadow shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.05f, 0.10f, 0.25f, 0.30f);
            shadow.effectDistance = new Vector2(0f, -2f);
            Image heart = CreateHeart(go.transform, BadgeHeartSize);
            heart.rectTransform.anchorMin = heart.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            heart.rectTransform.anchoredPosition = new Vector2(12f + BadgeHeartSize / 2f, 0f);
            heart.gameObject.SetActive(false);
            Text text = CreateText(go.transform, string.Empty, 24, TextAnchor.MiddleCenter);
            text.fontStyle = FontStyle.Bold;
            go.SetActive(false);
            return text;
        }

        private static readonly Color PrimaryButtonColor = new Color(0.27f, 0.40f, 0.92f);
        private static readonly Color SecondaryButtonColor = new Color(0.20f, 0.56f, 0.72f);
        private static readonly Vector2 ButtonSize = new Vector2(300f, 84f);
        private const float ButtonGap = 36f;
        private const float ButtonBottom = 28f;

        private static Vector2 GridPosition(int column, int row)
            => new Vector2((column - 1) * (ButtonSize.x + ButtonGap), ButtonBottom + row * (ButtonSize.y + 20f));

        private static Button CreateButton(Transform parent, string label, Vector2 position, Action action)
            => CreateButton(parent, label, position, action, PrimaryButtonColor);

        private static Button CreateButton(Transform parent, string label, Vector2 position, Action action, Color color)
        {
            var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = position;
            rect.sizeDelta = ButtonSize;
            go.GetComponent<Image>().color = color;
            Shadow shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.08f, 0.15f, 0.35f, 0.30f);
            shadow.effectDistance = new Vector2(0f, -5f);
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(() => action?.Invoke());
            CreateText(go.transform, label, 34, TextAnchor.MiddleCenter);
            return button;
        }

        private static GameObject CreateCompletePanel(Transform parent, Action onContinue)
        {
            GameObject root = CreateDimmedCard(parent, "LevelCompletePanel", new Vector2(760f, 440f), 0f, out GameObject panel);

            Text title = CreateText(panel.transform, "Sky Balanced!", 58, TextAnchor.MiddleCenter);
            title.color = new Color(0.15f, 0.22f, 0.29f);
            title.rectTransform.anchorMin = new Vector2(0.1f, 0.55f);
            title.rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
            title.rectTransform.offsetMin = title.rectTransform.offsetMax = Vector2.zero;
            CreateButton(panel.transform, "Continue", new Vector2(0f, 58f), onContinue);
            return root;
        }

        private static GameObject CreateBlockingPanel(Transform parent)
        {
            // Alttaki butonlar (Restart/Undo/Extra/Shuffle) kullanılabilsin diye karartma onları örtmez.
            GameObject root = CreateDimmedCard(parent, "BlockingPanel", new Vector2(760f, 380f), ButtonBottom + 2f * ButtonSize.y + 40f, out GameObject panel);
            root.AddComponent<Button>().onClick.AddListener(() => root.SetActive(false));
            Text title = CreateText(panel.transform, "No Moves", 50, TextAnchor.MiddleCenter);
            title.color = new Color(0.15f, 0.22f, 0.29f);
            title.rectTransform.anchorMin = new Vector2(0.08f, 0.55f);
            title.rectTransform.anchorMax = new Vector2(0.92f, 0.88f);
            title.rectTransform.offsetMin = title.rectTransform.offsetMax = Vector2.zero;
            Text detail = CreateText(panel.transform, "Use a safe option below", 29, TextAnchor.MiddleCenter);
            detail.color = new Color(0.25f, 0.34f, 0.44f);
            detail.rectTransform.anchorMin = new Vector2(0.08f, 0.18f);
            detail.rectTransform.anchorMax = new Vector2(0.92f, 0.55f);
            detail.rectTransform.offsetMin = detail.rectTransform.offsetMax = Vector2.zero;
            return root;
        }

        // Tam ekran yarı saydam karartma + ortada kart.
        private static GameObject CreateDimmedCard(Transform parent, string name, Vector2 size, float bottomInset, out GameObject card)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = new Vector2(0f, bottomInset);
            rootRect.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0.06f, 0.12f, 0.24f, 0.45f);

            card = new GameObject("Card", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(root.transform, false);
            RectTransform rect = card.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            card.GetComponent<Image>().color = new Color(0.97f, 0.98f, 1f);
            Shadow shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.05f, 0.10f, 0.25f, 0.30f);
            shadow.effectDistance = new Vector2(0f, -10f);
            return root;
        }

        private static Text CreateText(Transform parent, string value, int size, TextAnchor alignment)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }
    }
}
