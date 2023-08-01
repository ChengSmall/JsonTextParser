using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace Cheng.Json
{

    #region 内部方法
    internal static class JsonFunction
    {
        /// <summary>
        /// 换行符
        /// </summary>
        internal static readonly string NewLine;
        /// <summary>
        /// 制表符
        /// </summary>
        internal const string Tab = "\t";
        /// <summary>
        /// 忽略的Json文本字符
        /// </summary>
        public static readonly char[] IgnoreList;
        /// <summary>
        /// 可以转化为数字的json字符集合
        /// </summary>
        public static readonly char[] numberChar;
        static JsonFunction()
        {
            NewLine = System.Environment.NewLine;

            IgnoreList = new char[]
            {
                ' ','\r','\n','\t','\v'
            };
            numberChar = new char[]
            {
                '1','2','3','4','5','6','7','8','9','0',
                '.','e','-','+'
            };

        }
        /// <summary>
        /// 检查字符是否为Json的忽略字符
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        internal static bool CheckIgnore(char c)
        {
            char[] cs = IgnoreList;
            for(int i = 0; i < cs.Length; i++)
            {
                if(cs[i] == c)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 检查字符是否为数组里的字符
        /// </summary>
        /// <param name="c"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        internal static bool ChectChar(char c, char[] cs)
        {
            for (int i = 0; i < cs.Length; i++)
            {
                if (cs[i] == c)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 检查忽略文本
        /// </summary>
        /// <param name="text">要检查的有实例的文本</param>
        /// <param name="beginIndex">起始索引</param>
        /// <param name="endIndex">检查后的忽略文本后第一项索引</param>
        public static void CheckNotCharText(string text, int beginIndex, out int endIndex)
        {
            int length = text.Length;

            int i = beginIndex;

            while (i < length)
            {
                if (!CheckIgnore(text[i]))
                {
                    break;
                }
                i++;
            }
            endIndex = i;
        }

        /// <summary>
        /// 判断首字符是否为某元素类型的第一个字符
        /// </summary>
        /// <param name="text">第一个字符</param>
        /// <param name="type">类型</param>
        /// <returns>是否是指定类型</returns>
        internal static bool ChectType(char text, out JsonValueType type)
        {
            if ((48 <= text && text <= 57))
            {
                type = JsonValueType.Number;
                return true;
            }

            if (text == '"')
            {
                type = JsonValueType.String;
                return true;
            }

            if (text == '[')
            {
                type = JsonValueType.List;
                return true;
            }

            if(text == '{')
            {
                type = JsonValueType.Object;
                return true;
            }

            if(text == JsonBoolean.True[0] || text == JsonBoolean.False[0])
            {
                type = JsonValueType.Boolean;
                return true;
            }

            if (text == JsonValueNullable.NullableText[0])
            {
                type = JsonValueType.Null;
                return true;
            }

            type = 0;
            return false;
        }
        /// <summary>
        /// 检查可能布尔值的元素
        /// </summary>
        /// <param name="text">检查文本</param>
        /// <param name="beginIndex">起始索引</param>
        /// <param name="value">返回的值</param>
        /// <param name="endIndex">该值向后一位的索引</param>
        /// <returns>是否成功</returns>
        public static bool CheckBoolean(string text, int beginIndex, out bool value, out int endIndex)
        {
            int length = text.Length;
            //长度
            int count = length - beginIndex;
            value = false;

            if(count < 4)
            {
                endIndex = beginIndex;
                return false;
            }

            if (text.Substring(beginIndex, 4) == JsonBoolean.True)
            {
                value = true;
                endIndex = beginIndex + JsonBoolean.True.Length;
                return true;
            }

            if (count < 5)
            {
                endIndex = beginIndex;
                return false;
            }

            if (text.Substring(beginIndex, 5) == JsonBoolean.False)
            {
                endIndex = beginIndex + JsonBoolean.False.Length;
                return true;
            }          
            endIndex = beginIndex;
            return false;
        }
        /// <summary>
        /// 检查可能是空值的元素
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="beginIndex">起始索引</param>
        /// <param name="endIndex">文本读取的后一位索引</param>
        /// <returns>是否为空，true表示是空</returns>
        public static bool CheckNull(string text, int beginIndex, out int endIndex)
        {
            int length = text.Length;
            //长度
            int count = length - beginIndex;
            
            if(count < 4)
            {
                endIndex = beginIndex;
                return false;
            }

            var nullstr = JsonValueNullable.NullableText;

            if(text.Substring(beginIndex, 4) == nullstr)
            {
                endIndex = beginIndex + nullstr.Length;
                return true;
            }
            endIndex = beginIndex;
            return false;
        }
        /// <summary>
        /// 检查字符串并返回
        /// </summary>
        /// <param name="beginIndex">检查起始索引</param>
        /// <param name="text">检查参数，起始索引必为双引号</param>
        /// <param name="value">提取参数</param>
        /// <param name="endIndex">元素后的第一个字符索引</param>
        /// <returns>是否正确</returns>
        public static bool CheckString(string text, int beginIndex, out string value, out int endIndex)
        {
            int length = text.Length;
            value = null;

            int count = length - beginIndex;

            if (count <= 1)
            {
                value = null;
                endIndex = beginIndex;
                return false;
            }

            StringBuilder sb = new StringBuilder(count);

            int i = beginIndex + 1;
            //开始检查字符串
            while (true)
            {

                if (text[i] == '"' && text[i-1] != '\\')
                {
                    //检查到后引号
                    endIndex = i + 1;
                    break;
                }

                sb.Append(text[i]);
                i++;
            }

            value = sb.ToString();
            return true;
        }
        /// <summary>
        /// 检查数字并返回
        /// </summary>
        /// <param name="text"></param>
        /// <param name="beginIndex">起始索引</param>
        /// <param name="num">获取的数</param>
        /// <param name="endIndex">值结束的后一位索引</param>
        /// <returns>是否正确</returns>
        public static bool CheckNum(string text, int beginIndex, out decimal num, out int endIndex)
        {
            int length = text.Length;

            var nums = numberChar;

            int index = beginIndex;

            while (ChectChar(text[index], nums))
            {
                index++;
            }

            //此时数字索引范围 [beginIndex,index)

            string numstr = text.Substring(beginIndex, index - beginIndex);

            bool b = decimal.TryParse(numstr, out num);

            if (b)
            {
                endIndex = index;
                return true;
            }
            b = double.TryParse(numstr, out var dn);
            if (b)
            {
                try
                {
                    num = (decimal)dn;
                }
                catch (Exception)
                {
                    endIndex = index;
                    num = 0;
                    return false;
                }

                endIndex = index;
                return true;
            }

            endIndex = index;
            return false;

        }
        /// <summary>
        /// 向后检查集合并返回集合
        /// </summary>
        /// <param name="text">从此处向后有集合元素的文本</param>
        /// <param name="beginIndex">text起始索引</param>
        /// <param name="list">元素获取</param>
        /// <param name="endIndex">元素文本向后一位的索引</param>
        /// <returns>是否成功</returns>
        public static bool CheckList(string text, int beginIndex, out JsonList list, out int endIndex)
        {
            list = null;
            //数组分隔符
            const char fen = ',';

            int length = text.Length;

            //剩余长度
            int shenglong = length - beginIndex;

            if (shenglong <= 2)
            {
                list = null;
                endIndex = beginIndex;
                return false;
            }

            int index = beginIndex + 1;

            CheckNotCharText(text, index, out index);
            if(index >= length)
            {
                endIndex = index;
                return false;
            }
            if (text[index] == ']')
            {
                //空集合[]
                list = new JsonList(0);
                endIndex = index + 1;
                return true;
            }

            JsonList arr = new JsonList();

            bool bo;
            while (index < length)
            {
                CheckNotCharText(text, index, out index);
                if (index >= length)
                {
                    endIndex = index;
                    return false;
                }

                //判断元素
                bo = ChectType(text[index], out JsonValueType type);
                if (!bo)
                {
                    //错误
                    list = null;
                    endIndex = index;
                    return false;
                }

                if(type == JsonValueType.String)
                {
                    bo = CheckString(text, index, out var v, out index);
                    if (!bo)
                    {
                        list = null;
                        endIndex = index;
                        return false;
                    }

                    if(index >= length)
                    {
                        list = null;
                        endIndex = index;
                        return false;
                    }
                    arr.Add(v);
                    goto ChHou;
                }
                else if (type == JsonValueType.Number)
                {
                    bo = CheckNum(text, index, out var num, out index);
                    if (!bo)
                    {
                        list = null;
                        endIndex = index;
                        return false;
                    }

                    if (index >= length)
                    {
                        list = null;
                        endIndex = index;
                        return false;
                    }
                    arr.Add(num);
                    goto ChHou;
                }
                else if (type == JsonValueType.List)
                {
                    bo = CheckList(text, index, out var lit, out index);
                    if (!bo)
                    {
                        list = null;
                        endIndex = index;
                        return false;
                    }

                    if (index >= length)
                    {
                        list = null;
                        endIndex = index;
                        return false;
                    }

                    arr.Add(lit);
                    goto ChHou;
                }
                else if (type == JsonValueType.Object)
                {
                    bo = CheckDict(text, index, out var node, out index);
                    if (!bo)
                    {
                        list = null;
                        endIndex = index;
                        return false;
                    }

                    if (index >= length)
                    {
                        list = null;
                        endIndex = index;
                        return false;
                    }

                    arr.Add(node);
                    goto ChHou;
                }
                else if (type == JsonValueType.Boolean)
                {
                    bo = CheckBoolean(text, index, out bool bv, out index);
                    if (!bo)
                    {
                        list = null;
                        endIndex = index;
                        return false;
                    }

                    if (index >= length)
                    {
                        list = null;
                        endIndex = index;
                        return false;
                    }

                    arr.Add(new JsonBoolean(bv));
                    goto ChHou;
                }
                else if(type == JsonValueType.Null)
                {
                    bo = CheckNull(text, index, out index);
                    if (!bo)
                    {
                        list = null;
                        endIndex = index;
                        return false;
                    }

                    if (index >= length)
                    {
                        list = null;
                        endIndex = index;
                        return false;
                    }

                    arr.Add();
                    goto ChHou;
                }

                list = null;
                endIndex = index;
                return false;

                ChHou:
                //检查元素结尾
                CheckNotCharText(text, index, out index);
                if (index >= length)
                {
                    endIndex = index;
                    return false;
                }

                if(text[index] == fen)
                {
                    index++;
                    continue;
                }
                if(text[index] == ']')
                {
                    index++;
                    goto Out;
                }

                list = null;
                endIndex = index;
                arr.Clear();
                return false;

            }

            //未找到数组结束标识
            list = null;
            endIndex = index;
            arr.Clear();
            return false;

        Out:
            //完成

            endIndex = index;
            list = arr;
            return true;

        }
        /// <summary>
        /// 检查键值对并返回
        /// </summary>
        /// <param name="text">已确定首字符是键值对字符'{'</param>
        /// <param name="beginIndex">text的索引</param>
        /// <param name="node">获取键值对</param>
        /// <param name="endIndex">该元素json文本后的第一个索引</param>
        /// <returns>是否成功</returns>
        public static bool CheckDict(string text, int beginIndex, out JsonObject node, out int endIndex)
        {
            int length = text.Length;

            int sheng = length - beginIndex;
            node = null;

            if (sheng < 2)
            {
                endIndex = beginIndex;
                return false;
            }


            int i = beginIndex + 1;
            bool bo;
            JsonObject d = new JsonObject();
            //逗号
            const char fen = ',';
            const char kfen = ':';
            string key;

            CheckNotCharText(text, i, out i);
            if (i >= length)
            {
                endIndex = i;
                return false;
            }
            if(text[i] == '}')
            {

                node = d;
                endIndex = i + 1;
                return true;
            }

            //开始循环填充键值对
            while (i < length)
            {
                //检查忽略字符
                CheckNotCharText(text, i, out i);
                if(i >= length)
                {
                    endIndex = i;
                    return false;
                }

                //检查键 "key"
                key = null;
                if (text[i] != '"')
                {
                    endIndex = i;
                    return false;
                }

                //读取字符串
                bo = CheckString(text, i, out key, out i);
                if (!bo)
                {
                    //失败
                    endIndex = i;
                    return false;
                }
                //读取了key

                //跳过忽略字符
                CheckNotCharText(text, i, out i);
                if(i >= length)
                {
                    endIndex = i;
                    return false;
                }
                //到达可识别区域

                //读取是否冒号
                if (text[i] != kfen)
                {
                    //没有找到分隔符
                    endIndex = i;
                    return false;
                }
                //推进到元素头部
                i++;
                if(i >= length)
                {
                    //没了
                    endIndex = i;
                    return false;
                }

                //开始跳过忽略字符

                CheckNotCharText(text, i, out i);

                if(i >= length)
                {
                    endIndex = i;
                    return false;
                }
                //到达元素头部字符
                if (i >= length)
                {
                    //没了
                    endIndex = i;
                    return false;
                }
                //开始检查
                bo = ChectType(text[i], out var type);
                if (!bo)
                {
                    //不合格
                    endIndex = i;
                    return false;
                }

                //开始分辨
                if(type == JsonValueType.String)
                {
                    bo = CheckString(text, i, out var strv, out i);
                    if (!bo)
                    {
                        endIndex = i;
                        return false;
                    }
                    
                    if(i >= length)
                    {
                        endIndex = i;
                        return false;
                    }
                    //是字符串
                    d.Add(key, strv);
                    goto GetValue;
                }
                else if (type == JsonValueType.Number)
                {
                    bo = CheckNum(text, i, out var numv, out i);
                    if (!bo)
                    {
                        endIndex = i;
                        return false;
                    }

                    if (i >= length)
                    {
                        endIndex = i;
                        return false;
                    }
                    d.Add(key, numv);
                    goto GetValue;
                }
                else if (type == JsonValueType.List)
                {
                    bo = CheckList(text, i, out var arrv, out i);
                    if (!bo)
                    {
                        endIndex = i;
                        return false;
                    }
                    if (i >= length)
                    {
                        endIndex = i;
                        return false;
                    }
                    d.Add(key, arrv);
                    goto GetValue;
                }
                else if (type == JsonValueType.Object)
                {
                    bo = CheckDict(text, i, out var dv, out i);
                    if (!bo)
                    {
                        endIndex = i;
                        return false;
                    }
                    if (i >= length)
                    {
                        endIndex = i;
                        return false;
                    }
                    d.Add(key, dv);
                    goto GetValue;
                }
                else if (type == JsonValueType.Boolean)
                {
                    bo = CheckBoolean(text, i, out var bv, out i);
                    if (!bo)
                    {
                        endIndex = i;
                        return false;
                    }
                    if (i >= length)
                    {
                        endIndex = i;
                        return false;
                    }
                    d.Add(key, new JsonBoolean(bv));
                    goto GetValue;
                }
                else if (type == JsonValueType.Null)
                {
                    bo = CheckNull(text, i, out i);
                    if (!bo)
                    {
                        endIndex = i;
                        return false;
                    }
                    if (i >= length)
                    {
                        endIndex = i;
                        return false;
                    }
                    d.Add(key, new JsonValueNullable());
                    goto GetValue;
                }

                //未找到类型
                endIndex = i;
                return false;

            GetValue:
                //获取值
                //判断是否到底

                //排除无用字符
                CheckNotCharText(text, i, out i);

                if (i >= length)
                {
                    endIndex = i;
                    return false;
                }

                if (text[i] == fen)
                {
                    //到底
                    i++;
                    //循环
                    continue;
                }
                if(text[i] == '}')
                {
                    i++;
                    goto Out;
                }

            }

            //未找到集合终止符
            d.Clear();
            node = null;
            endIndex = i;
            return false;

        Out:
            //成功创建集合并填充
            node = d;
            endIndex = i;
            return true;
        }


    }
    #endregion

    #region 数据结构

    /// <summary>
    /// Json文件中的元素类型
    /// </summary>
    public enum JsonValueType : byte
    {
        /// <summary>
        /// 一个空对象
        /// </summary>
        Null = 0,
        /// <summary>
        /// 字符串类型元素
        /// </summary>
        String = 1,
        /// <summary>
        /// 数值类型元素
        /// </summary>
        Number = 2,
        /// <summary>
        /// 布尔值类型元素
        /// </summary>
        Boolean = 3,
        /// <summary>
        /// 表示键值对集合的对象元素
        /// </summary>
        Object = 4,
        /// <summary>
        /// 数组类型元素
        /// </summary>
        List = 5
    }

    #endregion

    #region 元素

    /// <summary>
    /// 表示一个Json中的一个元素
    /// <para>使用<see cref="JsonValue.JsonToObject(string)"/>将json转化为对象</para>
    /// </summary>
    public abstract class JsonValue
    {
        #region 参数
        /// <summary>
        /// 元素层级，根节点为0
        /// </summary>
        internal int m_level;
        internal JsonValue()
        {
            f_initJsonParameter();
        }
        /// <summary>
        /// 初始化参数
        /// </summary>
        protected virtual void f_initJsonParameter()
        {
            m_parantNode = null;
            m_level = 0;
        }
        /// <summary>
        /// 元素类型
        /// </summary>
        protected internal JsonValueType m_jsonType;
        /// <summary>
        /// 包含此元素的向上最近键值对节点
        /// </summary>
        internal JsonObject m_parantNode;
        #endregion

        #region 方法

        #region 参数访问
        /// <summary>
        /// 获取当前Json元素类型
        /// </summary>
        public JsonValueType JsonType => m_jsonType;
        #region 元素获取
        /// <summary>
        /// 提取元素到字符串
        /// <para>仅当<see cref="JsonType"/>为<see cref="JsonValueType.String"/>时有效</para>
        /// </summary>
        /// <exception cref="NotImplementedException"><see cref="JsonType"/>不为<see cref="JsonValueType.String"/></exception>
        public virtual string GetValueString
        {
            get
            {
                throw new NotImplementedException("当前元素不是该元素类型");
            }
        }
        /// <summary>
        /// 提取元素到值类型
        /// <para>仅当<see cref="JsonType"/>为<see cref="JsonValueType.Number"/>时有效</para>
        /// </summary>
        public virtual decimal GetValueNumber
        {
            get
            {
                throw new NotImplementedException("当前元素不是该元素类型");
            }
        }
        /// <summary>
        /// 提取元素到集合
        /// <para>仅当<see cref="JsonType"/>为<see cref="JsonValueType.List"/>时有效</para>
        /// </summary>
        public virtual IList<JsonValue> GetValueList
        {
            get
            {
                throw new NotImplementedException("当前元素不是该元素类型");
            }
        }
        /// <summary>
        /// 提取元素到键值对节点集合
        /// <para>仅当<see cref="JsonType"/>为<see cref="JsonValueType.Object"/>时有效</para>
        /// </summary>
        public virtual IDictionary<string, JsonValue> GetValueNode
        {
            get
            {
                throw new NotImplementedException("当前元素不是该元素类型");
            }
        }
        /// <summary>
        /// 提取元素到布尔值
        /// <para>仅当<see cref="JsonType"/>为<see cref="JsonValueType.Object"/>时有效</para>
        /// </summary>
        public virtual bool GetValueBoolean
        {
            get
            {
                throw new NotImplementedException("当前元素不是该元素类型");
            }
        }
        /// <summary>
        /// 当前元素是否为空元素
        /// </summary>
        public virtual bool Null
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 提取该元素的值
        /// </summary>
        public virtual object GetValue
        {
            get
            {
                var d = JsonType;

                switch (d)
                {
                    case JsonValueType.String:
                        return GetValueString;
                    case JsonValueType.Number:
                        return GetValueNumber;
                    case JsonValueType.Object:
                        return GetValueNode;
                    case JsonValueType.List:
                        return GetValueList;
                    default:
                        throw new ArgumentException();
                }

            }
        }
        #endregion

        #endregion

        #region 设置
        /// <summary>
        /// 调整元素层级
        /// </summary>
        /// <param name="lvl">元素父层级</param>
        internal virtual void f_setLevel(int lvl)
        {
            var d = JsonType;
            m_level = lvl + 1;
            if (d ==  JsonValueType.List)
            {
                var list = GetValueList;

                for(int i = 0; i < list.Count; i++)
                {
                    list[i].f_setLevel(m_level);
                }
                return;
            }
            else if (d == JsonValueType.Object)
            {
                var node = GetValueNode;
                foreach (var item in node)
                {
                    item.Value.f_setLevel(m_level);
                }
            }
        }
        #endregion

        #region 转化
        /// <summary>
        /// 将此节点内容转化为json文本格式
        /// </summary>
        /// <returns></returns>
        internal virtual string f_toJsonText()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 将此实例转化为特定的派生类型
        /// </summary>
        /// <typeparam name="T">转化的类型</typeparam>
        /// <returns>该实例的派生类型实例，若实例不是类型<typeparamref name="T"/>则返回null</returns>
        public T TransObject<T>() where T : JsonValue
        {
            return this as T;
        }
        /// <summary>
        /// 将此实例转化为特定的派生类型
        /// </summary>
        /// <typeparam name="T">转化的类型</typeparam>
        /// <param name="obj">获取该实例的派生类型实例</param>
        /// <returns>是否转化成功</returns>
        public bool TransObject<T>(out T obj) where T : JsonValue
        {
            if(this is T)
            {
                obj = (T)this;
                return true;
            }
            obj = null;
            return false;
        }
        #endregion

        #region 读写
        /// <summary>
        /// 返回此实例的Json格式字符串
        /// </summary>
        /// <returns>此实例的Json格式字符串</returns>
        public override string ToString()
        {
            return f_toJsonText();
        }

        /// <summary>
        /// 使用Json格式文本返回此实例内容
        /// </summary>
        /// <returns>Json格式文本</returns>
        public string ToJsonText()
        {
            return f_toJsonText();
        }

        /// <summary>
        /// 将指定Json文本转化为json对象
        /// </summary>
        /// <param name="jsonText">Json格式的文本</param>
        /// <returns>使用<paramref name="jsonText"/>转化的对象</returns>
        /// <exception cref="ArgumentNullException">参数为null</exception>
        /// <exception cref="JsonException">json文本格式错误，无法正常读取</exception>
        /// <exception cref="System.StackOverflowException">文本嵌套次数过多导致在递归读取时堆栈溢出</exception>
        public static JsonValue JsonToObject(string jsonText)
        {
            if (jsonText is null)
            {
                throw new ArgumentNullException("jsonText");
            }

            if (jsonText.Length == 0)
            {
                throw new JsonException("json文本不得为空");
            }

            JsonFunction.CheckNotCharText(jsonText, 0, out var index);

            if (index >= jsonText.Length)
            {
                throw new JsonException("json文本不得为空");
            }
            bool b = JsonFunction.ChectType(jsonText[index], out var t);

            if (!b)
            {
                throw new JsonException("无效json文本");
            }

            if(t == JsonValueType.Object)
            {
                b = JsonFunction.CheckDict(jsonText, index, out var node, out index);
                if (!b)
                {
                    throw new JsonException("无效json文本");
                }
                return node;
            }
            else if (t == JsonValueType.List)
            {
                b = JsonFunction.CheckList(jsonText, index, out var list, out index);
                if (!b)
                {
                    throw new JsonException("无效json文本");
                }
                return list;
            }
            else if (t == JsonValueType.Number)
            {
                b = JsonFunction.CheckNum(jsonText, index, out var num, out index);
                if (!b)
                {
                    throw new JsonException("无效json文本");
                }
                return new JsonNumber(num);
            }
            else if (t == JsonValueType.String)
            {
                b = JsonFunction.CheckString(jsonText, index, out var str, out index);
                if (!b)
                {
                    throw new JsonException("无效json文本");
                }
                return new JsonString(str);
            }
            else if (t == JsonValueType.Boolean)
            {
                b = JsonFunction.CheckBoolean(jsonText, index, out var bo, out index);
                if (!b)
                {
                    throw new JsonException("无效json文本");
                }
                return new JsonBoolean(bo);
            }
            else if (t == JsonValueType.Null)
            {
                b = JsonFunction.CheckNull(jsonText, index, out index);
                if (!b)
                {
                    throw new JsonException("无效json文本");
                }
                return new JsonValueNullable();
            }

            throw new JsonException("无效json文本");
        }
        /// <summary>
        /// 将指定Json文本转化为json对象
        /// </summary>
        /// <param name="jsonText">Json格式的文本</param>
        /// <param name="beginIndex">指定读取<paramref name="jsonText"/>文本的起始索引</param>
        /// <param name="endIndex">获取读取完毕后，json文本末尾后一位的索引</param>
        /// <returns>转化后的对象</returns>
        public static JsonValue JsonToObject(string jsonText, int beginIndex, out int endIndex)
        {
            if (jsonText is null)
            {
                throw new ArgumentNullException("jsonText");
            }

            int index = beginIndex;

            if (index >= jsonText.Length)
            {
                throw new JsonException("json文本不得为空");
            }

            JsonFunction.CheckNotCharText(jsonText, index, out index);

            if (index >= jsonText.Length)
            {
                throw new JsonException("json文本不得为空");
            }
            bool b = JsonFunction.ChectType(jsonText[index], out var t);

            if (!b)
            {
                throw new JsonException("无效json文本");
            }

            if (t == JsonValueType.Object)
            {
                b = JsonFunction.CheckDict(jsonText, index, out var node, out endIndex);
                if (!b)
                {
                    throw new JsonException("无效json文本");
                }
                return node;
            }
            else if (t == JsonValueType.List)
            {
                b = JsonFunction.CheckList(jsonText, index, out var list, out endIndex);
                if (!b)
                {
                    throw new JsonException("无效json文本");
                }
                return list;
            }
            else if (t == JsonValueType.Number)
            {
                b = JsonFunction.CheckNum(jsonText, index, out var num, out endIndex);
                if (!b)
                {
                    throw new JsonException("无效json文本");
                }
                return new JsonNumber(num);
            }
            else if (t == JsonValueType.String)
            {
                b = JsonFunction.CheckString(jsonText, index, out var str, out endIndex);
                if (!b)
                {
                    throw new JsonException("无效json文本");
                }
                return new JsonString(str);
            }
            else if (t == JsonValueType.Boolean)
            {
                b = JsonFunction.CheckBoolean(jsonText, index, out var bo, out endIndex);
                if (!b)
                {
                    throw new JsonException("无效json文本");
                }
                return new JsonBoolean(bo);
            }
            else if (t == JsonValueType.Null)
            {
                b = JsonFunction.CheckNull(jsonText, index, out endIndex);
                if (!b)
                {
                    throw new JsonException("无效json文本");
                }
                return new JsonValueNullable();
            }

            throw new JsonException("无效json文本");

        }

        /// <summary>
        /// 将指定流文本转化为json对象
        /// </summary>
        /// <param name="streamReader">读取的流文本</param>
        /// <returns>使用<paramref name="streamReader"/>读取文本转化的对象</returns>
        /// <exception cref="ArgumentNullException">参数为null</exception>
        /// <exception cref="JsonException">json文本格式错误，无法正常读取</exception>
        /// <exception cref="System.IO.IOException">IO错误</exception>
        /// <exception cref="OutOfMemoryException">没有足够的内存读取字符串文本</exception>
        /// <exception cref="System.StackOverflowException">文本嵌套次数过多导致在递归读取时堆栈溢出</exception>
        public static JsonValue StreamToObject(StreamReader streamReader)
        {
            if (streamReader is null)
            {
                throw new ArgumentNullException("streamReader");
            }

            var json = streamReader.ReadToEnd();

            return JsonToObject(json);
        }
        /// <summary>
        /// 指定路径文件中的json文本转化为对象
        /// </summary>
        /// <param name="path">读取的文件路径</param>
        /// <returns>路径文件的json文本转化的对象</returns>
        /// <exception cref="ArgumentNullException">参数为null</exception>
        /// <exception cref="JsonException">json文本格式错误，无法正常读取</exception>
        /// <exception cref="System.IO.IOException">IO错误</exception>
        /// <exception cref="OutOfMemoryException">没有足够的内存读取字符串文本</exception>
        /// <exception cref="System.IO.FileNotFoundException">无法找到文件</exception>
        /// <exception cref="ArgumentException">路径不正确</exception>
        /// <exception cref="System.NotSupportedException">path 包含一个冒号 (":")，它不是属于卷标识符 (例如，"c:\")</exception>
        /// <exception cref="System.IO.PathTooLongException">指定的路径和/或文件名超过了系统定义的最大长度</exception>
        /// <exception cref="System.StackOverflowException">文本嵌套次数过多导致在递归读取时堆栈溢出</exception>
        public static JsonValue StreamToObject(string path)
        {
            using (StreamReader streamReader = new StreamReader(Path.GetFullPath(path)))
            {
                return JsonToObject(streamReader.ReadToEnd());
            }
        }
        #endregion

        #endregion
    }

    /// <summary>
    /// 表示Json中使用花括号"{}"囊括键值对的对象元素；同时也可以当作一个Json文本的根节点
    /// </summary>
    public sealed class JsonObject : JsonValue, IDictionary<string, JsonValue>
    {
        #region 参数
        private Dictionary<string, JsonValue> m_dictionary;
        private bool m_toTextIsNewLine = false;
        #endregion

        #region 初始化
        /// <summary>
        /// 实例化一个键值对节点
        /// </summary>
        public JsonObject()
        {
            m_dictionary = new Dictionary<string, JsonValue>();
            m_level = 0;
        }
        /// <summary>
        /// 实例化一个键值对节点集合，并指定转化文本时是否换行
        /// </summary>
        /// <param name="toTextIsNoewLine">转化为文本时是否使用换行</param>
        public JsonObject(bool toTextIsNoewLine)
        {
            m_toTextIsNewLine = toTextIsNoewLine;
            m_dictionary = new Dictionary<string, JsonValue>();
            m_level = 0;
        }

        protected override void f_initJsonParameter()
        {
            base.f_initJsonParameter();
            m_jsonType = JsonValueType.Object;
        }
        #endregion

        #region 参数访问
        /// <summary>
        /// 转化为文本时是否使用换行和缩进；默认为false
        /// </summary>
        public bool ToTextIsNewLine
        {
            get => m_toTextIsNewLine;
            set
            {
                m_toTextIsNewLine = value;
            }
        }

        /// <summary>
        /// 提取元素到键值对节点集合
        /// </summary>
        public override IDictionary<string, JsonValue> GetValueNode => this;
        /// <summary>
        /// 当前节点内的键值对数量
        /// </summary>
        public int Count => m_dictionary.Count;
        ICollection<string> IDictionary<string, JsonValue>.Keys => m_dictionary.Keys;
        ICollection<JsonValue> IDictionary<string, JsonValue>.Values => m_dictionary.Values;
        public bool IsReadOnly => false;

        #endregion

        #region 键值对功能
        /// <summary>
        /// 添加一对键值对，指定值为数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentNullException">key为null</exception>
        public void Add(string key, decimal value)
        {
            JsonNumber n = new JsonNumber(value);
            Add(key, n);
        }
        /// <summary>
        /// 添加一对键值对，指定值为字符串
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentNullException">参数为null</exception>
        public void Add(string key, string value)
        {
            var n = new JsonString(value);
            Add(key, n);
        }
        private void f_add(string key, JsonValue value)
        {
            if (value is null) value = new JsonValueNullable();
            m_dictionary.Add(key, value);
            value.f_setLevel(m_level);
            value.m_parantNode = this;
        }
        private void f_set(string key, JsonValue value)
        {
            if (value is null) value = new JsonValueNullable();
            value.f_setLevel(m_level);
            value.m_parantNode = this;
            m_dictionary[key] = value;
        }
        /// <summary>
        /// 获取或设置与指定键的值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键的值，若未找到则返回一个null</returns>
        /// <exception cref="ArgumentNullException">键为null</exception>
        public JsonValue this[string key]
        {
            get
            {
                if(!m_dictionary.TryGetValue(key, out var value))
                {
                    return null;
                }
                return value;
            }
            set => f_set(key, value);
        }
        /// <summary>
        /// 获取指定值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">获取的值</param>
        /// <returns>是否找到键；若找到指定的键则返回true；若找不到则返回false，并将<paramref name="value"/>设为null</returns>
        public bool Get(string key, out JsonValue value)
        {
            return m_dictionary.TryGetValue(key, out value);
        }
        /// <summary>
        /// 查询指定键是否存在于当前键值对中
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>存在返回true；不存在返回false</returns>
        public bool ContainsKey(string key)
        {
            return m_dictionary.ContainsKey(key);
        }
        /// <summary>
        /// 添加一对键值到节点
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentNullException">key为null</exception>
        public void Add(string key, JsonValue value)
        {
            f_add(key, value);
        }
        /// <summary>
        /// 添加一对键值到节点
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentNullException">参数为null</exception>
        public void Add<T>(string key, T value) where T : JsonValue
        {
            f_add(key, value);
        }
        /// <summary>
        /// 删除指定键的一对数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns>是否删除成功</returns>
        /// <exception cref="ArgumentNullException">key为null</exception>
        public bool Remove(string key)
        {
            return m_dictionary.Remove(key);
        }
        /// <summary>
        /// 添加一个布尔值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, bool value)
        {
            var n = new JsonBoolean(value);
            Add(key, n);
        }
        /// <summary>
        /// 添加一个空值
        /// </summary>
        /// <param name="key">键</param>
        /// <exception cref="ArgumentNullException">key为null</exception>
        public void Add(string key)
        {
            f_add(key, new JsonValueNullable());
        }
        /// <summary>
        /// 给指定的键设置一个空值
        /// </summary>
        /// <param name="key">key为null</param>
        public void Set(string key)
        {
            f_set(key, new JsonValueNullable());
        }
        public bool TryGetValue(string key, out JsonValue value)
        {
            return m_dictionary.TryGetValue(key, out value);
        }

        void ICollection<KeyValuePair<string, JsonValue>>.Add(KeyValuePair<string, JsonValue> item)
        {
            f_add(item.Key, item.Value);
        }
        /// <summary>
        /// 移除当前键值对节点元素
        /// </summary>
        public void Clear()
        {
            m_dictionary.Clear();
        }
        bool ICollection<KeyValuePair<string, JsonValue>>.Contains(KeyValuePair<string, JsonValue> item)
        {
            return ((ICollection<KeyValuePair<string, JsonValue>>)m_dictionary).Contains(item);
        }

        void ICollection<KeyValuePair<string, JsonValue>>.CopyTo(KeyValuePair<string, JsonValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, JsonValue>>)m_dictionary).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, JsonValue>>.Remove(KeyValuePair<string, JsonValue> item)
        {
            return m_dictionary.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, JsonValue>> GetEnumerator()
        {
            return m_dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_dictionary.GetEnumerator();
        }
        #endregion

        #region 派生
        internal override void f_setLevel(int lvl)
        {
            m_level = lvl + 1;
            foreach (var item in m_dictionary)
            {
                item.Value.f_setLevel(m_level);
            }
        }
        internal override string f_toJsonText()
        {
            string NewLine = JsonFunction.NewLine;
            const char Tab = '\t';
            var hash = this;
            bool isNewLine = m_toTextIsNewLine;

            StringBuilder sb = new StringBuilder(hash.Count);


            int i;
            if (hash.Count == 0)
            {
                return "{}";
            }

            int lvlt = m_level + 1;
            
            sb.Append('{');
            if (isNewLine)
            {
                sb.Append(NewLine);

                for (i = 0; i < lvlt; i++)
                {
                    sb.Append(Tab);
                }
            }

            int count = hash.Count - 1;

            i = 0;
            foreach (var item in hash)
            {
                sb.Append("\"" + item.Key + "\"");
                sb.Append(":");
                var value = item.Value;
                sb.Append(value.f_toJsonText());

                if (i != count)
                {
                    sb.Append(',');
                }
                if (isNewLine)
                {
                    sb.Append(NewLine);
                    if (i != count)
                    {
                        lvlt = m_level + 1;
                    }
                    else
                    {
                        lvlt = m_level;
                    }

                    for (int j = 0; j < lvlt; j++)
                    {
                        sb.Append(Tab);
                    }

                }

                i++;
            }

            sb.Append('}');

            return sb.ToString();

        }
        #endregion

        #region 节点功能
        /// <summary>
        /// 返回以Json格式的键值对集合文本
        /// </summary>
        /// <returns>Json文件文本</returns>
        public override string ToString()
        {
            return f_toJsonText();
        }
        #endregion

    }

    /// <summary>
    /// 表示Json中使用中括号"[]"的集合元素
    /// </summary>
    public sealed class JsonList : JsonValue, IList<JsonValue>
    {
        #region 参数
        private List<JsonValue> m_list;
        #endregion

        #region 初始化
        /// <summary>
        /// 初始化Json数组元素
        /// </summary>
        public JsonList()
        {
            m_list = new List<JsonValue>();
        }
        /// <summary>
        /// 初始化Json数组元素，指定初始容量
        /// </summary>
        /// <param name="capactiy">集合初始容量</param>
        /// <exception cref="ArgumentOutOfRangeException">参数小于0</exception>
        public JsonList(int capactiy)
        {
            m_list = new List<JsonValue>(capactiy);
        }
        /// <summary>
        /// 使用集合初始化Json数组元素
        /// </summary>
        /// <param name="arr">集合</param>
        /// <exception cref="ArgumentNullException">参数为null</exception>
        public JsonList(IEnumerable<JsonValue> arr)
        {
            m_list = new List<JsonValue>(arr);
        }
        #endregion

        #region Json派生

        public override object GetValue => this;
        public override IList<JsonValue> GetValueList => this;

        internal override void f_setLevel(int lvl)
        {
            m_level = lvl + 1;
            var list = m_list;
            int count = list.Count;
            for(int i = 0; i < count; i++)
            {
                list[i].f_setLevel(m_level);
            }
        }

        internal override string f_toJsonText()
        {
            var list = m_list;

            int count = list.Count;

            StringBuilder sb = new StringBuilder(count);

            sb.Append('[');

            count -= 1;
            for(int i = 0; i <= count; i++)
            {
                sb.Append(list[i].f_toJsonText());

                if (i != count) sb.Append(',');
            }
            sb.Append(']');

            return sb.ToString();
        }

        protected override void f_initJsonParameter()
        {
            base.f_initJsonParameter();
            m_jsonType = JsonValueType.List;
        }
        #endregion

        #region List功能
        /// <summary>
        /// 设置索引为新值
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentNullException">值为null</exception>
        /// <exception cref="ArgumentOutOfRangeException">索引超出范围</exception>
        public void Set(int index, string value)
        {
            if (index < 0 || index >= m_list.Count) throw new ArgumentOutOfRangeException("index");
            f_set(new JsonString(value), index);
        }
        /// <summary>
        /// 设置索引为新值
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentOutOfRangeException">索引超出范围</exception>
        public void Set(int index, decimal value)
        {
            if (index < 0 || index >= m_list.Count) throw new ArgumentOutOfRangeException("index");
            f_set(new JsonNumber(value), index);
        }
        /// <summary>
        /// 设置索引为新值
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentOutOfRangeException">索引超出范围</exception>
        public void Set(int index, bool value)
        {
            if (index < 0 || index >= m_list.Count) throw new ArgumentOutOfRangeException("index");
            f_set(new JsonBoolean(value), index);
        }

        /// <summary>
        /// 添加一个布尔值
        /// </summary>
        /// <param name="value">布尔值</param>
        public void Add(bool value)
        {
            f_add(new JsonBoolean(value));
        }
        /// <summary>
        /// 向后添加一个元素
        /// </summary>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentNullException">参数为null</exception>
        public void Add(string value)
        {
            JsonString v = new JsonString(value);
            Add(v);
        }
        /// <summary>
        /// 向后添加一个元素
        /// </summary>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentNullException">参数为null</exception>
        public void Add(int value)
        {
            JsonNumber v = new JsonNumber(value);
            Add(v);
        }
        /// <summary>
        /// 向后添加一个数值
        /// </summary>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentNullException">参数为null</exception>
        public void Add(decimal value)
        {
            JsonNumber v = new JsonNumber(value);
            Add(v);
        }
        /// <summary>
        /// 向后添加一个元素
        /// </summary>
        /// <typeparam name="T">派生类型</typeparam>
        /// <param name="value">元素</param>
        public void Add<T>(T value) where T : JsonValue
        {
            f_add(value);
        }

        /// <summary>
        /// 向后添加一个空元素
        /// </summary>
        public void Add()
        {
            f_add(null);
        }
        private void f_add(JsonValue value)
        {
            if(value is null) value = new JsonValueNullable();
            value.f_setLevel(m_level);
            value.m_parantNode = m_parantNode;
            m_list.Add(value);
        }
        /// <summary>
        /// <see cref="ArgumentOutOfRangeException"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        private void f_set(JsonValue value, int index)
        {
            if (value is null) value = new JsonValueNullable();
            m_list[index] = value;
            value.m_parantNode = m_parantNode;
            value.f_setLevel(m_level);
        }
        private void f_insert(JsonValue value, int index)
        {
            if (value is null) value = new JsonValueNullable();
            m_list.Insert(index, value);
            value.f_setLevel(m_level);
        }
        /// <summary>
        /// 获取集合的元素数
        /// </summary>
        public int Count => m_list.Count;
        /// <summary>
        /// 获取或设置集合的实际容量
        /// </summary>
        /// <value>设置的值不得小于<see cref="Count"/></value>
        /// <exception cref="ArgumentOutOfRangeException">值小于<see cref="Count"/></exception>
        /// <exception cref="System.OutOfMemoryException"></exception>
        public int Capactiy
        {
            get => m_list.Capacity;
            set
            {
                m_list.Capacity = value;
            }
        }
        bool ICollection<JsonValue>.IsReadOnly => false;
        /// <summary>
        /// 向后添加一个元素
        /// </summary>
        /// <param name="value">元素</param>
        public void Add(JsonValue value)
        {
            f_add(value);
        }
        /// <summary>
        /// 删除指定索引的元素
        /// </summary>
        /// <param name="index">元素</param>
        public void RemoveAt(int index)
        {
            m_list.RemoveAt(index);
        }
        /// <summary>
        /// 获取或设置指定索引的元素
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>元素</returns>
        /// <exception cref="ArgumentOutOfRangeException">索引超出范围</exception>
        public JsonValue this[int index]
        {
            get
            {
                return m_list[index];
            }
            set
            {
                f_set(value, index);
            }
        }
        /// <summary>
        /// 在指定索引插入元素
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="value">元素</param>
        /// <exception cref="ArgumentOutOfRangeException">索引超出范围</exception>
        public void Insert(int index, JsonValue value)
        {
            f_insert(value, index);
        }
        /// <summary>
        /// 在指定索引插入元素
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="num">元素</param>
        /// <exception cref="ArgumentOutOfRangeException">索引超出范围</exception>
        public void Insert(int index, decimal num)
        {
            JsonNumber value = new JsonNumber(num);
            f_insert(value, index);
        }
        /// <summary>
        /// 在指定索引插入元素
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="str">元素</param>
        /// <exception cref="ArgumentOutOfRangeException">索引超出范围</exception>
        public void Insert(int index, string str)
        {
            JsonValue value = new JsonString(str);
            f_insert(value, index);
        }

        /// <summary>
        /// 清空数组元素
        /// </summary>
        public void Clear() => m_list.Clear();
        /// <summary>
        /// 从数组中移除第一个匹配项
        /// </summary>
        /// <param name="value">匹配的元素</param>
        /// <returns>是否移除成功</returns>
        public bool Remove(JsonValue value)
        {
            return m_list.Remove(value);
        }
        public int IndexOf(JsonValue value)
        {
            return m_list.IndexOf(value);
        }
        public bool Contains(JsonValue value)
        {
            return m_list.Contains(value);
        }
        void ICollection<JsonValue>.CopyTo(JsonValue[] array, int arrayIndex)
        {
            m_list.CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// 返回一个循环访问枚举器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<JsonValue> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_list.GetEnumerator();
        }


        #endregion
    }
    /// <summary>
    /// 表示Json中的字符串元素
    /// </summary>
    public sealed class JsonString : JsonValue
    {
        private string m_value;
        /// <summary>
        /// 实例化一个空字符串Json元素
        /// </summary>
        public JsonString()
        {
            m_value = "";
        }
        /// <summary>
        /// 使用字符串实例化一个Json字符串元素
        /// </summary>
        /// <param name="value">元素内容</param>
        /// <exception cref="ArgumentNullException">参数不可为null</exception>
        public JsonString(string value)
        {
            if(value is null)
            {
                throw new ArgumentNullException("str");
            }
            m_value = value;
        }
        /// <summary>
        /// 获取或设置字符串元素
        /// </summary>
        /// <value>不可设置为null</value>
        /// <exception cref="ArgumentNullException">值设置为null</exception>
        public string Value
        {
            get => m_value;
            set
            {
                if(value is null)
                {
                    throw new ArgumentNullException("str");
                }
                m_value = value;
            }
        }

        #region 派生
        
        protected override void f_initJsonParameter()
        {
            base.f_initJsonParameter();
            m_jsonType = JsonValueType.String;
        }

        /// <summary>
        /// 比较实例的值是否相同
        /// </summary>
        /// <param name="obj">实例</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if(obj is JsonString strjson)
            {
                return m_value == strjson.m_value;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        internal override void f_setLevel(int lvl)
        {
            m_level = lvl + 1;
        }
        internal override string f_toJsonText()
        {
            return "\"" + m_value + "\"";
        }

        public override string GetValueString => m_value;
        public override object GetValue => m_value;
        /// <summary>
        /// 返回字符串元素
        /// </summary>
        /// <returns>字符串元素</returns>
        public override string ToString()
        {
            return m_value;
        }

        #endregion

    }
    /// <summary>
    /// 表示Json中的数字元素
    /// </summary>
    public sealed class JsonNumber : JsonValue
    {
        #region 参数
        private decimal m_number;
        #endregion
        /// <summary>
        /// 初始化一个数字Json元素
        /// </summary>
        public JsonNumber()
        {
            m_number = 0;
        }
        /// <summary>
        /// 初始化一个数字Json元素
        /// </summary>
        /// <param name="number">初始化值</param>
        public JsonNumber(decimal number)
        {
            m_number = number;
        }
        /// <summary>
        /// 初始化一个数字Json元素
        /// </summary>
        /// <param name="number">初始化值</param>
        public JsonNumber(int number)
        {
            m_number = number;
        }
        /// <summary>
        /// 初始化一个数字Json元素
        /// </summary>
        /// <param name="number">初始化值</param>
        public JsonNumber(float number)
        {
            m_number = (decimal)number;
        }
        /// <summary>
        /// 访问或设置其中的值
        /// </summary>
        public decimal Decimal
        {
            get => m_number;
            set => m_number = value;
        }

        #region 派生
        protected override void f_initJsonParameter()
        {
            base.f_initJsonParameter();
            m_jsonType = JsonValueType.Number;
        }
        /// <summary>
        /// 提取数字对象
        /// </summary>
        public override object GetValue => m_number;

        internal override string f_toJsonText()
        {
            return m_number.ToString();
        }
        internal override void f_setLevel(int lvl)
        {
            m_level = lvl + 1;
        }
        public override decimal GetValueNumber => m_number;
        /// <summary>
        /// 对比两实例值是否相等
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if(obj is JsonNumber jn)
            {
                return m_number == jn.m_number;
            }
            return false;
        }
        /// <summary>
        /// 返回此实例的哈希代码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return m_number.GetHashCode();
        }
        /// <summary>
        /// 将值以字符串的形式返回
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return m_number.ToString();
        }
        #endregion
        /// <summary>
        /// 数字隐式转换
        /// </summary>
        /// <param name="m"></param>
        public static implicit operator JsonNumber(decimal m)
        {
            return new JsonNumber(m);
        }

    }
    /// <summary>
    /// 表示Json中的布尔真值元素
    /// </summary>
    public sealed class JsonBoolean : JsonValue
    {
        /// <summary>
        /// 布尔元素表示真的json格式字符串
        /// </summary>
        public const string True = "true";
        /// <summary>
        /// 布尔元素表示假的json格式字符串
        /// </summary>
        public const string False = "false";
        /// <summary>
        /// 实例化布尔类型元素，默认为false
        /// </summary>
        public JsonBoolean()
        {
            m_value = false;
        }
        /// <summary>
        /// 实例化布尔类型元素
        /// </summary>
        /// <param name="value">布尔值</param>
        public JsonBoolean(bool value)
        {
            m_value = value;
        }
        private bool m_value;
        /// <summary>
        /// 获取或设置值
        /// </summary>
        public bool Value
        {
            get => m_value;
            set => m_value = value;
        }
        public override object GetValue => m_value;
        internal override void f_setLevel(int lvl)
        {
            m_level = lvl + 1;
        }
        internal override string f_toJsonText()
        {
            return m_value ? True : False;
        }
        protected override void f_initJsonParameter()
        {
            base.f_initJsonParameter();
            m_jsonType = JsonValueType.Boolean;
        }
        public override bool GetValueBoolean => m_value;
        public override string ToString()
        {
            return m_value ? True : False;
        }
        public override bool Equals(object obj)
        {
            if(obj is JsonBoolean b)
            {
                return m_value == b.m_value;
            }
            if(obj is bool bo)
            {
                return m_value == bo;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator JsonBoolean(bool value)
        {
            return new JsonBoolean(value);
        }
    }
    /// <summary>
    /// 表示Json中的一个空对象
    /// </summary>
    public sealed class JsonValueNullable : JsonValue
    {
        internal const string NullableText = "null";
        /// <summary>
        /// 实例化一个空元素
        /// </summary>
        public JsonValueNullable()
        {          
        }
        internal override void f_setLevel(int lvl)
        {
            m_level = lvl + 1;         
        }
        internal override string f_toJsonText()
        {
            return NullableText;
        }
        protected override void f_initJsonParameter()
        {
            base.f_initJsonParameter();
            m_jsonType = JsonValueType.Null;
        }
        public override object GetValue => null;
        public override bool Null => true;
        public override string ToString()
        {
            return NullableText;
        }
        /// <summary>
        /// 这是一个空哈希代码
        /// </summary>
        /// <returns>0</returns>
        public override int GetHashCode()
        {
            return 0;
        }
        /// <summary>
        /// 指示对象是否一致
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>若对象是与当前对象相同的实例类型，返回true；若为其它实例返回false；若对象<paramref name="obj"/>是一个null则同样返回true</returns>
        public override bool Equals(object obj)
        {
            return (obj == null) ? true : (obj is JsonValueNullable ? true : false);
        }
    }

    #endregion

    #region 异常
    /// <summary>
    /// Json引发异常的类
    /// </summary>
    public class JsonException : ArgumentException
    {
        /// <summary>
        /// 实例化异常
        /// </summary>
        public JsonException() : base("Json异常")
        {
        }
        /// <summary>
        /// 实例化消息异常
        /// </summary>
        /// <param name="message">消息</param>
        public JsonException(string message) : base(message)
        {
        }
        /// <summary>
        /// 实例化异常
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="exception">导致异常的异常</param>
        public JsonException(string message, Exception exception) : base(message, exception)
        {
        }
    }

    #endregion

}
