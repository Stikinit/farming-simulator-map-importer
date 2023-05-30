using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using UnityEngine.Rendering.HighDefinition;
using Debug = UnityEngine.Debug;



namespace I3dImporter {
public class I3dImportWindow : EditorWindow 
{
    ImportHandler importHandler = new ImportHandler();
    Rect headerSection;
    Rect bodySection;
    Rect footerSection;
    string FSpath = "C:/Program Files (x86)/Farming Simulator 2022";
    string I3Dpath = "Assets";
    string ModPath = "Assets";
    bool isMod = false;
    bool createTexArrays = false;
    bool buttonError = false;
    GameObject treeParent;
    GameObject meshParent;
    GUISkin skin;
    Texture2D headerTexture;
    float smoothingAngle = 0;
    
    //  Create the menu short cut for the tool
    [MenuItem("I3D Map Import Helper/Open Tool")]
    static void openWindow()
    {
        I3dImportWindow window = (I3dImportWindow)GetWindow(typeof(I3dImportWindow));
        window.minSize = new Vector2(400,600);
        window.maxSize = new Vector2(400,600);
        window.Show();
    }

    //  Same behavior as Start() or Awake()
    void OnEnable()
    {
        skin = Resources.Load<GUISkin>("guiStyles/I3DMI_Skin");
        initTextures();
        createMeshFolders();
    }

    //  Setup the Project files by creating new folders
    void createMeshFolders()
    {
         if (!System.IO.Directory.Exists("Assets/Resources/RecalculatedMeshes")) 
            {System.IO.Directory.CreateDirectory("Assets/Resources/RecalculatedMeshes");} 

        IEnumerable<GameObject> parents = GameObject.FindObjectsOfType<GameObject>().Where(o => o.GetComponent<MeshRenderer>() == null);

        foreach (var p in parents)
        {
            if (!System.IO.Directory.Exists("Assets/Resources/RecalculatedMeshes/" + p.name)) 
                {System.IO.Directory.CreateDirectory("Assets/Resources/RecalculatedMeshes/" + p.name);}  
        }

        AssetDatabase.Refresh();
    }

    //  Load the header texture for the GUI of the tool
    void initTextures()
    {
        headerTexture = Resources.Load<Texture2D>("fs_header");
    }

    // Similar to Update(), but it happens on specific inputs by the user (click, mouse move, typing, ...)
    void OnGUI()
    {
        skin = Resources.Load<GUISkin>("guiStyles/I3DMI_Skin");
        DrawLayouts();
        DrawHeader();
        DrawBody();
        GUIUtility.ExitGUI();
    }

    //Define GUI sections
    void DrawLayouts()
    {
        headerSection.x = 0;
        headerSection.y = 0;
        headerSection.width = 400;
        headerSection.height = (float)(Screen.height * 0.15);

        GUI.DrawTexture(headerSection, headerTexture);

        bodySection.x = 0;
        bodySection.y = (float)(Screen.height * 0.15);
        bodySection.width = 400;
        bodySection.height = (float)(Screen.height * 0.8);
    }

