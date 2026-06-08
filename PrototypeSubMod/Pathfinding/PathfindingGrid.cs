using PrototypeSubMod.Pathfinding.SaveSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace PrototypeSubMod.Pathfinding;

public class PathfindingGrid : MonoBehaviour
{
    [HideInInspector] public bool initialized;

    [Header("Gizmos")]
    public bool displayGridGizmos;
    public bool displaySurfaceAngleGizmos;
    public Transform focusObject;

    [Header("Grid Generation")]
    public Transform root;
    public Vector3 gridWorldSize;
    public float nodeRadius;
    public int rayCount;
    public int obstacleProximityPenalty = 10;
    public Transform centerOfObjects;
    public float intersectionCheckDistance;
    [Range(-90, 90)] public float minSurfaceAngle = 0f;
    public TerrainType[] walkableRegions;

    [Header("Saves")]
    public TextAsset gridSaveFile;
    public bool useSaveFileAsGrid;

    private LayerMask walkableMask;
    private GridNode[,,] grid;
    private Vector3[] pointsOnSphere;
    private Vector3 posAtGridGen;
    private Quaternion rotationAtGridGen;

    private float nodeDiameter;
    private int gridSizeX, gridSizeY, gridSizeZ;
    private int penaltyMin = int.MaxValue;
    private int penaltyMax = int.MinValue;

    private void Awake()
    {
        Initialize();
        
        if (useSaveFileAsGrid)
        {
            /*
            if (Plugin.PathfindingGridSaveData != null)
            {
                OnGridSaveDataLoaded(Plugin.PathfindingGridSaveData);
            }
            else
            {
                Plugin.onLoadGridSaveData += OnGridSaveDataLoaded;
            }
            */
        }
        else
        {
            CreateGrid();
        }
    }
    
    private void OnGridSaveDataLoaded(GridSaveData data)
    {
        grid = data.grid;
        posAtGridGen = data.positionAtGridGen.Vector;
        rotationAtGridGen = data.rotationAtGridGen.Quaternion;
        initialized = true;
        Plugin.onLoadGridSaveData -= OnGridSaveDataLoaded;
    }

    private void OnValidate()
    {
        if (gridSaveFile != null && !gridSaveFile.name.Contains(".grid"))
        {
            Debug.LogError($"\"{gridSaveFile.name}\" has an invalid file type. Only .grid is accepted");
            gridSaveFile = null;
        }
    }

    private void Initialize()
    {
        posAtGridGen = transform.position;
        rotationAtGridGen = transform.rotation;

        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        gridSizeZ = Mathf.RoundToInt(gridWorldSize.z / nodeDiameter);

        pointsOnSphere = PointsOnSphere(rayCount);

        if (walkableRegions == null) return;

        foreach (var region in walkableRegions)
        {
            walkableMask.value |= region.terrainMask.value;
            if (region.terrainPenalty < penaltyMin)
            {
                penaltyMin = region.terrainPenalty;
            }

            if (region.terrainPenalty > penaltyMax)
            {
                penaltyMax = region.terrainPenalty;
            }
        }
    }

