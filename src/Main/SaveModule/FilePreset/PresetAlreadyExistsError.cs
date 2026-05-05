namespace Monod.SaveModule.FilePreset;

public class PresetAlreadyExistsError : FilePresetError
{
    public readonly string PresetName;

    public PresetAlreadyExistsError(string presetName)
    {
        PresetName = presetName;
    }

    public override string GetText() => $"Preset with name {PresetName} already exists";
}