    void DrawHeader()
    {
        GUILayout.BeginArea(headerSection);

        GUILayout.BeginHorizontal();
        GUILayout.Label("I3D Map Importer", skin.GetStyle("header"));
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    void DrawBody()
    {
        GUILayout.BeginArea(bodySection);
        GUILayout.Space(20);
        DrawFileExplorers();
        GUILayout.Space(20);
        DrawBodyButtons();
        GUILayout.Space(10);
        DrawNormalManager();
        GUILayout.Space(10);
        DrawTreeManager();
        GUILayout.EndArea();
    }

    void DrawFileExplorers()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Farming Simulator installation folder");
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.TextField(FSpath, GUILayout.Width(340), GUILayout.Height(20));    //  Folder text field

        if(GUILayout.Button("...", GUILayout.Width(30), GUILayout.Height(20))) {    // Button that opens file explorer
            FSpath = EditorUtility.OpenFolderPanel("Select the Farming Simulator installation folder", FSpath, "");
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.Label("I3D Map File Path");
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.TextField(I3Dpath, GUILayout.Width(340), GUILayout.Height(20));   //  Folder text field

        if(GUILayout.Button("...", GUILayout.Width(30), GUILayout.Height(20))) {    // Button that opens file explorer
            I3Dpath = EditorUtility.OpenFilePanel("Select the I3D map file", I3Dpath, "");
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Create Terrain Texture Arrays after Import");
        GUILayout.Space(10);
        createTexArrays = EditorGUILayout.Toggle(createTexArrays);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Importing Mod Map");
        GUILayout.Space(-100);
        isMod = EditorGUILayout.Toggle(isMod);
        GUILayout.EndHorizontal();

        if (isMod) {
            GUILayout.Label("Mod Map File Path (Parent folder of the 'maps' folder)");
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.TextField(ModPath, GUILayout.Width(340), GUILayout.Height(20));
            if(GUILayout.Button("...", GUILayout.Width(30), GUILayout.Height(20))) {
                ModPath = EditorUtility.OpenFolderPanel("Select parent of mod's 'maps' folder", ModPath, "");
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    //  Main buttons for import workflow
    void DrawBodyButtons()
    {
        if (GUILayout.Button("Import All Textures", GUILayout.Height(25), GUILayout.Width(394))) {
            if (FSpath == "" || I3Dpath == "") {
                buttonError = true;
            }
            else {
                buttonError = false;
                if (isMod) {
                    importHandler.importTextures(I3Dpath, FSpath, ModPath, createTexArrays);
                }   
                else {
                    importHandler.importTextures(I3Dpath, FSpath, createTexArrays);
                }
                AssetDatabase.Refresh();
            }
             
        }
        

        if (GUILayout.Button("Assign Textures and Materials", GUILayout.Height(25), GUILayout.Width(394))) {
            if (FSpath == "" || I3Dpath == "") {
                buttonError = true;
            }
            else {
                buttonError = false;
                importHandler.editMaterials();
            }
            
        }

        if (GUILayout.Button("Clean the Scene and Configure LOD levels", GUILayout.Height(25), GUILayout.Width(394))) {
            if (FSpath == "" || I3Dpath == "") {
                buttonError = true;
            }
            else {
                buttonError = false;
                I3dUtils.cleanAndConfigureLODs();
            }
            
        }

    }

    //  Create the Normal Recalculation portion of the GUI
    void DrawNormalManager()
    {
        meshParent = (GameObject) EditorGUILayout.ObjectField(meshParent, typeof(GameObject), true, GUILayout.Width(394));

        if (GUILayout.Button("Recalculate Normals for Map objects", GUILayout.Height(25), GUILayout.Width(394))) {
            if (meshParent == null) {
                buttonError = true;
            }
            else {
                I3dUtils.recalculateNormals(meshParent);
                buttonError = false;
            }

        }

        if (buttonError) {
            EditorGUILayout.HelpBox("Before any operation, make sure that both the [FS Installation Folder] and the [I3D Map File Folder] fields are correct.", MessageType.Error);
        }
    }

    //  Create the Tree Substitution portion of the GUI
    void DrawTreeManager()
    {
        GUILayout.Label("Select parent object for all trees");
        treeParent = (GameObject) EditorGUILayout.ObjectField(treeParent, typeof(GameObject), true, GUILayout.Width(394));

        if (GUILayout.Button("Create trees based on map position", GUILayout.Height(25), GUILayout.Width(394))) {
            if (treeParent == null) {
                buttonError = true;
            }
            else {
                buttonError = false;
                importHandler.substituteMapTrees(treeParent);
            }
            
        }

        if (buttonError) {
            EditorGUILayout.HelpBox("Before any operation, make sure that the [Tree Parent Object] field is not empty.", MessageType.Error);
        }
    }
}
}
