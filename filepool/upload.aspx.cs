using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class filepool_upload : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        if (Request.HttpMethod == "GET")
        {
            Response.Write("{\"code\":-1,\"error\":\"this is a post method,so post a file to here.\"}");
            return;
        }
        HttpPostedFile file = this.Request.Files[0];
        if (file.ContentLength == 0)
        {
            Response.Write("{\"code\":-2,\"error\":\"file length to small.\"}");
            return;
        }
        if (file.ContentLength > 1024 * 1024 * 4)
        {
            Response.Write("{\"code\":-3,\"error\":\"file length to large.\"}");
            return;
        }
        string name = this.Request.Params["name"];
        var i = name.IndexOf('.');
        var iext = name.Substring(i).ToLower();
        string sha1 = name.Substring(0, i).ToUpper();
        name = sha1 + iext;

        byte[] fileinfo = new byte[file.ContentLength];
        int iread = file.InputStream.Read(fileinfo, 0, fileinfo.Length);

        string sha1check = calcHash(fileinfo);
        if (sha1check != sha1)
        {
            Response.Write("{\"code\":-4,\"error\":\"file sha1 not match.\"}");
            return;
        }
        //七牛配置
        Qiniu.Conf.Config.ACCESS_KEY = "Lw8bPXoadiuwwb99k2zJ8PB7bJvnA25JGhaA67Mq";
        Qiniu.Conf.Config.SECRET_KEY = "Qv9V8cYgvyqWAP-hQKnx1tnuIDiq6Kt2DHChYk24";
        //上传文件
        var ms = new System.IO.MemoryStream(fileinfo);
        Qiniu.IO.IOClient target = new Qiniu.IO.IOClient();
        Qiniu.IO.PutExtra extra = new Qiniu.IO.PutExtra();

        Qiniu.RS.PutPolicy put = new Qiniu.RS.PutPolicy("voxdata", 3600);
        string token = put.Token();

        //七牛上传
        var ret = target.Put(token, name, ms, extra);
        if (qiniu2sha1(ret.Hash) != sha1)
        {
            Response.Write("{\"code\":-5,\"error\":\"upload error.\"}");
        }
        else
        {
            Response.Write("{\"code\":0,\"succ\":\"uploaded.\"}");
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
    string calcHash(byte[] bs)
    {
        SHA1 sha1 = SHA1.Create();
        var hasharray = sha1.ComputeHash(bs);
        string hash = "";
        for (var i = 0; i < hasharray.Length; i++)
        {
            hash += hasharray[i].ToString("X02");
        }
        return hash;
    }
}