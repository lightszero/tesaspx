using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class plugin : System.Web.UI.Page
{
    class plugincall
    {
        public string plugin = null;
        public string param = null;
        public bool IsVaild()
        {
            if (string.IsNullOrEmpty(plugin)) return false;
            if (string.IsNullOrEmpty(param)) return false;
            return true;
        }
        public static plugincall FromQuery(string query)
        {
            plugincall call = new plugincall();
            if (string.IsNullOrEmpty(query) == false)
            {
                string[] pp = query.Substring(1).Split('&', '=');
                for (int i = 0; i < pp.Length / 2; i++)
                {
                    string key = pp[i * 2];
                    string value = Uri.UnescapeDataString(pp[i * 2 + 1]);
                    if (key == "plugin")
                        call.plugin = value;
                    if (key == "param")
                        call.param = value;
                }
            }
            return call;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (this.Request.HttpMethod == "POST")
        {
            HttpPostedFile file = this.Request.Files[0];
            string plugin =this.Request.Params["plugin"];
            byte[] filebs = new byte[file.ContentLength];
            file.InputStream.Read(filebs, 0, filebs.Length);

            string info;
            string filename = ProcessTool.SaveFileForPlugin(plugin,filebs,out info);
            MyJson.JsonNode_Object json = ProcessTool.RunPageParser(plugin, filename);
            string anyfile = json["return"].AsString();
            this.Response.ContentType = "application/octet-stream";//返回10进制内容
            this.Response.WriteFile(anyfile);
        }
        else
        {
            plugincall call = plugincall.FromQuery(this.Request.Url.Query);

            this.Response.ContentType = "text/plain";//返回10进制内容
            this.Response.ContentEncoding = System.Text.Encoding.UTF8;



            System.Text.StringBuilder sb = new System.Text.StringBuilder();


            //调用对应逻辑
            MyJson.JsonNode_Object json = null;
            if (call.IsVaild())
            {
                if (call.param == "_help_")
                {
                    json = ProcessTool.RunPageHelp(call.plugin);
                }
                else
                {


                        json = ProcessTool.RunPageParser(call.plugin, call.param);


                }
            }
            else
            {
                json = new MyJson.JsonNode_Object();

                json["error"] = new MyJson.JsonNode_ValueString("需要参数 plugin 和 param");
            }

            //输出
            json.ConvertToStringWithFormat(sb, 4);
            this.Response.Write(sb.ToString());
        }



    }
}