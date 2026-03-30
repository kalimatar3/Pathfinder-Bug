using System.Collections; 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bug : MonoBehaviour
{
    public float moveSpeed = 2f; 
    private List<Cell> currentPath;
    private int pathIndex; 
    protected Coroutine FollowPathCr;
    IGamePlayController gamePlayController;
    protected void Start()
    {
        gamePlayController = GamePlayManager.Instance;
    }
    public void MovetoPath(List<Cell> path)
    {
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("Path is empty or null for Bug. Cannot move.");
            return;
        }
        currentPath = path; 
        pathIndex = 0; 
        if(FollowPathCr !=null) StopCoroutine(FollowPathCr);
        FollowPathCr = StartCoroutine(FollowPath());
    }
    private IEnumerator FollowPath()
    {
        while (pathIndex < currentPath.Count)
        {
            Vector3 targetPosition = new Vector3(currentPath[pathIndex].X,currentPath[pathIndex].Y) + new Vector3(.5f,.5f);
            transform.localRotation = Quaternion.LookRotation(transform.forward,(targetPosition - transform.localPosition).normalized);
            while (Vector3.Distance(transform.localPosition, targetPosition) > 0.01f) 
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.localPosition = targetPosition; 
            pathIndex++; 
        }
        gamePlayController.EndLevel();
    }
}