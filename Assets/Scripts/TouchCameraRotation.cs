using UnityEngine;
using System.Collections;
using IE.RSB;
using DG.Tweening;
using UnityEngine.EventSystems;

public class TouchCameraRotation : MonoBehaviour
{
    public float rotateSpeed = 0.07f; // 回転速度
    public float minYAngle = -80f; // X軸回転の最小角度
    public float maxYAngle = 80f; // X軸回転の最大角度

    private float rotationX = 0f;
    private float rotationY = 0f;

    [SerializeField] private PlayerWeaponController m_weaponController = null;
    private bool m_isControlsInAimEnabled = false;

    [SerializeField] private GameObject UI_Scope = null;
    [SerializeField] private GameObject UICrossScope = null;
    public bool touchFlg = false;

    private const float clickTimeSpanThreshould = 0.5f;

    // ボタンのクリックを受け付けるかどうか
    private bool _clickable = true;

    // カウンタ
    private float _timer = 0f;
    private float _bulletTimer = 0f;


    //
    [Header("Zoom Settings")] [SerializeField]
    private Camera zoomCamera;

    [SerializeField] private float defaultFOV = 60f;
    [SerializeField] private float zoomFOV = 15f;
    [SerializeField] private float deepZoomFOV = 5f;
    private float zoomHoldTime = 0f;
    private Tween zoomTween;
    private bool isDeepZooming = false;

    [Header("Y-Axis Rotation Limit")] [SerializeField]
    private float minYRotation = -50f;

    [SerializeField] private float maxYRotation = 50f;

    [SerializeField] private RectTransform buttonCloseScopeRectTransform;

    [SerializeField] private float scopeCancelRadius = 100f;
    [SerializeField] private CanvasGroup buttonCloseHighlight;

    [SerializeField] private EnemyIndicator _enemyIndicator;

    [SerializeField] private ButtonHuntingDog btnHuntingDog;


    [Header("Edge Auto-Rotate Settings")]
    [SerializeField, Range(0f, 0.5f)] private float edgePercent = 0.1f; // 10% ngoài cùng
    [SerializeField, Range(0f, 0.5f)] private float innerEdgePercent = 0.9f; 
    [SerializeField] private float innerEdgeSpeedMultiplier = 2f;

    void Start()
    {
        touchFlg = true;
        ResetRotation(false);
        _timer = 1.0f;

        if (zoomCamera == null) zoomCamera = Camera.main;
        if (zoomCamera != null)
        {
            zoomCamera.fieldOfView = defaultFOV;
        }
    }

