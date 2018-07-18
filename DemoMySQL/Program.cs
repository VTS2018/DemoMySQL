using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using VTS.Excel;
using MySql.Data;
using MySql.Data.MySqlClient;

/*
 * 1.清空WordPress表数据
 * 2.清除无效数据
 * 3.删除表Post数据
 * 
 * 1.批量导入数据功能
 *      导入主分类
 *      导入主分类描述
 *      
 *      导入Post信息
 *      导入Post图片
 */

namespace DemoMySQL
{
    class Program
    {
        #region Main
        static void Main(string[] args)
        {
            Run(args);
        }
        #endregion

        #region 开始运行
        public static void Run(string[] args)
        {
            Console.WriteLine("欢迎使用Wordpress V1.0");
            Console.WriteLine("Wordpress V1.0 支持的命令集有：");
            Console.WriteLine("\t1:\t清空所有表数据");
            Console.WriteLine("\t2:\t清除无效数据");
            Console.WriteLine("\t3:\t删除表数据");

            Console.WriteLine("\t4:\t导入数据");
            Console.WriteLine("\t5:\t查看分类");
            Console.WriteLine("\t6:\t设置自动发布");
            Console.WriteLine("\texit:\t退出程序");
            // command用于存储用户的命令
            string command;
            BatchHelper bat = new BatchHelper();
            do
            {
                // 打印命令输入符
                Console.Write(">");
                // 读入用户的命令
                command = Console.ReadLine();
                switch (command)
                {
                    case "1":
                        #region 清空数据
                        Console.WriteLine("清空所有数据：" + bat.TruncateTable("wp_"));
                        #endregion
                        break;
                    case "2":
                        #region 清除无效
                        bool blInvalidData = bat.DeleteInvalidData("wp_");
                        Console.WriteLine("清除无效数据：" + blInvalidData);
                        #endregion
                        break;
                    case "3":
                        #region 删除表
                        bool blDelete = bat.DeleteTable();
                        Console.WriteLine("删除表：" + blDelete);
                        #endregion
                        break;
                    case "4":
                        #region 数据文件
                        string dataFilepath = @"C:\Users\Administrator\Desktop\VTSBlog开发\wp_data_format.xls";
                        #endregion
                        #region 导入数据

                        #region 写入分类
                        DataTable dt = ExcelFile.GetData(dataFilepath, ExcelVersion.Excel8, HDRType.Yes, false).FirstOrDefault(x => x.TableName == "主分类");
                        bool blcate = bat.BatchInsert_wp_terms(dt, "wp_", true);
                        Console.WriteLine("写入主分类：" + blcate);
                        #endregion

                        #region 写入分类描述
                        dt.Clear();
                        dt = ExcelFile.GetData(dataFilepath, ExcelVersion.Excel8, HDRType.Yes, false).FirstOrDefault(x => x.TableName == "分类描述");
                        bool bl2 = bat.BatchInsert_wp_term_taxonomy(dt, "wp_", "category");//post_tag
                        Console.WriteLine("写入分类描述：" + bl2);
                        #endregion

                        #region 写入文章
                        dt.Clear();
                        dt = ExcelFile.GetData(dataFilepath, ExcelVersion.Excel8, HDRType.Yes, false).FirstOrDefault(x => x.TableName == "文章");
                        bool blPost = bat.BatchInsert_wp_posts(dt, "wp_");
                        Console.WriteLine("写入文章：" + blPost);
                        #endregion

                        #region 写入文章关系
                        dt.Clear();
                        dt = ExcelFile.GetData(dataFilepath, ExcelVersion.Excel8, HDRType.Yes, false).FirstOrDefault(x => x.TableName == "文章分类关系");
                        bool bl3 = bat.BatchInsert_wp_term_relationships(dt, "wp_");
                        Console.WriteLine("写入文章分类关系：" + bl3);
                        #endregion

                        #region 写入文章图片
                        dt.Clear();
                        dt = ExcelFile.GetData(dataFilepath, ExcelVersion.Excel8, HDRType.Yes, false).FirstOrDefault(x => x.TableName == "文章图片");
                        bool blimg = bat.BatchInsert_wp_postmeta(dt, "wp_");
                        Console.WriteLine("写入文章图片：" + blimg);
                        #endregion

                        #region 更新分类
                        bool blUpdateCateCount = bat.UpdateTermsCount("wp_", "category");
                        Console.WriteLine("更新分类统计：" + blUpdateCateCount);
                        #endregion

                        #region 导入标签
                        //要求：1.标签去除重复；第二：标签不能和分类同名
                        dt.Clear();
                        dt = ExcelFile.GetData(dataFilepath, ExcelVersion.Excel8, HDRType.Yes, false).FirstOrDefault(x => x.TableName == "标签库");
                        bool blTag = bat.BatchInsert_wp_terms(dt, "wp_", true);
                        Console.WriteLine("写入标签：" + blTag);
                        #endregion

                        #region 写入标签描述
                        dt.Clear();
                        dt = ExcelFile.GetData(dataFilepath, ExcelVersion.Excel8, HDRType.Yes, false).FirstOrDefault(x => x.TableName == "标签库");
                        bool blTagRelationships = bat.BatchInsert_wp_term_taxonomy(dt, "wp_", "post_tag");
                        Console.WriteLine("写入标签描述：" + blTagRelationships);
                        #endregion

                        #region 导入标签关系
                        dt.Clear();
                        dt = ExcelFile.GetData(dataFilepath, ExcelVersion.Excel8, HDRType.Yes, false).FirstOrDefault(x => x.TableName == "文章标签");
                        //清空标签
                        BatchHelper.dicTag = BatchHelper.GetTagList();

                        DataTable dtTag = bat.CreatePostTag(dt);
                        bool blPostTag = bat.BatchInsert_wp_term_relationships(dtTag, "wp_");
                        Console.WriteLine("写入文章标签关系：" + blPostTag);
                        #endregion

                        #region 更新标签
                        bool blUpdateTagCount = bat.UpdateTermsCount("wp_", "post_tag");
                        Console.WriteLine("更新标签统计：" + blUpdateTagCount);
                        #endregion

                        #endregion
                        break;
                    case "5":
                        #region 显示分类
                        MySqlDataReader reader = bat.GetCategory();
                        while (reader.Read())
                        {
                            Console.WriteLine(string.Format("{0},{1},{2}", reader["term_id"].ToString(), reader["name"].ToString(), reader["taxonomy"].ToString()));
                        }
                        reader.Close();
                        #endregion
                        break;
                    case "6":
                        #region 自动发布
                        //"yyyy-MM-dd HH:mm:ss"
                        Console.WriteLine("请输入每天更新的条数,默认5：");
                        string lineCount = Console.ReadLine();

                        Console.WriteLine("请输入发布的时间间隔,默认1440：");
                        string lineMin = Console.ReadLine();

                        Console.WriteLine("请输入起始发布时间,格式：2016-07-06 01:00:00");
                        string lineDate = Console.ReadLine();

                        bool updateTile = bat.UpdatePublishTime(int.Parse(lineCount), Convert.ToDateTime(lineDate),
                            int.Parse(lineMin), string.Empty, "select ID from wp_posts where post_type='post' order by ID asc");
                        Console.WriteLine("自动更新发布时间：" + updateTile);
                        #endregion
                        break;
                    case "7":
                        Console.WriteLine("开始生成脚本.....");
                        CreateSQL();
                        Console.WriteLine("生成脚本完成.....");
                        break;
                    default:
                        doDefault();
                        break;
                }
            } while (command != "exit");
        }
        #endregion

