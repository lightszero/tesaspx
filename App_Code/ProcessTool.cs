using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// ProcessTool 的摘要说明
/// </summary>
public class ProcessTool
{
    public ProcessTool()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    public static Dictionary<string, MyJson.JsonNode_Object> plugininfo = null;
    //public static MyJson.JsonNode_Array plugininfo = null;
    static void InitPluginInfo()
    {
        string jsonfilename = System.Web.HttpContext.Current.Server.MapPath("plugins/config.json");
        string json = System.IO.File.ReadAllText(jsonfilename, System.Text.Encoding.UTF8);
        plugininfo = new Dictionary<string, MyJson.JsonNode_Object>();
        MyJson.JsonNode_Array jsonplugininfo = MyJson.Parse(json).asDict()["plugins"] as MyJson.JsonNode_Array;
        foreach (MyJson.JsonNode_Object o in jsonplugininfo)
        {
            if (o.ContainsKey("pname"))
            {
                plugininfo[o["pname"].AsString()] = o;
            }
        }
    }
    //plugins page
    public static MyJson.JsonNode_Object RunPagePlugins(string effect)
    {
        MyJson.JsonNode_Object objout = new MyJson.JsonNode_Object();
        objout["effectin"] = new MyJson.JsonNode_ValueString("effect");
        //this.Response.Write("query=" + effect + "\n");

        //this.Response.Write("hello there<br>\n");

        if (effect == "?clear")
        {
            objout["effect"] = new MyJson.JsonNode_ValueString("[clear json.]");
            plugininfo = null;
        }
        if (plugininfo == null)
        {
            objout["readjson"] = new MyJson.JsonNode_ValueNumber(true);
            InitPluginInfo();
        }
        bool bTest = effect == "?test";
        {
            MyJson.JsonNode_Array op = new MyJson.JsonNode_Array();
            objout["plugins"] = op;

            foreach (MyJson.JsonNode_Object p in plugininfo.Values)
            {
                MyJson.JsonNode_Object plugin = new MyJson.JsonNode_Object();
                op.Add(plugin);
                plugin["pname"] = p["pname"];
                plugin["pversion"] = p["pversion"];
                plugin["pexe"] = p["pexe"];
                plugin["pfolder"] = p["pfolder"];
                if (bTest)
                {
                    string info;
                    bool b = ProcessTool.RunPlugin(p["pexe"].AsString(), p["pfolder"].AsString(), "<test>", out info);
                    plugin["testresult"] = new MyJson.JsonNode_ValueNumber(b);
                    plugin["testinfo"] = new MyJson.JsonNode_ValueString(info);
                }
            }
        }
        return objout;
    }

