using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Player1
{
    private Socket server;
    private const int port = 9050;
    private const string ip = "127.0.0.1";

    public Player1()
    {
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public void ConnectToServer()
    {
        try
        {
            server.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
            Console.WriteLine("Connected to the game server.");
        }
        catch (SocketException e)
        {
            Console.WriteLine("Unable to connect to server.");
            Console.WriteLine(e.ToString());
            Environment.Exit(-1);
        }
    }

    public void StartGameLoop()
    {
        ReceiveMessage(); // İlk hoşgeldiniz mesajını al.

        while (true)
        {
            Console.WriteLine("Enter 'roll' to roll the dice, 'exit' to quit:");
            string input = Console.ReadLine();

            if (input == "exit")
                break;

            if (input == "roll")
            {
                SendMessage("roll");
                ReceiveMessage(); // Zar atma sonucunu al.
            }
            // Buraya diğer komutları işleyecek kodları ekleyebilirsiniz.
        }

        DisconnectFromServer();
    }

    private void SendMessage(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        server.Send(data);
    }

    private void ReceiveMessage()
    {
        byte[] data = new byte[1024];
        int recv = server.Receive(data);
        string stringData = Encoding.ASCII.GetString(data, 0, recv);
        Console.WriteLine(stringData);

        // Burada sunucudan gelen mesajlara göre ek işlemler yapabilirsiniz.
    }

    private void DisconnectFromServer()
    {
        Console.WriteLine("Disconnecting from server...");
        server.Shutdown(SocketShutdown.Both);
        server.Close();
    }

    public static void Main()
    {
        Player1 player1 = new Player1();
        player1.ConnectToServer();
        player1.StartGameLoop();
    }
}
