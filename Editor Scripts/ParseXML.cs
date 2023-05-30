using System.Collections;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace I3dImporter
{
    
public class ParseXML
{
    private XDocument xmlDoc;
    public Dictionary<int, string> textures = new Dictionary<int, string>();
    public Dictionary<int, MaterialInfo> materials = new Dictionary<int, MaterialInfo>();
    public Dictionary<ShapeInfo, MaterialInfo> shapes = new Dictionary<ShapeInfo, MaterialInfo>();
    public Dictionary<string, int> primaryTransformGroups = new Dictionary<string, int>();

    private void parseTextures()
    {
        var textureNodes = xmlDoc.Descendants("Files").Elements();
        foreach (var tn in textureNodes)
        {
            if (textures.ContainsKey(int.Parse(tn.Attribute("fileId").Value))) {
                continue;
            }

            textures.Add(int.Parse(tn.Attribute("fileId").Value), tn.Attribute("filename").Value);

        }
    }

    private void parseMaterials()
    {
        var materialNodes = xmlDoc.Descendants("Materials").Elements();

        foreach (var mn in materialNodes)
        {
            MaterialInfo mi = new MaterialInfo();
            if (!mn.HasElements || materials.ContainsKey(int.Parse(mn.Attribute("materialId").Value))) {
                continue;
            }

            if (mn.Element("Texture") != null) {mi.baseMap = textures[int.Parse(mn.Element("Texture").Attribute("fileId").Value)];}
            if (mn.Element("Normalmap") != null) {mi.normalMap = textures[int.Parse(mn.Element("Normalmap").Attribute("fileId").Value)];}
            if (mn.Element("Glossmap") != null) {mi.detailMap = textures[int.Parse(mn.Element("Glossmap").Attribute("fileId").Value)];}
            if (mn.Attribute("alphaBlending") != null) {mi.alphaBlending = true;}

            materials.Add(int.Parse(mn.Attribute("materialId").Value), mi);
        }
    }

    private void parseShapes()
    {
        var shapeNodes = xmlDoc.Descendants("Shape");
        foreach (var sn in shapeNodes)
        {

            if (sn.Attribute("materialIds").Value == "") {continue;}

            if (sn.Attribute("materialIds").Value.Contains(",")) {
                sn.Attribute("materialIds").SetValue(sn.Attribute("materialIds").Value.Split(',')[0]);
            }

            if (sn.Attribute("materialIds") == null || sn.Attribute("name").Value.Equals("snow") || 
            !materials.ContainsKey(int.Parse(sn.Attribute("materialIds").Value))) {continue;}

            if (shapes.ContainsKey(new ShapeInfo(sn.Attribute("name").Value, int.Parse(sn.Attribute("nodeId").Value)))) {continue;}
            //shapes.Keys.Where(k => k.nodeId == int.Parse(sn.Attribute("nodeId").Value)).Count() > 0 ||

            ShapeInfo shape = new ShapeInfo();
            shape.name = sn.Attribute("name").Value;
            shape.nodeId = int.Parse(sn.Attribute("nodeId").Value);
            shapes.Add(shape, materials[int.Parse(sn.Attribute("materialIds").Value)]);
        }
    }

    private void parseMapGroups()
    {
        var sceneNodes = xmlDoc.Descendants("TransformGroup").Where(n => n.Parent.Name == "Scene");
        foreach (var sceneNode in sceneNodes)
        {
            if (primaryTransformGroups.ContainsKey(sceneNode.Attribute("name").Value)) {continue;}
            
            primaryTransformGroups.Add(sceneNode.Attribute("name").Value, int.Parse(sceneNode.Attribute("nodeId").Value));
        }
        primaryTransformGroups = primaryTransformGroups.OrderBy(i => i.Value).ToDictionary(i => i.Key, i => i.Value);
    }

    public bool parseFile(string filepath)
    {
        xmlDoc = XDocument.Load(filepath);
        if (xmlDoc == null) {
            Debug.Log("The XML file does not exist...");
            return false;
        }
        else {
            parseTextures();
            parseMaterials();
            parseMapGroups();
            parseShapes();
        }
        return true;
    }

    public string getPrimaryTransformGroupName(int nodeId)
    {
        if (primaryTransformGroups.Count() == 0) {return "";}

        var group = primaryTransformGroups.TakeWhile(x => x.Value < nodeId).DefaultIfEmpty(primaryTransformGroups.Last()).LastOrDefault();
 
        if (group.Key == null) {return "";}

        return group.Key;
    }

    private void clearTextureDict() 
    {
        this.textures.Clear();
    }

    private void clearMaterialDict() 
    {
        this.materials.Clear();
    }

    private void clearShapeDict() 
    {
        this.shapes.Clear();
    }

    private void clearGroupDict()
    {
        this.primaryTransformGroups.Clear();
    }

    public void clearAllDicts()
    {
        clearTextureDict();
        clearMaterialDict();
        clearShapeDict();
        clearGroupDict();
    }
}
}
