using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace I3dImporter {
    public class ShapeInfo
    {
        public string name;
        public int nodeId;

        public ShapeInfo() {
            name = "";
            nodeId = 0;
        }

        public ShapeInfo(string name, int nodeId) {
            this.name = name;
            this.nodeId = nodeId;
        }

        public override string ToString()
        {
            return "Name: " + name + "\nnodeId: " + nodeId;
        }
    }
}
