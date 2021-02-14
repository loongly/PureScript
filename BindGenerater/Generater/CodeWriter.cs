using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generater
{
    /// <summary>
    /// code source
    /// </summary>
    public class CS : IDisposable
    {
        private static Stack<CodeWriter> writers = new Stack<CodeWriter>();
        public static CodeWriter Writer { get { return writers.Peek(); } }
        private bool AutoFlush = false;
        public CS(CodeWriter writer, bool autoFlush = true)
        {
            writers.Push(writer);
            AutoFlush = autoFlush;
        }

        public void Dispose()
        {
            var writer = writers.Pop();
            if (AutoFlush)
                writer.Flush();
        }
    }

    /// <summary>
    /// code block
    /// </summary>
    public class LP : IDisposable
    {
        private static Stack<LinePointer> writers = new Stack<LinePointer>();

        CodeWriter writer;
        LinePointer pointer;
        public LP(LinePointer _pointer)
        {
            writer = CS.Writer;
            pointer = _pointer;
            writer.UsePointer(pointer);
        }

        public void Dispose()
        {
            writer.UnUsePointer(pointer);
        }
    }


    public class CodeWriter
    {
        const string start = "{";
        const string end = "}";
        LinkedList<Line> lines = new LinkedList<Line>();

        private LinePointer pointer { get { return pointers.Peek(); } }

        private TextWriter writer;
        private int deeps = 0;

        private Stack<LinePointer> pointers = new Stack<LinePointer>();
        private Dictionary<string, LinePointer> pointerDic = new Dictionary<string, LinePointer>();

        public CodeWriter(TextWriter _writer)
        {
            writer = _writer;
            UsePointer(CreateLinePoint("// auto gengerated !"));
        }

        public void Write(string str)
        {
            if (lines.Count == 0)
                WriteLine(str,false);
            else
                pointer.Last().Value.Write(str);
        }

        public void WritePrevious(string str)
        {
            if (lines.Count == 0)
                WriteLine(str, false);
            else
                pointer.Last().Value.WritePrevious(str);
        }

        public void WriteLine(string str, bool endCode = true)
        {
            if (endCode)
                str = str + ";";

            pointer.Move(lines.AddAfter(pointer.Last(),new Line(str,deeps)));
        }

        public void WritePreviousLine(string str, bool endCode = true)
        {
            if (endCode)
                str = str + ";";

            if (lines.Count == 0)
                WriteLine(str, false);
            else
            {
                var lastLine = pointer.Last();
                if(lastLine.Value.Code.Equals(start))
                    WriteLine(str, false);
                else
                {
                    int i = lines.Count - 1;
                    int pd = lines.Last().Deep;
                    //pointer.Move(lines.AddBefore(pointer.Last(), new Line(str, pd)));
                    lines.AddBefore(pointer.Last(), new Line(str, pd));
                }
            }
                
        }

        public void WriteHead(string str, bool endCode = true)
        {
            if (endCode)
                str = str + ";";

            if (lines.Count == 0)
                WriteLine(str, false);
            else
                lines.AddFirst(new Line(str, 0));
        }

        public void Start(string str = null)
        {
            if (str != null)
                WriteLine(str,false);

                WriteLine(start,false);
            deeps++;
        }

        public void End(bool newLine = true)
        {
            deeps--;
            WriteLine(end,false);
        }

        public void EndAll(bool newLine = true)
        {
            int count = deeps;
            for(int i= 0;i< count; i++)
                End(newLine);

            Flush();
        }

        public LinePointer CreateLinePoint(string name,bool previous = false)
        {
            var line = new Line("", deeps);
            LinkedListNode<Line> node;
            if (previous)
                node = lines.AddBefore(pointer.Last(), line);
            else if (pointers.Count > 0)
                node = lines.AddAfter(pointer.Last(), line);
            else
                node = lines.AddLast(line);

            if (pointers.Count > 0)
                pointer.Move(node);

            var newPointer = new LinePointer(name, node);
            pointerDic[name] = newPointer;
            return newPointer;
        }

        public LinePointer GetLinePoint(string name)
        {
            if (pointerDic.TryGetValue(name, out var pointer))
                return pointer;
            return null;
        }

        public void UsePointer(LinePointer pointer)
        {
            if (pointer == null)
                return;
            pointers.Push(pointer);
        }
        public void UnUsePointer(LinePointer pointer)
        {
            if (pointer == null)
                return;

            if (pointers.Peek() == pointer)
                pointers.Pop();
        }

        public void Flush()
        {
            foreach(var line in lines)
            {
                writer.WriteLine(line);
            }

            lines.Clear();
            writer.Flush();

            pointers.Clear();
            pointerDic.Clear();
            UsePointer(CreateLinePoint("// -- flush --"));
        }


        StringBuilder previewSB = new StringBuilder();
        public string Preview()
        {
            previewSB.Clear();
            foreach (var line in lines)
            {
                previewSB.Append(line);
            }
            return previewSB.ToString();
        }


       
    }

    public class Line
    {
        public string Code { get; private set; }
        public int Deep { get; private set; }

        public Line(string str, int deep)
        {
            Code = str;
            Deep = deep;
        }
        public void Write(string str)
        {
            Code += str;
        }

        public void WritePrevious(string str)
        {
            Code = str + Code;
        }


        public override string ToString()
        {
            string speace = "";
            for (int i = 0; i < Deep; i++)
                speace += "\t";

            return speace + Code;
        }
    }

    public class LinePointer
    {
        private LinkedListNode<Line> pointer;
        public string Name { get; private set; }
        public LinePointer(string name, LinkedListNode<Line> node)
        {
            Name = name;
            pointer = node;
        }

        public void Move(LinkedListNode<Line> node)
        {
            pointer = node;
        }

        public LinkedListNode<Line> Last()
        {
            return pointer;
        }

        public LinkedListNode<Line> Previous()
        {
            return pointer.Previous;
        }
    }
}
