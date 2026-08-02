using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FishMatch.Match3
{
    // Builds its row list at runtime (not editor-baked like the scene shells) because owned/affordable
    // state depends on save data and current gold, same reasoning as LevelMapController.
    public class DecorationShopController : MonoBehaviour
    {
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private RectTransform rowContainer;
        [SerializeField] private DecorationDefinition[] catalog;

        private const float RowHeight = 110f;

        private readonly List<GameObject> spawnedRows = new List<GameObject>();

        private void Awake()
        {
            openButton.onClick.AddListener(OpenShop);
            closeButton.onClick.AddListener(CloseShop);
            shopPanel.SetActive(false);
        }

        private void OnEnable()
        {
            CurrencyManager.OnGoldChanged += HandleGoldChanged;
        }

        private void OnDisable()
        {
            CurrencyManager.OnGoldChanged -= HandleGoldChanged;
        }

        private void HandleGoldChanged(int gold)
        {
            if (shopPanel.activeSelf)
            {
                RefreshRows();
            }
        }

        private void OpenShop()
        {
            shopPanel.SetActive(true);
            RefreshRows();
        }

        private void CloseShop()
        {
            shopPanel.SetActive(false);
        }

        private void RefreshRows()
        {
            foreach (var row in spawnedRows)
            {
                Destroy(row);
            }

            spawnedRows.Clear();

            if (catalog == null)
            {
                return;
            }

            for (int i = 0; i < catalog.Length; i++)
            {
                CreateRow(catalog[i], i);
            }
        }

        private void CreateRow(DecorationDefinition decoration, int index)
        {
            if (decoration == null)
            {
                return;
            }

            var rowGO = new GameObject($"Row_{decoration.decorationId}");
            rowGO.transform.SetParent(rowContainer, false);
            spawnedRows.Add(rowGO);

            var rect = rowGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -index * RowHeight);
            rect.sizeDelta = new Vector2(560f, 100f);

            var rowImage = rowGO.AddComponent<Image>();
            rowImage.color = new Color(0.15f, 0.2f, 0.3f, 0.85f);

            CreateNameLabel(rowGO.transform, decoration.displayName);
            CreateBuyButton(rowGO.transform, decoration);
        }

        private static void CreateNameLabel(Transform parent, string displayName)
        {
            var nameGO = new GameObject("Name");
            nameGO.transform.SetParent(parent, false);

            var nameText = nameGO.AddComponent<Text>();
            nameText.text = displayName;
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 32;
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.color = Color.white;

            var nameRect = nameGO.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0f);
            nameRect.anchorMax = new Vector2(0.55f, 1f);
            nameRect.offsetMin = new Vector2(20f, 0f);
            nameRect.offsetMax = Vector2.zero;
        }

        private void CreateBuyButton(Transform parent, DecorationDefinition decoration)
        {
            var buttonGO = new GameObject("BuyButton");
            buttonGO.transform.SetParent(parent, false);

            var buttonImage = buttonGO.AddComponent<Image>();

            var buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.58f, 0.15f);
            buttonRect.anchorMax = new Vector2(0.98f, 0.85f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;

            var button = buttonGO.AddComponent<Button>();

            var buttonTextGO = new GameObject("Text");
            buttonTextGO.transform.SetParent(buttonGO.transform, false);
            var buttonText = buttonTextGO.AddComponent<Text>();
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 28;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;

            var buttonTextRect = buttonTextGO.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;

            bool owned = DecorationOwnershipRegistry.IsOwned(decoration);
            if (owned)
            {
                buttonImage.color = new Color(0.3f, 0.3f, 0.3f);
                buttonText.text = "Sahip";
                button.interactable = false;
                return;
            }

            buttonText.text = $"{decoration.price} Altın";

            if (CurrencyManager.Gold >= decoration.price)
            {
                buttonImage.color = new Color(0.2f, 0.6f, 0.3f);
                button.interactable = true;
                button.onClick.AddListener(() => HandleBuyClicked(decoration));
            }
            else
            {
                buttonImage.color = new Color(0.5f, 0.2f, 0.2f);
                button.interactable = false;
            }
        }

        private void HandleBuyClicked(DecorationDefinition decoration)
        {
            if (!CurrencyManager.TrySpendGold(decoration.price))
            {
                return;
            }

            DecorationOwnershipRegistry.MarkOwnedById(decoration.decorationId);
            SaveManager.Save();
            RefreshRows();
        }
    }
}
