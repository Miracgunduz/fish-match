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
            GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(sceneName));
        }
    }
}
