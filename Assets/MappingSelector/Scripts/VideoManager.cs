using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using Klak.Spout;
using UnityEngine.UI;

public class VideoManager : MonoBehaviour
{
    public static readonly string[] VIDEO_EXTENSIONS = { ".mp4", ".mov", ".webm" };

    [SerializeField]
    GameObject _GridPanel;

    [Header("Options")]

    [SerializeField]
    bool _LoopVideos = false;

    [Header("Debug")]
    [SerializeField]
    bool _DebugMode = false;
    [SerializeField]
    TextMeshProUGUI _DebugText;


    /* Private variables */
    string[][] _VideoUrls;
    VideoPlayer[] _VideoPlayers;
    RenderTexture[] _RenderTextures;
    Transform _SpoutSendersTransform;

    Coroutine WaitAllCoroutine = null;

    void Start()
    {
        _SpoutSendersTransform = new GameObject("--- Spout Senders ---").transform;
        SyncStreamingAssets();
    }

    public void SyncStreamingAssets()
    {
        CleanData();
        RefreshVideoList();
        CreateMedia();
    }

    private void CleanData()
    {
        _DebugText.text = "";
        _VideoUrls = null;

        if (_VideoPlayers != null)
        {
            foreach (VideoPlayer vp in _VideoPlayers)
            {
                Destroy(vp);
            }
            _VideoPlayers = null;
        }

        if (_RenderTextures != null)
        {
            foreach (RenderTexture rt in _RenderTextures)
            {
                Destroy(rt);
            }
        }
        _RenderTextures = null;

        if (WaitAllCoroutine != null)
        {
            StopCoroutine(WaitAllCoroutine);
            WaitAllCoroutine = null;
        }

        foreach (Transform child in _SpoutSendersTransform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in _GridPanel.transform)
        {
            Destroy(child.gameObject);
        }

    }

    private void RefreshVideoList()
    {
        string[] directories = Directory.GetDirectories(Application.streamingAssetsPath); //Get folders where we have all the videos we want use with spout

        // Check if there are folders
        if (directories.Length == 0)
        {
            Debug.LogWarning("There are no folders in streaming assets");

            if (_DebugMode)
            {
                _DebugText.text = "No folders found";
            }
        }

        _VideoUrls = new string[directories.Length][];

        for (int i = 0; i < directories.Length; i++)
        {
            _VideoUrls[i] = GetFolderVideoUrls(directories[i]);
        }

        string videosCarpeta = "";
        for (int carpeta = 0; carpeta < _VideoUrls.Length; carpeta++)
        {
            Debug.Log("Carpeta " + carpeta + ":");
            for (int video = 0; video < _VideoUrls[carpeta].Length; video++)
            {
                videosCarpeta += "Video " + video + ":" + _VideoUrls[carpeta][video] + "\n";
            }
            Debug.Log(videosCarpeta);
            videosCarpeta = "";
        }
    }

    private string[] GetFolderVideoUrls(string folderUrl)
    {

        List<string> videoUrls = new List<string>();

        string[] filesPath = Directory.GetFiles(folderUrl);

        foreach (string file in filesPath)
        {

            if (IsVideoExtension(Path.GetExtension(file)))
            {
                videoUrls.Add(file);
            }

        }

        return videoUrls.ToArray();
    }

    private bool IsVideoExtension(string extension)
    {
        if (extension == ".meta") // We hate Facebook :)
        {
            return false;
        }

        foreach (string vidExtension in VIDEO_EXTENSIONS)
        {
            if (extension.ToLower() == vidExtension) //Lowercase to evade errors
            {
                return true;
            }
        }

        return false;
    }