        #region 设置默认
        private static int doDefault()
        {
            // 打印出错信息
            Console.WriteLine("命令错误");

            // 提示正确用法
            Console.WriteLine("欢迎使用Wordpress V1.0");
            Console.WriteLine("Wordpress V1.0 支持的命令集有：");

            Console.WriteLine("\t1:\t清空所有表数据");
            Console.WriteLine("\t2:\t清除无效数据");
            Console.WriteLine("\t3:\t删除表数据");

            Console.WriteLine("\t4:\t导入数据");
            Console.WriteLine("\t5:\t查看分类");
            Console.WriteLine("\t6:\t设置自动发布");

            Console.WriteLine("\texit:\t退出程序");


            return 0;
        }
        #endregion

        #region 创建脚本
        public static void CreateSQL()
        {
            StringBuilder sbr = new StringBuilder();
            MySQLHelper.Connection = MySQLHelper.Connection.Replace("database=wordpress", "database=mydata");
            MySqlDataReader reader = MySQLHelper.ExecuteReader(MySQLHelper.Connection, "select * from oc_country");
            while (reader.Read())
            {
                //sbr.Append(string.Format("INSERT INTO [Zones]([ZoneID],[CountryID],[ZoneCode],[ZoneName]) VALUES({0}, {1}, '{2}', '{3}');", reader[0].ToString(), reader[1].ToString(), reader[3].ToString(), reader[2].ToString().Replace("'", "''")));
                sbr.Append(string.Format("INSERT INTO [Country]([CountryID],[CountryName],[ISOCode1],[ISOCode2]) VALUES({0}, '{1}', '{2}', '{3}');", reader[0].ToString(), reader[1].ToString().Replace("'", "''"), reader[2].ToString(), reader[3].ToString()));
                sbr.Append(Environment.NewLine);
            }
            VTS.Common.VTSCommon.CreateFile(sbr.ToString(), "E:\\a.sql", System.Text.Encoding.UTF8);
        } 
        #endregion
    }

    #region Batch助手==========================
    /// <summary>
    /// 
    /// </summary>
    public class BatchHelper
    {
        #region Fields
        public static Dictionary<string, int> dicTag = new Dictionary<string, int>();
        public static char[] chSplit = new char[] { ' ', ',', '，' };
        #endregion

        #region 清除数据模块

