using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class Log : IDisposable
{
    protected FileStream stream;
    protected int Day;
    public string Folder;
    public Log(string folder)
    {
        Folder = folder;
    }
    ~Log()
    {
        Dispose();
    }
    protected Stream GetStream(ref DateTime now)
    {
        if (now.Day != Day)
        {
            if (stream != null)
                stream.Dispose();
            stream = null;
            Day = now.Day;
        }
        if (stream == null)
        {
            string dic = Environment.CurrentDirectory + "/log";
            if (!Directory.Exists(dic))
                Directory.CreateDirectory(dic);
            dic += "/" + Folder;
            if (!Directory.Exists(dic))
                Directory.CreateDirectory(dic);
            string name = now.Year + "-" + now.Month + "-" + now.Day;
            string path = dic + "/" + name;
            if (File.Exists(path))
            {
                stream = File.Open(path, FileMode.OpenOrCreate);
                stream.Seek(stream.Length, SeekOrigin.Begin);
            }
            else stream = File.Create(path);
        }
        return stream;
    }
    public void Write(string msg)
    {
        var now = DateTime.Now;
        Stream s = GetStream(ref now);
        string time = now.Hour + "-" + now.Minute + "-" + now.Second + "\r";
        var buf = Encoding.UTF8.GetBytes(time);
        s.Write(buf);
        buf = Encoding.UTF8.GetBytes(msg + "\r");
        s.Write(buf);
        s.Flush();
    }
    public void Dispose()
    {
        if (stream != null)
        {
            stream.Dispose();
            stream = null;
        }
    }
}