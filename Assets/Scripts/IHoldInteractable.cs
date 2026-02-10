public interface IHoldInteractable
{
    string GetHoldPromptText();
    float HoldDuration { get; }
    void OnHoldComplete();
}