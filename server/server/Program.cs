using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading;

class SimpleTcpSrvr
{
    private List<Socket> clients = new List<Socket>();
    private int currentPlayerIndex = -1;
    private Random random = new Random();
    private Socket newsock;

    static void Main()
    {
        SimpleTcpSrvr server = new SimpleTcpSrvr();
        server.SetupServer();
    }

    public void SetupServer()
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);

        newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        newsock.Bind(ipep);
        newsock.Listen(10);
        Console.WriteLine("Waiting for clients...");

        new Thread(() =>
        {
            while (true)
            {
                Socket client = newsock.Accept();
                clients.Add(client);
                IPEndPoint clientep = (IPEndPoint)client.RemoteEndPoint;
                Console.WriteLine("Connected with {0} at port {1}", clientep.Address, clientep.Port);

                if (clients.Count == 2) // İki oyuncu bağlandığında oyunu başlat.
                {
                    StartGame();
                }

                new Thread(() => ClientHandler(client)).Start();
            }
        }).Start();
    }

    private void StartGame()
    {
        currentPlayerIndex = random.Next(clients.Count);
        Broadcast($"Player {currentPlayerIndex + 1} starts the game.");
        NotifyPlayerTurn(currentPlayerIndex);
    }

    private void Broadcast(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        foreach (var client in clients)
        {
            client.Send(data);
        }
    }

    private void NotifyPlayerTurn(int playerIndex)
    {
        string message = $"It's your turn, player {playerIndex + 1}";
        Broadcast(message);
    }

    private void ClientHandler(Socket client)
    {
        while (true)
        {
            byte[] data = new byte[1024];
            int recv = client.Receive(data);
            if (recv == 0)
            {
                client.Close();
                clients.Remove(client);
                Console.WriteLine("Client disconnected.");
                break;
            }

            string receivedString = Encoding.ASCII.GetString(data, 0, recv);
            Console.WriteLine("Received: {0}", receivedString);

            if (clients[currentPlayerIndex] == client)
            {
                ProcessCommand(receivedString, client);
            }
            else
            {
                // Oyuncunun sırası değilse uyarı mesajı gönder
                string notYourTurnMessage = "It's not your turn.";
                byte[] notYourTurnData = Encoding.ASCII.GetBytes(notYourTurnMessage);
                client.Send(notYourTurnData);
            }
        }
    }

    private void ProcessCommand(string command, Socket client)
    {
        switch (command.ToLower())
        {
            case "roll":
                RollDiceForPlayer();
                break;
            case "buy":
            case "pass":
                Broadcast($"Player {currentPlayerIndex + 1} has chosen to '{command.ToLower()}'");
                NextPlayer();
                break;
            default:
                Broadcast("Invalid command received. Please try again.");
                break;
        }
    }

    private void RollDiceForPlayer()
    {
        int diceRoll = random.Next(1, 10); // 1 ile 9 arasında rastgele bir sayı üretir.
        string diceResult = $"Player {currentPlayerIndex + 1} rolled a {diceRoll}.";
        Broadcast(diceResult);

        NextPlayer(); // Zar atma işleminden sonra sıradaki oyuncuya geç.
    }

    private void NextPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % clients.Count;
        NotifyPlayerTurn(currentPlayerIndex);
    }
}
