using System.Collections;
using System.Collections.Generic;
using OBLib.QuadTree;
using UnityEngine;

using Square = OBLib.QuadTree.Square;

public class QuadTreeVisualizer : MonoBehaviour
{
    public QuadTree<GameObject> quadTree;
    [Auto]
    private BoxCollider2D spawnArea;
    [Auto]
    private ObjectsSpawner spawner;

    public bool activate_search;
    public Collider2D search_collider;

    // Start is called before the first frame update
    void Awake()
    {
        Square bounds = new Square(new Vector2(spawnArea.bounds.center.x, spawnArea.bounds.center.y), spawnArea.bounds.extents.x + 1);
        quadTree = new QuadTree<GameObject>(bounds, 4, 8);

        spawner.OnObjectSpawned += AddSpawnedObj_ToQuadTree;
    }

    void AddSpawnedObj_ToQuadTree(GameObject obj){
        Debug.Log("Add Spawned Obj");
        quadTree.Add(obj, new Vector2(obj.transform.position.x, obj.transform.position.y), obj.transform.lossyScale.x/2);
    }

    private List<GameObject> prev_search_results = new List<GameObject>();

    public void Update(){
        if(activate_search)
        {
            QuadTreeSearch();
        }
    }
    
    private void QuadTreeSearch()
    {
        Vector2 center = new Vector2(search_collider.bounds.center.x, search_collider.bounds.center.y);
        Square search_area = new Square(center, search_collider.bounds.extents.x);

        List<GameObject> results = quadTree.Search(search_area);

        if (prev_search_results != null)
        {
            foreach (var c_result in prev_search_results)
            {
                c_result.GetComponent<SpriteRenderer>().color = Color.cyan;
            }
        }

        if (results != null)
        {
            foreach (var c_result in results)
            {
                c_result.GetComponent<SpriteRenderer>().color = Color.red;
            }
        }

        prev_search_results = results;
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
            new Vector3(corners[0].x, corners[0].y, 0),
            new Vector3(corners[1].x, corners[1].y, 0)
        );
        
        Gizmos.DrawLine(
            new Vector3(corners[1].x, corners[1].y, 0),
            new Vector3(corners[2].x, corners[2].y, 0)
        );

        Gizmos.DrawLine(
            new Vector3(corners[2].x, corners[2].y, 0),
            new Vector3(corners[3].x, corners[3].y, 0)
        );

        Gizmos.DrawLine(
            new Vector3(corners[3].x, corners[3].y, 0),
            new Vector3(corners[0].x, corners[0].y, 0)
        );

        if(node.IsSubdivided){

            DrawQuad(node.TopLeft);
            DrawQuad(node.BottomLeft);
            DrawQuad(node.TopRight);
            DrawQuad(node.BottomRight);
        } 
    }

}
