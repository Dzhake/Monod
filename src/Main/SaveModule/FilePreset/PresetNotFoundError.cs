namespace Monod.SaveModule.FilePreset;

public class PresetNotFoundError : FilePresetError
{
    public readonly string PresetName;

    public PresetNotFoundError(string presetName)
    {
        PresetName = presetName;
    }

    public override string GetText() => $"Preset with name {PresetName} not found"; //TODO (localization - low priority) localize preset error messages? probably should be done not here, but in preset user.
}
