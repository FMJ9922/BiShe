using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerMy : MonoBehaviour
{
    RawImage rawImage;
    VideoPlayer videoPlayer;
    void Start()
    {
        rawImage = this.GetComponent<RawImage>();
        videoPlayer = this.GetComponent<VideoPlayer>();
    }


    void Update()
    {
        if (videoPlayer.texture == null)
        {
            return;
        }
        rawImage.texture = videoPlayer.texture;
    }

}