        #region TruncateTable
        //参数：表前缀
        public bool TruncateTable(string Prefix)
        {
            List<string> ls = new List<string>();

            //清空terms表
            ls.Add("truncate table " + Prefix + "terms");
            //清空terms描述
            ls.Add("truncate table " + Prefix + "term_taxonomy");

            //清空posts表
            ls.Add("truncate table " + Prefix + "posts");
            //清空postmeta表
            ls.Add("truncate table " + Prefix + "postmeta");

            //清空terms和posts关系
            ls.Add("truncate table " + Prefix + "term_relationships");
            //清空termmeta
            ls.Add("truncate table " + Prefix + "termmeta");

            //清空评论
            ls.Add("truncate table " + Prefix + "comments");
            ls.Add("truncate table " + Prefix + "commentmeta");
            return MySQLHelper.ExecuteSqlTran(MySQLHelper.Connection, ls);
        }
        #endregion

        #region 清除无效数据
        public bool DeleteInvalidData(string Prefix = "wp_")
        {
            List<string> ls = new List<string>();

            ls.Add("DELETE FROM `" + Prefix + "posts` WHERE post_type = 'revision'");
            ls.Add("DELETE FROM `" + Prefix + "posts` WHERE ping_status = 'auto-draft'");

            ls.Add("DELETE FROM `" + Prefix + "postmeta` WHERE meta_key = '_edit_last'");
            ls.Add("DELETE FROM `" + Prefix + "postmeta` WHERE meta_key = '_edit_lock'");
            ls.Add("DELETE FROM `" + Prefix + "postmeta` WHERE meta_value = '{{unknown}}'");

            return MySQLHelper.ExecuteSqlTran(MySQLHelper.Connection, ls);
        }
        #endregion

        #region 删除表Post数据
        public bool DeleteTable()
        {
            List<string> ls = new List<string>();
            ls.Add("DROP TABLE  wp_postmeta;");
            ls.Add("DROP TABLE  wp_posts;");

            ls.Add("DROP TABLE  wp_termmeta;");
            ls.Add("DROP TABLE  wp_terms;");

            ls.Add("DROP TABLE  wp_term_relationships;");
            ls.Add("DROP TABLE  wp_term_taxonomy;");
            return MySQLHelper.ExecuteSqlTran(MySQLHelper.Connection, ls);
        }
        #endregion

        #endregion

        #region 导入数据模块

        #region BatchInsert_wp_terms
        //参数：表格数据 标前缀 是否编码
        public bool BatchInsert_wp_terms(DataTable dt, string Prefix, bool blisEncode)
        {
            #region 3.0
            Hashtable hs = new Hashtable();
            int i = 0;
            foreach (DataRow dr in dt.Rows)
            {
                //INSERT INTO `wp_terms` (`term_id`, `name`, `slug`, `term_group`) VALUES(1, '未分类', 'uncategorized', 0)
                StringBuilder strSql = new StringBuilder();
                strSql.Append("INSERT INTO `" + Prefix + "terms`(");
                strSql.Append("`term_id`,`name`,`slug`,`term_group`)");
                strSql.Append(" VALUES (");
                strSql.Append("@term_id" + i + ",@name" + i + ",@slug" + i + ",@term_group" + i + ")");
                MySqlParameter[] parameters = 
                {
					new MySqlParameter("@term_id" + i, MySqlDbType.Int64,20),
					new MySqlParameter("@name" + i, MySqlDbType.VarChar,200),
					new MySqlParameter("@slug" + i, MySqlDbType.VarChar,200),
					new MySqlParameter("@term_group" + i, MySqlDbType.Int64,10)
                };
                parameters[0].Value = dr["term_id"];
                parameters[1].Value = dr["name"];
                parameters[2].Value = dr["slug"];
                if (blisEncode)
                {
                    //如果启用编码
                    parameters[2].Value = System.Web.HttpUtility.UrlEncode(dr["name"].ToString()); ;
                }
                parameters[3].Value = dr["term_group"];
                i++;
                hs.Add(strSql.ToString(), parameters);
            }
            return MySQLHelper.ExecuteSqlTran(MySQLHelper.Connection, hs);
            #endregion
        }
        #endregion

