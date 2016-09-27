using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class filepool_data : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string user = Request.Params["user"];
        string code = Request.Params["code"];
        string func = Request.Params["func"];//find //update
        if (user == null)
        {
            Response.Write("{\"error\":\"need user param.\"}");
            return;
        }
        if (func == null)
        {
            Response.Write("{\"error\":\"need func param.\"}");
            return;
        }
        if (func == "find")//find 查找一条记录， user 必选
        {
            //如果code 不为null，可以自己制定 findproj 过滤查询结果
            //否则只能固定查询public字段
            string projstr = Request.Params["findproj"];
            if (projstr == null)
            {
                projstr = "{\"public\":1}";
            }
            string filterstr = "{\"_id\":\"" + user + "\"}";
            if (code != null)
            {
                filterstr = "{\"_id\":\"" + user + "\",\"code\":\"" + code + "\"}";
            }
            else
            {
                projstr = "{\"public\":1}";
            }
            //config mongodb.
            var mongolinkurl = "mongodb://admintest:admintest@ds044699.mlab.com:44699/freemongo";
            var mclient = new MongoDB.Driver.MongoClient(new MongoDB.Driver.MongoUrl(mongolinkurl));
            var coll = mclient.GetDatabase("freemongo").GetCollection<MongoDB.Bson.BsonDocument>("userdata");

            //find
            var bsonFilter = BsonDocument.Parse(filterstr);
            var op = new MongoDB.Driver.FindOptions<BsonDocument, BsonDocument>();
            op.Projection = BsonDocument.Parse(projstr);
            var result = coll.FindSync<BsonDocument>(bsonFilter, op);
            result.MoveNext();
            if (result.Current.Count() > 0)
            {
                var got = result.Current.First();
                Response.Write(got);
            }
            else
            {
                Response.Write("{\"_id\":\"null\"}");
            }
        }
        else if (func == "update")//修改一条记录 user code 为必选参数，updateparam为mongo的 update选项json字符串
        {
            if (code == null)
            {
                Response.Write("{\"error\":\"must have code param.\"}");
                return;

            }
            string updateoption = Request["updateparam"];
            //config mongodb.
            var mongolinkurl = "mongodb://admintest:admintest@ds044699.mlab.com:44699/freemongo";
            var mclient = new MongoDB.Driver.MongoClient(new MongoDB.Driver.MongoUrl(mongolinkurl));
            var coll = mclient.GetDatabase("freemongo").GetCollection<MongoDB.Bson.BsonDocument>("userdata");

            string filterstr = "{\"_id\":\"" + user + "\",\"code\":\"" + code + "\"}";


            var bsonFilter = BsonDocument.Parse(filterstr);
            MongoDB.Driver.UpdateDefinition<BsonDocument> op = BsonDocument.Parse(updateoption);
            try
            {
                var result = coll.UpdateOne(bsonFilter, op);
                if (result.MatchedCount > 0)
                {
                    Response.Write("{\"_id\":\"" + user + "\"}");
                    return;
                }
                else
                {
                    Response.Write("{\"_id\":\"null\"}");
                    return;
                }
            }
            catch(Exception err)
            {
                Response.Write("{\"_id\":\"null\"}");
                return;
            }
        }
        else if (func == "new")//插入一条记录，参数 user code
        {
            if (code == null)
            {
                Response.Write("{\"error\":\"must have code param.\"}");
                return;
            }
            //config mongodb.
            var mongolinkurl = "mongodb://admintest:admintest@ds044699.mlab.com:44699/freemongo";
            var mclient = new MongoDB.Driver.MongoClient(new MongoDB.Driver.MongoUrl(mongolinkurl));
            var coll = mclient.GetDatabase("freemongo").GetCollection<MongoDB.Bson.BsonDocument>("userdata");

            string doc = "{\"_id\":\"" + user + "\",\"code\":\"" + code + "\"}";
            try
            {
                coll.InsertOne(BsonDocument.Parse(doc));
                Response.Write("{\"_id\":\"" + user + "\"}");
                return;

            }
            catch (Exception err)
            {
                Response.Write("{\"_id\":\"null\"}");
                return;
            }
        }
        else
        {
            Response.Write("{\"error\":\"error func param.\"}");
            return;
        }
    }
}