    public static MyJson.JsonNode_Object RunPageHelp(string plugin)
    {
        MyJson.JsonNode_Object objout = new MyJson.JsonNode_Object();

        if (plugininfo == null)
        {
            objout["readjson"] = new MyJson.JsonNode_ValueNumber(true);
            InitPluginInfo();
        }
        if (plugininfo.ContainsKey(plugin) == false)
        {
            objout["error"] = new MyJson.JsonNode_ValueString("插件不存在");
        }
        else
        {
            MyJson.JsonNode_Object p = plugininfo[plugin];
            string info;
            bool b = ProcessTool.RunPlugin(p["pexe"].AsString(), p["pfolder"].AsString(), "<help>", out info);
            objout["result"] = new MyJson.JsonNode_ValueNumber(b);
            objout["help"] = new MyJson.JsonNode_ValueString(info);
        }
        return objout;
    }
    public static string SaveFileForPlugin(string plugin, byte[] filebs, out string info)
    {
        if (plugininfo == null)
        {
            InitPluginInfo();
        }
        MyJson.JsonNode_Object p = plugininfo[plugin];
        info = "";
        string path = p["pfolder"].AsString();
        string exe = p["pexe"].AsString();
        string exepath = path + System.IO.Path.DirectorySeparatorChar + exe;
        if (System.IO.File.Exists(exepath) == false)
        {
            exepath = System.Web.HttpContext.Current.Server.MapPath(path) + System.IO.Path.DirectorySeparatorChar + exe;
            if (System.IO.File.Exists(exepath) == false)
            {
                exepath = null;
            }
        }
        if (exepath == null)
        {
            info = "file not found";
            return null;
        }

        path = System.IO.Path.GetDirectoryName(exepath);
        path = System.IO.Path.Combine(path, "tempin");
        if (System.IO.Directory.Exists(path) == false)
            System.IO.Directory.CreateDirectory(path);
        string sha1 = ProcessTool.CalcSha1(filebs);
        string filename = System.IO.Path.Combine(path, sha1);
        if (p.ContainsKey("tmpfile"))
            filename += "." + p["tmpfile"].AsString();
        else
            filename += ".bin";
        System.IO.File.WriteAllBytes(filename, filebs);
        return filename;
    }
    public static MyJson.JsonNode_Object RunPageParser(string plugin, string filename)
    {
        MyJson.JsonNode_Object objout = new MyJson.JsonNode_Object();
        if (plugininfo == null)
        {
            objout["readjson"] = new MyJson.JsonNode_ValueNumber(true);
            InitPluginInfo();
        }
        if (plugininfo.ContainsKey(plugin) == false)
        {
            objout["error"] = new MyJson.JsonNode_ValueString("插件不存在");
        }
        else
        {
            MyJson.JsonNode_Object p = plugininfo[plugin];
            string info;
            bool b = ProcessTool.RunPlugin(p["pexe"].AsString(), p["pfolder"].AsString(), filename, out info);
            objout["result"] = new MyJson.JsonNode_ValueNumber(b);
            objout["info"] = new MyJson.JsonNode_ValueString(info);
            int i = info.IndexOf("<return>");
            int i2 = info.IndexOf("</return>");
            if (i > 0 && i2 > 0)
            {
                objout["return"] = new MyJson.JsonNode_ValueString(info.Substring(i + 8, i2 - i - 8));
            }
        }
        return objout;
    }
    //执行插件测试协议
    public static bool RunPlugin(string exe, string path, string param, out string info)
    {
        info = "";
        string exepath = path + System.IO.Path.DirectorySeparatorChar + exe;
        if (System.IO.File.Exists(exepath) == false)
        {
            exepath = System.Web.HttpContext.Current.Server.MapPath(path) + System.IO.Path.DirectorySeparatorChar + exe;
            if (System.IO.File.Exists(exepath) == false)
            {
                exepath = null;
            }
        }
        if (exepath == null)
        {
            info = "file not found";
            return false;
        }


        info = RunProcess(path, exe, param);
        if (info.IndexOf("<return>") == 0)
            return true;
        else
            return false;
    }


    static string RunProcess(string path, string exe, string param)
    {
        System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(path + System.IO.Path.DirectorySeparatorChar + exe, param);
        info.WorkingDirectory = path;
        info.RedirectStandardError = true;
        info.RedirectStandardOutput = true;
        info.UseShellExecute = false;
        //info.RedirectStandardInput;//插件不用Input
        System.Diagnostics.Process process = System.Diagnostics.Process.Start(info);

        while (process.HasExited == false)
        {
            System.Threading.Thread.Sleep(50);
        }
        string err = process.StandardError.ReadToEnd();
        string output = process.StandardOutput.ReadToEnd();

        return string.IsNullOrEmpty(err) ? output : err;
    }

    static System.Security.Cryptography.SHA1 sha1 = System.Security.Cryptography.SHA1.Create();
    public static string CalcSha1(byte[] buf)
    {
        string hash = "";
        byte[] bhash = sha1.ComputeHash(buf);
        foreach (byte b in bhash)
        {
            hash += b.ToString("X02");
        }
        return hash;
    }

}