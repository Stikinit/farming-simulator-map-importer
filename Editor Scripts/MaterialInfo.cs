using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace I3dImporter {
    public class MaterialInfo
        {
        public string name;
        public string baseMap;
        public string normalMap;
        public string detailMap;
        public bool alphaBlending;

        public MaterialInfo() {
            name = "";
            baseMap = "";
            normalMap = "";
            detailMap = "";
            alphaBlending = false;
        }

        public override string ToString()
        {
            return "baseMap: " + baseMap + "\nnormalMap: " + normalMap + "\ndetailMap: " + detailMap + "\nalphaBending: " + alphaBlending;
        }
    }
}
