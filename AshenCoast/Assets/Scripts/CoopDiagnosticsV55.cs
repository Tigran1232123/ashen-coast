using System;
using System.IO;
using UnityEngine;
namespace AshenCoast
{
    public sealed partial class CoopSessionV49
    {
        public string SyncErrorReportV55 {get;private set;}
        public void RecordSyncErrorV55(LanPacketV49 packet,Exception error)
        {
            string report="Ashen Coast "+Application.version+"\n"+DateTime.UtcNow.ToString("O")+
                "\nRole: "+(IsHost?"host":"guest")+"\nPacket: "+(packet?.kind??"decode")+
                "\nRegion: "+(packet?.region??"")+"\n"+error;
            Debug.LogError(report);
            // No packet bodies, addresses, player names, or private inventory in the report.
            foreach(var directory in new[]{Path.GetFullPath(Path.Combine(Application.dataPath,"..")),Application.persistentDataPath})
            {
                try{string path=Path.Combine(directory,"Coop-Sync-Error.txt");File.WriteAllText(path,report);SyncErrorReportV55=path;return;}
                catch(IOException){}catch(UnauthorizedAccessException){}
            }
        }
    }
}
