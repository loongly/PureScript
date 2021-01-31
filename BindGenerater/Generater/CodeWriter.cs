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

        public CS(CodeWriter writer)
        {
            writers.Push(writer);
        }

        public void Dispose()
        {
            writers.Pop();
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
                int i = lines.Count - 1;
                int pd = lines.Last().Deep;
                pointer.Move(lines.AddBefore(pointer.Last(), new Line(str, pd)));
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

            Close();
        }

        public LinePointer CreateLinePoint(string name)
        {
            return new LinePointer(lines.AddLast(new Line(name, deeps)));
        }
        public void UsePointer(LinePointer pointer)
        {
            pointers.Push(pointer);
        }
        public void UnUsePointer(LinePointer pointer)
        {
            if(pointers.Peek() == pointer)
                pointers.Pop();
        }

        public void Close()
        {
            foreach(var line in lines)
            {
                writer.WriteLine(line);
            }

            pointers.Clear();
            lines.Clear();
            writer.Flush();
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
        public LinePointer(LinkedListNode<Line> node)
        {
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
