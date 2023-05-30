using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace I3dImporter {
static class TextureArrayUtil
{
    // Start is called before the first frame update
    public static void createTextureArrays()
    {
        if (!System.IO.Directory.Exists("Assets/Resources/TextureArrays")) {System.IO.Directory.CreateDirectory("Assets/Resources/TextureArrays");}      

        string[] textureNames = AssetDatabase.FindAssets("04_diffuse t:texture2D");
        ArrayUtility.AddRange(ref textureNames, AssetDatabase.FindAssets("04_DLC_diffuse t:texture2D"));
        for (int i = 0; i < textureNames.Length; i++)
        {
            textureNames[i] = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(textureNames[i]));
        }
        bool DLC = false;

        foreach (var textureName in textureNames)
        {
            Texture2D[] texturesDiffuse = new Texture2D[4];
            Texture2D[] texturesNormal = new Texture2D[4];
            
            DLC = false;
            if (textureName.Contains("DLC")) {DLC = true;}
            string texturePrefix = textureName.Split('0')[0];

            for (int i=0; i<texturesDiffuse.Length; i++) {
                if (DLC) {
                    texturesDiffuse[i] = Resources.Load<Texture2D>("Textures/" + texturePrefix + "0" + (i+1) + "_DLC_diffuse");
                }
                else {
                    texturesDiffuse[i] = Resources.Load<Texture2D>("Textures/" + texturePrefix + "0" + (i+1) + "_diffuse");
                }

                if (texturesDiffuse[i] != null) {
                    SetTextureImporterFormat(texturesDiffuse[i], true, false);
                    texturesDiffuse[i].Apply();
                }
            }
            for (int i=0; i<texturesNormal.Length; i++) {
                if (DLC) {
                    texturesNormal[i] = Resources.Load<Texture2D>("Textures/" + texturePrefix + "0" + (i+1) + "_DLC_normal");
                }
                else {
                    texturesNormal[i] = Resources.Load<Texture2D>("Textures/" + texturePrefix + "0" + (i+1) + "_normal");
                }

                if (texturesNormal[i] != null) {    
                    SetTextureImporterFormat(texturesNormal[i], true, true);
                    texturesNormal[i].Apply();
                }
            }

            //If there's any texture missing, don't create these arrays
            if (checkMissingTextures(ref texturesDiffuse, ref texturesNormal)) {continue;}

            
            Texture2DArray arrayDiffuse = new Texture2DArray(texturesDiffuse[0].width, texturesDiffuse[0].height, texturesDiffuse.Length, texturesDiffuse[0].format, true);
            Texture2DArray arrayNormal = new Texture2DArray(texturesNormal[0].width, texturesNormal[0].height, texturesNormal.Length, texturesNormal[0].format, true, true);

            for (int i=0; i<texturesDiffuse.Length; i++)
            {   
                for (int mip=0; mip<texturesDiffuse[i].mipmapCount; mip++) {
                    Graphics.CopyTexture(texturesDiffuse[i], 0, mip, arrayDiffuse, i, mip);
                }
            }
            AssetDatabase.CreateAsset(arrayDiffuse, "Assets/Resources/TextureArrays/" + texturePrefix + "Array_Diffuse.asset");

            for (int i=0; i<texturesNormal.Length; i++)
            {
                for (int mip=0; mip<texturesNormal[i].mipmapCount; mip++) {
                    Graphics.CopyTexture(texturesNormal[i], 0, mip, arrayNormal, i, mip);
                }
            }
            AssetDatabase.CreateAsset(arrayNormal, "Assets/Resources/TextureArrays/" + texturePrefix + "Array_Normal.asset");
        }
    }

    private static bool checkMissingTextures(ref Texture2D[] texturesDiffuse, ref Texture2D[] texturesNormal) {

        // Diffuse and Normal arrays' length is the same
        for (int i = 0; i < texturesDiffuse.Length; i++) {
            if (texturesDiffuse[i] == null || texturesNormal[i] == null) {
                return true;
            }
        }

        return false;
    }

    private static void SetTextureImporterFormat( Texture2D texture, bool isReadable, bool isNormal)
    {
        if ( null == texture ) return;

        string assetPath = AssetDatabase.GetAssetPath( texture );
        var tImporter = AssetImporter.GetAtPath( assetPath ) as TextureImporter;
        if ( tImporter != null )
        {
            tImporter.textureType = TextureImporterType.Default;
            if (isNormal) {
                tImporter.textureType = TextureImporterType.NormalMap;
            }

            tImporter.isReadable = isReadable;
            tImporter.mipmapEnabled = true;
            tImporter.maxTextureSize = 512;

            AssetDatabase.ImportAsset( assetPath );
            AssetDatabase.Refresh();
        }
    }
}
}
