using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CinemachineCamera startCam;

    [Header("Settings")]
    private float flipYRotationTime = 0.5f;
    
    private PlayerData playerData;

    private bool isFacingRight;

    private CinemachineCamera currentCamera;
    
    public static CameraController Instance;

    private void Awake() {
	    playerData    = playerTransform.gameObject.GetComponent<PlayerData>();
	    isFacingRight = playerData.IsLookingRight;
	    if (Instance == null) {
		    Instance = this;
	    }
	    currentCamera = startCam;
    }

    private void Update() {
	    transform.position = playerTransform.position;
    }

    public void CallTurnCamera(bool isLookingRight) {
	    if (isLookingRight != isFacingRight) {
		    LeanTween.rotateY(gameObject, DetermineEndRotation(), flipYRotationTime).setEase(LeanTweenType.easeInOutSine);
	    }
    }

    private float DetermineEndRotation() {
	    isFacingRight = !isFacingRight;

	    if (isFacingRight) {
		    return 180f;
	    }
	    return 0f;
    }
    
    #region swapTargets

    public void SwapCamera(CinemachineCamera cameraFromLeft, CinemachineCamera cameraFromRight, Vector2 triggerExitDirection) {
	    print(currentCamera + " " + cameraFromLeft + " " + cameraFromRight + " "  + triggerExitDirection );
	    if (currentCamera == cameraFromLeft && triggerExitDirection.x > 0f) {
		    cameraFromRight.gameObject.SetActive(true);
		    cameraFromLeft.gameObject.SetActive(false);
		    currentCamera = cameraFromRight;
		    print("Switched Camera");
	    } 
	    if (currentCamera == cameraFromRight && triggerExitDirection.x < 0f) {
		    cameraFromLeft.gameObject.SetActive(true);
		    cameraFromRight.gameObject.SetActive(false);
		    currentCamera           = cameraFromLeft;
		    print("Switched camera");
	    }
    }
    
    #endregion
}
