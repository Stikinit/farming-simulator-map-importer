-- Author:C05156B
-- Name:setup_scene_hierarchy
-- Description:
-- Icon:
-- Hide: no

local namesToRename = {"LOD0", "LOD1", "LOD0Attachments"}
local treeNames = {"americanElm", "birch", "cypress", "downeyServiceBerry", "maple", "oak",
    "oliveTree", "pagodaDogwood", "pine", "poplar", "shagbarkHickory", "spruce", "stonePine", "willow"}
local treeCountTable = {}
local visitedIds = {}
local mapIds = {}
local mapRootId = 0
local rootId = getChildAt(getRootNode(), 0)

treeCountTable["americanElm"] = 0;
treeCountTable["birch"] = 0;
treeCountTable["cypress"] = 0;
treeCountTable["downeyServiceBerry"] = 0;
treeCountTable["maple"] = 0;
treeCountTable["oak"] = 0;
treeCountTable["oliveTree"] = 0;
treeCountTable["pagodaDogwood"] = 0;
treeCountTable["pine"] = 0;
treeCountTable["poplar"] = 0;
treeCountTable["shagbarkHickory"] = 0;
treeCountTable["spruce"] = 0;
treeCountTable["stonePine"] = 0;
treeCountTable["willow"] = 0;

function getSpecificGroupId(groupName, groupIds)
    return getSpecificGroupIdRecursive(rootId, groupName, groupIds)
end

function getSpecificGroupIdRecursive(id, groupName, groupIds)
    local numChildren = getNumOfChildren(id)
    for i=0, numChildren - 1 do
        local childId = getChildAt(id, i)
        local childName = getName(childId)

        if (string.match(childName, groupName)) then
            print(childId)
            groupIds[groupName] = childId
            return true
        end

        if (getSpecificGroupIdRecursive(childId, groupName, groupIds)) then
            return true
        end
    end
    return false
end

function deleteLOD0Trees()
    deleteLOD0TreesRecursive(rootId)
end

function deleteLOD0TreesRecursive(id)
    if getNumOfChildren(id) then
        local numChildren = getNumOfChildren(id)
        for i=0, numChildren - 1 do
            if getChildAt(id, i) then
                local childId = getChildAt(id, i)
                local childName = getName(childId)
                for j=1, #treeNames do
                    if string.match(getName(id), treeNames[j]) and childName == "LOD0" then
                        print("Deleting LOD0 for: " .. getName(id))
                        visitedIds[id] = true
                        removeFromPhysics(childId)
                        unlink(childId)
                    end
                end
                if visitedIds[id] == nil then
                    deleteLOD0TreesRecursive(childId)
                else
                    return
                end
            else
                return
            end
        end
    else
        return
    end
end

function distinguishObjectsFromName(name)
    distinguishObjectsFromNameRecursive(rootId, name)
end

function distinguishObjectsFromNameRecursive(id, name)
    local numChildren = getNumOfChildren(id)
    local newName = ""
    for i=0, numChildren - 1 do
        local childId = getChildAt(id, i)
        local childName = getName(childId)
        if name == childName then
            newName = getName(id) .. "_" .. name
            setName(childId, newName);
        end
        for j=1, #treeNames do
            if string.match(newName, treeNames[j]) then
                treeCountTable[treeNames[j]] = treeCountTable[treeNames[j]] + 1
                newName = newName .. "_" .. treeCountTable[treeNames[j]]
                setName(childId, newName);
            end
        end
        distinguishObjectsFromNameRecursive(childId, name)
    end
end

function setupParentTreeStructure(groupName)
    unlink(mapIds[groupName])
    link(rootId, mapIds[groupName])
end

function setupParentStructure(groupName)
    local numChildren = getNumOfChildren(mapIds[groupName])
    for i=0, numChildren - 1 do
        local childId = getChildAt(mapIds[groupName], 0)
        
        unlink(childId)
        link(rootId, childId) 
    end

    print(getName(getParent(mapIds[groupName])))
    mapRootId = getParent(mapIds[groupName])
    numChildren = getNumOfChildren(mapRootId)
    print(numChildren)
    for i=0, numChildren - 1 do
        local childId = getChildAt(mapRootId, 0)
        print(getName(childId))
        unlink(childId)
        link(rootId, childId) 
    end
    unlink(mapIds[groupName])
end



--================================== MAIN ====================================--

local transformGroup = "commercial"
local treesTransformGroup = "trees"

if getSpecificGroupId(transformGroup, mapIds) then
    setupParentStructure(transformGroup)
end

if getSpecificGroupId(treesTransformGroup, mapIds) then
    setupParentTreeStructure(treesTransformGroup)
end

deleteLOD0Trees()

for i=0, #namesToRename do
   distinguishObjectsFromName(namesToRename[i])
end

unlink(getChild(rootId, "gameplay"))
unlink(getChild(rootId, "careerStartPoint"))
unlink(getChild(rootId, "sun"))
unlink(mapRootId)
unlink(getChild(rootId, "structures"))

