using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebAppchat.Extensions;
using WebAppchat.RabbitMQ;

namespace WebAppchat.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly RabbitMQClient _rabbitMQClient;
        private readonly IHubContext<ChatHub> _chatHub;
        public ValuesController(RabbitMQClient rabbitMQClien, IHubContext<ChatHub> chatHub) {
            this._rabbitMQClient = rabbitMQClien;
            _chatHub = chatHub;
        }
          [HttpGet]
        public async Task<string> CreateMQ(string routingKey, string Msg) {
            _rabbitMQClient.PushMessage(routingKey,Msg);
            return "数据发送成功";
        }
        [HttpGet]
        public async Task<string> GetMQ(string user, string message)
        {
            _chatHub.Clients.All.SendAsync("ReceiveMessage", user + "says", message).Wait();
            return "消息已发送";
        }
        [HttpGet]
        public string GetSetValue(string value)
        {
            string result = string.Empty;
            result = Method(value);
            return result;
        }

        private static void Dgui(string vaule, float result)
        {
            //判定是否存在 括号
            if (vaule.IndexOf('(') != -1)
            {
                int eq = vaule.IndexOf('(');
                int eq1 = vaule.IndexOf('(');
                string res = vaule.Substring(eq + 1, eq1 - eq - 1);
                //从新定义字符
                vaule = vaule.Remove(eq, eq1);
                if (vaule.Length > 1) Dgui(vaule, result);
            }
            else if (vaule.IndexOf('*') != -1)
            {
                //判定是否存在 乘法
                int eq = vaule.IndexOf('*');

                //从新定义字符
                vaule = vaule.Remove(eq - 1, eq + 1);
                result += 1;
                if (vaule.Length > 1) Dgui(vaule, result);
            }
            else if (vaule.IndexOf('/') != -1)
            {
                //判定是否存在 乘法
                int eq = vaule.IndexOf('/');
                //从新定义字符
                vaule = vaule.Remove(eq - 1, eq + 1);
                result += 1;
                if (vaule.Length > 1) Dgui(vaule, result);
            }
            else if (vaule.IndexOf('+') != -1)
            {
                //判定是否存在 乘法
                int eq = vaule.IndexOf('+');
                //从新定义字符
                vaule = vaule.Remove(eq - 1, eq + 1);
                result += 1;
                if (vaule.Length > 1) Dgui(vaule, result);
            }
            else if (vaule.IndexOf('-') != -1)
            {
                //判定是否存在 乘法
                int eq = vaule.IndexOf('-');
                //从新定义字符
                vaule = vaule.Remove(eq - 1, eq + 1);
                result += 1;
                if (vaule.Length > 1) Dgui(vaule, result);
            }
        }

        private static string Method(string s)
        {
            if (Regex.IsMatch(s, "\\((.*?)\\)"))
            {
                //正则表达式获取计算具体内容字符串 1为具体表达式
                var p = Regex.Match(s, "\\((.*?)\\)").Groups[1].Value;
                s = Getstring(s, p, true);
            }
            else if (Regex.IsMatch(s, @"\d+([\*\/])\d+"))
            {
                //判定是否包含乘除
                var p = Regex.Match(s, @"\d+([\*\/])\d+").Groups[0].Value;
                s = Getstring(s, p);
            }
            else if (Regex.IsMatch(s, @"\d+([\+\-])\d+"))
            {
                //判定是否包含加减
                var p = Regex.Match(s, @"(\-?)\d+([\+\-])\d+").Groups[0].Value;
                s = Getstring(s, p);
            }
            //递归
            return Regex.IsMatch(s, @"\d+([\+\-\*\/])\d+") ? Method(s) : s;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s">原始字符串</param>
        /// <param name="e">分解后某段字符串</param>
        /// <param name="istrue"></param>
        /// <returns></returns>
        private static string Getstring(string s, string e, bool istrue = false)
        {
            //继续拆分 字符串
            //得出具体运算符
            var fh = Regex.Match(e, @"\d+([\+\-\*\/])+\d+").Groups[1].Value;
            var eq = e.IndexOf(fh) == 0 ? e.Substring(1, e.Length - 1).IndexOf(fh) : e.IndexOf(fh);
            //分割表达式得出值1
            var value1 = e.Substring(0, eq);
            //得出值二
            var value2 = e.Substring(eq + 1, e.Length - eq - 1);
            double result = 0;
            switch (fh)
            {
                case "+":
                    result = double.Parse(value1) + double.Parse(value2);
                    break;
                case "-":
                    result = double.Parse(value1) - double.Parse(value2);
                    break;
                case "*":
                    result = double.Parse(value1) * double.Parse(value2);
                    break;
                case "/":
                    result = double.Parse(value1) / double.Parse(value2);
                    break;
                default:
                    break;
            }
            s = s.Replace(istrue ? $"({e})" : e, result.ToString());
            return s;

        }
    }
}
