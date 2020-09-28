using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARRaycastManager))]
public class OrnamentCreator : MonoBehaviour
{
    [SerializeField] private GameObject _ornamentPrefab;
    [SerializeField] private Transform _anchorTrackingTransform;

    private GameObject _ornamentObj;
    private ARRaycastManager _arRaycastManager;
    private static List<ARRaycastHit> _raycastHits = new List<ARRaycastHit>();

    void Start()
    {
        _arRaycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        // 2本タップ以外はスルー
        if (Input.touchCount != 2)
        {
            return;
        }
        
        // uGUI操作時は無視
        if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) {
            return;
        }

        // タップ開始時以外はスルー
        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
        {
            return;
        }

        // タップ処理
        if (_arRaycastManager.Raycast(touch.position, _raycastHits, TrackableType.FeaturePoint))
        {
            Pose hitPose = _raycastHits[0].pose;
            UpdateOrnament(hitPose);
        }
    }

    /// <summary>
    /// 置物の作成・位置の更新
    /// </summary>
    /// <param name="pose">配置位置</param>
    private void UpdateOrnament(Pose pose, bool isLocalPose = false)
    {
        if (_ornamentObj == null)
        {
            _ornamentObj = Instantiate(_ornamentPrefab, _anchorTrackingTransform);
        }

        if (isLocalPose)
        {
            _ornamentObj.transform.localPosition = pose.position;
            _ornamentObj.transform.localRotation = pose.rotation;
        }
        else
        {
            _ornamentObj.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
    }
    
    /// <summary>
    /// アンカーに対する置物の位置を保存
    /// </summary>
    public void SaveOrnamentPoseFromAnchor()
    {
        // 置物がないときはスルー
        if (_ornamentObj == null)
        {
            Debug.LogError("ARAnchor or ornament is not found;");
            return;
        }

        // Anchorとの位置関係を保存
        Vector3 diffPos = _ornamentObj.transform.localPosition;
        Quaternion diffRot =  _ornamentObj.transform.localRotation;
        PlayerPrefs.SetFloat("OrnamentPosX", diffPos.x);
        PlayerPrefs.SetFloat("OrnamentPosY", diffPos.y);
        PlayerPrefs.SetFloat("OrnamentPosZ", diffPos.z);
        PlayerPrefs.SetFloat("OrnamentRotX", diffRot.x);
        PlayerPrefs.SetFloat("OrnamentRotY", diffRot.y);
        PlayerPrefs.SetFloat("OrnamentRotZ", diffRot.z);
        PlayerPrefs.SetFloat("OrnamentRotW", diffRot.w);
    }

    /// <summary>
    /// アンカーに対する置物の位置を読み込んで移動
    /// </summary>
    public void LoadOrnamentPoseFromAnchor()
    {
        // Anchorとの位置関係を読み込み
        float posX = PlayerPrefs.GetFloat("OrnamentPosX");
        float posY = PlayerPrefs.GetFloat("OrnamentPosY");
        float posZ = PlayerPrefs.GetFloat("OrnamentPosZ");
        float rotX = PlayerPrefs.GetFloat("OrnamentRotX");
        float rotY = PlayerPrefs.GetFloat("OrnamentRotY");
        float rotZ = PlayerPrefs.GetFloat("OrnamentRotZ");
        float rotW = PlayerPrefs.GetFloat("OrnamentRotW");
        Vector3 diffPos = new Vector3(posX, posY, posZ);
        Quaternion diffRot = new Quaternion(rotX, rotY, rotZ, rotW);
        Pose diffPose = new Pose(diffPos, diffRot);

        // 置物移動
        UpdateOrnament(diffPose, true);
    }
}