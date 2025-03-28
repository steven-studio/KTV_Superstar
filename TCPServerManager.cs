namespace KTV_Superstar;

public static class TCPServerManager
{
    public static void StartServer()
    {
        TCPServer server = new TCPServer();
        server.Start();
    }
}