    private void CreateGrid()
    {
        grid = new GridNode[gridSizeX, gridSizeY, gridSizeZ];
        Vector3 worldMin = transform.position - (Vector3.right * (gridWorldSize.x / 2)) - (Vector3.up * (gridWorldSize.y / 2)) - (Vector3.forward * (gridWorldSize.z / 2));

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3 xOffset = Vector3.right * ((x * nodeDiameter) + nodeRadius);
                    Vector3 yOffset = Vector3.up * ((y * nodeDiameter) + nodeRadius);
                    Vector3 zOffset = Vector3.forward * ((z * nodeDiameter) + nodeRadius);

                    Vector3 worldPoint = worldMin + xOffset + yOffset + zOffset;
                    Vector3 pointOnBounds = Vector3.zero;
                    Vector3 surfaceNormal = Vector3.zero;
                    Collider hitCollider = null;
                    bool walkable = false;

                    int arrayIndex = 0;
                    int movementPenalty = 0;

                    foreach (var point in pointsOnSphere)
                    {
                        Vector3 offsetPoint = (point * nodeRadius) + worldPoint;
                        Vector3 dir = worldPoint - offsetPoint;

                        if (Physics.Raycast(offsetPoint, dir, out RaycastHit hitInfo, nodeDiameter + (nodeRadius / 4f), walkableMask))
                        {
                            if (Vector3.Dot(hitInfo.normal, Vector3.up) < minSurfaceAngle / 90f) continue;

                            var halfBounds = new Bounds(hitInfo.collider.bounds.center, hitInfo.collider.bounds.size / 2.5f);
                            if (halfBounds.Contains(hitInfo.point)) continue;
                            
                            int layerValue = (int)Mathf.Pow(2, hitInfo.collider.gameObject.layer);
                            TerrainType type = walkableRegions.FirstOrDefault(i => (i.terrainMask & layerValue) != 0);

                            walkable = true;
                            movementPenalty = type.terrainPenalty;
                            pointOnBounds = hitInfo.point;
                            surfaceNormal = hitInfo.normal;
                            hitCollider = hitInfo.collider;
                            arrayIndex++;
                            break;
                        }
                    }

                    if (walkable)
                    {
                        if (Physics.Raycast(pointOnBounds, surfaceNormal, intersectionCheckDistance))
                        {
                            walkable = false;
                        }

                        Vector3 dirToCenter = (centerOfObjects.position - pointOnBounds).normalized;
                        float dot = Vector3.Dot(dirToCenter, surfaceNormal);

                        if (dot >= -(minSurfaceAngle / 90f))
                        {
                            walkable = false;
                        }
                    }

                    /*
                    bool invalidPos = colliders.Any(i =>
                        {
                            return i != hitCollider && i.bounds.Contains(pointOnBounds);
                        });

                        if (invalidPos) walkable = false; 
                    */

                    if (!walkable)
                    {
                        movementPenalty += obstacleProximityPenalty;
                    }

                    var wrappedWorldPoint = new SerializableV3Wrapper(worldPoint);
                    var wrappedPointOnBounds = new SerializableV3Wrapper(pointOnBounds);
                    var wrappedNormal = new SerializableV3Wrapper(surfaceNormal);

                    grid[x, y, z] = new GridNode(walkable, wrappedWorldPoint, wrappedPointOnBounds, wrappedNormal, x, y, z, movementPenalty);
                }
            }
        }

        initialized = true;
    }

    public List<GridNode> GetAdjacentNodes(GridNode node)
    {
        List<GridNode> adjacentNodes = new List<GridNode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;

                    int checkX = node.gridPosX + x;
                    int checkY = node.gridPosY + y;
                    int checkZ = node.gridPosZ + z;

                    // Make sure the node is in the grid
                    bool xInBounds = checkX >= 0 && checkX < gridSizeX;
                    bool yInBounds = checkY >= 0 && checkY < gridSizeY;
                    bool zInBounds = checkZ >= 0 && checkZ < gridSizeZ;

                    if (xInBounds && yInBounds && zInBounds)
                    {
                        adjacentNodes.Add(grid[checkX, checkY, checkZ]);
                    }
                }
            }
        }

        return adjacentNodes;
    }

    public GridNode GetNodeAtWorldPosition(Vector3 worldPosition)
    {
        Vector3 offsetPosition = root.InverseTransformPoint(worldPosition) - posAtGridGen;
        
        float normalizedX = (offsetPosition.x + (gridWorldSize.x / 2)) / gridWorldSize.x;
        float normalizedY = (offsetPosition.y + (gridWorldSize.y / 2)) / gridWorldSize.y;
        float normalizedZ = (offsetPosition.z + (gridWorldSize.z / 2)) / gridWorldSize.z;
        
        normalizedX = Mathf.Clamp01(normalizedX);
        normalizedY = Mathf.Clamp01(normalizedY);
        normalizedZ = Mathf.Clamp01(normalizedZ);

        int x = Mathf.RoundToInt((gridSizeX - 1) * normalizedX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * normalizedY);
        int z = Mathf.RoundToInt((gridSizeZ - 1) * normalizedZ);
        
        return grid[x, y, z];
    }

    public GridNode[,,] GetGrid() => grid;

    public int GetMaxSize()
    {
        return gridSizeX * gridSizeY * gridSizeZ;
    }

    private Vector3[] PointsOnSphere(int num)
    {
        List<Vector3> points = new List<Vector3>();
        float increment = Mathf.PI * (3 - Mathf.Sqrt(5));
        float offset = 2f / num;

        for (int i = 0; i < num; i++)
        {
            float y = (i * offset) - 1 + (offset / 2);
            float r = Mathf.Sqrt(1 - (y * y));
            float phi = i * increment;
            float x = Mathf.Cos(phi) * r;
            float z = Mathf.Sin(phi) * r;

            points.Add(new Vector3(x, y, z));
        }

        return points.ToArray();
    }

    public Vector3 GetPositionAtGridGen()
    {
        return posAtGridGen;
    }

    public Quaternion GetRotationAtGridGen()
    {
        return rotationAtGridGen;
    }

    private void OnDrawGizmos()
    {
        var size = new Vector3(gridWorldSize.x * transform.localScale.x, gridWorldSize.y * transform.localScale.y, gridWorldSize.z * transform.localScale.z);

        Gizmos.DrawWireCube(transform.position, size);

        if (focusObject)
        {
            HandleFocusGizmos();
        }
        else
        {
            HandleNodeGizmos();
        }
        HandleSurfanceAngleGizmos();
    }

    private void HandleFocusGizmos()
    {
        if (grid == null || !displayGridGizmos) return;

        var focusNode = GetNodeAtWorldPosition(focusObject.transform.position);
        var nodeLocalPos = root.TransformPoint(focusNode.worldPosition.Vector);
        var col = focusNode.walkable ? Color.green : Color.red;
        col.a = 0.5f;
        Gizmos.color = col;
        Gizmos.DrawCube(nodeLocalPos, Vector3.one * nodeDiameter);
        
        foreach (var node in GetAdjacentNodes(focusNode))
        {
            if (node == focusNode) continue;
            
            var localPos = root.TransformPoint(node.worldPosition.Vector);
            var color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, node.movementPenalty));
            color = node.walkable ? color : Color.red;
            color.a = 0.5f;
            Gizmos.color = color;
            Gizmos.DrawWireCube(localPos, Vector3.one * nodeDiameter);
        }
    }
    
    private void HandleNodeGizmos()
    {
        if (grid == null || !displayGridGizmos) return;

        foreach (GridNode node in grid)
        {
            if (node.pointOnBounds.Vector == Vector3.one * Mathf.NegativeInfinity) continue;

            if (!node.walkable) continue;

            Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, node.movementPenalty));

            Color col = node.walkable ? Gizmos.color : Color.red;
            col.a = 0.5f;
            Gizmos.color = col;
            
            var localPos = root.TransformPoint(node.worldPosition.Vector);
            Gizmos.DrawWireCube(localPos, Vector3.one * nodeDiameter);
            Gizmos.color = Color.yellow;
        }
    }

    private void HandleSurfanceAngleGizmos()
    {
        if (!displaySurfaceAngleGizmos) return;

        Vector3[] points = PointsOnSphere(500);
        foreach (var point in points)
        {
            Vector3 pointPosition = point + transform.position;
            Vector3 dir = pointPosition - transform.position;
            float dot = Vector3.Dot(dir, Vector3.up);
            Gizmos.color = dot >= minSurfaceAngle / 90f ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, pointPosition);
        }
    }

    private Vector3 Average(Vector3[] array)
    {
        Vector3 sum = Vector3.zero;
        foreach (var item in array)
        {
            sum += item;
        }

        sum /= array.Length;
        return sum;
    }

    private int Average(int[] array)
    {
        float sum = 0;
        foreach (var item in array)
        {
            sum += item;
        }

        sum /= array.Length;
        return Mathf.RoundToInt(sum);
    }
}

[Serializable]
public class TerrainType
{
    public LayerMask terrainMask;
    public int terrainPenalty;
}