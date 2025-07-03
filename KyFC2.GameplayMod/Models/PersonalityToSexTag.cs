namespace KyFC2.GameplayMod.Models;
internal class PersonalityToSexTag {
    public int PersonalityID { get; set; }
    public string PersonalityName { get; set; } = string.Empty;
    public bool IsDominant { get; set; }
    public bool IsSensual { get; set; }
    public bool IsService { get; set; }
    public bool IsSmothering { get; set; }
    public bool IsWresting { get; set; }

    internal bool SexMoveCheck(SexMoveExtended sexMove) {
        if (sexMove.IsDominant == IsDominant)
            return true;

        if (sexMove.IsSensual == IsSensual)
            return true;

        if (sexMove.IsService == IsService)
            return true;

        if (sexMove.IsSmothering == IsSmothering)
            return true;

        if (sexMove.IsWresting == IsWresting)
            return true;

        return false;
    }
}