    public void ResetRotation(bool isAnimation)
    {
        rotationX = 0f;
        rotationY = 0f;
        if (isAnimation)
        {
            transform.DOLocalRotate(Vector3.zero, 0.5f);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }

    void Update()
    {
        if (touchFlg)
        {
#if !(!UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS))
            HandleMouseInput();
#else
                HandleTouchInput(); // モバイル用の操作
#endif
        }

        if (!m_weaponController.isAiming)
        {
            _timer += Time.deltaTime;
        }
        else
        {
            _bulletTimer += Time.deltaTime;
        }
    }


    public void UpdateZoom(bool isHitEnemy)
    {
        //temp disable
        //    if (zoomCamera == null) return;

        //    zoomTween?.Kill();
        //    Debug.Log("Switch zoom: " + isHitEnemy);
        //    if (isHitEnemy)
        //    {
        //        zoomTween = zoomCamera.DOFieldOfView(deepZoomFOV, 0.2f).SetEase(Ease.OutSine);
        //    }
        //    else
        //    {
        //        zoomTween = zoomCamera.DOFieldOfView(zoomFOV, 0.2f).SetEase(Ease.OutSine);
        //    }
    }

    private float GetEdgeSpeed(float posX, float screenWidth)
    {
        float outerEdge = screenWidth * edgePercent;

       
        float innerEdge = screenWidth * (edgePercent * 2f); 

        float baseSpeed = rotateSpeed * 200f;
        float slowSpeed = baseSpeed * 0.3f; 
        float fastSpeed = baseSpeed;        

       
        if (posX < innerEdge)
        {
           
            if (posX < outerEdge)
            {
                return -fastSpeed;
            }
            else
            {
                
                float t = Mathf.InverseLerp(innerEdge, outerEdge, posX);
                return -Mathf.Lerp(slowSpeed, fastSpeed, 1f - t);
            }
        }

      
        float rightOuterEdge = screenWidth * (1f - edgePercent);
        float rightInnerEdge = screenWidth * (1f - edgePercent * 2f);

        if (posX > rightInnerEdge)
        {
           
            if (posX > rightOuterEdge)
            {
                return fastSpeed;
            }
            else
            {
               
                float t = Mathf.InverseLerp(rightInnerEdge, rightOuterEdge, posX);
                return Mathf.Lerp(slowSpeed, fastSpeed, t);
            }
        }

        return 0f; 
    }


  
void HandleTouchInput()
    {
        if (CheckClickUI())
        {
            return;
        }

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            float screenWidth = Screen.width;

            // Calculate left/right edge area (10%)
            float edgePercent = 0.1f;
            float leftEdge = screenWidth * edgePercent;
            float rightEdge = screenWidth * (1f - edgePercent);

            Vector2 pos = touch.position;

            if (touch.phase == TouchPhase.Moved)
            {
                if (Mathf.Abs(touch.deltaPosition.x) > 1f || Mathf.Abs(touch.deltaPosition.y) > 1f)
                {
                    float deltaX = touch.deltaPosition.x * rotateSpeed;
                    float deltaY = touch.deltaPosition.y * rotateSpeed;

                    rotationY += deltaX;
                    rotationX -= deltaY;
                    rotationX = Mathf.Clamp(rotationX, minYAngle, maxYAngle);

                    if (rotationY > 180f) rotationY -= 360f;
                    rotationY = Mathf.Clamp(rotationY, minYRotation, maxYRotation);
                    transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
                }


                if (buttonCloseHighlight != null)
                {
                    if (IsInCancelArea(touch.position))
                    {
                        buttonCloseHighlight.alpha = 1f; // Sáng lên khi gần cancel
                    }
                    else
                    {
                        buttonCloseHighlight.alpha = 0.3f; // Làm mờ
                    }
                }
            }
            else if (touch.phase == TouchPhase.Stationary)
            {
                //// If you hold your hand on the left/right edge -> auto rotate
                //if (pos.x < leftEdge)
                //{
                //    rotationY -= rotateSpeed * 200f * Time.deltaTime;
                //}
                //else if (pos.x > rightEdge)
                //{
                //    rotationY += rotateSpeed * 200f * Time.deltaTime;
                //}

                //if (rotationY > 180f) rotationY -= 360f;
                //rotationY = Mathf.Clamp(rotationY, minYRotation, maxYRotation);
                //transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);


                float autoSpeed = GetEdgeSpeed(pos.x, screenWidth);
                if (Mathf.Abs(autoSpeed) > 0.01f)
                {
                    rotationY += autoSpeed * Time.deltaTime;
                    if (rotationY > 180f) rotationY -= 360f;
                    rotationY = Mathf.Clamp(rotationY, minYRotation, maxYRotation);
                    transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
                }
            }

            if (touch.phase == TouchPhase.Began)
            {
                if (!m_weaponController.isAiming && _timer >= 0.2f)
                {
                    _timer = 0f;
                    ScopeChange();

                    m_weaponController.scopeMode = true;
                    ScopeUIChange();
                }
            }

            if (touch.phase == TouchPhase.Ended)
            {
                if (m_weaponController.isAiming && m_weaponController.scopeMode)
                {
                    if (IsInCancelArea(touch.position))
                    {
                        //do nothing
                        // cancel scope
                        m_weaponController.scopeMode = false;
                        StartCoroutine(WaiteScopeChange(0));
                    }
                    else
                    {
                        // Fire
                        m_weaponController.FireInput();
                        // cancel scope
                        m_weaponController.scopeMode = false;
                        StartCoroutine(WaiteScopeChange(1));
                    }
                }

                // Hide highlight cancel
                if (buttonCloseHighlight != null) buttonCloseHighlight.alpha = 0f;
            }
        }
    }

    void HandleMouseInput()
    {
        if (CheckClickUI())
        {
            return;
        }

        float screenWidth = Screen.width;

        // Calculate left/right edge area (10%)
        float edgePercent = 0.1f;
        float leftEdge = screenWidth * edgePercent;
        float rightEdge = screenWidth * (1f - edgePercent);

        Vector2 pos = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            {
                float deltaX = Input.GetAxis("Mouse X") * rotateSpeed * 10f;
                float deltaY = Input.GetAxis("Mouse Y") * rotateSpeed * 10f;

                rotationY += deltaX;
                rotationX -= deltaY;
                rotationX = Mathf.Clamp(rotationX, minYAngle, maxYAngle);

                if (rotationY > 180f) rotationY -= 360f;
                rotationY = Mathf.Clamp(rotationY, minYRotation, maxYRotation);
                transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
            }

            if (buttonCloseHighlight != null)
            {
                if (IsInCancelArea(pos))
                    buttonCloseHighlight.alpha = 1f;
                else
                    buttonCloseHighlight.alpha = 0.3f;
            }

            //if (pos.x < leftEdge)
            //{
            //    rotationY -= rotateSpeed * 200f * Time.deltaTime;
            //}
            //else if (pos.x > rightEdge)
            //{
            //    rotationY += rotateSpeed * 200f * Time.deltaTime;
            //}

            //if (rotationY > 180f) rotationY -= 360f;
            //rotationY = Mathf.Clamp(rotationY, minYRotation, maxYRotation);
            //transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);


            // 🔥 Sửa: dùng GetEdgeSpeed()
            float autoSpeed = GetEdgeSpeed(pos.x, screenWidth);
            if (Mathf.Abs(autoSpeed) > 0.01f)
            {
                rotationY += autoSpeed * Time.deltaTime;
                if (rotationY > 180f) rotationY -= 360f;
                rotationY = Mathf.Clamp(rotationY, minYRotation, maxYRotation);
                transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!m_weaponController.isAiming && _timer >= 0.2f)
            {
                _timer = 0f;
                ScopeChange();

                m_weaponController.scopeMode = true;
                ScopeUIChange();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (m_weaponController.isAiming && m_weaponController.scopeMode)
            {
                if (!IsInCancelArea(pos))
                {
                    m_weaponController.FireInput();
                    m_weaponController.scopeMode = false;
                    StartCoroutine(WaiteScopeChange(1));
                }
                else
                {
                    m_weaponController.scopeMode = false;
                    StartCoroutine(WaiteScopeChange(0));
                }
            }

            if (buttonCloseHighlight != null) buttonCloseHighlight.alpha = 0f;
        }
    }

