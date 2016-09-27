using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class getanyfile : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string anyfile = Uri.UnescapeDataString(this.Request.Url.Query.Substring(1));
        this.Response.ContentType = "application/octet-stream";//返回10进制内容
        this.Response.WriteFile(anyfile);
    }
}