namespace Intentum.AI.MultiModal;

public enum InputModality { Behavior, Image, Audio, Text }

public sealed record MultiModalInput(
    InputModality Modality,
    string Value,
    float[]? Embedding = null);