    void ScopeChange(bool forceOff = false)
    {
        if (forceOff)
        {
            m_weaponController.AimInput(false);
            m_isControlsInAimEnabled = false;
        }
        else
        {
            m_weaponController.AimInput(!m_isControlsInAimEnabled);
            m_isControlsInAimEnabled = !m_isControlsInAimEnabled;
        }
    }

    //public void ScopeUIChange()
    //{
    //    if (m_weaponController.scopeMode)
    //    {
    //        UI_Scope.SetActive(true);
    //        _enemyIndicator.SetSize(1.5f, true);
    //        DOVirtual.DelayedCall(0.01f, () => btnHuntingDog.Show(false));
    //    }
    //    else
    //    {
    //        UI_Scope.SetActive(false);
    //        rotationX = 0;
    //        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
    //        //rotationX = 0;
    //        //transform.DOLocalRotate(new Vector3(rotationX, rotationY, 0), 0.5f)
    //        //         .SetEase(Ease.OutSine);

    //        _enemyIndicator.SetSize(0.5f, false);
    //        btnHuntingDog.Show(true);
    //    }
    //}



    public void ScopeUIChange()
    {
        if (m_weaponController.scopeMode)
        {
           
            zoomTween?.Kill();
            zoomTween = zoomCamera.DOFieldOfView(zoomFOV, 0.25f).SetEase(Ease.OutSine);

           
            UI_Scope.SetActive(true);
            CanvasGroup scopeCanvas = UI_Scope.GetComponent<CanvasGroup>();
            if (scopeCanvas != null)
            {
                scopeCanvas.alpha = 0f;
                scopeCanvas.DOFade(1f, 0.2f);
            }

            _enemyIndicator.SetSize(1.5f, true);
            DOVirtual.DelayedCall(0.01f, () => btnHuntingDog.Show(false));
        }
        else 
        {
           
            zoomTween?.Kill();
            zoomTween = zoomCamera.DOFieldOfView(defaultFOV, 0.15f).SetEase(Ease.OutSine);

           
            CanvasGroup scopeCanvas = UI_Scope.GetComponent<CanvasGroup>();
            if (scopeCanvas != null)
            {
                scopeCanvas.DOFade(0f, 0.2f).OnComplete(() =>
                {
                    UI_Scope.SetActive(false);
                });
            }
            else
            {
                UI_Scope.SetActive(false);
            }

            rotationX = 0f;
            transform.DOLocalRotate(new Vector3(0, rotationY, 0), 0.25f).SetEase(Ease.OutSine);

            _enemyIndicator.SetSize(0.5f, false);
            btnHuntingDog.Show(true);
        }
    }

    public IEnumerator WaiteScopeChange(float delayTime, bool forceOff = false)
    {
        if (!m_weaponController.scopeMode)
        {
            yield return new WaitForSecondsRealtime(delayTime);
            if (!SniperAndBallisticsSystem.instance.BulletTimeRunning) ScopeUIChange();
            yield return new WaitUntil(() => !SniperAndBallisticsSystem.instance.BulletTimeRunning);
            ScopeChange(forceOff);
        }
    }


    bool IsInCancelArea(Vector2 touchPosition)
    {
        if (buttonCloseScopeRectTransform == null) return false;

        Vector2 cancelScreenPos = RectTransformUtility.WorldToScreenPoint(null, buttonCloseScopeRectTransform.position);
        float distance = Vector2.Distance(touchPosition, cancelScreenPos);
        return distance <= scopeCancelRadius;
    }

    private bool CheckClickUI()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) && btnHuntingDog.IsShowing())
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
#else
        if (Input.touchCount > 0 && btnHuntingDog.IsShowing())
        {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
                }
        }

#endif
        return false;
    }
}