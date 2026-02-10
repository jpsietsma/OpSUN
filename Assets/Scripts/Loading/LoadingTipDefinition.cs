using UnityEngine;

[CreateAssetMenu(menuName = "Game/Loading/Loading Tip Definition", fileName = "LoadingTip_")]
public class LoadingTipDefinition : ScriptableObject
{
    [Header("Tip Content")]
    public string title;

    [TextArea(3, 8)]
    public string tipText;

    [Header("Tip Image")]
    public Sprite tipSprite;

    [Header("Optional")]
    [Tooltip("If true, this tip can be picked randomly. Useful for temporarily disabling tips.")]
    public bool enabledForRandom = true;
}