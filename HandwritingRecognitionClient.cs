using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace KTV_Superstar;

public class HandwritingRecognitionClient
{
    public async Task<List<string>> RecognizeAsync(byte[] inkData)
    {
        using var pipeClient = new NamedPipeClientStream(".", "HandwritingPipe", PipeDirection.InOut);
        await pipeClient.ConnectAsync();

        using var writer = new BinaryWriter(pipeClient, Encoding.UTF8);
        using var reader = new BinaryReader(pipeClient, Encoding.UTF8);

        writer.Write(inkData.Length);
        writer.Write(inkData);
        writer.Flush();

        string serializedResults = reader.ReadString();

        return serializedResults.Split('|').ToList();
    }
}
