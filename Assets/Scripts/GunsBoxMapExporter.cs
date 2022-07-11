#if UNITY_EDITOR
using Gunsbox;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GunsBoxMapExporter : EditorWindow
{
    private string mapName = "";
    private string basePath = "";
    private string exportPath = "";
    private string scenePath = "";
    private string previewPath = "";

    private int height = 40;
    private float offset = 10;

    private Texture2D previewImage;
    private Vector2 scrollPos;
    private GameObject[] selectedObjects;


    private bool mapNameFoldoutBool = false;
    private bool screenshotFoldoutBool = false;
    private bool buildBool = false;
    private bool teleportAreaBool = false;
    private bool requiredObjectsBool = false;

    [MenuItem("GunsBox VR/Map Exporter")]
    public static void ShowMapWindow()
    {
        GetWindow<GunsBoxMapExporter>("Map Exporter");
    }

    private void Awake()
    {
        mapName = EditorPrefs.GetString("map", "");
        previewPath = EditorPrefs.GetString("preview");

        mapNameFoldoutBool = EditorPrefs.GetBool("mapNameFoldoutBool");
        screenshotFoldoutBool = EditorPrefs.GetBool("screenshotFoldoutBool");
        buildBool = EditorPrefs.GetBool("buildBool");
        teleportAreaBool = EditorPrefs.GetBool("teleportAreaBool");
        requiredObjectsBool = EditorPrefs.GetBool("requiredObjectsBool");

        if (!string.IsNullOrEmpty(previewPath))
        {
            previewImage = AssetDatabase.LoadAssetAtPath(previewPath, typeof(Texture2D)) as Texture2D;
            if (previewImage == null)
            {
                previewPath = "";
                EditorPrefs.SetString("preview", "");
            }
        }
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));
        selectedObjects = Selection.gameObjects;
        SceneSettings();

        EditorGUILayout.LabelField("Map Exporter");

        MapUtils.DrawUILine(Color.grey, 5);

        EditorGUIUtility.labelWidth = 80;
        MapNameSettings();

        MapUtils.DrawUILine(Color.grey);

        RequiredObjects();

        MapUtils.DrawUILine(Color.grey);

        TeleportAreasSettings();

        MapUtils.DrawUILine(Color.grey);

        ScreenshotsButtons();

        MapUtils.DrawUILine(Color.grey);

        BuildButtons();

        MapUtils.DrawUILine(Color.grey);

        OpenExportFolder();

        MapUtils.DrawUILine(Color.grey, 5);

        EditorGUILayout.EndScrollView();
    }

    private void MapNameSettings()
    {
        mapNameFoldoutBool = EditorGUILayout.BeginFoldoutHeaderGroup(mapNameFoldoutBool, "Map Settings");

        if (mapNameFoldoutBool)
        {
            EditorGUILayout.HelpBox("1. Give a name to the map", MessageType.Info);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Map name", EditorStyles.label);
            mapName = EditorGUILayout.TextField(mapName);
            EditorPrefs.SetString("map", mapName);
            GUILayout.EndHorizontal();
        }

        EditorPrefs.SetBool("mapNameFoldoutBool", mapNameFoldoutBool);
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    private void SceneSettings()
    {
        scenePath = EditorSceneManager.GetActiveScene().path;
        basePath = Path.Combine(Application.persistentDataPath, "Mods");
        exportPath = AssemblyExportPath();
    }
    private void RequiredObjects()
    {
        requiredObjectsBool = EditorGUILayout.BeginFoldoutHeaderGroup(requiredObjectsBool, "Required Objects");

        if (requiredObjectsBool)
        {
            EditorGUILayout.HelpBox("2. Add only one inventory and  spawn point", MessageType.Info);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add inventory", GUILayout.Height(height), GUILayout.Width((position.width - offset) / 2)))
            {
                var inventory = FindObjectOfType<GunsBoxInventory>();

                if (inventory == null)
                {
                    var inventoryObject = Instantiate(Resources.Load<GunsBoxInventory>("REQUIRED OBJECTS/Inventory"));

                    Selection.activeGameObject = inventoryObject.gameObject;
                    SceneView.FrameLastActiveSceneView();
                    EditorUtility.SetDirty(inventoryObject);
                }
                else
                {
                    Selection.activeGameObject = inventory.gameObject;
                    SceneView.FrameLastActiveSceneView();
                }
            }

            if (GUILayout.Button("Add spawn point", GUILayout.Height(height), GUILayout.Width((position.width - offset) / 2)))
            {
                var point = FindObjectOfType<GunsBoxSpawnPoint>();

                if (point == null)
                {
                    var pointObject = Instantiate(Resources.Load<GunsBoxSpawnPoint>("REQUIRED OBJECTS/SpawnPoint"));

                    Selection.activeGameObject = pointObject.gameObject;
                    SceneView.FrameLastActiveSceneView();
                    EditorUtility.SetDirty(pointObject);
                }
                else
                {
                    Selection.activeGameObject = point.gameObject;
                    SceneView.FrameLastActiveSceneView();
                }
            }
            GUILayout.EndHorizontal();
        }
        EditorPrefs.SetBool("requiredObjectsBool", requiredObjectsBool);
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    private void TeleportAreasSettings()
    {
        teleportAreaBool = EditorGUILayout.BeginFoldoutHeaderGroup(teleportAreaBool, "Teleport Areas");

        if (teleportAreaBool)
        {
            EditorGUILayout.HelpBox("3. Select the objects that will be add/remove the teleport area and press button\n" +
                "Note: TELEPORT AREAS SHOULD HAVE A COLLIDER", MessageType.Info);

            if (GUILayout.Button("Make it as teleport area", GUILayout.Height(height), GUILayout.Width(position.width - offset)))
            {
                selectedObjects = Selection.gameObjects;

                if (selectedObjects.Length > 0)
                    MapUtils.MakeTeleportZone(selectedObjects);
                else
                    MapUtils.DisplayError("No selected objects", "Select objects in Hierarchy or Scene View");
            }
            if (GUILayout.Button("Remove it as teleport area", GUILayout.Height(height), GUILayout.Width(position.width - offset)))
            {
                selectedObjects = Selection.gameObjects;

                if (selectedObjects.Length > 0)
                    MapUtils.RemoveTeleportZone(selectedObjects);
                else
                    MapUtils.DisplayError("No selected objects", "Select objects in Hierarchy or Scene View");
            }
        }
        EditorPrefs.SetBool("teleportAreaBool", teleportAreaBool);
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    private void ScreenshotsButtons()
    {
        screenshotFoldoutBool = EditorGUILayout.BeginFoldoutHeaderGroup(screenshotFoldoutBool, "Screenshots");
        //Select Image
        if (screenshotFoldoutBool)
        {
            //EditorGUILayout.HelpBox("Current Icon Path: " + previewPath, MessageType.None);

            //Screenshot
            EditorGUILayout.HelpBox("Choose a nice angle in the Scene View and press \"Create screenshot for game\"", MessageType.Info);

            if (GUILayout.Button("Create screenshot for game", GUILayout.Height(height), GUILayout.Width(position.width - offset)))
            {
                MapUtils.CreateImageToDisplayInGame(mapName, true);
            }

            EditorGUILayout.HelpBox("Create screenshots for workshop\nYou will need to add them manually, in Steam itself.", MessageType.Info);
            if (GUILayout.Button("Create screenshots for workshop", GUILayout.Height(height), GUILayout.Width(position.width - offset)))
            {
                MapUtils.CreateImageForAdditionalPreview(Application.persistentDataPath);
            }

            EditorGUILayout.Space();

            GUILayout.Label("Select screenshot for game", EditorStyles.boldLabel);
            previewImage = EditorGUILayout.ObjectField(previewImage, typeof(Texture2D), false) as Texture2D;
            previewPath = AssetDatabase.GetAssetPath(previewImage);

            EditorPrefs.SetString("preview", previewPath);
        }

        EditorPrefs.SetBool("screenshotFoldoutBool", screenshotFoldoutBool);
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    private void BuildButtons()
    {
        buildBool = EditorGUILayout.BeginFoldoutHeaderGroup(buildBool, "Build");
        if (buildBool)
        {
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(mapName) || previewImage == null))
            {
                EditorGUILayout.HelpBox("4. Select \"Build\" to create a map, or \"Build and Run\" to build and run this map in-game for testing purposes.", MessageType.Info);

                if (GUILayout.Button("Build", GUILayout.Height(height), GUILayout.Width(position.width - offset)))
                {
                    if (!ValidateFields())
                        return;

                    if (previewPath != null)
                    {
                        MapUtils.ImporterSettings(previewPath);
                    }

                    MapUtils.CreateConfig(exportPath, mapName);
                    MapUtils.Build(scenePath, exportPath, previewPath);
                }

                if (GUILayout.Button("Build and Run", GUILayout.Height(height), GUILayout.Width(position.width - offset)))
                {
                    if (!ValidateFields())
                        return;

                    if (previewPath != null)
                        MapUtils.ImporterSettings(previewPath);

                    MapUtils.CreateConfig(exportPath, mapName);
                    MapUtils.Build(scenePath, exportPath, previewPath, true);
                }
                EditorGUILayout.HelpBox("4.1. After assembly you can launch the game to view the map in the lobby", MessageType.Info);
            }
        }
        EditorPrefs.SetBool("buildBool", buildBool);
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    private void OpenExportFolder()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Open export folder", GUILayout.Height(height), GUILayout.Width(position.width - offset)))
        {
            if (!Directory.Exists(basePath))
                CreateFolder(basePath);

            EditorUtility.RevealInFinder(basePath + "/");
        }
        GUILayout.EndHorizontal();
    }

    private string AssemblyExportPath()
    {
        return Path.Combine(basePath, MapUtils.RegexRename(mapName));
    }
    private bool ValidateFields()
    {
        if (MapUtils.ValidateFields(basePath, mapName))
        {
            GunsBoxSpawnPoint[] gBoxSpawnPoints = FindObjectsOfType<GunsBoxSpawnPoint>();

            if (gBoxSpawnPoints.Length == 0)
            {
                MapUtils.DisplayError("No spawn point", "Please added spawn point in scene.");
                return false;
            }
            else if (gBoxSpawnPoints.Length > 1)
            {
                MapUtils.DisplayError("Spawn Points Error", "There are more than one spawn points.");

                return false;
            }

            GunsBoxInventory[] inventories = FindObjectsOfType<GunsBoxInventory>();

            if (inventories.Length == 0)
            {
                MapUtils.DisplayError("No inventory", "Please added inventory in scene.");
                return false;
            }
            else if (inventories.Length > 1)
            {
                MapUtils.DisplayError("Inventory Error", "There are more than inventories.");

                return false;
            }
        }
        else
        {
            return false;
        }

        return true;
    }
    private void CreateFolder(string path)
    {
        Directory.CreateDirectory(path);
    }
}

#endif