namespace EagleFlow.Services;

public class DocumentNumberGenerator : IDocumentNumberGenerator
{
    public string Generate()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = Random.Shared;
        var suffix = new char[9];

        for (var i = 0; i < suffix.Length; i++)
        {
            suffix[i] = chars[random.Next(chars.Length)];
        }

        return $"DOC-{new string(suffix)}";
    }
}
