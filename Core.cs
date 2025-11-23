using MelonLoader;
using UnityEngine;
using MimicAPI.GameAPI;
using Mimic;
using Mimic.Actors;
using TMPro;
using System.Linq;

[assembly: MelonInfo(typeof(TooltipMod.Core), "Tooltip Mod", "1.0.0", "TheConanRider", null)]
[assembly: MelonGame("ReLUGames", "MIMESIS")]

namespace TooltipMod
{
    public class Core : MelonMod
    {
        int lastId = -1;

        private TMP_FontAsset? desiredFontAsset;
        private const string TargetFontName = "LanaPixel SDF";
        private static TMP_Text? itemInfoTextComponent;
        private L10NManager? localisationManager;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
            CreateScalableCanvasWithGameFont();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            localisationManager = ManagerAPI.GetManager<L10NManager>("lcman");
            if (desiredFontAsset == null && itemInfoTextComponent != null)
            {
                LoadGameFont();
            }
        }

        public override void OnLateUpdate()
        {
            UpdateToolTip();
        }




        public void UpdateToolTip()
        {
            if (itemInfoTextComponent == null)
            {
                LoggerInstance.Error("Tried to UpdateToolTip while itemInfoTextComponent was null");
                return;
            }

            ProtoActor? player = PlayerAPI.GetLocalPlayer();
            if (player != null)
            {
                InventoryItem? item = player.GetSelectedInventoryItem();
                if (item != null)
                {
                    if (item.ItemMasterID != lastId && localisationManager != null)
                    {
                        string text = $"{localisationManager.GetText(item.MasterInfo.Name)} ${item.Price.ToString()} {item.MasterInfo.Weight / 1000f}kg";
                        itemInfoTextComponent.text = text;
                        itemInfoTextComponent.gameObject.SetActive(true);
                        lastId = item.ItemMasterID;
                    }
                }
                else
                {
                    itemInfoTextComponent.gameObject.SetActive(false);
                    lastId = -1;
                }
            }
            else 
            {
                itemInfoTextComponent.gameObject.SetActive(false);
                lastId = -1;
            }
        }

        public void LoadGameFont()
        {
            desiredFontAsset = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(font => font.name == TargetFontName);
            if (desiredFontAsset == null)
            {
                MelonLogger.Error($"Font not found in resources.");
                return;
            }

            itemInfoTextComponent.font = desiredFontAsset;
        }


        public void CreateScalableCanvasWithGameFont()
        {
            GameObject canvas = new GameObject("ModItemInfoCanvas");
            Canvas canvasComp = canvas.AddComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;

            UnityEngine.UI.CanvasScaler canvasScaler = canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(2556, 1440);
            canvasScaler.matchWidthOrHeight = 0.5f;

            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            UnityEngine.Object.DontDestroyOnLoad(canvas);

            GameObject itemInfoTextObject = new GameObject("ItemInfoTMP");
            itemInfoTextObject.transform.SetParent(canvas.transform, false);
            itemInfoTextComponent = itemInfoTextObject.AddComponent<TextMeshProUGUI>();

            RectTransform rect = itemInfoTextComponent.rectTransform;

            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(1f, 0f);

            rect.sizeDelta = new Vector2(400, 100);

            rect.anchoredPosition = new Vector2(690f, 70f);

            InitialiseTextMeshPro();
        }

        public void InitialiseTextMeshPro()
        {
            itemInfoTextComponent.alignment = TextAlignmentOptions.Left;
            itemInfoTextComponent.color = Color.white;
            itemInfoTextComponent.text = "Loading...";
            itemInfoTextComponent.gameObject.SetActive(false);
            itemInfoTextComponent.fontSize = 30f;
        }
    }
}