using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProceduralCities.Helpers
{
    public class QuitScript : MonoBehaviour
    {
        [Header("Buttons")] [SerializeField] private KeyCode _quitButton = KeyCode.Escape;
        [SerializeField] private KeyCode _reloadButton = KeyCode.R;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(_quitButton))
            {
                Application.Quit();
            }

            if (Input.GetKeyDown(_reloadButton))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}