        #region BatchInsert_wp_term_taxonomy
        //参数：表数据 标前缀 类型
        public bool BatchInsert_wp_term_taxonomy(DataTable dt, string Prefix, string termType)
        {
            #region 3.0
            Hashtable hs = new Hashtable();
            int i = 0;
            foreach (DataRow dr in dt.Rows)
            {
                //INSERT INTO `wp_term_taxonomy` (`term_taxonomy_id`, `term_id`, `taxonomy`, `description`, `parent`, `count`) VALUES (1, 1, 'category', '', 0, 1),

                StringBuilder strSql = new StringBuilder();
                strSql.Append("INSERT INTO `" + Prefix + "term_taxonomy`(");
                strSql.Append("`term_taxonomy_id`, `term_id`, `taxonomy`, `description`, `parent`, `count`)");
                strSql.Append(" VALUES (");
                strSql.Append("@term_taxonomy_id" + i + ",@term_id" + i + ",@taxonomy" + i + ",@description" + i + ",@parent" + i + ",@count" + i + ")");

                MySqlParameter[] parameters = 
                {
                    new MySqlParameter("@term_taxonomy_id" + i, MySqlDbType.Int64,20),
					new MySqlParameter("@term_id" + i, MySqlDbType.Int64,20),
					new MySqlParameter("@taxonomy" + i, MySqlDbType.VarChar,32),
					new MySqlParameter("@description" + i, MySqlDbType.LongText),
					new MySqlParameter("@parent" + i, MySqlDbType.Int64,20),
					new MySqlParameter("@count" + i, MySqlDbType.Int64,20)
                };
                parameters[0].Value = dr["term_taxonomy_id"];
                parameters[1].Value = dr["term_id"];
                //parameters[2].Value = dr["taxonomy"];
                parameters[2].Value = termType;//标识是分类还是tag

                parameters[3].Value = dr["description"];
                parameters[4].Value = dr["parent"];
                parameters[5].Value = dr["count"];
                i++;
                hs.Add(strSql.ToString(), parameters);
            }
            return MySQLHelper.ExecuteSqlTran(MySQLHelper.Connection, hs);
            #endregion
        }
        #endregion

        #region BatchInsert_wp_posts
        public bool BatchInsert_wp_posts(DataTable dt, string Prefix)
        {
            #region 3.0
            Hashtable hs = new Hashtable();
            int i = 0;
            foreach (DataRow dr in dt.Rows)
            {
                //INSERT INTO `wp_posts` (`ID`, `post_author`, `post_date`, `post_date_gmt`, `post_content`, `post_title`, `post_excerpt`, `post_status`, `comment_status`, `ping_status`, `post_password`, `post_name`, `to_ping`, `pinged`, `post_modified`, `post_modified_gmt`, `post_content_filtered`, `post_parent`, `guid`, `menu_order`, `post_type`, `post_mime_type`, `comment_count`) VALUES
                StringBuilder strSql = new StringBuilder();
                strSql.Append("INSERT INTO `" + Prefix + "posts`(");
                strSql.Append("`ID`, `post_author`, `post_date`, `post_date_gmt`, `post_content`, `post_title`, `post_excerpt`, `post_status`, `comment_status`, `ping_status`, `post_password`, `post_name`, `to_ping`, `pinged`, `post_modified`, `post_modified_gmt`, `post_content_filtered`, `post_parent`, `guid`, `menu_order`, `post_type`, `post_mime_type`, `comment_count`)");
                strSql.Append(" VALUES (");
                strSql.Append(
                                 "@ID" + i +
                                ",@post_author" + i +
                                ",@post_date" + i +
                                ",@post_date_gmt" + i +

                                ",@post_content" + i +
                                ",@post_title" + i +
                                ",@post_excerpt" + i +
                                ",@post_status" + i +
                                ",@comment_status" + i +

                                ",@ping_status" + i +
                                ",@post_password" + i +
                                ",@post_name" + i +
                                ",@to_ping" + i +

                                ",@pinged" + i +
                                ",@post_modified" + i +
                                ",@post_modified_gmt" + i +
                                ",@post_content_filtered" + i +
                                ",@post_parent" + i +


                                ",@guid" + i +
                                ",@menu_order" + i +
                                ",@post_type" + i +
                                ",@post_mime_type" + i +
                                ",@comment_count" + i +

                ")");

                MySqlParameter[] parameters = 
                {
					new MySqlParameter("@ID" + i, MySqlDbType.Int64,20),
					new MySqlParameter("@post_author" + i, MySqlDbType.Int64,20),
					new MySqlParameter("@post_date" + i, MySqlDbType.DateTime),
					new MySqlParameter("@post_date_gmt" + i, MySqlDbType.DateTime),
					new MySqlParameter("@post_content" + i, MySqlDbType.LongText),

					new MySqlParameter("@post_title" + i, MySqlDbType.Text),
					new MySqlParameter("@post_excerpt" + i, MySqlDbType.Text),
					new MySqlParameter("@post_status" + i, MySqlDbType.VarChar,20),
					new MySqlParameter("@comment_status" + i, MySqlDbType.VarChar,20),
					new MySqlParameter("@ping_status" + i, MySqlDbType.VarChar,20),

					new MySqlParameter("@post_password" + i, MySqlDbType.VarChar,20),
					new MySqlParameter("@post_name" + i, MySqlDbType.VarChar,200),
					new MySqlParameter("@to_ping" + i, MySqlDbType.Text),
					new MySqlParameter("@pinged" + i, MySqlDbType.Text),
					new MySqlParameter("@post_modified" + i, MySqlDbType.DateTime),

					new MySqlParameter("@post_modified_gmt" + i, MySqlDbType.DateTime),
					new MySqlParameter("@post_content_filtered" + i, MySqlDbType.LongText),
					new MySqlParameter("@post_parent" + i, MySqlDbType.Int64,20),
					new MySqlParameter("@guid" + i, MySqlDbType.VarChar,255),//注意：域名+ID
					new MySqlParameter("@menu_order" + i, MySqlDbType.Int32,11),

					new MySqlParameter("@post_type" + i, MySqlDbType.VarChar,20),
					new MySqlParameter("@post_mime_type" + i, MySqlDbType.VarChar,100),
					new MySqlParameter("@comment_count" + i, MySqlDbType.Int64,20)
                };

                parameters[0].Value = dr["ID"];
                parameters[1].Value = dr["post_author"];
                parameters[2].Value = dr["post_date"];
                parameters[3].Value = dr["post_date_gmt"];
                parameters[4].Value = dr["post_content"];
                parameters[5].Value = dr["post_title"];
                parameters[6].Value = dr["post_excerpt"];
                parameters[7].Value = dr["post_status"];
                parameters[8].Value = dr["comment_status"];
                parameters[9].Value = dr["ping_status"];
                parameters[10].Value = dr["post_password"];

                //parameters[11].Value = dr["post_name"];//注意这个字段
                parameters[11].Value = System.Web.HttpUtility.UrlEncode(dr["post_title"].ToString());//注意这个字段

                parameters[12].Value = dr["to_ping"];
                parameters[13].Value = dr["pinged"];
                parameters[14].Value = dr["post_modified"];
                parameters[15].Value = dr["post_modified_gmt"];
                parameters[16].Value = dr["post_content_filtered"];
                parameters[17].Value = dr["post_parent"];
                parameters[18].Value = dr["guid"];
                parameters[19].Value = dr["menu_order"];
                parameters[20].Value = dr["post_type"];
                parameters[21].Value = dr["post_mime_type"];
                parameters[22].Value = dr["comment_count"];

                i++;
                hs.Add(strSql.ToString(), parameters);
            }
            return MySQLHelper.ExecuteSqlTran(MySQLHelper.Connection, hs);
            #endregion
        }
        #endregion

