using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

[System.Serializable]
public class Data
{
    public Content content;
}

[System.Serializable]
public class Content
{
    public float[] bboxes;
    public string[] pred_labels;
}

public class WebcamUploader : MonoBehaviour
{
    public TMP_Dropdown url_dropdown;
    public string uploadUrl = "http://165.194.115.91:8001/upload";
    private WebCamTexture webcamTexture;
    private Texture2D snap;
    public float captureInterval = 3.0f; // 캡처 및 업로드 주기(초)
    private float timer = 0.0f;

    void Awake()
    {
        url_dropdown.onValueChanged.AddListener(OnDropdownEvent);
        string selectedText = url_dropdown.options[url_dropdown.value].text;
        Debug.Log("현재 선택된 옵션: " + selectedText);
    }

    void Start()
    {
        // 웹캠 시작
        webcamTexture = new WebCamTexture();
        RawImage rawImage = GetComponent<RawImage>();
        rawImage.texture = webcamTexture;
        webcamTexture.Play();
        StartCoroutine(UpdateRawImage());
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= captureInterval)
        {
            timer = 0f;
            StartCoroutine(UpdateRawImage());
            StartCaptureAndUpload();
        }
    }

    public void OnDropdownEvent(int index)
    {
        Debug.Log($"Dropdown Value : {index}");         
        string selectedText = url_dropdown.options[url_dropdown.value].text;
        uploadUrl = selectedText;
    }

    IEnumerator UpdateRawImage()
    {
        // 웹캠 텍스처를 Texture2D로 변환
        RawImage rawImage = GetComponent<RawImage>();
        Rect rawImageRect = rawImage.rectTransform.rect;
        float displayWidth = rawImageRect.width;
        float displayHeight = rawImageRect.height;

        float webcamWidth = webcamTexture.width;
        float webcamHeight = webcamTexture.height;

        float webcamRatio = webcamWidth / webcamHeight;
        float displayRatio = displayWidth / displayHeight;

        int scaledWidth;
        int scaledHeight;

        
        if (webcamRatio > displayRatio)
        {
            scaledWidth = (int)displayWidth;
            scaledHeight = (int)(displayWidth / webcamRatio);
        }
        else
        {
            scaledHeight = (int)displayHeight;
            scaledWidth = (int)(displayHeight * webcamRatio);
        }

        if (snap == null || snap.width != (int)displayWidth || snap.height != (int)displayHeight)
        {
            snap = new Texture2D((int)displayWidth, (int)displayHeight);
        }
        Color[] pixels = webcamTexture.GetPixels();
        Color[] scaledPixels = new Color[(int)displayWidth * (int)displayHeight];

        // Fill with black
        for (int i = 0; i < scaledPixels.Length; i++)
        {
            scaledPixels[i] = Color.black;
        }

        int startX = (int)((displayWidth - scaledWidth) / 2);
        int startY = (int)((displayHeight - scaledHeight) / 2);

        float widthRatio = webcamWidth / scaledWidth;
        float heightRatio = webcamHeight / scaledHeight;

        for (int y = 0; y < scaledHeight; y++)
        {
            for (int x = 0; x < scaledWidth; x++)
            {
                int webcamX = Mathf.Clamp(Mathf.FloorToInt(x * widthRatio), 0, (int)webcamWidth - 1);
                int webcamY = Mathf.Clamp(Mathf.FloorToInt(y * heightRatio), 0, (int)webcamHeight - 1);

                int index = (x + startX) + (y + startY) * (int)displayWidth;
                if (index < scaledPixels.Length)
                {
                    scaledPixels[index] = pixels[webcamX + webcamY * (int)webcamWidth];
                }
            }
        }

        snap.SetPixels(scaledPixels);
        snap.Apply();
        GetComponent<RawImage>().texture = snap;
        yield return null;
    }

    void StartCaptureAndUpload()
    {
        Debug.Log("StartCaptureAndUpload called");
        StartCoroutine(CaptureAndUpload());
    }

    IEnumerator CaptureAndUpload()
    {
        // Debug.Log("CaptureAndUpload called");
        // yield return new WaitForSeconds(2f); // 웹캠 초기화 대기

        
        // PNG/JPG로 인코딩
        byte[] imageData = snap.EncodeToJPG();

        // API #1 - 이미지 업로드
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageData, "webcam" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg", "image/jpeg");
        
        using (UnityWebRequest uploadRequest = UnityWebRequest.Post(uploadUrl, form))
        {
            yield return uploadRequest.SendWebRequest();
            if (uploadRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Upload complete");
            }
            else
            {
                Debug.LogError("Upload failed: " + uploadRequest.error);
            }
        }
    }

}