    private void CreateMedia()
    {
        if (_VideoUrls.GetLength(0) == 0)
        {
            Debug.LogWarning("_VideoUrls is empty");

            if (_DebugMode)
            {
                _DebugText.text = "_VideoUrls is empty";
            }
        }

        _RenderTextures = new RenderTexture[_VideoUrls.GetLength(0)];
        _VideoPlayers = new VideoPlayer[_VideoUrls.GetLength(0)];
        GameObject tempGO;
        SpoutSender tempSpout;
        GameObject tempRawImage;

        for (int i = 0; i < _VideoUrls.GetLength(0); i++)
        {
            //RT
            _RenderTextures[i] = new RenderTexture(4096, 4096, 16);
            _RenderTextures[i].name = i + "_RT";

            //VideoPlayer
            tempGO = new GameObject(i + "_Video", typeof(VideoPlayer));
            tempGO.transform.SetParent(_SpoutSendersTransform);
            _VideoPlayers[i] = tempGO.GetComponent<VideoPlayer>();
            _VideoPlayers[i].isLooping = _LoopVideos;
            _VideoPlayers[i].playOnAwake = false;
            _VideoPlayers[i].renderMode = VideoRenderMode.RenderTexture;
            _VideoPlayers[i].targetTexture = _RenderTextures[i];
            _VideoPlayers[i].source = VideoSource.Url;
            _VideoPlayers[i].loopPointReached += OnVideoEnds;

            if (0 < _VideoUrls[i].Length)
            {
                _VideoPlayers[i].url = _VideoUrls[i][0];
            }

            //Spout
            tempSpout = tempGO.AddComponent<SpoutSender>();
            tempSpout.captureMethod = CaptureMethod.Texture;
            tempSpout.sourceTexture = _RenderTextures[i];
            tempSpout.spoutName = "Unity_" + i;

            //Debug
            if (_DebugMode)
            {
                tempRawImage = new GameObject(i + "_RawImage", typeof(RawImage));

                tempRawImage.transform.SetParent(_GridPanel.transform, false); //tempRawImage.transform.parent = _GridPanel.transform;
                tempRawImage.GetComponent<RawImage>().texture = _RenderTextures[i];
            }
        }
    }

    public void PlayAllVideos()
    {
        StopAllVideos();

        for (int i = 0; i < _VideoPlayers.Length; i++) //_VideoUrls.GetLength(0)
        {
            _VideoPlayers[i].Prepare();
        }

        if (WaitAllCoroutine != null)
        {
            StopCoroutine(WaitAllCoroutine);
            WaitAllCoroutine = null;
        }

        WaitAllCoroutine = StartCoroutine(WaitAllPrepared());
    }

    IEnumerator WaitAllPrepared()
    {
        for (int i = 0; i < _VideoPlayers.Length; i++)
        {
            if (_VideoPlayers[i].url == string.Empty)
            {
                Debug.LogWarning("Trying to play a video player without URL");
            }
            //else if (File.Exists(_VideoPlayers[i].url))
            //{
            //    Debug.LogWarning("File moved or path doesn't exist anymore");
            //}
            else
            {
                yield return new WaitUntil((() => _VideoPlayers[i].isPrepared == true));
            }
        }

        for (int i = 0; i < _VideoPlayers.Length; i++)
        {
            _VideoPlayers[i].Play();
        }

    }

    public void PlayVideo(int spoutIndex, int videoIndex)
    {
        if (_VideoUrls.Length <= spoutIndex)
        {
            Debug.LogWarning("Play video -> Spout index out of range");
            return;
        }

        if (_VideoUrls[spoutIndex].Length <= videoIndex)
        {
            Debug.LogWarning("Play video -> Video index out of range");
            return;
        }

        _VideoPlayers[spoutIndex].url = _VideoUrls[spoutIndex][videoIndex];

        if (File.Exists(_VideoPlayers[spoutIndex].url))
        {
            _VideoPlayers[spoutIndex].Play();
        }
        else
        {
            Debug.LogWarning("File moved or path doesn't exist anymore");
        }

    }

    public void StopProjector(int spoutIndex)
    {
        if (_VideoUrls.Length <= spoutIndex)
        {
            Debug.LogWarning("Stop projector -> Spout index out of range");
            return;
        }

        _VideoPlayers[spoutIndex].Stop();
        Fenikkel.ClearOutRenderTexture(_VideoPlayers[spoutIndex].targetTexture);
    }

    public void StopAllVideos()
    {
        for (int i = 0; i < _VideoPlayers.Length; i++)
        {
            _VideoPlayers[i].Stop();
            Fenikkel.ClearOutRenderTexture(_VideoPlayers[i].targetTexture);
        }
    }

    public int[] GetInfoArray()
    {
        int[] infoArray = new int[_VideoUrls.Length];

        for (int i = 0; i < _VideoUrls.Length; i++)
        {
            infoArray[i] = _VideoUrls[i].Length;
        }
        return infoArray;
    }

    private void OnVideoEnds(VideoPlayer vp)
    {
        if (!_LoopVideos)
        {
            vp.Stop();
            Fenikkel.ClearOutRenderTexture(vp.targetTexture);
        }
    }
}
