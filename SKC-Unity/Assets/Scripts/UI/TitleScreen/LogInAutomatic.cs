using UnityEngine;
using UI.TitleScreen;

public class LogInAutomatic : MonoBehaviour
{
    [SerializeField] private LogInLayoutController _loginLayout;
    
    private void Start() 
    {
        _loginLayout.SetCredentials("admin", "admin");
        _loginLayout.SubmitLogin();
    }
}
