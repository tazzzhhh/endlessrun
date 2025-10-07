using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingScene : MonoBehaviour
{
    public static string sceneToLoad; // MenuGame sẽ gán giá trị này
    public Slider sliderLoading;

    void Start()
    {
        Debug.Log("Scene cần load: " + sceneToLoad);
        // Nếu chưa gán scene, mặc định load ScenePlay
        StartCoroutine(LoadSceneAsync(sceneToLoad));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // Chặn vào ngay tức khắc

        float fakeProgress = 0f; // Lệnh giả thanh chạy từ từ

        while (!operation.isDone)
        {
            float targetprogress = Mathf.Clamp01(operation.progress / 0.9f);

            // Tăng dần fakeProgress để thanh chạy mượt
            fakeProgress = Mathf.MoveTowards(fakeProgress, targetprogress, Time.deltaTime * 0.3f);

            sliderLoading.value = fakeProgress;

           if(fakeProgress >= 1f && operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.5f); // Đợt chút tầm nửa giây
                Debug.Log("Đã load xong, chuyển sang: " + sceneToLoad);
                operation.allowSceneActivation = true; // Vào scene gameplay
            }    

            yield return null;
        }
    }
}
