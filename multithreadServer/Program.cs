using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;
using System.Threading;  
  
public class threadData
{
    public string username;
    public string gameid;
    public string mark;
}

public class gameData
{
    public string X;
    public string O;
    public string gameid;
    public char[] board;
}

public class SynchronousSocketListener {  

    public static char[] dic = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

    public Queue<string> waiting = new Queue<string>();

    public List<threadData> dataList = new List<threadData>();

    public List<gameData> gameList = new List<gameData>();

    public string respondHttp(string body){
        string statusLine = "HTTP/1.1 200 OK\r\n";
        string responseBody = body;
        string bodyLengh="Content-Length: "+responseBody.Length.ToString()+"\r\n";
        string responseHeader = "Content-Type: text/html\r\nConnection: keep-alive\r\nKeep-Alive: timeout=60, max=100\r\nAccess-Control-Allow-Origin: *\r\n"+bodyLengh+"\r\n"; 
        return statusLine+responseHeader+responseBody;
    }

    public void handleRequest(object socket){
        Socket handler=(Socket) socket;
        IPEndPoint newclient = (IPEndPoint)handler.RemoteEndPoint;
        // string username="";
        // string gameid="";
        // string mark="";
        Console.WriteLine("Connection established with " + newclient.Address.ToString() + ":" + newclient.Port.ToString());
        try{
            while(true){
                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                string request = "";

                while (true)
                {
                    bytesRead = handler.Receive(buffer);
                    if(bytesRead == 0){
                        break;
                    }   
                    request += Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    if (buffer[bytesRead - 1] == 10 && buffer[bytesRead - 2] == 13) // && buffer[bytesRead-3] == 10 && buffer[bytesRead-4] == 13) 
                    {
                        break;
                    }
                }

                if(bytesRead == 0){
                    Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " closing connection with " + newclient.Address.ToString() + ":" + newclient.Port.ToString() + " and terminating");
                    break;
                }

                string[] splits = request.Split(' ');

                //endpoints
                if (splits[1] == "/register")
                {
                    Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " sent response to " + newclient.Address.ToString() + ":" + newclient.Port.ToString() + " for /register");
                    string name=register();
                    handler.Send(Encoding.UTF8.GetBytes(respondHttp(name)));
                }
                else if (splits[1] == "/favicon.ico")
                {
                    Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " sent response to " + newclient.Address.ToString() + ":" + newclient.Port.ToString() + " for /favicon.ico");
                    handler.Send(Encoding.UTF8.GetBytes(respondHttp("")));
                }
                else if(splits[1].Length < 8)
                {
                    string statusLine = "HTTP/1.1 404 Bad Request\r\n";
                    string responseHeader = "Content-Type: text/html\r\nConnection: keep-alive\r\nKeep-Alive: timeout=60, max=100\r\nAccess-Control-Allow-Origin: *Content-Length: 0\r\n\r\n";
                    handler.Send(Encoding.UTF8.GetBytes(statusLine+responseHeader));
                }
                else if (string.Compare(splits[1].Substring(1, 6), "pairme") == 0)
                {
                    //paireme
                    Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " sent response to " + newclient.Address.ToString() + ":" + newclient.Port.ToString() + " for "+splits[1]);
                    string name = splits[1].Substring(15);
                    if (dataList.Exists(x => x.username == name))             //exist user
                    {
                        var tmp=dataList.Where(x => x.username == name).FirstOrDefault();
                        if(tmp.gameid == ""){
                            handler.Send(Encoding.UTF8.GetBytes(respondHttp("wait")));
                        }else{
                            var game = gameList.Where(x => x.gameid == tmp.gameid).FirstOrDefault();
                            string oppName = (tmp.mark == "X" ? game.O : game.X);
                            string responseBody = tmp.mark + " " + oppName + " " + game.gameid;
                            handler.Send(Encoding.UTF8.GetBytes(respondHttp(responseBody)));
                        }
                    }
                    else if (waiting.Count == 0)             //new user, the first one
                    {
                        threadData tmp = new threadData();
                        tmp.username = name;
                        tmp.gameid = "";
                        tmp.mark = "X";
                        lock (dataList)
                        {
                            dataList.Add(tmp);
                        }                        
                        waiting.Enqueue(name);
                        handler.Send(Encoding.UTF8.GetBytes(respondHttp("wait")));
                    }
                    else                        //new user, the second one
                    {
                        threadData tmp = new threadData();
                        tmp.username = name;
                        string UUID = Guid.NewGuid().ToString();
                        tmp.gameid = UUID;
                        tmp.mark = "O";
                        lock (dataList)
                        {
                            dataList.Add(tmp);
                        }
                        string tmpName = waiting.Dequeue();
                        lock (dataList)
                        {
                            var tmpD = dataList.Where(x => x.username == tmpName).FirstOrDefault();
                            tmpD.gameid = UUID;
                        }
                        gameData game = new gameData();
                        game.gameid = UUID;
                        game.board = new char[9]{'0', '0', '0', '0', '0', '0', '0', '0', '0'};
                        game.X = tmpName;
                        game.O = name;
                        lock (gameList)
                        {
                            gameList.Add(game);
                        }
                        string responseBody = tmp.mark + " " + tmpName + " " + UUID;
                        handler.Send(Encoding.UTF8.GetBytes(respondHttp(responseBody)));
                    }
                }
                else if(string.Compare(splits[1].Substring(1, 6), "mymove") == 0)
                {
                    Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " sent response to " + newclient.Address.ToString() + ":" + newclient.Port.ToString() + " for "+splits[1]);
                    string[] split = splits[1].Split(new char[3] { '?', '&', '=' });
                    string name = split[2];
                    if(dataList.Exists(x => x.username == name))
                    {
                        string gameId = split[4];
                        string move = split[6];
                        lock (gameList)
                        {
                            if (gameList.Exists(x => x.gameid == gameId))
                            {
                                var game = gameList.Where(x => x.gameid == gameId).FirstOrDefault();
                                for (int i = 0; i < game.board.Length; i++)
                                {
                                    game.board[i] = move[i];
                                }
                            }
                        }
                        handler.Send(Encoding.UTF8.GetBytes(respondHttp("")));
                    }
                    else
                    {
                        handler.Send(Encoding.UTF8.GetBytes(respondHttp("")));
                    }

                }
                else if(string.Compare(splits[1].Substring(1, 9), "theirmove") == 0)
                {
                    Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " sent response to " + newclient.Address.ToString() + ":" + newclient.Port.ToString() + " for "+splits[1]);
                    string[] split = splits[1].Split(new char[3] { '?', '=', '&' });
                    string gameId = split[4];
                    string move = "";
                    string name = split[2];
                    if(dataList.Exists(x => x.username == name)){
                        lock (gameList)
                        {
                            if(gameList.Exists(x => x.gameid == gameId)){
                                var game = gameList.Where(x => x.gameid == gameId).FirstOrDefault();
                                for (int i = 0; i < game.board.Length; i++)
                                {
                                    move += game.board[i];
                                }
                                handler.Send(Encoding.UTF8.GetBytes(respondHttp(move))); 
                            }
                            else{
                                handler.Send(Encoding.UTF8.GetBytes(respondHttp("000000000")));
                            }
                        }
                    }else{
                        handler.Send(Encoding.UTF8.GetBytes(respondHttp("000000000")));
                    }   
                }
                else if(string.Compare(splits[1].Substring(1, 4), "quit") == 0)
                {
                    Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " sent response to " + newclient.Address.ToString() + ":" + newclient.Port.ToString() + " for "+splits[1]);
                    string[] split = splits[1].Split(new char[3] { '?', '=', '&' });
                    string gameId = split[4];
                    string name = split[2];
                    if(dataList.Exists(x => x.username == name)){
                        lock (dataList)
                        {
                            var user1 = dataList.Where(x => x.username == name).FirstOrDefault();
                            dataList.Remove(user1);
                        }
                        lock (gameList)
                        {
                            if (gameList.Exists(x => x.gameid == gameId))
                            {
                                var game = gameList.Where(x => x.gameid == gameId).FirstOrDefault();
                                gameList.Remove(game);
                            }
                        }
                    }
                    handler.Send(Encoding.UTF8.GetBytes(respondHttp("")));      
                }
                else
                {
                    string statusLine = "HTTP/1.1 404 Bad Request\r\n";
                    string responseHeader = "Content-Type: text/html\r\nConnection: keep-alive\r\nKeep-Alive: timeout=60, max=100\r\nAccess-Control-Allow-Origin: *Content-Length: 0\r\n\r\n";
                    handler.Send(Encoding.UTF8.GetBytes(statusLine+responseHeader));
                }

            }
        }catch(Exception e){
            Console.WriteLine(e.ToString());
        }
        handler.Shutdown(SocketShutdown.Both);  
        handler.Close();  
    }
  
    public void StartListening() {  

        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());  
        IPAddress ipAddress = IPAddress.Loopback;
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8080);  
        //Console.WriteLine("Server running at " + ipAddress + ":8080\n");
    
        Socket listener = new Socket(ipAddress.AddressFamily,  
            SocketType.Stream, ProtocolType.Tcp );  
  
        try {  
            listener.Bind(localEndPoint);  
            listener.Listen(10);  
            Console.WriteLine("Listening at 127.0.0.1:8080");
            // Start listening for connections.
            while (true)
            {
                new Thread(new ParameterizedThreadStart(handleRequest)).Start(listener.Accept());
            }   
        } catch (Exception e) {  
            Console.WriteLine(e.ToString());  
        }  
  
        Console.WriteLine("\nPress ENTER to continue...");  
        Console.Read();  
  
    }

    public string register()
    {
        Random random = new Random();
        string name = "";
        for (int i = 0; i < 10; i++)
        {
            name += dic[random.Next(0, 15625) % 25];
        }
        return name;
    }  
}

public class main
{
    public static int Main(String[] args)
    {
        SynchronousSocketListener socket=new SynchronousSocketListener();
        socket.StartListening();
        return 0;
    }
}
