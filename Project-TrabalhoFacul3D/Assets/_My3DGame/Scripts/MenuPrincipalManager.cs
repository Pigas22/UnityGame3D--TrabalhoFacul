using UnityEngine;

public class MenuPrincipalManager : MonoBehaviour
{
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("JogoScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // Info Menu (Abrir GitHub do projeto)
    public void OpenLinkInfo()
    {
        Application.OpenURL("https://github.com/Pigas22/UnityGame3D--TrabalhoFacul");
    }
}