        #region BatchInsert_wp_term_relationships
        public bool BatchInsert_wp_term_relationships(DataTable dt, string Prefix)
        {
            #region 3.0
            Hashtable hs = new Hashtable();
            int i = 0;
            foreach (DataRow dr in dt.Rows)
            {
                //INSERT INTO `wp_term_relationships` (`object_id`, `term_taxonomy_id`, `term_order`) VALUES(1, 1, 0),
                StringBuilder strSql = new StringBuilder();
                strSql.Append("INSERT INTO `" + Prefix + "term_relationships`(");
                strSql.Append("`object_id`, `term_taxonomy_id`, `term_order`)");
                strSql.Append(" VALUES (");
                strSql.Append("@object_id" + i + ",@term_taxonomy_id" + i + ",@term_order" + i + ")");
                MySqlParameter[] parameters = 
                {
					new MySqlParameter("@object_id" + i, MySqlDbType.Int64,20),
					new MySqlParameter("@term_taxonomy_id" + i, MySqlDbType.Int64,20),
					new MySqlParameter("@term_order" + i, MySqlDbType.Int32,11)
                };
                parameters[0].Value = dr["object_id"];
                parameters[1].Value = dr["term_taxonomy_id"];
                parameters[2].Value = dr["term_order"];
                i++;
                hs.Add(strSql.ToString(), parameters);
            }
            return MySQLHelper.ExecuteSqlTran(MySQLHelper.Connection, hs);
            #endregion
        }
        #endregion

        #region BatchInsert_wp_terms
        public bool BatchInsert_wp_postmeta(DataTable dt, string Prefix)
        {
            int i = 1;
            //INSERT INTO `wp_postmeta` (`meta_id`, `post_id`, `meta_key`, `meta_value`) VALUES(1, 272, '_foldername', '027-xianren/1001'),
            List<string> ls = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                string json = new ImageInfo()
                {
                    _foldername = dr["_foldername"].ToString(),
                    _mainimg = dr["_mainimg"].ToString(),
                    _detailimg = dr["_detailimg"].ToString()
                }.GetJson();
                i++;
                StringBuilder sbr = new StringBuilder();
                sbr.Append("INSERT INTO `" + Prefix + "postmeta` (`meta_id`, `post_id`, `meta_key`, `meta_value`) VALUES(");
                sbr.Append("" + i + ", " + int.Parse(dr["ID"].ToString()) + ", '_imginfo', '" + json + "')");
                ls.Add(sbr.ToString());
            }
            return MySQLHelper.ExecuteSqlTran(MySQLHelper.Connection, ls);
        }
        #endregion

