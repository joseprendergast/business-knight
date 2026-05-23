#if UNITY_EDITOR
using System.IO;
using BusinessNight;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor.Build.Reporting;

public static class BusinessNightProjectBootstrap
{
    const string Root = "Assets/Game";
    const string Imported = "Assets/Game/Imported/business_knight_separated_assets";

    [MenuItem("Business Night/Build Placeholder Framework")]
    public static void BuildPlaceholderFramework()
    {
        EnsureFolders();
        ConfigureImportedSprites();
        CreateCleanedImportedSprites();
        CreateRoomSprite("Assets/Game/Art/night_desk_background.png", RoomArtKind.NightDesk);
        CreateRoomSprite("Assets/Game/Art/ledger_hall_background.png", RoomArtKind.LedgerHall);
        CreateRoomSprite("Assets/Game/Art/archive_door_background.png", RoomArtKind.ArchiveDoor);
        CreateTitleBackgroundSprite("Assets/Game/Art/title_background.png");
        CreateLogoSprite("Assets/Game/Art/business_knight_logo.png");
        CreateMaraSprite("Assets/Game/Art/mara_quill.png");
        CreateOpponentSprite("Assets/Game/Art/perry_audit.png");
        CreateTomatoBossSprite("Assets/Game/Art/tomato_boss.png");
        CreateHoodedAuditorSprite("Assets/Game/Art/hooded_auditor.png");
        CreateStampSprite("Assets/Game/Art/black_stamp.png");
        CreateDoorGlowSprite("Assets/Game/Art/door_glow.png");
        CreateMaterials();

        CreateTitleScene();
        CreatePrototypeScene("RoomPrototypeA", "Night Desk", "night_desk_background", true, "RoomPrototypeB");
        CreatePrototypeScene("RoomPrototypeB", "Ledger Hall", "ledger_hall_background", false, "RoomPrototypeC");
        CreatePrototypeScene("RoomPrototypeC", "Archive Door", "archive_door_background", false, "RoomPrototypeA");

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Game/Rooms/RoomTitle.unity", true),
            new EditorBuildSettingsScene("Assets/Game/Rooms/RoomPrototypeA.unity", true),
            new EditorBuildSettingsScene("Assets/Game/Rooms/RoomPrototypeB.unity", true),
            new EditorBuildSettingsScene("Assets/Game/Rooms/RoomPrototypeC.unity", true)
        };

        PlayerSettings.WebGL.template = "APPLICATION:Default";
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.defaultWebScreenWidth = 1280;
        PlayerSettings.defaultWebScreenHeight = 720;
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

