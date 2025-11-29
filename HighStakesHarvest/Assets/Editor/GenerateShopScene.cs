using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

/// <summary>
/// One-click generator for the standalone ShopScene and its UI/prefab wiring.
/// Run from menu: Tools/Shop/Generate Shop Scene
/// </summary>
public static class GenerateShopScene
{
    private const string ScenePath = "Assets/Scenes/ShopScene.unity";
    private const string PrefabFolder = "Assets/Prefabs/Shop";
    private const string ItemSlotPrefabPath = PrefabFolder + "/ItemSlot.prefab";
    private const string PixelFontPath = "Assets/Fonts/GNF.asset"; // Project's pixel-style TMP font
    private const string MusicClipPath = "Assets/Casual & Relaxing Game Music/Happy.wav"; // Default casino track
    private const bool UseExistingManagers = true; // true = rely on already-persistent managers

    // Palette lifted from Casino scene screenshot
    private static readonly Color32 ForestGreen = new Color32(16, 74, 16, 255);
    private static readonly Color32 DirtBrown = new Color32(90, 45, 22, 255);
    private static readonly Color32 PanelWhite = new Color32(245, 245, 245, 255);
    private static readonly Color32 BorderBlack = new Color32(0, 0, 0, 255);
    private static readonly Color32 MoneyGold = new Color32(220, 170, 0, 255);

