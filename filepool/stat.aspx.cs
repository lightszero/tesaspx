using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class filepool_stat : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string name = this.Request.Params["name"];
        if (name == null)
        {
            Response.Write("{'code':-1,'error':'need name param'}".Replace('\'', '"'));
            return;
        }
        var i = name.IndexOf('.');
        var iext = name.Substring(i).ToLower();
        string sha1 = name.Substring(0, i).ToUpper();
        name = sha1 + iext;
        //var qiniuhash = sha12qiniu(sha1);
        //七牛配置
        Qiniu.Conf.Config.ACCESS_KEY = "Lw8bPXoadiuwwb99k2zJ8PB7bJvnA25JGhaA67Mq";
        Qiniu.Conf.Config.SECRET_KEY = "Qv9V8cYgvyqWAP-hQKnx1tnuIDiq6Kt2DHChYk24";
        string defdomain = "7xn12u.com1.z0.glb.clouddn.com";

        //七牛状态获取
        Qiniu.RS.RSClient client = new Qiniu.RS.RSClient();
        var stat = client.Stat(new Qiniu.RS.EntryPath("voxdata", name));
        if (stat.OK)
        {
            var bu = Qiniu.RS.GetPolicy.MakeBaseUrl(defdomain, name);

            string rsha1 = qiniu2sha1(stat.Hash);
            string fout = "{'code':0,'match':" + (sha1 == rsha1).ToString().ToLower() + ",'filelen':" + stat.Fsize + ",'sha1':'" + qiniu2sha1(stat.Hash) + "','url':'" + bu + "'}";
            fout = fout.Replace('\'', '"');
            Response.Write(fout);
            return;
        }
        else
        {
            Response.Write("{'code':-2,'error':'file not exist。'}".Replace('\'', '"'));
            return;
        }

    }

    string sha12qiniu(string strSha1)
    {
        byte[] _out = new byte[strSha1.Length / 2 + 1];
        _out[0] = 0x16;
        for (var i = 0; i < strSha1.Length / 2; i++)
        {
            var s = strSha1.Substring(i * 2, 2);
            _out[i + 1] = byte.Parse(s, System.Globalization.NumberStyles.HexNumber);
        }

        return Convert.ToBase64String(_out).Replace('+', '-').Replace('/', '_');
    }
    string qiniu2sha1(string qiniu)
    {
        qiniu = qiniu.Replace('-', '+').Replace('_', '/');
        byte[] _out = Convert.FromBase64String(qiniu);
        string outstr = "";
        for (var i = 1; i < _out.Length; i++)
        {
            outstr += _out[i].ToString("X02");
        }
        return outstr;
    }
}