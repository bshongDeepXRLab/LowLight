using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class WebcamUploader : MonoBehaviour
{
    public string uploadUrl = "http://165.194.114.40:7531/upload";
    public string latestJsonUrl = "http://165.194.114.40:7531/latest";
    private WebCamTexture webcamTexture;
    float timer = 0.0f;

    void Start()
    {
        // 웹캠 시작
        webcamTexture = new WebCamTexture();
        RawImage rawImage = GetComponent<RawImage>();
        rawImage.texture = webcamTexture;
        webcamTexture.Play();
        InitRawImage();
        // 일정 시간 후 이미지 캡처 & 업로드
        //InvokeRepeating("StartCaptureAndUpload", 1f, 3f);
    }

    void InitRawImage()
    {
        // 웹캠 텍스처를 Texture2D로 변환
        RawImage rawImage = GetComponent<RawImage>();
        Rect rawImageRect = rawImage.rectTransform.rect;
        float displayWidth = rawImageRect.width;
        float displayHeight = rawImageRect.height;

        float webcamWidth = webcamTexture.width;
        float webcamHeight = webcamTexture.height;

        Debug.Log("displayWidth = " + displayWidth);
        Debug.Log("displayHeight = " + displayHeight);
        Debug.Log("webcamWidth = " + webcamWidth);
        Debug.Log("webcamHeight = " + webcamHeight);

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

        Texture2D snap = new Texture2D((int)displayWidth, (int)displayHeight);
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
    }

    void StartCaptureAndUpload()
    {
        Debug.Log("StartCaptureAndUpload called");
        //StartCoroutine(CaptureAndUpload());
    }

    // IEnumerator CaptureAndUpload()
    // {
    //     // Debug.Log("CaptureAndUpload called");
    //     // yield return new WaitForSeconds(2f); // 웹캠 초기화 대기

        
    //     // // PNG/JPG로 인코딩
    //     // byte[] imageData = snap.EncodeToJPG();

    //     // // API #1 - 이미지 업로드
    //     // WWWForm form = new WWWForm();
    //     // form.AddBinaryData("image", imageData, "webcam" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg", "image/jpeg");
        
    //     // using (UnityWebRequest uploadRequest = UnityWebRequest.Post(uploadUrl, form))
    //     // {
    //     //     yield return uploadRequest.SendWebRequest();
    //     //     if (uploadRequest.result == UnityWebRequest.Result.Success)
    //     //     {
    //     //         Debug.Log("Upload complete");
    //     //         // API #2 - 결과 JSON 가져오기
    //     //         StartCoroutine(GetJsonData());
    //     //     }
    //     //     else
    //     //     {
    //     //         Debug.LogError("Upload failed: " + uploadRequest.error);
    //     //     }
    //     // }
    // }

    IEnumerator GetJsonData()
    {
        using (UnityWebRequest jsonRequest = UnityWebRequest.Get(latestJsonUrl))
        {
            yield return jsonRequest.SendWebRequest();

            if (jsonRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("JSON result: " + jsonRequest.downloadHandler.text);
                // JsonUtility.FromJson<T>() 또는 Newtonsoft.Json 사용 가능
            }
            else
            {
                Debug.LogError("Get JSON failed: " + jsonRequest.error);
            }
        }
    }
}
