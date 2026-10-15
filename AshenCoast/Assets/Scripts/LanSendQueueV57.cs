using System;
using System.Collections.Generic;
using System.Threading;

namespace AshenCoast
{
    // State can supersede older unsent state. Commands remain in their original order.
    public sealed class LanSendQueueV57:IDisposable
    {
        sealed class Entry{public string json,key;}
        readonly object gate=new object();readonly LinkedList<Entry> queue=new LinkedList<Entry>();
        readonly Dictionary<string,LinkedListNode<Entry>> latest=new Dictionary<string,LinkedListNode<Entry>>();
        readonly AutoResetEvent ready=new AutoResetEvent(false);bool closed;
        public int MaxObserved{get;private set;}public int Superseded{get;private set;}
        public int Count{get{lock(gate)return queue.Count;}}
        public bool Enqueue(string json,string key)
        {
            lock(gate)
            {
                if(closed)return false;
                if(key!=null && latest.TryGetValue(key,out var old)){queue.Remove(old);latest.Remove(key);Superseded++;}
                if(queue.Count>=256)return false;
                var node=queue.AddLast(new Entry{json=json,key=key});if(key!=null)latest[key]=node;
                MaxObserved=Math.Max(MaxObserved,queue.Count);
            }
            ready.Set();return true;
        }
        public bool TryTake(out string json)
        {
            lock(gate)
            {
                if(queue.First==null){json=null;return false;}
                var entry=queue.First.Value;queue.RemoveFirst();if(entry.key!=null)latest.Remove(entry.key);
                json=entry.json;return true;
            }
        }
        public bool Take(out string json)
        {
            while(true){if(TryTake(out json))return true;lock(gate)if(closed)return false;ready.WaitOne();}
        }
        public void Dispose(){lock(gate){closed=true;queue.Clear();latest.Clear();}ready.Set();}
    }
}
