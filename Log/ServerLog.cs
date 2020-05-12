using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class ServerLog
{
    static Log ErrorLog;
    static Log FatalErrorLog;
    static Log WarningLog;
    static Log HttpLog;
    static Log TipLog;
    public static void Initial()
    {
        ErrorLog = new Log("Error");
        FatalErrorLog = new Log("FatalError");
        WarningLog = new Log("Warning");
        HttpLog = new Log("Http");
        TipLog = new Log("Tip");
    }
    public static void Tip(string msg)
    {
        //System.Diagnostics.Debug.WriteLine("Tip:"+msg);
        TipLog.Write(msg);
    }
    public static void Error(string msg)
    {
        //System.Diagnostics.Debug.WriteLine("Error:"+msg);
        ErrorLog.Write(msg);
    }
    public static void Warning(string msg)
    {
        //System.Diagnostics.Debug.WriteLine("Warning:"+msg);
        WarningLog.Write(msg);
    }
    public static void FatalError(string msg)
    {
        //System.Diagnostics.Debug.WriteLine("FataError:"+msg);
        WarningLog.Write(msg);
    }
    public static void Http(string msg)
    {
        //System.Diagnostics.Debug.WriteLine("HTTP:"+msg);
        HttpLog.Write(msg);
    }
    public static void Dispose()
    {
        TipLog.Dispose();
        TipLog = null;
        ErrorLog.Dispose();
        ErrorLog = null;
        FatalErrorLog.Dispose();
        FatalErrorLog = null;
        WarningLog.Dispose();
        WarningLog = null;
        HttpLog.Dispose();
        HttpLog = null;
    }
}
