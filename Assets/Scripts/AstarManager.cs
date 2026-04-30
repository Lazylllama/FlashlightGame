using UnityEngine;

public class AstarManager : MonoBehaviour
{
	public static AstarManager Instance;
	private AstarPath    astarPath;
	
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
	    astarPath = GetComponent<AstarPath>();
	    if (Instance == null) {
		    Instance = this;
	    }
    }

    public void UpdateBounds(Collider2D updateCollider) {
	    var bounds = updateCollider.bounds;
	    astarPath.UpdateGraphs(bounds);
    }
    
    public void UpdateAndDisableBounds(Collider2D updateCollider) {
	    var bounds = updateCollider.bounds;
	    updateCollider.enabled = false;
	    astarPath.UpdateGraphs(bounds);
    }
}
