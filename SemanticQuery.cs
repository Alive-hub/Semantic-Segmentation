using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.Lightship.AR.Semantics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

//This script was originally provided by Niantic and modified by Alive Studios
//https://lightship.dev/docs/ardk/how-to/ar/query_semantics_real_objects/
public class SemanticQuery : MonoBehaviour
{
    public ARCameraManager _cameraMan;
    public ARSemanticSegmentationManager _semanticMan;

    public TMP_Text _text;
    public RawImage _image;
    public Material _material;

    private string _channel = "ground";

    [SerializeField] private Transform spawnObjectParent;
    public List<ChannelToObject> ChannelToObjects;

    private void OnEnable()
    {
        _image.enabled = false;
        _cameraMan.frameReceived += CameraManOnframeReceived;
    }

    private void CameraManOnframeReceived(ARCameraFrameEventArgs args)
    {
        if (!_semanticMan.subsystem.running)
        {
            return;
        }


        Matrix4x4 mat = Matrix4x4.identity;
        var texture = _semanticMan.GetSemanticChannelTexture(_channel, out mat);

        if (texture)
        {
            Matrix4x4 cameraMatrix = args.displayMatrix ?? Matrix4x4.identity;
            _image.material = _material;
            _image.material.SetTexture("_SemanticTex", texture);
            _image.material.SetMatrix("_SemanticMat", mat);
        }

    }

    private float timer = 0.0f;
    void Update()
    {
        if(!_semanticMan.subsystem.running)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.touches.Length > 0)
        {
            var pos = Input.mousePosition;


            if (pos.x > 0 && pos.x < Screen.width)
            {
                if (pos.y > 0 && pos.y < Screen.height)
                {
                    timer += Time.deltaTime;
                    _image.enabled = true;


                    if (timer > 0.05f)
                    {
                        var list = _semanticMan.GetChannelNamesAt((int)pos.x, (int)pos.y);

                        if (list.Count > 0)
                        {
                            _channel = list[0];
                            _text.text = _channel;

                            foreach (var channelToObject in ChannelToObjects)
                            {
                                if (channelToObject.channel == _channel)
                                {
                                    Debug.Log($"The channel {_channel} has been detected and will spawn an object!");
                                    GameObject newObject = Instantiate(channelToObject.GameObject, pos,
                                        Quaternion.identity, spawnObjectParent);
                                    Destroy(newObject, 3f);
                                }
                            }
                        }
                        else
                        {
                            _text.text = "?";
                        }

                        timer = 0.0f;

                    }
                    
                }
            }

        }
        
    }
}

[System.Serializable]
public struct ChannelToObject
{
    public string channel;
    public GameObject GameObject;
}