using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace I3dImporter {

    public class ImportHandler 
    {
        string progressBarText = "";
        float progressBarValue;
        bool buttonError = false;
        ParseXML parser = new ParseXML();


        /*===================== TEXTURE IMPORTING SECTION =======================*/
        /* We have a standard version and 1 override:
            1) THE STANDARD VERSION IMPORTS A NATIVE FARMING SIMULATOR MAP
            2) THE OVERRIDE VERSION IMPORTS A MODDED MAP
        */

        public void importTextures(string I3Dpath, string FSpath, bool texArrays)   // IMPORT NATIVE MAP TEXTURES
        {   
            parser.clearAllDicts();
            progressBarText = "Parsing I3D File...";
            if (!parser.parseFile(I3Dpath)) {
                buttonError = true;
                return;
            }
            buttonError = false;

            // Setup Directories for textures: Standard and Mask Textures
            if (!System.IO.Directory.Exists("Assets/Resources/Textures")) {System.IO.Directory.CreateDirectory("Assets/Resources/Textures");}
            if (!System.IO.Directory.Exists("Assets/Resources/Textures/Masks")) {System.IO.Directory.CreateDirectory("Assets/Resources/Textures/Masks");}

            string texPath = "";
            progressBarValue = (float) 0;
            float progressBarStep = (float) 1/parser.textures.Count;

            foreach (var texPathTemp in parser.textures.Values)
            {
                if ( ( !texPathTemp.Contains("png") && !texPathTemp.Contains("dds") ) || System.IO.File.Exists("Assets/Resources/Textures/" + System.IO.Path.GetFileName(texPathTemp))) {continue;}

                progressBarText = "Importing Texture: " + texPath;
                progressBarValue += progressBarStep;
                EditorUtility.DisplayProgressBar("Importing and Converting Textures...", progressBarText, (float) progressBarValue);

                texPath = texPathTemp.Replace("$", FSpath + "/");

                if (texPath.Contains("weight")) {    // If a texture mask for the terrain
                    if (!System.IO.File.Exists(texPath) || System.IO.File.Exists("Assets/Resources/Textures/Masks/" + System.IO.Path.GetFileName(texPathTemp))) {continue;}
                    System.IO.File.Copy(texPath, "Assets/Resources/Textures/Masks/" + System.IO.Path.GetFileName(texPath), overwrite: true);
                }
                else {                              // If a standard texture
                    if (!System.IO.File.Exists(texPath)) {           // Try original texture name                   
                        texPath = texPath.Replace("png", "dds");    // Try DDS texture
                        if (!System.IO.File.Exists(texPath)) {continue;}
                    }
                    System.IO.File.Copy(texPath, "Assets/Resources/Textures/" + System.IO.Path.GetFileName(texPath), overwrite: true);
                }
            }

            if (!System.IO.File.Exists("Assets/Resources/Textures/ddsToPng.bat")) {System.IO.File.Copy("Assets/Resources/ddsToPng.bat", "Assets/Resources/Textures/ddsToPng.bat");}

            EditorUtility.DisplayProgressBar("Importing and Converting Textures...", "Converting all textures to PNG...", (float) progressBarValue);
            I3dUtils.ExecuteCommand("ddsToPng.bat");

            EditorUtility.ClearProgressBar();  
            AssetDatabase.Refresh();

            // Create texture arrays for the terrain
            if (texArrays) {createTexArrays();}
            AssetDatabase.Refresh(); 
        }

        public void importTextures(string I3Dpath, string FSpath, string ModPath, bool texArrays)   // // IMPORT MODDED MAP TEXTURES
        {   
            parser.clearAllDicts();
            progressBarText = "Parsing I3D File...";
            if (!parser.parseFile(I3Dpath)) {
                buttonError = true;
                return;
            }
            buttonError = false;

            // Setup Directories for textures: Standard and Mask Textures
            if (!System.IO.Directory.Exists("Assets/Resources/Textures")) {System.IO.Directory.CreateDirectory("Assets/Resources/Textures");}
            if (!System.IO.Directory.Exists("Assets/Resources/Textures/Masks")) {System.IO.Directory.CreateDirectory("Assets/Resources/Textures/Masks");}

            string texPath = "";
            progressBarValue = (float) 0;
            float progressBarStep = (float) 1/parser.textures.Count;

            foreach (var texPathTemp in parser.textures.Values)
            {
                if ( ( !texPathTemp.Contains("png") && !texPathTemp.Contains("dds") ) || System.IO.File.Exists("Assets/Resources/Textures/" + System.IO.Path.GetFileName(texPathTemp))) {continue;}

                progressBarText = "Importing Texture: " + texPath;
                progressBarValue += progressBarStep;
                EditorUtility.DisplayProgressBar("Importing and Converting Textures...", progressBarText, (float) progressBarValue);

                
                if (texPathTemp.Contains("$")) {    // TEXUTRE COMES FROM GAME FOLDER
                    texPath = texPathTemp.Replace("$", FSpath + "/");
                }
                else {                              // TEXTURE COMES FROM MOD FOLDER
                    texPath = ModPath + "/" + texPathTemp;
                }

                if (texPath.Contains("weight")) {    // If a texture mask for the terrain
                    if (!System.IO.File.Exists(texPath) || System.IO.File.Exists("Assets/Resources/Textures/Masks/" + System.IO.Path.GetFileName(texPathTemp))) {continue;}
                    System.IO.File.Copy(texPath, "Assets/Resources/Textures/Masks/" + System.IO.Path.GetFileName(texPath), overwrite: true);
                }
                else {                              // If a standard texture
                    if (!System.IO.File.Exists(texPath)) {           // Try PNG texture                     
                        texPath = texPath.Replace("png", "dds");    // Try DDS texture
                        if (!System.IO.File.Exists(texPath)) {continue;}
                    }
                    System.IO.File.Copy(texPath, "Assets/Resources/Textures/" + System.IO.Path.GetFileName(texPath), overwrite: true);
                }
            }

            if (!System.IO.File.Exists("Assets/Resources/Textures/ddsToPng.bat")) {System.IO.File.Copy("Assets/Resources/ddsToPng.bat", "Assets/Resources/Textures/ddsToPng.bat");}

            EditorUtility.DisplayProgressBar("Importing and Converting Textures...", "Converting all textures to PNG...", (float) progressBarValue);
            I3dUtils.ExecuteCommand("ddsToPng.bat");

            EditorUtility.ClearProgressBar();  
            AssetDatabase.Refresh();

            // Create texture arrays for the terrain
            if (texArrays) {createTexArrays();}
            AssetDatabase.Refresh(); 
        }

        /*========================================================================*/

        /*=================================== SHADER SECTION =====================================*/

        public void editMaterials()
        {
            GameObject currentObject = null;
            Material transparentMaterialReference = Resources.Load<Material>("ReferenceMaterials/transparent_mat");

            progressBarValue = (float) 0;
            float progressBarStep = (float) 1/parser.shapes.Count;

            foreach (var shape in parser.shapes)
            {
                string primaryTransformGroup = parser.getPrimaryTransformGroupName(shape.Key.nodeId);
                if (primaryTransformGroup == "") {
                    Debug.Log("Primary Group not found for shape [" + shape.Key.name + "]");
                }
                var objectsWithName = Resources.FindObjectsOfTypeAll<GameObject>().Where(o => o.name == shape.Key.name);
                if (objectsWithName.Count<GameObject>() > 1) {
                    foreach (var o in objectsWithName)
                    {
                        var parentTransform = o.GetComponentsInParent<Transform>();
                        
                        //Debug.Log(o.name + " " + shape.Key.nodeId + " " + primaryTransformGroup + " " + parentTransform.Count());

                        if(parentTransform.Count() != 0 && parentTransform[1].gameObject.name.Contains(primaryTransformGroup)) {
                            currentObject = o;
                            break;
                        }

                    }
                }
                else if (objectsWithName.Count<GameObject>() == 1){
                    currentObject = objectsWithName.First<GameObject>();
                }
                else {continue;}

                if(currentObject == null) {continue;}

                Renderer renderer = currentObject.GetComponent<Renderer>();

                //Material materialOfObject = renderer.sharedMaterial;
                Material material = renderer.sharedMaterial;

                Shader FS_standard_Shader = Shader.Find("Shader Graphs/FS_Lit");
                Shader HDRP_standard_Shader = Shader.Find("HDRP/Lit");
                if (FS_standard_Shader == null || HDRP_standard_Shader == null) {
                    Debug.Log("Couldn't find the requested shaders...");
                    break;
                }
                progressBarText = "Editing TMaterial: " + material.name;
                progressBarValue += progressBarStep;

                EditorUtility.DisplayProgressBar("Editing Materials from current Scene...", progressBarText, (float) progressBarValue);

                material.shader = FS_standard_Shader;
                
                // If Material is Transparent
                if (shape.Value.alphaBlending) {
                    material.CopyPropertiesFromMaterial(transparentMaterialReference);
                    //renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }

                //BaseColorMap
                if (shape.Value.baseMap != "") {
                    Texture2D baseColorMap = Resources.Load<Texture2D>("Textures/" + System.IO.Path.GetFileNameWithoutExtension(shape.Value.baseMap));
                    if(baseColorMap != null) {
                        material.SetTexture("_DiffuseMap", baseColorMap);
                    }
                }

                //NormalMap
                if (shape.Value.normalMap != "") {
                    Texture2D normalMap = Resources.Load<Texture2D>("Textures/" + System.IO.Path.GetFileNameWithoutExtension(shape.Value.normalMap));
                    if(normalMap != null) {
                        material.SetTexture("_NormalMap", normalMap);
                    }
                }

                //DetailMap
                if (shape.Value.detailMap != "") {
                    Texture2D detailMap = Resources.Load<Texture2D>("Textures/" + System.IO.Path.GetFileNameWithoutExtension(shape.Value.detailMap));
                    if(detailMap != null) {
                        material.SetTexture("_SpecularMap", detailMap);
                    }
                }
                renderer.sharedMaterial = material;
                material = null;
                currentObject = null;
            }
            EditorUtility.ClearProgressBar();
        }

        /*========================================================================*/

        public void substituteMapTrees(GameObject treeParent)
        {
            System.Random r = new System.Random();
            if (treeParent == null) {Debug.Log("NOOOOOOOOOO");}
            Renderer[] trees = treeParent.GetComponentsInChildren<Renderer>();
            foreach (var treeRenderer in trees)
            {
                GameObject tree;
                GameObject clone;
                string treeType = treeRenderer.gameObject.name.Split('_')[0];
                string treeStage = treeRenderer.gameObject.name.Split('_')[1];
                Debug.Log(treeType + "_" + treeStage);
                tree = Resources.Load<GameObject>("Prefabs/" + treeType + "_" + treeStage);
                Vector3 worldCoords = treeRenderer.bounds.center;
                worldCoords.y = treeRenderer.bounds.min.y;

                if (tree == null) {
                    tree = Resources.Load<GameObject>("Prefabs/spruceVar01_stage03");
                }

                clone = UnityEngine.Object.Instantiate(tree, worldCoords, Quaternion.Euler(0, r.Next(0, 360), 0));
                clone.GetComponent<Transform>().parent = treeParent.transform;
                UnityEngine.Object.DestroyImmediate(treeRenderer.gameObject);
            }
        }

        // CREATE TEXTURE ARRAYS FOR THE TERRAIN SHADER
        private void createTexArrays() {
            LayerMaskArrayUtil.createMaskArrays();
            TextureArrayUtil.createTextureArrays();
        }
    }

}