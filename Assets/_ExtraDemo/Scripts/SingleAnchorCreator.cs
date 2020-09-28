using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARRaycastManager))]
public class SingleAnchorCreator : MonoBehaviour
{
    [SerializeField] private Transform _anchorTrackingTransform;
    
    private ARAnchor _anchor;
    private ARRaycastManager _raycastManager;
    private ARAnchorManager _anchorManager;
    private static List<ARRaycastHit> _raycastHits = new List<ARRaycastHit>();
    
    public ARAnchor Anchor => _anchor;

    void Awake()
    {
        _raycastManager = GetComponent<ARRaycastManager>();
        _anchorManager = GetComponent<ARAnchorManager>();

        _anchorManager.anchorsChanged += OnAnchorsChanged;
    }

    void Update()
    {
        // シングルタップ時以外は無視
        if (Input.touchCount != 1)
        {
            return;
        }

        // uGUI操作時は無視
        if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            return;
        }

        // タッチ開始時以外は無視
        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
        {
            return;
        }


        if (_raycastManager.Raycast(touch.position, _raycastHits, TrackableType.FeaturePoint))
        {
            // 古いAnchor削除
            RemoveAnchor();
            
            // 新規Anchor作成
            Pose hitPose = _raycastHits[0].pose;
            AddAnchor(hitPose);
        }
    }
    
    private void OnAnchorsChanged(ARAnchorsChangedEventArgs eventArgs)
    {
        if (eventArgs.added.Count > 0)
        {
            Transform targetTransform = eventArgs.added.Last().transform;
            Pose targetPose = new Pose(targetTransform.position, targetTransform.rotation);
            MoveTrackingObj(targetPose);
        }
    }
    
    public void AddAnchor(Pose hitPose)
    {
        _anchor = _anchorManager.AddAnchor(hitPose);
        if (_anchor == null)
        {
            Logger.Log("Error creating anchor");
        }
    }

    public void RemoveAnchor()
    {
        if (_anchor == null)
        {
            return;
        }
        
        _anchorManager.RemoveAnchor(_anchor);
        _anchor = null;
    }
    
    public void MoveTrackingObj(Pose pose)
    {
        _anchorTrackingTransform.SetPositionAndRotation(pose.position, pose.rotation);
    }
    
}