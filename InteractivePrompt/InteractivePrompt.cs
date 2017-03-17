using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cintio
{
    public static class InteractivePrompt
    {
        private static string _prompt;
        private static void ClearLine(List<char> input)
        {
            Console.SetCursorPosition(_prompt.Length, Console.CursorTop);
            Console.Write(new string(' ', input.Count + 5));
        }
        private static void RewriteLine(List<char> input, int inputPosition)
        {
            Console.SetCursorPosition(_prompt.Length, Console.CursorTop);
            Console.Write(String.Concat(input));
            Console.SetCursorPosition(inputPosition + _prompt.Length, Console.CursorTop);
        }
        private static IEnumerable<string> GetMatch(List<string> s, string input)
        {
            s.Add(input);
            for (int i = 0; i < s.Count; i = (i+1)%s.Count)
                if (Regex.IsMatch(s[i], ".*(?:" + input + ").*", RegexOptions.IgnoreCase))
                    yield return s[i];
        }
        /// <summary>
        /// Run will start an interactive prompt
        /// </summary>
        /// <param name="lambda">This func is provided for the user to handle the input.  Input is provided in both string and List&lt;char&gt;. A return response is provided as a string.</param>
        /// <param name="prompt">The prompt for the interactive shell</param>
        /// <param name="startupMsg">Startup msg to display to user</param>
        public static void Run(Func<string, List<char>, string> lambda, string prompt, string startupMsg, List<string> completionList = null)
        {
            _prompt = prompt;
            Console.WriteLine(startupMsg);
            List<List<char>> inputHistory = new List<List<char>>();
            IEnumerator<string> wordIterator = null;
            
            while (true)
            {
                string completion = null;
                List<char> input = new List<char>();
                int inputPosition = 0;
                int inputHistoryPosition = inputHistory.Count;

                ConsoleKeyInfo key, lastKey = new ConsoleKeyInfo();
                Console.Write(prompt);
                do
                {
                    key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.LeftArrow)
                    {
                        if (inputPosition > 0)
                        {
                            inputPosition--;
                            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        }
                    }
                    else if (key.Key == ConsoleKey.RightArrow)
                    {
                        if (inputPosition < input.Count)
                        {
                            inputPosition++;
                            Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                        }
                    }

                    else if (key.Key == ConsoleKey.Tab && completionList != null && completionList.Count > 0)
                    {
                        int tempPosition = inputPosition;
                        List<char> word = new List<char>();
                        while (tempPosition-- > 0 && !string.IsNullOrWhiteSpace(input[tempPosition].ToString()))
                            word.Insert(0, input[tempPosition]);

                        if (lastKey.Key == ConsoleKey.Tab)
                        {
                            wordIterator.MoveNext();
                            if (completion != null)
                            {
                                ClearLine(input);
                                for (var i = 0; i < completion.Length; i++)
                                {
                                    input.RemoveAt(--inputPosition);
                                }
                                RewriteLine(input, inputPosition);
                            }
                            else
                            {
                                ClearLine(input);
                                for (var i = 0; i < string.Concat(word).Length; i++)
                                {
                                    input.RemoveAt(--inputPosition);
                                }
                                RewriteLine(input, inputPosition);
                            }
                        }
                        else
                        {
                            ClearLine(input);
                            for (var i = 0; i < string.Concat(word).Length; i++)
                            {
                                input.RemoveAt(--inputPosition);
                            }
                            RewriteLine(input, inputPosition);
                            wordIterator = GetMatch(completionList, string.Concat(word)).GetEnumerator();
                            while (wordIterator.Current == null)
                                wordIterator.MoveNext();
                        }

                        completion = wordIterator.Current;
                        ClearLine(input);
                        foreach (var c in completion.ToCharArray())
                        {
                            input.Insert(inputPosition++, c);
                        }
                        RewriteLine(input, inputPosition);

                    }
                    else if (key.Key == ConsoleKey.Home || (key.Key == ConsoleKey.H && key.Modifiers == ConsoleModifiers.Control))
                    {
                        Console.WriteLine("crap");
                        inputPosition = 0;
                        Console.SetCursorPosition(prompt.Length, Console.CursorTop);
                    }

                    else if (key.Key == ConsoleKey.End || (key.Key == ConsoleKey.E && key.Modifiers == ConsoleModifiers.Control))
                    {
                        inputPosition = input.Count;
                        Console.SetCursorPosition(inputPosition + _prompt.Length, Console.CursorTop);
                    }

                    else if (key.Key == ConsoleKey.Delete)
                    {
                        if (inputPosition < input.Count)
                        {
                            input.RemoveAt(inputPosition);
                            ClearLine(input);
                            RewriteLine(input, inputPosition);
                        }
                    }

                    else if (key.Key == ConsoleKey.UpArrow)
                    {
                        if (inputHistoryPosition > 0)
                        {
                            inputHistoryPosition -= 1;
                            ClearLine(input);

                            // ToList() so we make a copy and don't use the reference in the list
                            input = inputHistory[inputHistoryPosition].ToList();
                            RewriteLine(input, input.Count);
                            inputPosition = input.Count;
                        }
                    }
                    else if (key.Key == ConsoleKey.DownArrow)
                    {
                        if (inputHistoryPosition < inputHistory.Count - 1)
                        {
                            inputHistoryPosition += 1;
                            ClearLine(input);

                            // ToList() so we make a copy and don't use the reference in the list
                            input = inputHistory[inputHistoryPosition].ToList();
                            RewriteLine(input, input.Count);
                            inputPosition = input.Count;
                        }
                        else
                        {
                            inputHistoryPosition = inputHistory.Count;
                            ClearLine(input);
                            Console.SetCursorPosition(prompt.Length, Console.CursorTop);
                            input = new List<char>();
                            inputPosition = 0;
                        }
                    }
                    else if (key.Key == ConsoleKey.Backspace)
                    {
                        if (inputPosition > 0)
                        {
                            inputPosition--;
                            input.RemoveAt(inputPosition);
                            ClearLine(input);
                            RewriteLine(input, inputPosition);
                        }
                    }

                    else if (key.Key == ConsoleKey.Escape)
                    {
                        if (lastKey.Key == ConsoleKey.Escape)
                            Environment.Exit(0);
                        else
                            Console.WriteLine("Press Escape again to exit.");
                    }

                    else if (key.Key != ConsoleKey.Enter)
                    {
                        input.Insert(inputPosition++, key.KeyChar);
                        RewriteLine(input, inputPosition);
                    }


                    lastKey = key;
                } while (key.Key != ConsoleKey.Enter);

                Console.WriteLine();
                Console.SetCursorPosition(prompt.Length, Console.CursorTop);


                var cmd = string.Concat(input);
                if (String.IsNullOrWhiteSpace(cmd))
                    continue;

                if (!inputHistory.Contains(input))
                    inputHistory.Add(input);

                Console.Write(lambda(cmd, input));

            }
        }
    }

}
