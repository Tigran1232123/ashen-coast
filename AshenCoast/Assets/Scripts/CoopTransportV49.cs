using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace AshenCoast
{
    public sealed class LanReceivedV49
    {public int peer;public string json,error;public bool connected,disconnected;}
    // Background workers handle bytes only; every Unity object and JSON DTO is used on the main thread.
    public sealed class LanTransportV49:IDisposable
    {
        public const int DefaultPort=27849,MaxPlayers=4,MaxFrame=4*1024*1024;
        public readonly ConcurrentQueue<LanReceivedV49> inbox=new ConcurrentQueue<LanReceivedV49>();
        readonly ConcurrentDictionary<int,Link> links=new ConcurrentDictionary<int,Link>();
        TcpListener listener;Thread acceptThread;volatile bool stopped;int nextPeer;
        public bool Host{get;private set;}public int ConnectionCount=>links.Count;
        long sentBytesV57,compressedFramesV57;
        public long SentBytesV57=>Interlocked.Read(ref sentBytesV57);public long CompressedFramesV57=>Interlocked.Read(ref compressedFramesV57);
        public int CompressionThreadV57{get;private set;}public double CompressionMillisecondsV57{get;private set;}
        public int PendingFramesV57=>links.Values.Sum(link=>link.Pending);
        public int ArtificialSendDelayV57;
        public void Listen(int port,bool localTest=false)
        {
            if(listener!=null || links.Count>0)throw new InvalidOperationException("Session already started");
            Host=true;listener=new TcpListener(localTest?IPAddress.Loopback:IPAddress.Any,port);listener.Start();
            acceptThread=new Thread(Accept){IsBackground=true,Name="Ashen LAN accept"};acceptThread.Start();
        }
        void Accept()
        {
            try
            {
                while(!stopped)
                {
                    var client=listener.AcceptTcpClient();if(stopped){client.Close();break;}
                    if(links.Count>=MaxPlayers-1){client.Close();continue;}
                    Add(client,Interlocked.Increment(ref nextPeer));
                }
            }
            catch(Exception e){if(!stopped)inbox.Enqueue(new LanReceivedV49{error=e.Message});}
        }
        public void Connect(string address,int port)
        {
            if(listener!=null || links.Count>0)throw new InvalidOperationException("Session already started");Host=false;
            new Thread(()=>
            {
                TcpClient client=null;
                try{client=new TcpClient();var pending=client.BeginConnect(address,port,null,null);using(pending.AsyncWaitHandle){if(!pending.AsyncWaitHandle.WaitOne(5000))throw new IOException("Connection timed out");client.EndConnect(pending);}if(stopped){client.Close();return;}Add(client,0);}
                catch(Exception e){client?.Close();if(!stopped)inbox.Enqueue(new LanReceivedV49{error=e.Message});}
            }){IsBackground=true,Name="Ashen LAN connect"}.Start();
        }
        void Add(TcpClient client,int peer)
        {
            client.NoDelay=true;client.SendTimeout=4000;var link=new Link(this,client,peer);
            if(!links.TryAdd(peer,link)){client.Close();return;}
            inbox.Enqueue(new LanReceivedV49{peer=peer,connected=true});link.Start();
        }
        void Lost(int peer,Link link,string error)
        {
            if(links.TryRemove(peer,out var old)){old.Dispose();if(!stopped)inbox.Enqueue(new LanReceivedV49{peer=peer,disconnected=true,error=error});}
        }
        public bool Send(int peer,string json,string latestKey=null)
        {
            if(stopped || !links.TryGetValue(peer,out var link))return false;
            if(string.IsNullOrEmpty(json) || json.Length>MaxFrame)return false;
            if(link.Enqueue(json,latestKey))return true;Lost(peer,link,"Send queue full");return false;
        }
        public void Broadcast(string json,int except=-1,string latestKey=null){foreach(var peer in links.Keys)if(peer!=except)Send(peer,json,latestKey);}
        public void Drop(int peer){if(links.TryGetValue(peer,out var link))Lost(peer,link,"Disconnected");}
        public static byte[] Frame(byte[] payload)
        {
            if(payload==null || payload.Length<1 || payload.Length>MaxFrame)throw new InvalidDataException("Invalid LAN frame size");
            var frame=new byte[payload.Length+4];int size=payload.Length;frame[0]=(byte)size;frame[1]=(byte)(size>>8);frame[2]=(byte)(size>>16);frame[3]=(byte)(size>>24);Buffer.BlockCopy(payload,0,frame,4,size);return frame;
        }
        static void ReadExact(Stream stream,byte[] buffer,int length)
        {for(int at=0;at<length;){int count=stream.Read(buffer,at,length-at);if(count<=0)throw new EndOfStreamException();at+=count;}}
        public static byte[] ReadFrame(Stream stream)
        {
            var header=new byte[4];ReadExact(stream,header,4);int size=header[0]|header[1]<<8|header[2]<<16|header[3]<<24;
            if(size<1 || size>MaxFrame)throw new InvalidDataException("Invalid LAN frame size");var data=new byte[size];ReadExact(stream,data,size);return data;
        }
        public static byte[] Compress(byte[] payload)
        {using(var output=new MemoryStream()){using(var zip=new DeflateStream(output,CompressionLevel.Fastest,true))zip.Write(payload,0,payload.Length);return output.ToArray();}}
        public static byte[] Expand(byte[] payload)
        {
            using(var input=new MemoryStream(payload))using(var zip=new DeflateStream(input,CompressionMode.Decompress))using(var output=new MemoryStream())
            {var buffer=new byte[8192];for(int count;(count=zip.Read(buffer,0,buffer.Length))>0;){if(output.Length+count>MaxFrame)throw new InvalidDataException("Expanded LAN packet is too large");output.Write(buffer,0,count);}if(output.Length==0)throw new InvalidDataException("Empty LAN packet");return output.ToArray();}
        }
        public void Dispose()
        {if(stopped)return;stopped=true;try{listener?.Stop();}catch{}foreach(var link in links.Values)link.Dispose();links.Clear();}
        sealed class Link:IDisposable
        {
            readonly LanTransportV49 owner;readonly TcpClient client;readonly int peer;
            readonly LanSendQueueV57 outgoing=new LanSendQueueV57();volatile bool closed;
            public int Pending=>outgoing.Count;
            public Link(LanTransportV49 parent,TcpClient socket,int id){owner=parent;client=socket;peer=id;}
            public void Start()
            {new Thread(Read){IsBackground=true,Name="Ashen LAN receive"}.Start();new Thread(Write){IsBackground=true,Name="Ashen LAN send"}.Start();}
            public bool Enqueue(string json,string key)=>!closed && outgoing.Enqueue(json,key);
            void Read(){try{var stream=client.GetStream();while(!closed){var data=Expand(ReadFrame(stream));owner.inbox.Enqueue(new LanReceivedV49{peer=peer,json=Encoding.UTF8.GetString(data)});}}catch(Exception e){if(!closed)owner.Lost(peer,this,e.Message);}}
            void Write()
            {
                try
                {
                    var stream=client.GetStream();
                    while(outgoing.Take(out var json))
                    {
                        if(owner.ArtificialSendDelayV57>0)Thread.Sleep(owner.ArtificialSendDelayV57);
                        var bytes=Encoding.UTF8.GetBytes(json);if(bytes.Length>MaxFrame)throw new InvalidDataException("Expanded LAN packet is too large");
                        var timer=Stopwatch.StartNew();var compressed=Compress(bytes);timer.Stop();
                        owner.CompressionThreadV57=Thread.CurrentThread.ManagedThreadId;owner.CompressionMillisecondsV57+=timer.Elapsed.TotalMilliseconds;
                        Interlocked.Increment(ref owner.compressedFramesV57);
                        var frame=Frame(compressed);stream.Write(frame,0,frame.Length);Interlocked.Add(ref owner.sentBytesV57,frame.Length);
                    }
                }
                catch(Exception e){if(!closed)owner.Lost(peer,this,e.Message);}
            }

            public void Dispose(){if(closed)return;closed=true;outgoing.Dispose();client.Close();}
        }
    }
}
