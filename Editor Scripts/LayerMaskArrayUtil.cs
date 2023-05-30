using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace I3dImporter {
static class LayerMaskArrayUtil
{
    // Start is called before the first frame update
    public static void createMaskArrays()
    {
        if (!System.IO.Directory.Exists("Assets/Resources/MaskArrays")) {System.IO.Directory.CreateDirectory("Assets/Resources/MaskArrays");}

        string[] maskNames = AssetDatabase.FindAssets("04_weight");
        for (int i = 0; i < maskNames.Length; i++)
        {
            maskNames[i] = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(maskNames[i]));
        }

        foreach (var maskName in maskNames)
        {
            Texture2D[] masks = new Texture2D[4];
            Debug.Log(maskName);
            string maskPrefix = maskName.Split('0')[0];


            for (int i=0; i<masks.Length; i++) {      
                masks[i] = Resources.Load<Texture2D>("Textures/Masks/"+ maskPrefix + "0" + (i+1) + "_weight");
                            
                SetMaskImporterFormat(masks[i]);
                masks[i].Apply();
            }
            Texture2DArray array = new Texture2DArray(masks[0].width, masks[0].height, masks.Length, masks[0].format, false, true);

            for (int i=0; i<masks.Length; i++)
            {
                Graphics.CopyTexture(masks[i], 0, array, i);
            }

            AssetDatabase.CreateAsset(array, "Assets/Resources/MaskArrays/"+ maskPrefix +"LayerMaskArray.asset");
        }
    }

        private static void SetMaskImporterFormat(Texture2D texture)
    {
        if ( null == texture ) return;

        string assetPath = AssetDatabase.GetAssetPath( texture );
        var tImporter = AssetImporter.GetAtPath( assetPath ) as TextureImporter;
        if ( tImporter != null )
        {
            tImporter.textureType = TextureImporterType.Default;

            tImporter.isReadable = true;
            tImporter.sRGBTexture = false;
            tImporter.mipmapEnabled = false;

            AssetDatabase.ImportAsset( assetPath );
            AssetDatabase.Refresh();
        }
    }
}
}