        #region UpdateTermsCount
        public bool UpdateTermsCount(string Prefix = "wp_", string TermType = "category")
        {
            #region SQL
            /*
            select
            wp_term_relationships.object_id,
            wp_term_relationships.term_taxonomy_id,
            wp_term_relationships.term_order,
            count(wp_term_relationships.object_id) as postcount

            from wp_term_taxonomy 
            inner join wp_term_relationships on wp_term_taxonomy.term_id=wp_term_relationships.term_taxonomy_id 
            where wp_term_taxonomy.taxonomy='post_tag'

            group by wp_term_relationships.term_taxonomy_id

            count(`object_id`) as postcount, 
            wp_term_relationships.`term_taxonomy_id`
             */
            #endregion

            //string cmdText = "SELECT count(`object_id`) as postcount, `term_taxonomy_id` FROM `" + Prefix + "term_relationships`group by `term_taxonomy_id`";
            string cmdText = "select count(" + Prefix + "term_relationships.object_id) as postcount," + Prefix + "term_relationships.`term_taxonomy_id` from " + Prefix + "term_taxonomy inner join " + Prefix + "term_relationships on " + Prefix + "term_taxonomy.term_id=" + Prefix + "term_relationships.term_taxonomy_id where " + Prefix + "term_taxonomy.taxonomy='" + TermType + "' group by " + Prefix + "term_relationships.term_taxonomy_id order by " + Prefix + "term_relationships.`term_taxonomy_id` asc";
            DataTable dt = MySQLHelper.ExecuteDataTable(MySQLHelper.Connection, cmdText);
            List<string> ls = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                ls.Add("update `" + Prefix + "term_taxonomy` set count=" + dr["postcount"].ToString() + " where term_id=" + dr["term_taxonomy_id"].ToString());
            }
            return MySQLHelper.ExecuteSqlTran(MySQLHelper.Connection, ls);
        }
        #endregion

        #endregion

        #region WordPressTest
        public static void WordPressTest()
        {
            string myconn = "server=45.34.1.66;database=wpcom1_wordpress;CharSet=utf8;pooling=false;port=2083;UId=wpcom1_wpcom1;Pwd='b~^~KG)DoHNy';Allow Zero Datetime=True";

            //string myconn = "server=localhost;database=wordpress;CharSet=utf8;pooling=false;port=3306;UId=root;Pwd='';Allow Zero Datetime=True";
            //string myconn = "Database='wordpress';Data Source=localhost;User ID=root;Password='';CharSet=utf8;Convert Zero Datetime=True;Allow Zero Datetime=True;";

            //需要执行的SQL语句
            string mysql = "SELECT * from wp_posts";

            //创建数据库连接
            MySqlConnection myconnection = new MySqlConnection(myconn);
            myconnection.Open();

            //创建MySqlCommand对象
            MySqlCommand mycommand = new MySqlCommand(mysql, myconnection);

            //通过MySqlCommand的ExecuteReader()方法构造DataReader对象
            MySqlDataReader myreader = mycommand.ExecuteReader();

            int col = myreader.FieldCount;
            while (myreader.Read())
            {
                //Console.WriteLine(myreader.GetInt32(0) + "," + myreader.GetString(1) + "," + myreader.GetString(2));
                //Console.WriteLine(myreader["post_title"].ToString());
                for (int i = 0; i < col; i++)
                {
                    //Console.WriteLine((myreader.GetString(i) == null).ToString());
                    Console.WriteLine(myreader.GetString(i) + ",");
                }
            }
            myreader.Close();
            myconnection.Close();
        }
        #endregion

        #region 辅助函数

        #region GetTag
        public static Dictionary<string, int> GetTagList()
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            string cmdText = "SELECT `wp_terms`.term_id,`name`,`taxonomy` FROM `wp_terms` inner join `wp_term_taxonomy` on `wp_terms`.term_id=`wp_term_taxonomy`.term_id and `wp_term_taxonomy`.taxonomy='post_tag' order by `wp_terms`.term_id asc";
            MySqlDataReader reader = MySQLHelper.ExecuteReader(MySQLHelper.Connection, cmdText);
            while (reader.Read())
            {
                dic.Add(reader["name"].ToString(), int.Parse(reader["term_id"].ToString()));
            }
            return dic;
        }
        #endregion

        #region GetCategory
        public MySqlDataReader GetCategory()
        {
            string cmdText = "SELECT `wp_terms`.term_id,`name`,`taxonomy` FROM `wp_terms` inner join `wp_term_taxonomy` on `wp_terms`.term_id=`wp_term_taxonomy`.term_id and `wp_term_taxonomy`.taxonomy='category' order by `wp_terms`.term_id asc";
            MySqlDataReader reader = MySQLHelper.ExecuteReader(MySQLHelper.Connection, cmdText);
            return reader;
        }
        #endregion

        #region CreatePostTag
        public DataTable CreatePostTag(DataTable dtTag)
        {
            DataTable dt = new DataTable();

            //根据文章标签库 生成文章标签关系
            DataColumn dc1 = new DataColumn("object_id", System.Type.GetType("System.String"));
            DataColumn dc2 = new DataColumn("term_taxonomy_id", System.Type.GetType("System.String"));
            DataColumn dc3 = new DataColumn("term_order", System.Type.GetType("System.String"));

            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);
            dt.Columns.Add(dc3);

            //变量标签库
            foreach (DataRow item in dtTag.Rows)
            {
                //文章IF
                string ID = item["ID"].ToString();
                //标签
                string Tag = item["Tag"].ToString();
                //标签分组
                string[] arr = CalcTagIDList(Tag);

                if (arr != null)
                {
                    foreach (string strTag in arr)
                    {
                        DataRow dr = dt.NewRow();

                        //获得标签ID
                        int tagID = GetTagID(strTag);

                        //文章ID 标签ID 排序
                        dr.ItemArray = new object[] { item["ID"], tagID, "0" };

                        Console.WriteLine(string.Format("{0},{1},{2}", item["ID"].ToString(), tagID, "0"));

                        dt.Rows.Add(dr);

                    }
                }
            }

            return dt;
        }
        #endregion

        #region GetTagID
        public int GetTagID(string input)
        {
            if (dicTag.ContainsKey(input))
            {
                return dicTag[input];
            }
            return 1;
        }
        #endregion

        #region 计算标签
        //计算标签的ID
        public string[] CalcTagIDList(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                //测试数据 brooke,banner,欧美,av,女优,美女,写真,封面,
                string[] arr = input.Split(chSplit, StringSplitOptions.RemoveEmptyEntries);
                return arr;
            }
            return null;
        }
        #endregion

        #region SelectPostIDList
        /// <summary>
        /// 获取自定义的PostID
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public List<int> SelectPostIDList(string cmdText)
        {
            List<int> ls = new List<int>();
            MySqlDataReader reader = MySQLHelper.ExecuteReader(MySQLHelper.Connection, cmdText);
            while (reader.Read())
            {
                ls.Add(int.Parse(reader["id"].ToString()));
            }
            reader.Close();
            return ls;
        }
        #endregion

        #region UpdatePublishTime
        public bool UpdatePublishTime(int size, DateTime start, double Interval, string type, string sql)
        {
            List<int> ls = SelectPostIDList(sql);
            Dictionary<int, string> dic = PublishData.GenerateTime(size, start, Interval, ls);
            List<string> lsSQL = new List<string>();

            foreach (var item in dic)
            {
                lsSQL.Add(string.Format("update wp_posts set post_date='{0}',post_status='{1}' where ID={2}", item.Value, "future", item.Key));
            }
            //更新为future字段
            lsSQL.Add("update wp_posts set post_status='future'");
            return MySQLHelper.ExecuteSqlTran(MySQLHelper.Connection, lsSQL);
        }
        #endregion

        #endregion
    }
    #endregion

    #region MySQLHelper========================
    public class MySQLHelper
    {
        #region =============================链接字符串===============================
        public static string Connection = "server=localhost;database=wordpress;CharSet=utf8;pooling=false;port=3306;UId=root;Pwd='';Allow Zero Datetime=True";
        #endregion

        #region =============================MakeCommand==============================

        /// <summary>
        /// 创建Command命令
        /// </summary>
        /// <param name="conn">数据连接</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="cmdText">SQL语句或存储过程名</param>
        /// <param name="prams">参数级</param>
        /// <returns></returns>
        private static MySqlCommand MakeCommand(MySqlConnection conn, MySqlTransaction trans, CommandType cmdType, string cmdText, MySqlParameter[] prams)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            if (trans != null)
            {
                cmd.Transaction = trans;
            }
            cmd.CommandType = cmdType;

            if (prams != null)
            {
                foreach (MySqlParameter p in prams)
                {
                    cmd.Parameters.Add(p);
                }
            }
            return cmd;
        }

        #endregion

        #region =============================ExecuteReader============================

        /// <summary>
        /// 返回 SQLiteDataReader
        /// </summary>
        /// <param name="cmdText">SQL语句或存储过程名</param>
        /// <returns>SQLiteDataReader</returns>
        public static MySqlDataReader ExecuteReader(string connectionString, string cmdText)
        {
            return ExecuteReader(connectionString, CommandType.Text, cmdText);
        }

        /// <summary>
        /// 返回 SQLiteDataReader
        /// </summary>
        /// <param name="cmdType">命令类型</param>
        /// <param name="cmdText">SQL语句或存储过程名</param>
        /// <returns>SQLiteDataReader</returns>
        public static MySqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText)
        {
            return ExecuteReader(connectionString, cmdType, cmdText, null);
        }

        /// <summary>
        /// 返回 SQLiteDataReader
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="prams"></param>
        /// <returns></returns>
        public static MySqlDataReader ExecuteReader(string connectionString, string cmdText, MySqlParameter[] prams)
        {
            return ExecuteReader(connectionString, CommandType.Text, cmdText, prams);
        }

        /// <summary>
        /// 返回 MySqlDataReader
        /// </summary>
        /// <param name="cmdType">命令类型</param>
        /// <param name="cmdText">SQL语句或存储过程名</param>
        /// <param name="prams">参数组</param>
        /// <returns></returns>
        public static MySqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, MySqlParameter[] prams)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            MySqlCommand cmd = MakeCommand(conn, null, cmdType, cmdText, prams);
            MySqlDataReader read = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Parameters.Clear();
            return read;
        }

        #endregion

        #region =============================ExecuteDataTable=========================

        public static DataTable ExecuteDataTable(string connectionString, string cmdText)
        {
            return ExecuteDataTable(connectionString, CommandType.Text, cmdText, null);
        }

        public static DataTable ExecuteDataTable(string connectionString, CommandType cmdType, string cmdText)
        {
            return ExecuteDataTable(connectionString, cmdType, cmdText, null);
        }

        public static DataTable ExecuteDataTable(string connectionString, CommandType cmdType, string cmdText, MySqlParameter[] prams)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = MakeCommand(conn, null, cmdType, cmdText, prams);
                MySqlDataAdapter apt = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                apt.Fill(dt);
                cmd.Parameters.Clear();
                return dt;
            }
        }
        #endregion

        #region =============================ExecuteSqlTran===========================

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="connectionString">连接对象</param>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句,value是该语句的SQLiteParameter[]）</param>
        /// <returns></returns>
        public static bool ExecuteSqlTran(string connectionString, Hashtable SQLStringList)
        {
            bool bl = false;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Value;
                            MySqlCommand cmd = MakeCommand(conn, trans, CommandType.Text, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                        bl = true;
                    }
                    catch (Exception ex)
                    {
                        bl = false;
                        trans.Rollback();
                        throw ex;
                    }
                }
            }
            return bl;
        }

        public static bool ExecuteSqlTran(string connectionString, List<string> SQLStringList)
        {
            bool bl = false;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in SQLStringList)
                        {
                            //MySqlCommand cmd = MakeCommand(conn, null, CommandType.Text, item, null);
                            MySqlCommand cmd = MakeCommand(conn, trans, CommandType.Text, item, null);
                            int i = cmd.ExecuteNonQuery();
                        }
                        trans.Commit();
                        bl = true;
                    }
                    catch (Exception ex)
                    {
                        bl = false;
                        trans.Rollback();
                        throw ex;
                    }
                }
            }
            return bl;
        }

        #endregion
    }
    #endregion

    #region PublishData 自动发布===============
    public class PublishData
    {
        #region GenerateTime
        //说明：Y 生成文章的时间发布列表
        //每天发布的条数 起始时间 时间间隔 PostID列表
        public static Dictionary<int, string> GenerateTime(int size, DateTime start, double Interval, List<int> lsPostID)
        {
            //获得所有的ID【不是隐藏状态的】
            //List<int> lsPostID = instance.GetPostIDList(" Status=1  ORDER BY PostID ASC ");

            //系统文章总条数
            //修复不在从缓存中读取 这样不正确
            //int count = CachePostIDList().Count;
            int count = lsPostID.Count;

            //ID 字段值表
            Dictionary<int, string> dict = new Dictionary<int, string>();

            //时间段列表
            List<string> ls = GenerateInterval(count, size, start, Interval);

            for (int i = 1; i <= count; i++)//追加
            {
                int n = i / size;
                if (n == ls.Count)
                {
                    n = n - 1;
                }
                //是按照正序  还是倒叙 使用条数去索引时间段  i / size 表示该文章应该在哪个事件段里面
                //dict.Add(CachePostIDList()[i - 1], ls[n]);
                dict.Add(lsPostID[i - 1], ls[n]);
            }
            return dict;
        }
        #endregion

        #region GenerateInterval
        //说明：Y 文章总数 每天发布条数 起始时间  间隔分钟数
        public static List<string> GenerateInterval(int count, int size, DateTime start, double Interval)
        {
            //算法分析：使用文章的总条数除以每天发布的条数=多少个事件段，然后我们在时间段里面 添加时间
            List<string> ls = new List<string>();

            int Section = 0;//表示应该生成多少个时间段
            if (count % size == 0)//表示等于0 除尽了
            {
                Section = (count / size);
            }
            else//表示有余数
            {
                Section = (count / size) + 1;
            }

            //生成指定段数的时间间隔
            if (Section > 0)
            {
                for (int i = 1; i <= Section; i++)
                {
                    if (i == 1)//如果是第一次 就采取当前的起始时间
                    {
                        ls.Add(start.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    else
                    {
                        ls.Add(start.AddMinutes(Interval * (i - 1)).ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                }
            }
            return ls;
        }
        #endregion
    }
    #endregion

    #region ImageInfo==========================
    /// <summary>
    /// 图片信息
    /// </summary>
    public class ImageInfo
    {
        /// <summary>
        /// 文件夹
        /// </summary>
        public string _foldername { get; set; }

        /// <summary>
        /// 主图
        /// </summary>
        public string _mainimg { get; set; }

        /// <summary>
        /// 详细图
        /// </summary>
        public string _detailimg { get; set; }

        /// <summary>
        /// 获取json格式
        /// </summary>
        /// <returns></returns>
        public string GetJson()
        {
            return LitJson.JsonMapper.ToJson(this.MemberwiseClone());
        }
    }
    #endregion
}