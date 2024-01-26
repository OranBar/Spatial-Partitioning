using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeVisualizer : MonoBehaviour
{
    public QuadTree<GameObject> quadTree;

    [Auto]
    private BoxCollider2D spawnArea;
    [Auto]
    private ObjectsSpawner spawner;

    // Start is called before the first frame update
    void Awake()
    {
        Rect bounds = new Rect(new Vector2(spawnArea.bounds.center.x, spawnArea.bounds.center.y), spawnArea.bounds.extents.x );
        quadTree = new QuadTree<GameObject>(bounds, 4, 8);

        spawner.OnObjectSpawned += AddSpawnedObj_ToQuadTree;
    }

    void AddSpawnedObj_ToQuadTree(GameObject obj){
        Debug.Log("Add Spawned Obj");
        quadTree.Add(obj, new Vector2(obj.transform.position.x, obj.transform.position.y), obj.transform.lossyScale.x);
    }

    void OnDrawGizmos(){
        if(Application.isPlaying == false){ return; }

        if(quadTree == null){ return; }

        QuadTreeNode<GameObject> curr_node = quadTree.root;

        Gizmos.color = Color.yellow;

        DrawQuad(curr_node);
    }
    
    void DrawQuad(QuadTreeNode<GameObject> node){
        Vector2[] corners = node.area.GetCorners();
        
        Gizmos.DrawLine(
            new Vector3(corners[0].X, corners[0].Y, 0),
            new Vector3(corners[1].X, corners[1].Y, 0)
        );
        
        Gizmos.DrawLine(
            new Vector3(corners[1].X, corners[1].Y, 0),
            new Vector3(corners[2].X, corners[2].Y, 0)
        );

        Gizmos.DrawLine(
            new Vector3(corners[2].X, corners[2].Y, 0),
            new Vector3(corners[3].X, corners[3].Y, 0)
        );

        Gizmos.DrawLine(
            new Vector3(corners[3].X, corners[3].Y, 0),
            new Vector3(corners[0].X, corners[0].Y, 0)
        );

        if(node.IsSubdivided){

            DrawQuad(node.TopLeft);
            DrawQuad(node.BottomLeft);
            DrawQuad(node.TopRight);
            DrawQuad(node.BottomRight);
        } 
    }

}
