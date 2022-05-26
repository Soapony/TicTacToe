using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

public class threadData
{
    public Socket handler;
    public string username;
    public string gameid;
    public long timestamp;
    public string command;
    public string mark;
}

public class gameData
{
    public string X;
    public string O;
    public string gameid;
    public char[] board;
}

public class SynchronousSocketListener
{
    public static char[] dic = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

    public Queue<string> waiting = new Queue<string>();

    public List<threadData> dataList = new List<threadData>();

    public List<gameData> gameList = new List<gameData>();

    public static long getTimestamp()
    {
        return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
    }

    public static void register(Object Obj)
    {
        Socket handler = (Socket)Obj;
        IPEndPoint newclient = (IPEndPoint)handler.RemoteEndPoint;
        Console.WriteLine("Connection established with " + newclient.Address.ToString() + ":" + newclient.Port.ToString());
        Random random = new Random();
        string name = "";
        for (int i = 0; i < 10; i++)
        {
            name += dic[random.Next(0, 15625) % 25];
        }
        string statusLine = "HTTP/1.1 200 OK\r\n";
        string responseHeader = "Content-Type: text/html\r\nAccess-Control-Allow-Origin: *\r\n";
        string responseBody = name;
        Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " sent response to " + newclient.Address.ToString() + ":" + newclient.Port.ToString() + " for /register");
        handler.Send(Encoding.UTF8.GetBytes(statusLine));
        handler.Send(Encoding.UTF8.GetBytes(responseHeader));
        handler.Send(Encoding.UTF8.GetBytes("\r\n"));
        handler.Send(Encoding.UTF8.GetBytes(responseBody));
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
        Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " closing connection with " + newclient.Address.ToString() + ":" + newclient.Port.ToString() + " and terminating");
    }

    public static void favicon(Object Obj)
    {
        Socket handler = (Socket)Obj;
        IPEndPoint newclient = (IPEndPoint)handler.RemoteEndPoint;
        Console.WriteLine("Connection established with " + newclient.Address.ToString() + ":" + newclient.Port.ToString());
        string statusLine = "HTTP/1.1 200 OK\r\n";
        string responseHeader = "Content-Type: text/html\r\nAccess-Control-Allow-Origin: *\r\n";
        string responseBody = "favicon";
        Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " sent response to " + newclient.Address.ToString() + ":" + newclient.Port.ToString() + " for /favicon.ico");
        handler.Send(Encoding.UTF8.GetBytes(statusLine));
        handler.Send(Encoding.UTF8.GetBytes(responseHeader));
        handler.Send(Encoding.UTF8.GetBytes("\r\n"));
        handler.Send(Encoding.UTF8.GetBytes(responseBody));
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
        Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " closing connection with " + newclient.Address.ToString() + ":" + newclient.Port.ToString() + " and terminating");
    }

    public static void Error(Object Obj)
    {
        Socket handler = (Socket)Obj;
        IPEndPoint newclient = (IPEndPoint)handler.RemoteEndPoint;
        Console.WriteLine("Connection established with " + newclient.Address.ToString() + ":" + newclient.Port.ToString());
        string statusLine = "HTTP/1.1 404 Bad Request\r\n";
        string responseHeader = "Content-Type: text/html\r\nAccess-Control-Allow-Origin: *\r\n";
        string responseBody = "";
        Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " sent response to " + newclient.Address.ToString() + ":" + newclient.Port.ToString() + " for incorrect Endpoint");
        handler.Send(Encoding.UTF8.GetBytes(statusLine));
        handler.Send(Encoding.UTF8.GetBytes(responseHeader));
        handler.Send(Encoding.UTF8.GetBytes("\r\n"));
        handler.Send(Encoding.UTF8.GetBytes(responseBody));
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
        Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " closing connection with " + newclient.Address.ToString() + ":" + newclient.Port.ToString() + " and terminating");
    }


    public void playerThread(Object Obj)
    {
        //handle single player
        string name = dataList.Last().username;
        Socket handler1 = (Socket)Obj;
        IPEndPoint newclient = (IPEndPoint)handler1.RemoteEndPoint;
        Console.WriteLine("Connection established with " + newclient.Address.ToString() + ":" + newclient.Port.ToString());
        while (true)
        {
            //fetch command
            string command;
            Socket handler;
            string mark;
            string gameid;
            lock (dataList)
            {
                var tmp = dataList.Where(x => x.username == name).FirstOrDefault();
                command = tmp.command;
                handler = tmp.handler;
                mark = tmp.mark;
                gameid = tmp.gameid;
                tmp.command = "";
            }
            
            if (command == "")
            {
                Thread.Sleep(1000);
                continue;
            }
            //process command
            if(command == "timeout000000")
            {
                //timeout
                lock (dataList)
                {
                    var user1 = dataList.Where(x => x.username == name).FirstOrDefault();
                    dataList.Remove(user1);
                }
                break;
            }
            Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " sent response to " + newclient.Address.ToString() + ":" + newclient.Port.ToString() + " for " + command);
            if (string.Compare(command.Substring(1, 6), "pairme") == 0)
            {
                //paireme
                //Console.WriteLine("Userdata: "+tmp.username+" "+tmp.gameid+" "+tmp.command+" "+tmp.mark+" "+tmp.timestamp);
                string statusLine = "HTTP/1.1 200 OK\r\n";
                string responseHeader = "Content-Type: text/html\r\nAccess-Control-Allow-Origin: *\r\n";
                if (gameid == "")
                {
                    string responseBody = "wait";
                    handler.Send(Encoding.UTF8.GetBytes(statusLine));
                    handler.Send(Encoding.UTF8.GetBytes(responseHeader));
                    handler.Send(Encoding.UTF8.GetBytes("\r\n"));
                    handler.Send(Encoding.UTF8.GetBytes(responseBody));
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
                else
                {
                    var game = gameList.Where(x => x.gameid == gameid).FirstOrDefault();
                    string oppName = (mark == "X" ? game.O : game.X);
                    string responseBody = mark + " " + oppName + " " + game.gameid;
                    handler.Send(Encoding.UTF8.GetBytes(statusLine));
                    handler.Send(Encoding.UTF8.GetBytes(responseHeader));
                    handler.Send(Encoding.UTF8.GetBytes("\r\n"));
                    handler.Send(Encoding.UTF8.GetBytes(responseBody));
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            else if (string.Compare(command.Substring(1, 6), "mymove") == 0)
            {
                //mymove
                string statusLine = "HTTP/1.1 200 OK\r\n";
                string responseHeader = "Content-Type: text/html\r\nAccess-Control-Allow-Origin: *\r\n";
                string[] splits = command.Split(new char[3]{'?', '=', '&'});
                string gameId = splits[4];
                string move = splits[6];
                lock (gameList)
                {
                    if (gameList.Exists(x => x.gameid == gameId))
                    {
                        var game = gameList.Where(x => x.gameid == gameId).FirstOrDefault();
                        for (int i = 0; i < game.board.Length; i++)
                        {
                            game.board[i] = move[i];
                            //Console.Write(game.board[i]);
                        }
                    }
                } 
                //Console.WriteLine();
                handler.Send(Encoding.UTF8.GetBytes(statusLine));
                handler.Send(Encoding.UTF8.GetBytes(responseHeader));
                handler.Send(Encoding.UTF8.GetBytes("\r\n"));
                handler.Send(Encoding.UTF8.GetBytes(""));
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            else if (string.Compare(command.Substring(1, 9), "theirmove") == 0)
            {
                //theirmove
                string statusLine = "HTTP/1.1 200 OK\r\n";
                string responseHeader = "Content-Type: text/html\r\nAccess-Control-Allow-Origin: *\r\n";
                string[] splits = command.Split(new char[3] { '?', '=', '&' });
                string gameId = splits[4];
                string move = "";
                lock (gameList)
                {
                    if(gameList.Exists(x => x.gameid == gameId)){
                        var game = gameList.Where(x => x.gameid == gameId).FirstOrDefault();
                        for (int i = 0; i < game.board.Length; i++)
                        {
                            move += game.board[i];
                        }
                    }
                }         
                //Console.WriteLine(move);
                handler.Send(Encoding.UTF8.GetBytes(statusLine));
                handler.Send(Encoding.UTF8.GetBytes(responseHeader));
                handler.Send(Encoding.UTF8.GetBytes("\r\n"));
                handler.Send(Encoding.UTF8.GetBytes(move));
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            else if (string.Compare(command.Substring(1, 4), "quit") == 0)
            {
                //quit
                string statusLine = "HTTP/1.1 200 OK\r\n";
                string responseHeader = "Content-Type: text/html\r\nAccess-Control-Allow-Origin: *\r\n";
                string[] splits = command.Split(new char[3] { '?', '=', '&' });
                string gameId = splits[4];
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
                        string oppName = (mark == "X" ? game.O : game.X);
                        gameList.Remove(game);
                    }
                } 
                handler.Send(Encoding.UTF8.GetBytes(statusLine));
                handler.Send(Encoding.UTF8.GetBytes(responseHeader));
                handler.Send(Encoding.UTF8.GetBytes("\r\n"));
                handler.Send(Encoding.UTF8.GetBytes(""));
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                break;
            }
            else
            {
                //404
                Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " 404");
            }
            Thread.Sleep(1000);
        }
        Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " closing connection with " + newclient.Address.ToString() + ":" + newclient.Port.ToString() + " and terminating");
    }

    public void checkingThread()
    {
        //Console.WriteLine("chekcing thread started");
        while (true)
        {
            Thread.Sleep(30000);
            long curTime=getTimestamp();
            //Console.WriteLine("wake now: "+curTime);
            lock (dataList)
            {
                for(int i=0; i < dataList.Count; i++)
                {
                    //Console.WriteLine("thread timestamp: " + dataList[i].timestamp);
                    if((curTime - dataList[i].timestamp) > 60)
                    {
                        dataList[i].command = "timeout000000";
                        //Console.WriteLine("timeout command:"+dataList[i].command);
                    }
                }
            }
            //Console.WriteLine("sleep");
        }
    }

    public void StartListening()
    {

        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = IPAddress.Loopback;
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8080);
        Socket listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
        Thread the = new Thread(new ThreadStart(checkingThread));
        the.Start();
        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);
            Console.WriteLine("Listening at 127.0.0.1:8080");

            while (true)
            {
                Socket handler = listener.Accept();
                string data = null;
                byte[] bytes = new Byte[1024];
                int bytesRec = handler.Receive(bytes);
                data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                string[] splits = data.Split(' ');
                if(splits.Length < 2)
                {
                    continue;
                }

                //Console.WriteLine(data);

                if (splits[1] == "/register")
                {
                    Thread th = new Thread(new ParameterizedThreadStart(register));
                    th.Start(handler);
                }
                else if (splits[1] == "/favicon.ico")
                {
                    Thread th = new Thread(new ParameterizedThreadStart(favicon));
                    th.Start(handler);
                }
                else if(splits[1].Length < 8)
                {
                    Thread th = new Thread(new ParameterizedThreadStart(Error));
                    th.Start(handler);
                }
                else if (string.Compare(splits[1].Substring(1, 6), "pairme") == 0)
                {
                    //paireme
                    string name = splits[1].Substring(15);
                    if (dataList.Exists(x => x.username == name))             //exist user
                    {
                        lock (dataList)
                        {
                            var tmp = dataList.Where(X => X.username == name).FirstOrDefault();
                            tmp.command = splits[1];
                            tmp.handler = handler;
                            tmp.timestamp = getTimestamp();
                        }
                        
                    }
                    else if (waiting.Count == 0)             //new user, the first one
                    {
                        threadData tmp = new threadData();
                        tmp.username = name;
                        tmp.command = splits[1];
                        tmp.gameid = "";
                        tmp.timestamp = getTimestamp();
                        tmp.mark = "X";
                        tmp.handler= handler;
                        lock (dataList)
                        {
                            dataList.Add(tmp);
                        }                        
                        waiting.Enqueue(name);
                        Thread th = new Thread(new ParameterizedThreadStart(playerThread));
                        th.Start(handler);
                    }
                    else                        //new user, the second one
                    {
                        threadData tmp = new threadData();
                        tmp.username = name;
                        tmp.command = splits[1];
                        string UUID = Guid.NewGuid().ToString();
                        tmp.gameid = UUID;
                        tmp.timestamp = getTimestamp();
                        tmp.mark = "O";
                        tmp.handler = handler;
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
                        Thread th = new Thread(new ParameterizedThreadStart(playerThread));
                        th.Start(handler);
                    }
                }
                else if(string.Compare(splits[1].Substring(1, 6), "mymove") == 0 || string.Compare(splits[1].Substring(1, 9), "theirmove") == 0 || string.Compare(splits[1].Substring(1, 4), "quit") == 0)
                {
                    //other command
                    string[] split = splits[1].Split(new char[3] { '?', '&', '=' });
                    string name = split[2];
                    lock (dataList)
                    {
                        if(dataList.Exists(x => x.username == name))
                        {
                            var tmpD = dataList.Where(x => x.username == name).FirstOrDefault();
                            tmpD.timestamp = getTimestamp();
                            tmpD.handler = handler;
                            tmpD.command = splits[1];
                        }
                        else
                        {
                            string statusLine = "HTTP/1.1 200 OK\r\n";
                            string responseHeader = "Content-Type: text/html\r\nAccess-Control-Allow-Origin: *\r\n";
                            string responseBody = "";
                            handler.Send(Encoding.UTF8.GetBytes(statusLine));
                            handler.Send(Encoding.UTF8.GetBytes(responseHeader));
                            handler.Send(Encoding.UTF8.GetBytes("\r\n"));
                            handler.Send(Encoding.UTF8.GetBytes(responseBody));
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                        }
                    }

                }
                else
                {
                    Thread th = new Thread(new ParameterizedThreadStart(Error));
                    th.Start(handler);
                }


                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();

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