using UnityEngine;

[RequireComponent(typeof(TestPlayerController)), RequireComponent(typeof(TestPlayerPowerManager))]
public class TestPlayer : MonoBehaviour
{
    private TestPlayerController _playerController;
    private TestPlayerPowerManager _playerPowerManager;

    #region Getters

    public TestPlayerController PlayerController => _playerController;

    public TestPlayerPowerManager PlayerPowerManager => _playerPowerManager;

    #endregion

    #region Initialization Functions

    private void Awake()
    {
        // Initialize the components
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Get the TestPlayerController component
        _playerController = GetComponent<TestPlayerController>();

        // Get the TestPlayerPowerManager component
        _playerPowerManager = GetComponent<TestPlayerPowerManager>();
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}