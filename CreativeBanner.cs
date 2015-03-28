using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game;

namespace FirstSEMod
{
    class CreativeBanner
    {
        IMyGridTerminalSystem GridTerminalSystem;

        // #### SETTINGS #### //
        public static int LOG_GRID_PER_STEP = 1;
        public static int step = 0;
        void Main()
        {
            var _grp = new List<IMyBlockGroup>();
            SearchGroupsOfName("lcd banner info", _grp);

            if (_grp.Count == 0 || _grp[0].Blocks.Count == 0)
                return;

            List<IMyTerminalBlock> _banners = _grp[0].Blocks;

            var _blks = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName("[LOG]", _blks);
            IMyTerminalBlock _logblock = null;
            if (_blks.Count != 0)
            {
                _logblock = _blks[0];
            }

            //Log(_logblock, "TEST");

            for (int i = 0; i < LOG_GRID_PER_STEP; i++)
            {
                var _banner = _banners[(step * LOG_GRID_PER_STEP + i) % _banners.Count];
                Log(_banner, "clear");

                Log(_banner,
                "This is the creative world; be creative and do not mess\n\r" +
                "with stations or ships of other players.\n\r" +
                "If you want to destroy something, just make it\n\r" +
                "and then destroy it; no weapons enabled though.\n\r" +
                "I ban griefer so be aware, you might have a server\n\r" +
                "less in your server list.\n\r\n\r" +
                "If you have something to say or just recommend\n\r" +
                "some modifications please visit the forum\n\rdiscoworld.emawind.com.");

                // increment step for logging on text panel
                if (_banner is IMyTextPanel)
                    MMLCDTextManager.UpdatePanel(_banner as IMyTextPanel);
            }

            // IMPORTANT!!!!
            step++;

        }

        void Log(IMyTerminalBlock log, string content, bool append = true)
        {
            if (log is IMyTextPanel)
            {
                if (content.Equals("clear"))
                    MMLCDTextManager.ClearText(log as IMyTextPanel);
                else
                {
                    if (!append)
                        MMLCDTextManager.ClearText(log as IMyTextPanel);
                    MMLCDTextManager.AddLine(log as IMyTextPanel, content);
                }
                //((IMyTextPanel)block).WritePublicText(content, append);
                //((IMyTextPanel)block).ShowTextureOnScreen();
                //((IMyTextPanel)block).ShowPublicTextOnScreen();
            }
            else
            {
                if (content.Equals("clear"))
                    log.SetCustomName("");
                else if (append)
                    log.SetCustomName(log.CustomName + content + "\n");
                else
                    log.SetCustomName(content + "\n");
            }
        }

        void SearchGroupsOfName(string name, List<IMyBlockGroup> groups)
        {
            if (groups == null) return;
            // using String.Empty crash the server    
            //if( name == null || name == String.Empty ) return;      
            if (name == null || name == "") return;

            List<IMyBlockGroup> allGroups = GridTerminalSystem.BlockGroups;

            for (int i = 0; i < allGroups.Count; i++)
            {
                if (allGroups[i].Name.Contains(name))
                {
                    groups.Add(allGroups[i]);
                }
            }
        }
    }

    /// <summary>
    /// API by MMaster
    /// </summary>
    public static class MMLCDTextManager
    {
        private static Dictionary<IMyTextPanel, MMLCDText> panelTexts = new Dictionary<IMyTextPanel, MMLCDText>();

        public static MMLCDText GetLCDText(IMyTextPanel panel)
        {
            MMLCDText lcdText = null;

            if (!panelTexts.TryGetValue(panel, out lcdText))
            {
                lcdText = new MMLCDText();
                panelTexts.Add(panel, lcdText);
            }

            return lcdText;
        }

        public static void AddLine(IMyTextPanel panel, string line)
        {
            MMLCDText lcd = GetLCDText(panel);
            lcd.AddLine(line);
        }

        public static void Add(IMyTextPanel panel, string text)
        {
            MMLCDText lcd = GetLCDText(panel);

            lcd.AddFast(text);
            lcd.current_width += MMStringFunc.GetStringSize(text);
        }

        public static void AddRightAlign(IMyTextPanel panel, string text, float end_screen_x)
        {
            MMLCDText lcd = GetLCDText(panel);

            float text_width = MMStringFunc.GetStringSize(text);
            end_screen_x -= lcd.current_width;


            if (end_screen_x < text_width)
            {
                lcd.AddFast(text);
                lcd.current_width += text_width;
                return;
            }

            end_screen_x -= text_width;
            int fillchars = (int)Math.Round(end_screen_x / MMStringFunc.WHITESPACE_WIDTH, MidpointRounding.AwayFromZero);
            float fill_width = fillchars * MMStringFunc.WHITESPACE_WIDTH;

            string filler = new String(' ', fillchars);
            lcd.AddFast(filler + text);
            lcd.current_width += fill_width + text_width;

        }

        public static void AddCenter(IMyTextPanel panel, string text, float screen_x)
        {
            MMLCDText lcd = GetLCDText(panel);

            float text_width = MMStringFunc.GetStringSize(text);
            screen_x -= lcd.current_width;

            if (screen_x < text_width / 2)
            {
                lcd.AddFast(text);
                lcd.current_width += text_width;
                return;
            }

            screen_x -= text_width / 2;
            int fillchars = (int)Math.Round(screen_x / MMStringFunc.WHITESPACE_WIDTH, MidpointRounding.AwayFromZero);
            float fill_width = fillchars * MMStringFunc.WHITESPACE_WIDTH;

            string filler = new String(' ', fillchars);
            lcd.AddFast(filler + text);
            lcd.current_width += fill_width + text_width;
        }

