using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NaughtyAttributes;
using OBLib.QuadTree;
using UnityEngine;


public class QuadTreeVisualizer : MonoBehaviour
{
    public QuadTree<GameObject> quadTree;
    [Auto]
    private BoxCollider2D spawnArea;
    [Auto]
    private ObjectsSpawner spawner;

    public bool activate_search;
    public Collider2D search_collider;

    [ShowNativeProperty]
    public int search_iterations => Stats.search_iterations;


    // Start is called before the first frame update
    void Awake()
    {
        Rectangle bounds = new Rectangle(spawnArea.bounds.min, spawnArea.bounds.max);
        quadTree = new QuadTree<GameObject>(bounds, 4);

        spawner.OnObjectSpawned += AddSpawnedObj_ToQuadTree;
    }

    void AddSpawnedObj_ToQuadTree(GameObject obj){
        UnityEngine.Debug.Log("Add Spawned Obj");
        quadTree.Add(obj, new Vector2(obj.transform.position.x, obj.transform.position.y), obj.transform.lossyScale.x/2);
    }

    private List<GameObject> prev_search_results = new List<GameObject>();

    public void Update(){
        if(activate_search)
        {
            ClearPrevSearchColors(); 
            QuadTreeSearch();
        }
    }
    
    List<long> search_time_measurements = new List<long>();

    [ShowNativeProperty]
    public double Search_Time_Average => search_time_measurements.IsNullOrEmpty() ? 0 : search_time_measurements.Average();
    
    private List<GameObject> QuadTreeSearch()
    {
        var sw = new Stopwatch();
        sw.Start();

        Rectangle search_area = new Rectangle(search_collider.bounds.min, search_collider.bounds.max);

        IEnumerable<GameObject> results = quadTree.Search(search_area);

        prev_search_results.Clear();
        if (results != null)
        {
            foreach (var c_result in results)
            {
                c_result.GetComponent<SpriteRenderer>().color = Color.red;
                prev_search_results.Add(c_result);
            }
        }

        sw.Stop();
        if(search_time_measurements.Count >= 60){
            search_time_measurements.RemoveAt(0);
        }
        search_time_measurements.Add(sw.ElapsedMilliseconds);
        return prev_search_results;
        // UnityEngine.Debug.Log("Linear Search: " + sw.Elapsed);
    }

    private void ClearPrevSearchColors(){
        if (prev_search_results != null)
        {
            foreach (var c_result in prev_search_results)
            {
                c_result.GetComponent<SpriteRenderer>().color = Color.cyan;
            }
        }

    }


    private Transform random_obj;
    [Button]
    public void Select(){
        if(random_obj != null){
            random_obj.GetComponent<SpriteRenderer>().color = Color.cyan;
        }
        random_obj = this.transform.GetChildren().GetRandomElement();
        random_obj.GetComponent<SpriteRenderer>().color = Color.yellow;
        UnityEngine.Debug.Log(Stats.search_iterations);
    }

    [Button]
    public void TestRemove(){
        quadTree.Remove(random_obj.gameObject, random_obj.position, random_obj.lossyScale.x/2f);
        random_obj.GetComponent<SpriteRenderer>().color = Color.gray;
    }
    
    [Button]
    public void TestRelocate(){
        quadTree.Relocate(random_obj.gameObject, random_obj.position, Vector2.up, random_obj.lossyScale.x/2f);
        var new_obj = Instantiate(random_obj, this.transform);
        random_obj.transform.Translate(Vector2.up);
        // random_obj.GetComponent<SpriteRenderer>().color = Color.green;
    }

    [Button]
    public void TestSearch(){
        var results = QuadTreeSearch();
    }
    
    [Button]
    public void TestRemoveArea(){
        var results = QuadTreeSearch();
        foreach(var c_result in results){
            quadTree.Remove(c_result, c_result.transform.position, c_result.transform.lossyScale.x / 2);

        }
    }

    void OnDrawGizmos(){
        if(Application.isPlaying == false){ return; }

        if(quadTree == null){ return; }

        QuadTreeNode<GameObject> curr_node = quadTree.root;

        Gizmos.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.4f);

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
