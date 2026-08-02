using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FishMatch.Match3
{
    [RequireComponent(typeof(Button))]
    public class SceneLoadButton : MonoBehaviour
    {
        [SerializeField] private string sceneName;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                // A paused level sets Time.timeScale to 0 - without resetting it here, the scene we
                // navigate to would load already frozen (no animations, no coroutine progress).
                Time.timeScale = 1f;
                SceneManager.LoadScene(sceneName);
            });
        }
    }
}
