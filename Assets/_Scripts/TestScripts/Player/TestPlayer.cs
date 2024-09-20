using UnityEngine;

[RequireComponent(typeof(Playerinfo))]
[RequireComponent(typeof(TestPlayerController))]
[RequireComponent(typeof(TestPlayerPowerManager))]
public class TestPlayer : MonoBehaviour
{
    private Playerinfo _playerInfo;
    private TestPlayerController _playerController;
    private TestPlayerPowerManager _playerPowerManager;

    #region Getters

    public Playerinfo PlayerInfo => _playerInfo;

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
        // Get the Playerinfo component
        _playerInfo = GetComponent<Playerinfo>();
        
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