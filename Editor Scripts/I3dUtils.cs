using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Linq;
using UnityEditor;

namespace I3dImporter {

    static class I3dUtils 
    {

        private static ArrayList unwantedObjects = new ArrayList() {"snow", "occluderMesh", "col", "collision", "extraCollisions", "icicles"};
        private static ArrayList wantedLOD1s = new ArrayList() {"oak", "spruce", "rock", "birch", "pine", "poplar", "willow", "cypress"};
        private static ArrayList treeNames = new ArrayList() {"oak", "spruce", "birch", "pine", "poplar", "willow", "cypress"};

        public static void cleanAndConfigureLODs()
        {
            GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();

            foreach (var obj in objects)
            {
                deleteUnwantedObjects(obj);
                checkForVisVersionAndDelete(obj);
                handleLODVersions(obj);
            }
        }

        private static void deleteUnwantedObjects(GameObject o)
        {
            if (o != null && unwantedObjects.Contains(o.name)) {
                try
                {
                    UnityEngine.Object.DestroyImmediate(o);
                }
                catch (System.Exception)
                {
                    Debug.Log(o.name + " cannot be destroyed as it doesn't exist!");
                }
            }
        }

        private static void checkForVisVersionAndDelete(GameObject o)
        {
            if (o != null && o.name.EndsWith("Vis")) {

                try
                {
                    GameObject nonVisObj = GameObject.Find(o.name.Replace("Vis", ""));
                    if (nonVisObj != null) {
                        UnityEngine.Object.DestroyImmediate(GameObject.Find(o.name.Replace("Vis", "")));
                    }
                }
                catch (System.Exception)
                {
                    Debug.Log(o.name + " cannot be destroyed as it doesn't exist!");
                }
            }
        }

         static void handleLODVersions(GameObject o)
        {
            if (o == null) {return;}
            bool isWantedLOD1 = false;
            foreach (string wantedLod1 in wantedLOD1s)
            {
                if (o.name.ToLower().Contains(wantedLod1)) {isWantedLOD1=true;}
            }
            //For now, the "handling" just means deleting the LOD1 and LOD versions of everything but not the trees and rocks
            if ((o.name.EndsWith("LOD") || o.name.Contains("LOD1")) && !isWantedLOD1) {
                try
                {
                    UnityEngine.Object.DestroyImmediate(o);
                    isWantedLOD1 = false;
                }
                catch (System.Exception)
                {
                    Debug.Log(o.name + " cannot be destroyed as it doesn't exist!");
                }
            }
            return;
        }

        private static Texture2D loadTexture2D(string texture_path)
        {
            var rawData = System.IO.File.ReadAllBytes(texture_path);
            Texture2D tex = new Texture2D(2,2);
            if(!tex.LoadImage(rawData)) {
                return Texture2D.whiteTexture;
            }
            return tex;
        }

        public static void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.WorkingDirectory = Application.dataPath + "/Resources/Textures";
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("output>>" + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("error>>" + e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }

        public static void recalculateNormals(GameObject meshParent)
        {
            IEnumerable<MeshFilter> meshesFromParent = meshParent.GetComponentsInChildren<MeshFilter>();
            float progressBarValue = 0;
            float progressBarStep = (float) 1/meshesFromParent.Count();

            foreach (var m in meshesFromParent)
            {
                if (m == null) {continue;}

                string progressBarText = "Recalculating for: " + m.name;
                progressBarValue += progressBarStep;
                EditorUtility.DisplayProgressBar("Recalculating Normals and Tangents for each mesh.", progressBarText, (float) progressBarValue);

                m.sharedMesh.Optimize();
                m.sharedMesh.RecalculateNormals();
                m.sharedMesh.RecalculateTangents();
                Mesh newMesh = new Mesh();
                newMesh = Mesh.Instantiate(m.sharedMesh);
                AssetDatabase.CreateAsset(newMesh, "Assets/Resources/RecalculatedMeshes/" + m.gameObject.GetComponentsInParent<Transform>()[1].gameObject.name + "/" + newMesh.name + ".asset");
                newMesh = Resources.Load<Mesh>("RecalculatedMeshes/" + m.gameObject.GetComponentsInParent<Transform>()[1].gameObject.name + "/" + newMesh.name);
                //AssetDatabase.CreateAsset(newMesh, "Assets/Resources/RecalculatedMeshes/" + newMesh.name + ".asset");
                //newMesh = Resources.Load<Mesh>("RecalculatedMeshes/"+ newMesh.name);
                m.sharedMesh = newMesh;
                EditorUtility.SetDirty(m);
            }
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
    }
}
