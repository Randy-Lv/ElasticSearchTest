using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Elasticsearch.Net;
using Nest;

namespace ElasticSearchTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var node = new Uri("ElasticUrl");

            var connectionPool = new SniffingConnectionPool(new[] { node });

            var settings1 = new ConnectionSettings(node).BasicAuthentication("用户名", "密码").DefaultIndex("IndexName").DisableDirectStreaming();
            //var config = new ConnectionConfiguration(connectionPool).DisableDirectStreaming().BasicAuthentication("admin", "123qwe!@#").RequestTimeout(TimeSpan.FromSeconds(60));

            var client = new ElasticClient(settings1);
            var resultad = client.Search<logs>(p => p.MatchAll());

            var settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("2017.03.08").DisableDirectStreaming();

            var client1 = new ElasticClient(settings);

            var request = new SearchRequest
            {
                From = 0,
                Size = 10,
                //Query = new TermQuery { Field = "Name", Value = "FundPayService" },
                PostFilter = new QueryContainer(new MatchAllQuery())

            };

            //var response = client1.Search<logs>(request);

            //var result = client1.Search<logs>(s => s.Query(q => q.MatchAll()));
            //IReadOnlyCollection<logs> content = result.Documents;

            //按照字段查询--或关系
            var r1 = new SearchRequest { Query = new MatchQuery { Field = "IP", Query = " BACKEND1" } };
            var result2 = client1.Search<logs>(r1);
            //var r4 =client1.Search<logs>(s => s.Query(b => b.QueryString(t1 => t1.DefaultField("IP").Query("BACKEND"))));  
           
             var filter= new SearchRequest<logs>
            {
                Query = new MatchQuery
                {
                    Field = "IP",
                    Query = "BACKEND"
                }
            };
             var result4 = client1.Search<logs>(filter);
            IReadOnlyCollection<logs> content2 = result2.Documents;

            //模糊查询1
            var r5 = client1.Search<logs>(s => s.Query(q => q.QueryString(p => p.Query("BACKEN*"))));
            var r55 = r5.Documents;
            //模糊查询2
            var filter2 = new SearchRequest<logs>
            {
                Query =new MatchQuery
                {
                    Field = "IP",
                    Query = "BACKEN"
                }
            };
            r5 = client1.Search<logs>(filter2);
            //范围查询
            //下面这段代码会查询不出任何东西。因为生成json中的field字段：Scur 是为scur。 由于elastic里面的字段是Scur。导致无法找到该字段。建议以后elastic里面的字段都为小写
            //var r6 = client1.Search<logs>(s => s.Query(q => q.Range(p=>p.Field(obj=>obj.Scur).GreaterThanOrEquals(1).LessThanOrEquals(5))));
            var r6 = client1.Search<logs>(s => s.Query(q => q.Range(p => p.Field("Scur").GreaterThanOrEquals(1).LessThanOrEquals(5))));
            var r66 = r5.Documents;

            //创建索引 索引字段必须小写
            ICreateIndexResponse icr =client1.CreateIndex("ip2");
            //建立索引连接
            var settings2 = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("ip2").DisableDirectStreaming();
            var client2 = new ElasticClient(settings2);
            IIndexResponse iir =client2.Index<logs>(new logs { IP = "F", Scur = "2" });
            //查询所有index数据
             var r7 =client2.Search<logs>(q => q.MatchAll()).Documents;
            //对于简单结构的数据
             var r8 = client2.Search<logs>(p => p.Query(q => q.Term(f => f.Field(obj => obj.IP).Value('F'))));
             r8 = client2.Search<logs>(p => p.PostFilter(q => q.Term(f => f.Field(obj => obj.IP).Value('F'))));

            //对于有嵌套结构的数据
             r8 = client2.Search<logs>(p => p.Query(q => q.Nested(x => x.Query(k => k.Term(c => c.Field(obj => obj.IP).Value('F'))))));
            var r88=r8.Documents;

        }

        [ElasticsearchType(Name = "logs")]
        public class logs
        {
            public string @timestamp { get; set; }
            public string Status { get; set; }
            public string Stot { get; set; }
            //public string Bout { get; set; }
            public string Bin { get; set; }
            public string Smax { get; set; }
            public string IP { get; set; }

            [Text(Analyzer = "standard")]
            public string Scur { get; set; }
            public string Name { get; set; }
        }

    }
}