        public static void AddProgressBar(IMyTextPanel panel, double percent, int width = 22)
        {
            MMLCDText lcd = GetLCDText(panel);
            int totalBars = width - 2;
            int fill = (int)(percent * totalBars) / 100;
            if (fill > totalBars)
                fill = totalBars;
            string progress = "[" + new String('|', fill) + new String('\'', totalBars - fill) + "]";

            lcd.AddFast(progress);
            lcd.current_width += MMStringFunc.PROGRESSCHAR_WIDTH * width;
        }

        public static void ClearText(IMyTextPanel panel)
        {
            GetLCDText(panel).ClearText();
        }

        public static void UpdatePanel(IMyTextPanel panel)
        {
            MMLCDText lcd = GetLCDText(panel);
            panel.WritePublicText(lcd.GetDisplayString());
            panel.ShowTextureOnScreen();
            panel.ShowPublicTextOnScreen();
            //lcd.ScrollNextLine();
        }

        public class MMLCDText
        {
            public int scrollPosition = 0;
            public int scrollDirection = 1;
            public const int DisplayLines = 22; // 22 for font size 0.8 

            public List<string> lines = new List<string>();
            public int current_line = 0;
            public float current_width = 0;

            public void CheckCurLine()
            {
                if (current_line >= lines.Count)
                    lines.Add("");
            }

            public void AddFast(string text)
            {
                CheckCurLine();
                lines[current_line] += text;
            }

            public void AddLine(string line)
            {
                AddFast(line);
                current_line++;
                current_width = 0;
            }

            public void ClearText()
            {
                lines.Clear();
                current_width = 0;
                current_line = 0;
            }

            public string GetFullString()
            {
                return String.Join("\n", lines);
            }

            // Display only 22 lines from scrollPos 
            public string GetDisplayString()
            {
                if (lines.Count < DisplayLines)
                {
                    scrollPosition = 0;
                    scrollDirection = 1;
                    return GetFullString();
                }

                List<string> display =
                    lines.GetRange(scrollPosition,
                        Math.Min(lines.Count - scrollPosition, DisplayLines));

                return String.Join("\n", display);
            }

            //public void ScrollNextLine()
            //{
            //    int lines_cnt = lines.Count;
            //    if (lines_cnt < DisplayLines)
            //    {
            //        scrollPosition = 0;
            //        scrollDirection = 1;
            //        return;
            //    }

            //    if (scrollDirection > 0)
            //    {
            //        if (scrollPosition + LCDsProgram.SCROLL_LINES + DisplayLines > lines_cnt)
            //        {
            //            scrollDirection = -1;
            //            scrollPosition = lines_cnt - DisplayLines;
            //            return;
            //        }

            //        scrollPosition += LCDsProgram.SCROLL_LINES;
            //    }
            //    else
            //    {
            //        if (scrollPosition - LCDsProgram.SCROLL_LINES < 0)
            //        {
            //            scrollPosition = 0;
            //            scrollDirection = 1;
            //            return;
            //        }

            //        scrollPosition -= LCDsProgram.SCROLL_LINES;
            //    }
            //}
        }
    }

    public static class MMStringFunc
    {
        private static Dictionary<char, float> charSize = new Dictionary<char, float>();

        public const float WHITESPACE_WIDTH = 8f;
        public const float PROGRESSCHAR_WIDTH = 6f;

        public static void InitCharSizes()
        {
            if (charSize.Count > 0)
                return;

            AddCharsSize("3FKTabdeghknopqsuy", 17f);
            AddCharsSize("#0245689CXZ", 19f);
            AddCharsSize("$&GHPUVY", 20f);
            AddCharsSize("ABDNOQRS", 21f);
            AddCharsSize("(),.1:;[]ft{}", 9f);
            AddCharsSize("+<=>E^~", 18f);
            AddCharsSize(" !I`ijl", 8f);
            AddCharsSize("7?Jcz", 16f);
            AddCharsSize("L_vx", 15f);
            AddCharsSize("\"-r", 10f);
            AddCharsSize("mw", 27f);
            AddCharsSize("M", 26f);
            AddCharsSize("W", 31f);
            AddCharsSize("'|", 6f);
            AddCharsSize("*", 11f);
            AddCharsSize("\\", 12f);
            AddCharsSize("/", 14f);
            AddCharsSize("%", 24f);
            AddCharsSize("@", 25f);
            AddCharsSize("\n", 0f);
        }

        private static void AddCharsSize(string chars, float size)
        {
            for (int i = 0; i < chars.Length; i++)
                charSize.Add(chars[i], size);
        }

        public static float GetCharSize(char c)
        {
            float width = 17f;
            charSize.TryGetValue(c, out width);

            return width;
        }

        public static float GetStringSize(string str)
        {
            float sum = 0;
            for (int i = 0; i < str.Length; i++)
                sum += GetCharSize(str[i]);

            return sum;
        }

        public static string GetStringTrimmed(string text, float pixel_width)
        {
            int trimlen = Math.Min((int)pixel_width / 14, text.Length - 2);
            float stringSize = GetStringSize(text);
            if (stringSize <= pixel_width)
                return text;

            while (stringSize > pixel_width - 20)
            {
                text = text.Substring(0, trimlen);
                stringSize = GetStringSize(text);
                trimlen -= 2;
            }
            return text + "..";
        }

    }
}