    [MenuItem("Tools/Shop/Generate Shop Scene")]
    public static void Generate()
    {
        if (!EditorUtility.DisplayDialog("Generate Shop Scene",
                "This will build ShopScene with Buy/Sell panels and the Item Slot prefab.",
                "Generate", "Cancel"))
        {
            return;
        }

        EnsureFolders();
        GameObject itemSlotPrefab = CreateOrUpdateItemSlotPrefab();

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject canvas = CreateCanvas();
        CreateBackground(canvas.transform);
        CreateEventSystem();
        CreateCamera();
        SafeRun("music source", CreateMusicSource);
        if (!UseExistingManagers)
        {
            SafeRun("manager bootstrap", CreateManagerBootstrap);
        }

        CreateTopBar(canvas.transform, out Button closeButton, out TextMeshProUGUI moneyTextTMP);
        CreateTabButtons(canvas.transform, out Button buyTabButton, out Button sellTabButton);

        GameObject buyPanel = CreateScrollPanel("BuyPanel", canvas.transform, out Transform buyContainer);
        GameObject sellPanel = CreateScrollPanel("SellPanel", canvas.transform, out Transform sellContainer);
        sellPanel.SetActive(false);

        CreateNotifications(canvas.transform, out GameObject insufficientFundsPanel,
            out GameObject insufficientSpacePanel, out GameObject purchaseSuccessPanel,
            out TextMeshProUGUI purchaseSuccessTextTMP);

        GameObject manager = new GameObject("ShopManager");
        CasinoShop shop = manager.AddComponent<CasinoShop>();
        LinkShopReferences(shop, itemSlotPrefab, buyTabButton, sellTabButton, buyPanel, sellPanel,
            buyContainer, sellContainer, closeButton, moneyTextTMP, insufficientFundsPanel,
            insufficientSpacePanel, purchaseSuccessPanel, purchaseSuccessTextTMP);

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Shop Scene Generated",
            "ShopScene saved to Assets/Scenes and ItemSlot prefab to Assets/Prefabs/Shop.", "OK");
    }

    private static void LinkShopReferences(CasinoShop shop, GameObject itemSlotPrefab,
        Button buyTabButton, Button sellTabButton, GameObject buyPanelRoot, GameObject sellPanelRoot,
        Transform buyContainer, Transform sellContainer, Button closeButton, TextMeshProUGUI moneyTextTMP,
        GameObject insufficientFundsPanel, GameObject insufficientSpacePanel, GameObject purchaseSuccessPanel,
        TextMeshProUGUI purchaseSuccessTextTMP)
    {
        SerializedObject so = new SerializedObject(shop);
        SetRef(so, "closeShopButton", closeButton);
        SetRef(so, "buyTabButton", buyTabButton);
        SetRef(so, "sellTabButton", sellTabButton);
        SetRef(so, "buyPanelRoot", buyPanelRoot);
        SetRef(so, "sellPanelRoot", sellPanelRoot);
        SetRef(so, "itemSlotPrefab", itemSlotPrefab);
        SetRef(so, "buySlotContainer", buyContainer);
        SetRef(so, "sellSlotContainer", sellContainer);
        SetRef(so, "moneyDisplayTextTMP", moneyTextTMP);
        SetRef(so, "insufficientFundsPanel", insufficientFundsPanel);
        SetRef(so, "insufficientSpacePanel", insufficientSpacePanel);
        SetRef(so, "purchaseSuccessPanel", purchaseSuccessPanel);
        SetRef(so, "purchaseSuccessTextTMP", purchaseSuccessTextTMP);
        SetFloat(so, "notificationDuration", 2f);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetRef(SerializedObject so, string name, Object value)
    {
        SerializedProperty prop = so.FindProperty(name);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
        }
        else
        {
            Debug.LogWarning($"CasinoShop property not found: {name}");
        }
    }

    private static void SetFloat(SerializedObject so, string name, float value)
    {
        SerializedProperty prop = so.FindProperty(name);
        if (prop != null)
        {
            prop.floatValue = value;
        }
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        if (!AssetDatabase.IsValidFolder(PrefabFolder))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Shop");
        }
    }

    private static GameObject CreateCanvas()
    {
        GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.localScale = Vector3.one;
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        return canvasGO;
    }

    private static void CreateBackground(Transform parent)
    {
        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        RectTransform rt = SetupRectTransform(bg, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        rt.localScale = Vector3.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = bg.GetComponent<Image>();
        img.color = ForestGreen;
        img.raycastTarget = false;
    }

    private static void CreateEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private static void CreateCamera()
    {
        if (Camera.main != null) return;
        GameObject camGO = new GameObject("MainCamera", typeof(Camera), typeof(AudioListener));
        Camera cam = camGO.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = ForestGreen;
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        camGO.tag = "MainCamera";
    }

    private static void CreateMusicSource()
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicClipPath);
        if (clip == null)
        {
            Debug.LogWarning($"Shop generator: music clip not found at {MusicClipPath}. Assign a clip or update MusicClipPath.");
            return;
        }

        GameObject audioGO = new GameObject("ShopMusic", typeof(AudioSource));
        AudioSource source = audioGO.GetComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = true;
        source.volume = 0.3f;
    }

    private static void CreateManagerBootstrap()
    {
        // Item Database
        GameObject dbGO = new GameObject("ItemDatabase");
        ItemDatabase db = dbGO.AddComponent<ItemDatabase>();
        PopulateItemDatabase(db);

        // Money Manager
        GameObject moneyGO = new GameObject("MoneyManager");
        MoneyManager money = moneyGO.AddComponent<MoneyManager>();
        SetPrivateInt(money, "startingMoney", 100);
        money.SetMoney(100);

        // Inventory / PlayerInventory
        GameObject invGO = new GameObject("InventoryManager");
        PlayerInventory playerInv = invGO.AddComponent<PlayerInventory>();
        invGO.AddComponent<InventoryManager>();
        SetPrivateInt(playerInv, "totalSlots", 36);
        SetPrivateInt(playerInv, "hotbarSize", 10);

        // Do NOT call DontDestroyOnLoad in editor; the managers will persist themselves at runtime.
    }

    private static void CreateTopBar(Transform parent, out Button closeButton, out TextMeshProUGUI moneyTextTMP)
    {
        GameObject topBar = new GameObject("TopBar", typeof(RectTransform), typeof(Image));
        RectTransform rt = SetupRectTransform(topBar, parent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f));
        rt.sizeDelta = new Vector2(0, 80);
        rt.anchoredPosition = Vector2.zero;
        Image bg = topBar.GetComponent<Image>();
        bg.color = ForestGreen;
        AddOutline(topBar, 2f);

        moneyTextTMP = CreateTMPText("MoneyText", topBar.transform, "Money: $0", 32, TextAlignmentOptions.MidlineLeft, MoneyGold);
        RectTransform moneyRT = moneyTextTMP.GetComponent<RectTransform>();
        moneyRT.anchorMin = new Vector2(0, 0);
        moneyRT.anchorMax = new Vector2(0, 1);
        moneyRT.pivot = new Vector2(0, 0.5f);
        moneyRT.anchoredPosition = new Vector2(30, 0);

        closeButton = CreateButton("CloseButton", topBar.transform, "X", out TextMeshProUGUI _, PanelWhite, BorderBlack, Color.black);
        RectTransform closeRT = closeButton.GetComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(1, 0);
        closeRT.anchorMax = new Vector2(1, 1);
        closeRT.pivot = new Vector2(1, 0.5f);
        closeRT.sizeDelta = new Vector2(80, 60);
        closeRT.anchoredPosition = new Vector2(-30, 0);
    }

    private static void CreateTabButtons(Transform parent, out Button buyTab, out Button sellTab)
    {
        GameObject tabRoot = new GameObject("TabButtons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        RectTransform rt = SetupRectTransform(tabRoot, parent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f));
        rt.sizeDelta = new Vector2(0, 60);
        rt.anchoredPosition = new Vector2(0, -90);

        HorizontalLayoutGroup layout = tabRoot.GetComponent<HorizontalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        layout.spacing = 10f;
        layout.padding = new RectOffset(20, 20, 0, 0);

        buyTab = CreateButton("BuyTabButton", tabRoot.transform, "Buy", out _, PanelWhite, BorderBlack, Color.black);
        sellTab = CreateButton("SellTabButton", tabRoot.transform, "Sell", out _, PanelWhite, BorderBlack, Color.black);
    }

    private static GameObject CreateScrollPanel(string name, Transform parent, out Transform content)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rt = SetupRectTransform(panel, parent, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
        rt.offsetMin = new Vector2(20, 20);
        rt.offsetMax = new Vector2(-20, -170);
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0); // transparent over green

        GameObject scrollView = new GameObject("ScrollView", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        RectTransform svRT = SetupRectTransform(scrollView, panel.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        svRT.offsetMin = new Vector2(10, 10);
        svRT.offsetMax = new Vector2(-10, -10);
        Image svImg = scrollView.GetComponent<Image>();
        svImg.color = PanelWhite;
        AddOutline(scrollView, 3f);

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        RectTransform vpRT = SetupRectTransform(viewport, scrollView.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        viewport.GetComponent<Image>().color = new Color(0, 0, 0, 0.0f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        GameObject contentGO = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        RectTransform contentRT = SetupRectTransform(contentGO, viewport.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        contentRT.offsetMin = new Vector2(10, 10);
        contentRT.offsetMax = new Vector2(-10, 0);
        VerticalLayoutGroup vlg = contentGO.GetComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 12f;
        vlg.padding = new RectOffset(5, 5, 5, 5);

        ContentSizeFitter fitter = contentGO.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        ScrollRect scrollRect = scrollView.GetComponent<ScrollRect>();
        scrollRect.viewport = vpRT;
        scrollRect.content = contentRT;
        scrollRect.vertical = true;
        scrollRect.horizontal = false;

        content = contentRT;
        return panel;
    }

    private static void CreateNotifications(Transform parent, out GameObject insufficientFunds,
        out GameObject insufficientSpace, out GameObject purchaseSuccess, out TextMeshProUGUI successTextTMP)
    {
        insufficientFunds = CreateNotificationPanel("InsufficientFundsPanel", parent, "Not enough money!");
        insufficientSpace = CreateNotificationPanel("InsufficientSpacePanel", parent, "No inventory space!");
        purchaseSuccess = CreateNotificationPanel("PurchaseSuccessPanel", parent, "Success!");
        successTextTMP = purchaseSuccess.GetComponentInChildren<TextMeshProUGUI>();
    }

    private static GameObject CreateNotificationPanel(string name, Transform parent, string message)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rt = SetupRectTransform(panel, parent, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        rt.sizeDelta = new Vector2(520, 80);
        rt.anchoredPosition = new Vector2(0, 60);
        Image bg = panel.GetComponent<Image>();
        bg.color = PanelWhite;
        AddOutline(panel, 3f);

        TextMeshProUGUI text = CreateTMPText("Text", panel.transform, message, 26, TextAlignmentOptions.Center, Color.black);
        RectTransform textRT = text.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0, 0);
        textRT.anchorMax = new Vector2(1, 1);
        textRT.offsetMin = new Vector2(10, 10);
        textRT.offsetMax = new Vector2(-10, -10);

        panel.SetActive(false);
        return panel;
    }

    private static GameObject CreateOrUpdateItemSlotPrefab()
    {
        GameObject slot = BuildItemSlot();

        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(ItemSlotPrefabPath);
        if (existing != null)
        {
            PrefabUtility.SaveAsPrefabAsset(slot, ItemSlotPrefabPath);
            Object.DestroyImmediate(slot);
            return AssetDatabase.LoadAssetAtPath<GameObject>(ItemSlotPrefabPath);
        }

        PrefabUtility.SaveAsPrefabAsset(slot, ItemSlotPrefabPath);
        Object.DestroyImmediate(slot);
        return AssetDatabase.LoadAssetAtPath<GameObject>(ItemSlotPrefabPath);
    }

    private static GameObject BuildItemSlot()
    {
        GameObject slot = new GameObject("ItemSlot", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        RectTransform rt = slot.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 160);
        Image bg = slot.GetComponent<Image>();
        bg.color = PanelWhite;
        AddOutline(slot, 3f);
        LayoutElement le = slot.GetComponent<LayoutElement>();
        le.minHeight = 140;

        VerticalLayoutGroup vlg = slot.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 6f;
        vlg.padding = new RectOffset(10, 10, 10, 10);

        GameObject icon = new GameObject("IconImage", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        icon.transform.SetParent(slot.transform, false);
        RectTransform iconRT = icon.GetComponent<RectTransform>();
        iconRT.sizeDelta = new Vector2(96, 96);
        LayoutElement iconLE = icon.GetComponent<LayoutElement>();
        iconLE.preferredHeight = 96;
        iconLE.preferredWidth = 96;
        Image iconImg = icon.GetComponent<Image>();
        iconImg.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        AddOutline(icon, 2f);

        CreateTMPText("NameText", slot.transform, "Item Name", 26, TextAlignmentOptions.Left, Color.black);
        CreateTMPText("CostText", slot.transform, "$0", 22, TextAlignmentOptions.Left, Color.black);
        CreateTMPText("DescriptionText", slot.transform, "Description here", 20, TextAlignmentOptions.TopLeft, Color.black);

        Button buyButton = CreateButton("BuyButton", slot.transform, "BUY", out TextMeshProUGUI _, PanelWhite, BorderBlack, Color.black);
        RectTransform btnRT = buyButton.GetComponent<RectTransform>();
        btnRT.sizeDelta = new Vector2(0, 46);

        return slot;
    }

    private static Button CreateButton(string name, Transform parent, string label, out TextMeshProUGUI labelTMP,
        Color? background = null, Color? outline = null, Color? textColor = null)
    {
        GameObject btnGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(parent, false);
        Image img = btnGO.GetComponent<Image>();
        img.color = background ?? PanelWhite;
        if (outline.HasValue)
        {
            AddOutline(btnGO, 3f, outline.Value);
        }

        Button button = btnGO.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = img.color;
        colors.highlightedColor = img.color;
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        button.colors = colors;

        labelTMP = CreateTMPText("Label", btnGO.transform, label, 22, TextAlignmentOptions.Center, textColor ?? Color.black);
        RectTransform rt = labelTMP.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return button;
    }

    private static TextMeshProUGUI CreateTMPText(string name, Transform parent, string text, int size,
        TextAlignmentOptions alignment, Color? color = null)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color ?? Color.black;
        tmp.alignment = alignment;
        ApplyPixelFont(tmp);
        return tmp;
    }

    private static RectTransform SetupRectTransform(GameObject go, Transform parent, Vector2 anchorMin,
        Vector2 anchorMax, Vector2 pivot)
    {
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        return rt;
    }

    private static void PopulateItemDatabase(ItemDatabase db)
    {
        if (db == null) return;
        db.allSeeds = LoadAssets<SeedData>();
        db.allCrops = LoadAssets<CropData>();
        db.allTools = LoadAssets<ToolData>();
        db.allResources = LoadAssets<ResourceData>();
        db.allOtherItems = new List<ItemData>();
        EditorUtility.SetDirty(db);
    }

    private static List<T> LoadAssets<T>() where T : Object
    {
        List<T> results = new List<T>();
        string filter = $"t:{typeof(T).Name}";
        string[] guids = AssetDatabase.FindAssets(filter);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) results.Add(asset);
        }
        return results;
    }

    private static void SafeRun(string label, System.Action action)
    {
        try
        {
            action?.Invoke();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Shop generator: failed to create {label}: {ex}");
        }
    }

    private static void SetPrivateInt(object target, string fieldName, int value)
    {
        if (target == null) return;
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }

    private static void ApplyPixelFont(TextMeshProUGUI tmp)
    {
        if (tmp == null) return;
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PixelFontPath);
        if (font != null)
        {
            tmp.font = font;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.richText = false;
            tmp.fontSizeMin = tmp.fontSize;
            tmp.fontSizeMax = tmp.fontSize;
        }
        else
        {
            Debug.LogWarning($"Pixel font not found at {PixelFontPath}. Assign it in the generator or update the path.");
        }
    }

    private static void AddOutline(GameObject go, float size, Color? color = null)
    {
        Outline outline = go.GetComponent<Outline>();
        if (outline == null)
        {
            outline = go.AddComponent<Outline>();
        }
        outline.effectColor = color ?? BorderBlack;
        outline.effectDistance = new Vector2(size, -size);
        outline.useGraphicAlpha = true;
    }
}