        AssetDatabase.SaveAssets();
        Debug.Log("Business Night placeholder framework generated.");
    }

    public static void BuildWebGL()
    {
        BuildPlaceholderFramework();

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[]
            {
                "Assets/Game/Rooms/RoomTitle.unity",
                "Assets/Game/Rooms/RoomPrototypeA.unity",
                "Assets/Game/Rooms/RoomPrototypeB.unity",
                "Assets/Game/Rooms/RoomPrototypeC.unity"
            },
            locationPathName = "Builds/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception($"WebGL build failed: {report.summary.result}");

        Debug.Log($"Business Night WebGL build complete: {report.summary.outputPath}");
    }

    static void EnsureFolders()
    {
        string[] folders =
        {
            "Rooms", "Characters", "Inventory", "UI", "Scripts", "Audio", "Art", "Debug", "Atmosphere", "Editor"
        };

        foreach (string folder in folders)
        {
            string path = $"{Root}/{folder}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(Root, folder);
        }
    }

    static void CreateTitleScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SetupCamera();
        GameObject systems = CreateSystems();
        CreateUi(systems, true);
        CreateTitleArt();

        GameObject title = new GameObject("TitleMenu");
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        title.transform.SetParent(canvas.transform, false);
        CanvasGroup menuGroup = title.AddComponent<CanvasGroup>();
        VerticalLayoutGroup layout = title.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 6f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(520f, 310f);
        titleRect.anchoredPosition = new Vector2(0f, -54f);

        Button newGame = CreateButton("NewGame", title.transform, "New Game");
        UnityEventTools.AddPersistentListener(newGame.onClick, systems.GetComponent<BusinessNightSceneManager>().NewGame);
        Button cont = CreateButton("Continue", title.transform, "Continue");
        UnityEventTools.AddPersistentListener(cont.onClick, systems.GetComponent<BusinessNightSceneManager>().Continue);
        Button load = CreateButton("LoadGame", title.transform, "Load Game");
        CreateButton("Settings", title.transform, "Settings");
        CreateButton("Credits", title.transform, "Credits");
        Button quit = CreateButton("Quit", title.transform, "Quit");
        UnityEventTools.AddVoidPersistentListener(quit.onClick, Application.Quit);

        GameObject prompt = new GameObject("PressAnyButtonPrompt");
        prompt.transform.SetParent(canvas.transform, false);
        RectTransform promptRect = prompt.AddComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0f);
        promptRect.anchorMax = new Vector2(0.5f, 0f);
        promptRect.pivot = new Vector2(0.5f, 0.5f);
        promptRect.anchoredPosition = new Vector2(0f, 96f);
        promptRect.sizeDelta = new Vector2(620f, 54f);
        CanvasGroup promptGroup = prompt.AddComponent<CanvasGroup>();
        promptGroup.alpha = 1f;
        Text promptText = CreateText("PromptText", prompt.transform, "PRESS ANY BUTTON", 24, TextAnchor.MiddleCenter);
        promptText.color = new Color32(255, 205, 56, 255);
        promptText.fontStyle = FontStyle.Bold;
        Anchor(promptText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        BusinessNightTitleMenu titleMenu = title.AddComponent<BusinessNightTitleMenu>();
        SerializedObject titleSo = new SerializedObject(titleMenu);
        titleSo.FindProperty("newGameButton").objectReferenceValue = newGame.GetComponent<RectTransform>();
        titleSo.FindProperty("continueButton").objectReferenceValue = cont.GetComponent<RectTransform>();
        titleSo.FindProperty("loadButton").objectReferenceValue = load.GetComponent<RectTransform>();
        titleSo.FindProperty("menuGroup").objectReferenceValue = menuGroup;
        titleSo.FindProperty("pressAnyButtonPrompt").objectReferenceValue = prompt;
        titleSo.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, "Assets/Game/Rooms/RoomTitle.unity");
    }

    static void CreateTitleArt()
    {
        GameObject background = new GameObject("PaintedTitleBackground");
        SpriteRenderer backgroundRenderer = background.AddComponent<SpriteRenderer>();
        backgroundRenderer.sprite = LoadSprite($"{Imported}/poster/poster_vertical_5k.png", "Assets/Game/Art/title_background.png");
        backgroundRenderer.sortingOrder = 0;
        background.transform.localScale = new Vector3(1f, 1f, 1f);
        if (AssetDatabase.LoadAssetAtPath<Sprite>($"{Imported}/poster/poster_vertical_5k.png") != null)
            return;

        GameObject logo = new GameObject("BusinessKnightLogo");
        SpriteRenderer logoRenderer = logo.AddComponent<SpriteRenderer>();
        logoRenderer.sprite = LoadSprite($"{Imported}/branding/logo_5k.png", "Assets/Game/Art/business_knight_logo.png");
        logoRenderer.sortingOrder = 2;
        logo.transform.position = new Vector3(0f, 2.58f, -0.2f);
        logo.transform.localScale = new Vector3(0.43f, 0.43f, 1f);

        TextMesh title = new GameObject("BusinessKnightLogoText").AddComponent<TextMesh>();
        title.text = "BUSINESS\nKNIGHT";
        title.anchor = TextAnchor.MiddleCenter;
        title.alignment = TextAlignment.Center;
        title.characterSize = 0.052f;
        title.fontSize = 64;
        title.color = new Color32(255, 205, 56, 255);
        title.transform.position = new Vector3(0f, 2.55f, -0.45f);

        CreateTitleCharacter("TitleGary", "Assets/Game/Art/gary_clean.png", new Vector3(-0.42f, -0.88f, -0.25f), 1.16f);
        CreateTitleCharacter("TitleBrannon", "Assets/Game/Art/brannon_clean.png", new Vector3(0.9f, -0.9f, -0.25f), 1.04f);
        CreateTitleCharacter("TitleBoss", "Assets/Game/Art/strawberry_clean.png", new Vector3(-1.54f, -0.94f, -0.25f), 1.12f);
        CreateTitleCardText("ESCAPE THE OFFICE OR DIE TRYING", new Vector3(0f, -3.08f, -0.4f), 0.047f, new Color32(255, 201, 70, 255));
    }

    static void CreateTitleCharacter(string name, string spritePath, Vector3 position, float scale)
    {
        GameObject character = new GameObject(name);
        SpriteRenderer renderer = character.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        renderer.sortingOrder = 3;
        character.transform.position = position;
        character.transform.localScale = new Vector3(scale, scale, 1f);
    }

    static void CreateTitleCardText(string text, Vector3 position, float size, Color32 color)
    {
        TextMesh label = new GameObject("TitleTagline").AddComponent<TextMesh>();
        label.text = text;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = size;
        label.fontSize = 48;
        label.color = color;
        label.transform.position = position;
    }

    static void CreatePrototypeScene(string sceneName, string displayName, string backgroundSprite, bool firstRoom, string nextRoom)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Camera camera = SetupCamera();
        GameObject systems = CreateSystems();
        CreateUi(systems, false);
        GameObject input = new GameObject("RoomInput");
        BusinessNightRoomInput roomInput = input.AddComponent<BusinessNightRoomInput>();
        SerializedObject inputSo = new SerializedObject(roomInput);
        inputSo.FindProperty("roomCamera").objectReferenceValue = camera;
        inputSo.ApplyModifiedPropertiesWithoutUndo();

        GameObject room = new GameObject("RoomDefinition");
        BusinessNightRoom roomComponent = room.AddComponent<BusinessNightRoom>();
        roomComponent.definition.sceneId = sceneName;
        roomComponent.definition.displayName = displayName;
        roomComponent.definition.description = firstRoom
            ? "The first desk of the night shift, where the city sends impossible paperwork after midnight."
            : "A story-ready room shell with visible art, exit logic, and cinematic subtitles.";
        roomComponent.definition.backgroundArtReference = $"Assets/Game/Art/{backgroundSprite}.png";
        roomComponent.definition.charactersPresent.Add("Ari Vale");
        roomComponent.definition.narrativeBeats.Add(firstRoom ? "mara_finds_black_stamp" : "room_transition_beat");
        roomComponent.definition.completionFlag = "m_sceneOneComplete";
        roomComponent.openingSubtitle = firstRoom
            ? "Midnight again. The city only sends the impossible files after everyone honest has gone home."
            : "This hallway was not in the lease. That makes it either illegal or architectural.";
        roomComponent.openingSubtitleFlag = firstRoom ? "m_seenOpeningBeat" : string.Empty;

        CreateRoomArt(sceneName, displayName, backgroundSprite);
        CreateCharacter("Ari Vale", new Vector3(-2.15f, -1.03f, 0f));
        if (sceneName == "RoomPrototypeB")
            CreateOpponent("Perry Audit", new Vector3(1.55f, -1.0f, 0f));

        if (firstRoom)
        {
            CreateStampHotspot();
            CreateDoorHotspot(nextRoom);
        }
        else
        {
            CreateExitHotspot(nextRoom);
        }

        EditorSceneManager.SaveScene(scene, $"Assets/Game/Rooms/{sceneName}.unity");
    }

    static void CreateStampHotspot()
    {
        GameObject hotspot = new GameObject("Hotspot_BlackStamp");
        hotspot.transform.position = new Vector3(1.78f, -0.64f, 0f);
        hotspot.transform.localScale = new Vector3(1.8f, 1.3f, 1f);
        BoxCollider2D collider = hotspot.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        BusinessNightHotspot hotspotComponent = hotspot.AddComponent<BusinessNightHotspot>();
        hotspotComponent.displayName = "Black Stamp";
        hotspotComponent.inspectLine = "A seal from the Midnight Registry. It is still warm, as if someone approved the night itself.";
        hotspotComponent.interactLine = "Ari pockets the Black Stamp.";
        hotspotComponent.collectItemId = "prototype_item";
        hotspotComponent.setFlags.Add("m_collectedFirstItem");
        hotspotComponent.dialogueBeat.speaker = "Ari";
        hotspotComponent.dialogueBeat.text = "A stamp that authorizes doors. Useful, worrying, and exactly my size.";
        AddHotspotSprite(hotspot.transform, "BlackStampVisual", "Assets/Game/Art/black_stamp.png");
        CreateWorldLabel("Label_Stamp", "STAMP", new Vector3(1.78f, -0.04f, -0.25f), new Color32(245, 225, 170, 255));
    }

    static void CreateDoorHotspot(string nextRoom)
    {
        GameObject hotspot = new GameObject("Hotspot_GlowingDoor");
        hotspot.transform.position = new Vector3(2.75f, -0.22f, 0f);
        hotspot.transform.localScale = new Vector3(1.65f, 2.55f, 1f);
        BoxCollider2D collider = hotspot.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        BusinessNightHotspot hotspotComponent = hotspot.AddComponent<BusinessNightHotspot>();
        hotspotComponent.displayName = "Glowing Door";
        hotspotComponent.inspectLine = "The door has no handle. Just a thin cyan audit line waiting for authorization.";
        hotspotComponent.interactLine = "The door refuses to open. It wants something official.";
        hotspotComponent.requiredItemId = "prototype_item";
        hotspotComponent.roomChangeSceneId = nextRoom;
        hotspotComponent.dialogueBeat.speaker = "Ari";
        hotspotComponent.dialogueBeat.text = "No handle. Of course. I should select the Black Stamp below, then use it here.";
        AddHotspotSprite(hotspot.transform, "DoorGlow", "Assets/Game/Art/door_glow.png");
        CreateWorldLabel("Label_Door", "SEALED EXIT", new Vector3(2.94f, 1.55f, -0.25f), new Color32(135, 225, 238, 255));
    }

    static void CreateExitHotspot(string nextRoom)
    {
        GameObject hotspot = new GameObject("Hotspot_NextCorridor");
        hotspot.transform.position = new Vector3(2.75f, -0.22f, 0f);
        hotspot.transform.localScale = new Vector3(1.65f, 2.55f, 1f);
        BoxCollider2D collider = hotspot.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        BusinessNightHotspot hotspotComponent = hotspot.AddComponent<BusinessNightHotspot>();
        hotspotComponent.displayName = "Next Corridor";
        hotspotComponent.inspectLine = "A narrow corridor lit by copy-machine moonlight. It leads deeper into the building.";
        hotspotComponent.interactLine = "Ari steps through before the lights can change their mind.";
        hotspotComponent.roomChangeSceneId = nextRoom;
        hotspotComponent.setFlags.Add("m_sceneOneComplete");
        hotspotComponent.dialogueBeat.speaker = "Ari";
        hotspotComponent.dialogueBeat.text = "Fine. One more room. Then I ask why accounting has a horizon.";
        AddHotspotSprite(hotspot.transform, "ExitGlow", "Assets/Game/Art/door_glow.png");
    }

    static void CreateWorldLabel(string name, string text, Vector3 position, Color32 color)
    {
        TextMesh label = new GameObject(name).AddComponent<TextMesh>();
        label.text = text;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = 0.042f;
        label.fontSize = 48;
        label.color = color;
        label.transform.position = position;
    }

    static void AddHotspotSprite(Transform parent, string name, string spritePath)
    {
        GameObject visual = new GameObject(name);
        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = new Vector3(0f, 0f, -0.2f);
        SpriteRenderer visualRenderer = visual.AddComponent<SpriteRenderer>();
        visualRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        visualRenderer.sortingOrder = 4;
    }

    static void CreateRoomArt(string sceneName, string displayName, string backgroundSprite)
    {
        GameObject background = new GameObject("PaintedPixelRoom");
        SpriteRenderer renderer = background.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Game/Art/{backgroundSprite}.png");
        if (AssetDatabase.LoadAssetAtPath<Sprite>($"{Imported}/backgrounds/office_room_raw.png") != null)
        {
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{Imported}/backgrounds/office_room_raw.png");
            background.transform.localScale = new Vector3(1.08f, 2.25f, 1f);
        }
        renderer.sortingOrder = 0;
        background.transform.position = Vector3.zero;

        TextMesh label = new GameObject("RoomNamePixelLabel").AddComponent<TextMesh>();
        label.text = displayName.ToUpperInvariant();
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = 0.052f;
        label.fontSize = 48;
        label.color = new Color32(245, 240, 218, 255);
        label.transform.position = new Vector3(0f, 2.28f, -0.2f);

        if (sceneName == "RoomPrototypeB")
        {
            label.transform.position = new Vector3(0f, 2.18f, -0.2f);
        }
        else if (sceneName == "RoomPrototypeC")
        {
            label.transform.position = new Vector3(0f, 2.18f, -0.2f);
        }
    }

    static GameObject CreateQuad(string name, Vector3 position, Vector3 scale, string materialPath)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        quad.transform.position = position;
        quad.transform.localScale = scale;
        MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        Object.DestroyImmediate(quad.GetComponent<MeshCollider>());
        return quad;
    }

    static void CreateCharacter(string name, Vector3 position)
    {
        GameObject root = new GameObject(name);
        root.transform.position = position;
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = LoadSprite("Assets/Game/Art/gary_clean.png", "Assets/Game/Art/mara_quill.png");
        renderer.sortingOrder = 5;
        root.AddComponent<BusinessNightPlayer>();
    }

    static void CreateOpponent(string name, Vector3 position)
    {
        GameObject root = new GameObject(name);
        root.transform.position = position;
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = LoadSprite("Assets/Game/Art/brannon_clean.png", "Assets/Game/Art/perry_audit.png");
        renderer.sortingOrder = 5;
        BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1.1f, 2.2f);
        BusinessNightBattleHotspot battleHotspot = root.AddComponent<BusinessNightBattleHotspot>();
        SerializedObject so = new SerializedObject(battleHotspot);
        so.FindProperty("opponentName").stringValue = name;
        so.ApplyModifiedPropertiesWithoutUndo();
        CreateWorldLabel("Label_Perry", "CLICK: DUEL", position + new Vector3(0f, 1.28f, -0.25f), new Color32(255, 205, 56, 255));
    }

    static void CreateMaterials()
    {
        CreateMaterial("MatWall", new Color32(34, 43, 55, 255));
        CreateMaterial("MatFloor", new Color32(19, 24, 31, 255));
        CreateMaterial("MatFloorLine", new Color32(43, 51, 62, 255));
        CreateMaterial("MatWindow", new Color32(62, 91, 111, 255));
        CreateMaterial("MatDesk", new Color32(70, 55, 66, 255));
        CreateMaterial("MatLight", new Color32(229, 218, 165, 255));
        CreateMaterial("MatDoor", new Color32(49, 57, 72, 255));
        CreateMaterial("MatPanel", new Color32(48, 58, 70, 255));
        CreateMaterial("MatAccent", new Color32(207, 84, 86, 255));
        CreateMaterial("MatExit", new Color32(98, 185, 204, 255));
        CreateMaterial("MatCharacter", new Color32(45, 36, 61, 255));
        CreateMaterial("MatPaper", new Color32(218, 214, 191, 255));
        CreateMaterial("MatSkin", new Color32(192, 153, 125, 255));
        CreateMaterial("MatHair", new Color32(34, 24, 33, 255));
        CreateMaterial("MatShadow", new Color32(6, 8, 11, 255));
    }

    static void CreateMaterial(string name, Color color)
    {
        string path = $"Assets/Game/Art/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        EditorUtility.SetDirty(material);
    }

    static void CreateTitleBackgroundSprite(string path)
    {
        Texture2D texture = NewPixelTexture(320, 180, new Color32(8, 10, 14, 255));
        Color32 wall = new Color32(22, 28, 37, 255);
        Color32 desk = new Color32(86, 60, 42, 255);
        Color32 ember = new Color32(221, 83, 28, 255);
        Color32 gold = new Color32(255, 194, 54, 255);
        Color32 blue = new Color32(54, 78, 96, 255);

        Fill(texture, 0, 0, 320, 180, wall);
        Fill(texture, 0, 0, 320, 42, new Color32(8, 11, 16, 255));
        Fill(texture, 0, 130, 320, 50, new Color32(12, 15, 20, 255));
        Fill(texture, 0, 126, 320, 2, gold);

        Fill(texture, 22, 46, 88, 60, blue);
        Fill(texture, 26, 50, 80, 52, new Color32(28, 42, 58, 255));
        for (int x = 31; x < 102; x += 14)
            Fill(texture, x, 54, 5, 9, new Color32(236, 152, 52, 210));

        Fill(texture, 165, 52, 94, 44, desk);
        Fill(texture, 165, 48, 94, 7, new Color32(45, 38, 38, 255));
        Fill(texture, 178, 66, 56, 8, new Color32(235, 222, 176, 255));
        Fill(texture, 202, 77, 48, 12, new Color32(44, 50, 60, 255));

        Fill(texture, 250, 20, 38, 95, new Color32(17, 22, 30, 255));
        for (int y = 28; y < 106; y += 10)
            Fill(texture, 258, y, 12, 4, new Color32(224, 135, 44, 210));

        Fill(texture, 0, 21, 320, 6, new Color32(0, 0, 0, 95));
        for (int i = 0; i < 18; i++)
        {
            int x = 9 + i * 17;
            int h = 5 + (i % 4) * 3;
            Fill(texture, x, 35, 5, h, ember);
            Fill(texture, x + 1, 35, 2, Mathf.Max(2, h - 2), gold);
        }

        AddVignette(texture);
        SaveSprite(path, texture, 32);
    }

    static void CreateLogoSprite(string path)
    {
        Texture2D texture = NewPixelTexture(180, 58, new Color32(0, 0, 0, 0));
        Color32 dark = new Color32(8, 10, 14, 230);
        Color32 gold = new Color32(255, 193, 45, 255);
        Color32 orange = new Color32(230, 88, 27, 255);
        Color32 steel = new Color32(224, 225, 207, 255);

        Fill(texture, 3, 5, 174, 48, dark);
        Fill(texture, 3, 5, 174, 2, steel);
        Fill(texture, 3, 51, 174, 2, orange);
        Fill(texture, 27, 29, 106, 4, steel);
        Fill(texture, 20, 27, 18, 8, steel);
        Fill(texture, 132, 24, 32, 12, new Color32(109, 66, 28, 255));
        Fill(texture, 151, 21, 5, 18, orange);
        Fill(texture, 156, 25, 10, 10, gold);
        Fill(texture, 42, 12, 96, 10, gold);
        Fill(texture, 42, 35, 104, 10, orange);
        SaveSprite(path, texture, 32);
    }

    static Camera SetupCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 4.0f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(8, 10, 14, 255);
        cameraObject.tag = "MainCamera";
        cameraObject.AddComponent<Physics2DRaycaster>();

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
        return camera;
    }

    static GameObject CreateSystems()
    {
        GameObject systems = new GameObject("BusinessNightSystems");
        systems.AddComponent<BusinessNightGlobals>();
        systems.AddComponent<BusinessNightSceneManager>();
        systems.AddComponent<BusinessNightInventory>();
        systems.AddComponent<BusinessNightDialogue>();
        systems.AddComponent<BusinessNightBattle>();
        systems.AddComponent<BusinessNightSettings>();
        systems.AddComponent<BusinessNightPowerQuestBridge>();
        systems.AddComponent<BusinessNightDebug>();
        return systems;
    }

    static void CreateUi(GameObject systems, bool titleScene)
    {
        GameObject canvasObject = new GameObject("CinematicCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();
        canvasObject.transform.SetParent(systems.transform, false);

        BusinessNightUi ui = systems.AddComponent<BusinessNightUi>();

        Text roomTitle = CreateText("RoomTitle", canvasObject.transform, titleScene ? "" : "Prototype Room", 16, TextAnchor.UpperLeft);
        Anchor(roomTitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -16f), new Vector2(280f, 28f));

        Text hotspot = CreateText("HotspotLabel", canvasObject.transform, "Hotspot", 15, TextAnchor.MiddleCenter);
        hotspot.color = new Color32(240, 242, 232, 255);

        GameObject subtitlePanel = CreatePanel("SubtitlePanel", canvasObject.transform, new Color(0f, 0f, 0f, 0.62f));
        Anchor(subtitlePanel.GetComponent<RectTransform>(), new Vector2(0.18f, 0.05f), new Vector2(0.82f, 0.22f), Vector2.zero, Vector2.zero);
        CanvasGroup subtitleGroup = subtitlePanel.AddComponent<CanvasGroup>();
        Text speaker = CreateText("Speaker", subtitlePanel.transform, "", 15, TextAnchor.UpperLeft);
        Anchor(speaker.rectTransform, new Vector2(0.04f, 0.6f), new Vector2(0.96f, 0.95f), Vector2.zero, Vector2.zero);
        Text subtitle = CreateText("Subtitle", subtitlePanel.transform, "", 22, TextAnchor.UpperLeft);
        Anchor(subtitle.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.68f), Vector2.zero, Vector2.zero);

        GameObject inventoryPanel = CreatePanel("InventoryStrip", canvasObject.transform, new Color(0f, 0f, 0f, 0.44f));
        Anchor(inventoryPanel.GetComponent<RectTransform>(), new Vector2(0.72f, 0.88f), new Vector2(0.98f, 0.96f), Vector2.zero, Vector2.zero);
        HorizontalLayoutGroup stripLayout = inventoryPanel.AddComponent<HorizontalLayoutGroup>();
        stripLayout.childAlignment = TextAnchor.MiddleCenter;
        stripLayout.spacing = 6f;
        Button itemTemplate = CreateButton("InventoryButtonTemplate", inventoryPanel.transform, "Item");

        GameObject fade = CreatePanel("Fade", canvasObject.transform, Color.black);
        Anchor(fade.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        CanvasGroup fadeGroup = fade.AddComponent<CanvasGroup>();
        fadeGroup.blocksRaycasts = false;

        GameObject debug = CreatePanel("DebugPanel", canvasObject.transform, new Color(0f, 0f, 0f, 0.72f));
        Anchor(debug.GetComponent<RectTransform>(), new Vector2(0.02f, 0.48f), new Vector2(0.42f, 0.95f), Vector2.zero, Vector2.zero);
        Text debugText = CreateText("DebugText", debug.transform, "Debug", 14, TextAnchor.UpperLeft);
        Anchor(debugText.rectTransform, new Vector2(0.03f, 0.04f), new Vector2(0.97f, 0.96f), Vector2.zero, Vector2.zero);

        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("fadeGroup").objectReferenceValue = fadeGroup;
        so.FindProperty("subtitleGroup").objectReferenceValue = subtitleGroup;
        so.FindProperty("subtitleSpeaker").objectReferenceValue = speaker;
        so.FindProperty("subtitleText").objectReferenceValue = subtitle;
        so.FindProperty("hotspotLabel").objectReferenceValue = hotspot;
        so.FindProperty("roomTitle").objectReferenceValue = roomTitle;
        so.FindProperty("inventoryStrip").objectReferenceValue = inventoryPanel.transform;
        so.FindProperty("inventoryButtonTemplate").objectReferenceValue = itemTemplate;
        so.FindProperty("debugPanel").objectReferenceValue = debug;
        so.FindProperty("debugText").objectReferenceValue = debugText;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static Text CreateText(string name, Transform parent, string value, int size, TextAnchor anchor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text text = go.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.alignment = anchor;
        text.color = new Color32(240, 242, 232, 255);
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    static Button CreateButton(string name, Transform parent, string label)
    {
        GameObject panel = CreatePanel(name, parent, new Color(0.025f, 0.04f, 0.07f, 0.92f));
        Image panelImage = panel.GetComponent<Image>();
        Sprite normalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath(label, false));
        Sprite hoverSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath(label, true));
        if (normalSprite != null)
        {
            panelImage.sprite = normalSprite;
            panelImage.type = Image.Type.Sliced;
            panelImage.color = Color.white;
        }
        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color32(238, 224, 184, 255);
        outline.effectDistance = new Vector2(2f, -2f);
        Button button = panel.AddComponent<Button>();
        button.targetGraphic = panel.GetComponent<Image>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color32(255, 245, 220, 255);
        colors.pressedColor = new Color32(201, 96, 37, 255);
        colors.selectedColor = new Color32(111, 77, 42, 255);
        button.colors = colors;
        if (hoverSprite != null)
        {
            SpriteState spriteState = button.spriteState;
            spriteState.highlightedSprite = hoverSprite;
            spriteState.pressedSprite = hoverSprite;
            spriteState.selectedSprite = hoverSprite;
            button.spriteState = spriteState;
        }
        LayoutElement layout = panel.AddComponent<LayoutElement>();
        bool isInventoryButton = name.Contains("Inventory");
        layout.minWidth = isInventoryButton ? 112f : 220f;
        layout.preferredWidth = isInventoryButton ? 128f : 300f;
        layout.flexibleWidth = 0f;
        layout.minHeight = isInventoryButton ? 30f : 42f;
        layout.preferredHeight = isInventoryButton ? 30f : 42f;
        layout.flexibleHeight = 0f;

        Text text = CreateText("Label", panel.transform, normalSprite != null ? string.Empty : label.ToUpperInvariant(), isInventoryButton ? 13 : 18, TextAnchor.MiddleCenter);
        text.color = new Color32(245, 240, 218, 255);
        text.fontStyle = FontStyle.Bold;
        Anchor(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return button;
    }

    static string ButtonSpritePath(string label, bool hover)
    {
        string suffix = hover ? "_hover" : string.Empty;
        string normalized = label.ToLowerInvariant().Replace(" ", "_");
        if (normalized == "continue")
            return string.Empty;
        if (normalized == "settings" || normalized == "credits")
            normalized = normalized == "settings" ? "options" : string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
            return string.Empty;

        return $"{Imported}/ui/button_{normalized}{suffix}.png";
    }

    static Sprite LoadSprite(string preferredPath, string fallbackPath)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(preferredPath);
        return sprite != null ? sprite : AssetDatabase.LoadAssetAtPath<Sprite>(fallbackPath);
    }

    static void ConfigureImportedSprites()
    {
        if (!Directory.Exists(Imported))
            return;

        foreach (string file in Directory.GetFiles(Imported, "*.png", SearchOption.AllDirectories))
        {
            string assetPath = file.Replace("\\", "/");
            int ppu = 64;
            if (assetPath.Contains("/poster/"))
                ppu = assetPath.Contains("_5k") ? 640 : 64;
            else if (assetPath.Contains("/branding/"))
                ppu = assetPath.Contains("_5k") ? 640 : 64;
            else if (assetPath.Contains("/menu/"))
                ppu = assetPath.Contains("_5k") ? 640 : 80;
            else if (assetPath.Contains("/backgrounds/"))
                ppu = assetPath.Contains("_5k") ? 640 : 76;
            else if (assetPath.Contains("/characters/x4/"))
                ppu = 192;
            else if (assetPath.Contains("/characters/"))
                ppu = 48;
            else if (assetPath.Contains("/ui/x4/"))
                ppu = 256;
            else if (assetPath.Contains("/ui/"))
                ppu = 128;
            else if (assetPath.Contains("/inventory/x4/"))
                ppu = 256;
            else if (assetPath.Contains("/inventory/"))
                ppu = 64;
            else if (assetPath.Contains("/combat/x4/"))
                ppu = 256;
            else if (assetPath.Contains("/combat/"))
                ppu = 128;

            ConfigureSprite(assetPath, ppu);
        }
    }

    static void CreateCleanedImportedSprites()
    {
        CleanImportedSprite($"{Imported}/characters/gary_idle_01.png", "Assets/Game/Art/gary_clean.png", 48, 15, 42);
        CleanImportedSprite($"{Imported}/characters/brannon_elk_idle.png", "Assets/Game/Art/brannon_clean.png", 48, 0, 44);
        CleanImportedSprite($"{Imported}/characters/strawberry_boss_idle.png", "Assets/Game/Art/strawberry_clean.png", 48, 0, 44);
        CleanImportedSprite($"{Imported}/inventory/employee_badge.png", "Assets/Game/Art/employee_badge_clean.png", 64, 0, 40);
    }

    static void CleanImportedSprite(string sourcePath, string outputPath, int pixelsPerUnit, int cropTopPixels, int threshold)
    {
        if (!File.Exists(sourcePath))
            return;

        Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        source.LoadImage(File.ReadAllBytes(sourcePath));
        int outputHeight = Mathf.Max(1, source.height - cropTopPixels);
        Texture2D cleaned = NewPixelTexture(source.width, outputHeight, new Color32(0, 0, 0, 0));
        Color32 background = source.GetPixel(source.width - 1, 0);

        for (int y = 0; y < outputHeight; y++)
        {
            for (int x = 0; x < source.width; x++)
            {
                Color32 pixel = source.GetPixel(x, y);
                int distance =
                    Mathf.Abs(pixel.r - background.r) +
                    Mathf.Abs(pixel.g - background.g) +
                    Mathf.Abs(pixel.b - background.b);
                if (distance <= threshold)
                    pixel.a = 0;

                cleaned.SetPixel(x, y, pixel);
            }
        }

        SaveSprite(outputPath, cleaned, pixelsPerUnit);
    }

    static void ConfigureSprite(string path, int pixelsPerUnit)
    {
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }

    static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.color = color;
        return go;
    }

    static void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    static void CreateRoomSprite(string path, RoomArtKind kind)
    {
        Texture2D texture = NewPixelTexture(320, 180, new Color32(20, 26, 34, 255));

        Color32 wall = kind == RoomArtKind.NightDesk ? new Color32(30, 38, 50, 255) : new Color32(31, 36, 48, 255);
        Color32 wallDark = new Color32(15, 20, 28, 255);
        Color32 floor = new Color32(12, 15, 21, 255);
        Color32 floorLine = new Color32(58, 68, 76, 255);
        Color32 window = new Color32(58, 83, 101, 255);
        Color32 windowDark = new Color32(23, 35, 49, 255);
        Color32 warm = new Color32(233, 219, 158, 255);
        Color32 ember = new Color32(224, 91, 36, 255);
        Color32 gold = new Color32(255, 196, 57, 255);
        Color32 desk = new Color32(80, 58, 50, 255);
        Color32 paper = new Color32(216, 211, 185, 255);
        Color32 red = new Color32(202, 76, 78, 255);
        Color32 cyan = new Color32(85, 184, 208, 255);
        Color32 trim = new Color32(70, 82, 96, 255);

        Fill(texture, 0, 0, 320, 180, wall);
        Fill(texture, 0, 0, 320, 47, floor);
        Fill(texture, 0, 44, 320, 2, floorLine);
        Fill(texture, 0, 57, 320, 2, new Color32(29, 36, 45, 255));
        Fill(texture, 0, 154, 320, 26, wallDark);
        Fill(texture, 0, 151, 320, 2, gold);

        for (int x = 0; x < 320; x += 16)
            Fill(texture, x, 0, 1, 47, new Color32(21, 26, 33, 255));

        Fill(texture, 10, 86, 105, 62, new Color32(15, 23, 33, 255));
        Fill(texture, 16, 93, 92, 48, window);
        Fill(texture, 62, 93, 2, 44, new Color32(34, 53, 70, 255));
        Fill(texture, 22, 114, 88, 2, new Color32(34, 53, 70, 255));
        Fill(texture, 30, 125, 18, 18, new Color32(220, 214, 174, 255));
        Fill(texture, 28, 123, 3, 3, new Color32(246, 237, 192, 255));
        for (int i = 0; i < 10; i++)
            Fill(texture, 16 + i * 9, 93 + (i % 3) * 4, 3, 4, new Color32(244, 151, 55, 170));

        Fill(texture, 65, 56, 118, 25, desk);
        Fill(texture, 65, 48, 118, 8, new Color32(50, 43, 53, 255));
        Fill(texture, 68, 45, 112, 3, trim);
        Fill(texture, 102, 79, 64, 8, gold);
        Fill(texture, 167, 75, 36, 10, paper);
        Fill(texture, 180, 70, 28, 4, new Color32(234, 229, 203, 255));
        Fill(texture, 126, 65, 18, 4, new Color32(235, 224, 165, 255));
        Fill(texture, 95, 83, 78, 2, new Color32(146, 91, 52, 255));

        Fill(texture, 242, 47, 46, 92, new Color32(48, 57, 72, 255));
        Fill(texture, 246, 51, 38, 84, new Color32(38, 45, 58, 255));
        Fill(texture, 242, 47, 3, 92, cyan);
        Fill(texture, 284, 47, 2, 92, new Color32(22, 27, 36, 255));
        Fill(texture, 256, 88, 6, 6, warm);

        Fill(texture, 112, 148, 96, 14, new Color32(49, 59, 72, 255));
        Fill(texture, 121, 151, 78, 8, new Color32(31, 38, 49, 255));

        Fill(texture, 220, 70, 22, 15, red);
        Fill(texture, 223, 80, 16, 3, new Color32(126, 46, 54, 255));
        Fill(texture, 234, 81, 6, 2, warm);
        Fill(texture, 226, 74, 8, 3, new Color32(233, 131, 122, 255));
        Fill(texture, 291, 47, 6, 68, ember);
        Fill(texture, 292, 49, 2, 63, gold);

        if (kind == RoomArtKind.LedgerHall)
        {
            Fill(texture, 18, 52, 42, 92, new Color32(68, 56, 67, 255));
            for (int y = 58; y < 136; y += 10)
                Fill(texture, 22, y, 34, 2, paper);
            Fill(texture, 204, 112, 74, 8, red);
            Fill(texture, 214, 104, 54, 6, new Color32(244, 163, 141, 255));
        }
        else if (kind == RoomArtKind.ArchiveDoor)
        {
            Fill(texture, 121, 43, 78, 105, new Color32(48, 57, 72, 255));
            Fill(texture, 130, 53, 60, 86, new Color32(29, 35, 46, 255));
            Fill(texture, 154, 87, 14, 14, cyan);
            Fill(texture, 158, 91, 6, 6, new Color32(192, 239, 241, 255));
        }

        AddVignette(texture);
        SaveSprite(path, texture, 32);
    }

    static void CreateMaraSprite(string path)
    {
        Texture2D texture = NewPixelTexture(56, 80, new Color32(0, 0, 0, 0));
        Color32 outline = new Color32(8, 10, 13, 255);
        Color32 shirt = new Color32(236, 231, 204, 255);
        Color32 tie = new Color32(205, 47, 39, 255);
        Color32 suit = new Color32(36, 43, 58, 255);
        Color32 skin = new Color32(194, 142, 106, 255);
        Color32 steel = new Color32(176, 184, 188, 255);
        Color32 steelLight = new Color32(245, 238, 204, 255);

        Fill(texture, 10, 1, 34, 5, new Color32(5, 7, 10, 190));
        Fill(texture, 18, 8, 21, 40, outline);
        Fill(texture, 21, 10, 15, 36, suit);
        Fill(texture, 23, 18, 10, 29, shirt);
        Fill(texture, 27, 19, 3, 31, tie);
        Fill(texture, 14, 21, 7, 31, outline);
        Fill(texture, 35, 21, 7, 31, outline);
        Fill(texture, 16, 22, 4, 27, suit);
        Fill(texture, 36, 22, 4, 27, suit);
        Fill(texture, 18, 48, 9, 20, outline);
        Fill(texture, 30, 48, 9, 20, outline);
        Fill(texture, 20, 49, 6, 17, suit);
        Fill(texture, 31, 49, 6, 17, suit);
        Fill(texture, 16, 67, 13, 4, outline);
        Fill(texture, 28, 67, 14, 4, outline);

        Fill(texture, 17, 49, 6, 5, skin);
        Fill(texture, 36, 49, 6, 5, skin);
        Fill(texture, 18, 56, 20, 13, skin);
        Fill(texture, 15, 66, 26, 7, outline);
        Fill(texture, 13, 61, 5, 9, outline);
        Fill(texture, 34, 61, 7, 7, outline);
        Fill(texture, 30, 59, 2, 2, outline);
        Fill(texture, 24, 55, 4, 2, new Color32(230, 176, 130, 255));

        Fill(texture, 13, 68, 28, 3, steel);
        Fill(texture, 17, 71, 20, 5, steelLight);
        Fill(texture, 21, 74, 12, 2, new Color32(110, 120, 132, 255));
        Fill(texture, 8, 64, 7, 4, steelLight);
        Fill(texture, 41, 64, 7, 4, steelLight);

        Fill(texture, 7, 26, 4, 31, steelLight);
        Fill(texture, 6, 54, 6, 4, steel);
        Fill(texture, 8, 58, 2, 8, new Color32(118, 84, 54, 255));
        SaveSprite(path, texture, 32);
    }

    static void CreateStampSprite(string path)
    {
        Texture2D texture = NewPixelTexture(28, 18, new Color32(0, 0, 0, 0));
        Fill(texture, 3, 3, 22, 12, new Color32(198, 72, 75, 255));
        Fill(texture, 3, 3, 22, 2, new Color32(237, 122, 112, 255));
        Fill(texture, 3, 13, 22, 2, new Color32(112, 42, 51, 255));
        Fill(texture, 17, 12, 7, 2, new Color32(239, 226, 172, 255));
        Fill(texture, 6, 6, 9, 2, new Color32(135, 48, 55, 255));
        SaveSprite(path, texture, 32);
    }

    static void CreateOpponentSprite(string path)
    {
        Texture2D texture = NewPixelTexture(56, 80, new Color32(0, 0, 0, 0));
        Color32 outline = new Color32(8, 9, 12, 255);
        Color32 green = new Color32(42, 104, 70, 255);
        Color32 shirt = new Color32(232, 222, 182, 255);
        Color32 antler = new Color32(171, 116, 54, 255);
        Color32 fur = new Color32(166, 112, 76, 255);

        Fill(texture, 10, 1, 34, 5, new Color32(5, 7, 10, 190));
        Fill(texture, 18, 8, 21, 41, outline);
        Fill(texture, 21, 11, 15, 35, green);
        Fill(texture, 23, 18, 10, 27, shirt);
        Fill(texture, 27, 18, 3, 27, new Color32(205, 54, 41, 255));
        Fill(texture, 14, 21, 7, 31, outline);
        Fill(texture, 35, 21, 7, 31, outline);
        Fill(texture, 16, 22, 4, 27, green);
        Fill(texture, 36, 22, 4, 27, green);
        Fill(texture, 18, 48, 9, 20, outline);
        Fill(texture, 30, 48, 9, 20, outline);
        Fill(texture, 20, 49, 6, 17, new Color32(32, 45, 55, 255));
        Fill(texture, 31, 49, 6, 17, new Color32(32, 45, 55, 255));
        Fill(texture, 16, 67, 13, 4, outline);
        Fill(texture, 28, 67, 14, 4, outline);

        Fill(texture, 17, 51, 22, 16, fur);
        Fill(texture, 15, 64, 26, 7, outline);
        Fill(texture, 24, 58, 3, 2, outline);
        Fill(texture, 31, 58, 3, 2, outline);
        Fill(texture, 27, 54, 4, 3, new Color32(219, 158, 112, 255));

        Fill(texture, 9, 66, 9, 3, antler);
        Fill(texture, 6, 69, 12, 3, antler);
        Fill(texture, 6, 72, 3, 6, antler);
        Fill(texture, 13, 72, 3, 5, antler);
        Fill(texture, 38, 66, 9, 3, antler);
        Fill(texture, 38, 69, 12, 3, antler);
        Fill(texture, 47, 72, 3, 6, antler);
        Fill(texture, 40, 72, 3, 5, antler);
        SaveSprite(path, texture, 32);
    }

    static void CreateTomatoBossSprite(string path)
    {
        Texture2D texture = NewPixelTexture(58, 72, new Color32(0, 0, 0, 0));
        Color32 outline = new Color32(8, 8, 10, 255);
        Color32 suit = new Color32(28, 38, 52, 255);
        Color32 shirt = new Color32(238, 230, 201, 255);
        Color32 tie = new Color32(216, 45, 34, 255);
        Color32 tomato = new Color32(207, 48, 35, 255);
        Color32 tomatoLight = new Color32(244, 102, 65, 255);
        Color32 leaf = new Color32(76, 154, 72, 255);

        Fill(texture, 7, 0, 42, 5, new Color32(5, 6, 8, 190));
        Fill(texture, 14, 7, 30, 38, outline);
        Fill(texture, 17, 10, 24, 34, suit);
        Fill(texture, 21, 17, 15, 25, shirt);
        Fill(texture, 27, 18, 4, 24, tie);
        Fill(texture, 10, 21, 7, 26, outline);
        Fill(texture, 41, 21, 7, 26, outline);
        Fill(texture, 18, 45, 9, 16, outline);
        Fill(texture, 31, 45, 9, 16, outline);
        Fill(texture, 16, 60, 14, 4, outline);
        Fill(texture, 29, 60, 15, 4, outline);

        Fill(texture, 11, 41, 36, 22, outline);
        Fill(texture, 13, 43, 32, 18, tomato);
        Fill(texture, 17, 47, 10, 4, tomatoLight);
        Fill(texture, 23, 39, 13, 5, leaf);
        Fill(texture, 28, 36, 5, 5, leaf);
        Fill(texture, 20, 51, 4, 3, outline);
        Fill(texture, 34, 51, 4, 3, outline);
        Fill(texture, 26, 56, 9, 2, new Color32(104, 19, 20, 255));
        SaveSprite(path, texture, 32);
    }

    static void CreateHoodedAuditorSprite(string path)
    {
        Texture2D texture = NewPixelTexture(52, 76, new Color32(0, 0, 0, 0));
        Color32 outline = new Color32(6, 8, 11, 255);
        Color32 cloak = new Color32(45, 58, 68, 255);
        Color32 cloakLight = new Color32(92, 107, 116, 255);
        Color32 cyan = new Color32(87, 203, 217, 255);

        Fill(texture, 8, 0, 36, 5, new Color32(5, 6, 8, 190));
        Fill(texture, 13, 9, 27, 55, outline);
        Fill(texture, 16, 13, 21, 48, cloak);
        Fill(texture, 20, 48, 10, 17, new Color32(27, 34, 43, 255));
        Fill(texture, 10, 30, 7, 28, outline);
        Fill(texture, 36, 30, 7, 28, outline);
        Fill(texture, 13, 62, 29, 5, outline);
        Fill(texture, 18, 57, 19, 8, cloakLight);
        Fill(texture, 19, 42, 16, 11, new Color32(13, 18, 24, 255));
        Fill(texture, 23, 45, 8, 2, cyan);
        Fill(texture, 18, 18, 18, 18, cloakLight);
        Fill(texture, 22, 21, 12, 12, cloak);
        SaveSprite(path, texture, 32);
    }

    static void CreateDoorGlowSprite(string path)
    {
        Texture2D texture = NewPixelTexture(12, 78, new Color32(0, 0, 0, 0));
        Fill(texture, 4, 3, 4, 72, new Color32(89, 189, 214, 225));
        Fill(texture, 2, 6, 2, 66, new Color32(65, 134, 164, 150));
        Fill(texture, 8, 6, 2, 66, new Color32(65, 134, 164, 150));
        SaveSprite(path, texture, 32);
    }

    static Texture2D NewPixelTexture(int width, int height, Color32 clear)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                texture.SetPixel(x, y, clear);
        }
        return texture;
    }

    static void Fill(Texture2D texture, int x, int y, int width, int height, Color32 color)
    {
        for (int yy = Mathf.Max(0, y); yy < Mathf.Min(texture.height, y + height); yy++)
        {
            for (int xx = Mathf.Max(0, x); xx < Mathf.Min(texture.width, x + width); xx++)
                texture.SetPixel(xx, yy, color);
        }
    }

    static void AddVignette(Texture2D texture)
    {
        Vector2 center = new Vector2(texture.width * 0.5f, texture.height * 0.5f);
        float maxDistance = center.magnitude;
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / maxDistance;
                if (distance < 0.45f)
                    continue;

                Color color = texture.GetPixel(x, y);
                float shade = Mathf.Lerp(1f, 0.56f, Mathf.InverseLerp(0.45f, 1f, distance));
                texture.SetPixel(x, y, new Color(color.r * shade, color.g * shade, color.b * shade, color.a));
            }
        }
    }

    static void SaveSprite(string path, Texture2D texture, int pixelsPerUnit)
    {
        File.WriteAllBytes(path, texture.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }

    enum RoomArtKind
    {
        NightDesk,
        LedgerHall,
        ArchiveDoor
    }
}
#endif
