using System.Collections;
using UnityEngine;

// góc nhìn của nhân vật
public class FieldOfView : MonoBehaviour
{
    [Range(0, 360)]
    public float viewAngle = 90f; // Góc nhìn của nhân vật
    public float viewRadius = 20f;
    
    public LayerMask obstacleLayerMask;
    public LayerMask targetLayerMask;
    public bool canSeePlayer = false;
    public Vector3 lastKnownPlayerPosition;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(FindTargetWithDelay(0.1f));
    }

    private Collider[] searchResults = new Collider[10];

    // Update is called once per frame
    IEnumerator FindTargetWithDelay(float delay)
    {
        while (true)
        {
            FindVisibleTargets();
            yield return new WaitForSeconds(delay);
        }
    }

    private void FindVisibleTargets()
    {
        canSeePlayer = false;
        // 1. tim tat ca muc tieu trong pham vi ban kinh
        int count = Physics.OverlapSphereNonAlloc(transform.position, 
            viewRadius, searchResults, targetLayerMask);

        for (int i = 0; i < count; i++)
        {
            var target = searchResults[i].transform;
var direction = (target.position - transform.position).normalized;
            
            // 2. tinh goc
            var angle = Vector3.Angle(transform.forward, direction);
            if (angle < viewAngle / 2)
            {
                var distanceToTarget = Vector3.Distance(transform.position, target.position);
                // 3. kiem tra vat can
                if (!Physics.Raycast(transform.position,
                        direction, distanceToTarget, obstacleLayerMask))
                {
                    canSeePlayer = true;
                    lastKnownPlayerPosition = target.position;
                    Debug.DrawLine(transform.position, 
                                    target.position, Color.blue, 1f);
                }
            }
        }
        
    }
}