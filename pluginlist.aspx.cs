using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class pluginlist : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string effect = Uri.UnescapeDataString(this.Request.Url.Query);
        ////this.Response.ContentType = "application/octet-stream";//返回10进制内容
        this.Response.ContentType = "text/plain";//返回10进制内容
        this.Response.ContentEncoding = System.Text.Encoding.UTF8;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        MyJson.JsonNode_Object json = ProcessTool.RunPagePlugins(effect);
        json.ConvertToStringWithFormat(sb, 4);
        this.Response.Write(sb.ToString());


    }


}