public interface IGameMenu
{
    public bool IsCursorRequired { get; }
    public bool DisablePlayerControls { get; }
    public bool PausesGame { get; }
    public bool PausesGameMusic { get; }

    public void OnBackPressed();
}