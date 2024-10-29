using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos;
using Cosmos.System.Graphics.Fonts;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.Drawing;
using Sys = Cosmos.System;
using System.Text;

namespace NovaOS1
{
    public class Kernel : Sys.Kernel
    {
        public static VBECanvas canvas = new VBECanvas(new Mode(1920, 1080, ColorDepth.ColorDepth32));
        [ManifestResourceStream(ResourceName = "NovaOS1.Cursor.bmp")] public static byte[] Cursor_raw;
        [ManifestResourceStream(ResourceName = "NovaOS1.Wallp.bmp")] public static byte[] Wallp_raw;
        [ManifestResourceStream(ResourceName = "NovaOS1.Arial.psf")] public static byte[] Arial;
        public static Bitmap Cursor = new Bitmap(Cursor_raw);
        public static Bitmap wallp;
        public static PCScreenFont Arial_ = PCScreenFont.LoadFont(Arial);

        private bool isStartMenuOpen = false;
        private bool isNotepadOpen = false;
        private bool isFileManagerOpen = false;
        private bool isSettingsOpen = false;
        private List<char> notepadContent = new List<char>();
        private int notepadX = 300, notepadY = 100;
        private int notepadWidth = 800, notepadHeight = 600;
        private bool isDragging = false;
        private int dragOffsetX, dragOffsetY;
        private List<string> fileManagerContent = new List<string>();
        private int fileManagerX = 200, fileManagerY = 150;
        private int fileManagerWidth = 800, fileManagerHeight = 600;

        private int settingsX = 400, settingsY = 150;
        private int settingsWidth = 600, settingsHeight = 400;
        private Color taskbarColor = Color.DarkBlue;
        private string dateFormat = "MM/dd/yy";
        private string timeFormat = "HH:mm";

        protected override void BeforeRun()
        {
            VFSManager.RegisterVFS(new CosmosVFS());
            MouseManager.ScreenWidth = 1920;
            MouseManager.ScreenHeight = 1080;
            wallp = new Bitmap(Wallp_raw);
        }

        protected override void Run()
        {
            canvas.Clear(Color.Black);
            canvas.DrawImage(wallp, 0, 0);
            DrawTaskbar();
            if (isStartMenuOpen) DrawStartMenu();

            if (MouseManager.MouseState == MouseState.Left && MouseManager.X >= 0 && MouseManager.X <= 50 && MouseManager.Y >= 0 && MouseManager.Y <= 50)
            {
                isStartMenuOpen = !isStartMenuOpen;
                MouseManager.MouseState = MouseState.None;
            }

            if (isStartMenuOpen && MouseManager.MouseState == MouseState.Left && MouseManager.X >= 10 && MouseManager.X <= 150 && MouseManager.Y >= 60 && MouseManager.Y <= 80)
            {
                isNotepadOpen = true;
                isStartMenuOpen = false;
                MouseManager.MouseState = MouseState.None;
            }

            if (isStartMenuOpen && MouseManager.MouseState == MouseState.Left && MouseManager.X >= 10 && MouseManager.X <= 150 && MouseManager.Y >= 100 && MouseManager.Y <= 120)
            {
                isFileManagerOpen = true;
                LoadFileManagerContent("0:\\");
                isStartMenuOpen = false;
                MouseManager.MouseState = MouseState.None;
            }

            if (isStartMenuOpen && MouseManager.MouseState == MouseState.Left && MouseManager.X >= 10 && MouseManager.X <= 150 && MouseManager.Y >= 140 && MouseManager.Y <= 160)
            {
                isSettingsOpen = true;
                isStartMenuOpen = false;
                MouseManager.MouseState = MouseState.None;
            }

            if (isNotepadOpen)
            {
                DrawNotepad();
                HandleKeyboardInput();
                HandleWindowDragging(ref notepadX, ref notepadY, notepadWidth, notepadHeight);
            }

            if (isFileManagerOpen)
            {
                DrawFileManager();
                HandleWindowDragging(ref fileManagerX, ref fileManagerY, fileManagerWidth, fileManagerHeight);
            }

            if (isSettingsOpen)
            {
                DrawSettings();
                HandleWindowDragging(ref settingsX, ref settingsY, settingsWidth, settingsHeight);
            }

            DrawDateTime();
            canvas.DrawImageAlpha(Cursor, (int)MouseManager.X, (int)MouseManager.Y);
            canvas.Display();
        }

        private void DrawTaskbar()
        {
            canvas.DrawFilledRectangle(taskbarColor, 0, 0, 1920, 50);
            canvas.DrawFilledRectangle(Color.Blue, 0, 0, 50, 50);
        }

        private void DrawStartMenu()
        {
            canvas.DrawFilledRectangle(Color.Blue, 0, 50, 200, 300);
            canvas.DrawString("Notepad", Arial_, Color.White, 10, 60);
            canvas.DrawString("File Manager", Arial_, Color.White, 10, 100);
            canvas.DrawString("Settings", Arial_, Color.White, 10, 140);
        }

        private void DrawNotepad()
        {
            canvas.DrawFilledRectangle(Color.Blue, notepadX, notepadY, notepadWidth, 30);
            canvas.DrawString("Notepad", Arial_, Color.White, notepadX + 10, notepadY + 5);
            canvas.DrawFilledRectangle(Color.Red, notepadX + notepadWidth - 30, notepadY, 30, 30);
            canvas.DrawString("X", Arial_, Color.White, notepadX + notepadWidth - 20, notepadY + 5);

            canvas.DrawFilledRectangle(Color.Gray, notepadX, notepadY + 30, notepadWidth, notepadHeight - 30);

            int lineHeight = 20;
            int maxCharsPerLine = (notepadWidth - 20) / 10;
            int cursorX = notepadX + 10;
            int cursorY = notepadY + 40;
            int charsInLine = 0;

            foreach (char c in notepadContent)
            {
                canvas.DrawString(c.ToString(), Arial_, Color.Black, cursorX, cursorY);
                cursorX += 10;
                charsInLine++;

                if (charsInLine >= maxCharsPerLine)
                {
                    cursorX = notepadX + 10;
                    cursorY += lineHeight;
                    charsInLine = 0;
                }
            }

            if (MouseManager.MouseState == MouseState.Left && MouseManager.X >= notepadX + notepadWidth - 30 &&
                MouseManager.X <= notepadX + notepadWidth && MouseManager.Y >= notepadY &&
                MouseManager.Y <= notepadY + 30)
            {
                isNotepadOpen = false;
                MouseManager.MouseState = MouseState.None;
                notepadContent.Clear();
            }
        }

        private void DrawFileManager()
        {
            canvas.DrawFilledRectangle(Color.DarkBlue, fileManagerX, fileManagerY, fileManagerWidth, 30);
            canvas.DrawString("File Manager", Arial_, Color.White, fileManagerX + 10, fileManagerY + 5);
            canvas.DrawFilledRectangle(Color.Red, fileManagerX + fileManagerWidth - 30, fileManagerY, 30, 30);
            canvas.DrawString("X", Arial_, Color.White, fileManagerX + fileManagerWidth - 20, fileManagerY + 5);

            canvas.DrawFilledRectangle(Color.LightGray, fileManagerX, fileManagerY + 30, fileManagerWidth, fileManagerHeight - 30);

            int contentY = fileManagerY + 40;
            foreach (string entry in fileManagerContent)
            {
                canvas.DrawString(entry, Arial_, Color.Black, fileManagerX + 10, contentY);
                contentY += 20;
            }

            if (MouseManager.MouseState == MouseState.Left && MouseManager.X >= fileManagerX + fileManagerWidth - 30 &&
                MouseManager.X <= fileManagerX + fileManagerWidth && MouseManager.Y >= fileManagerY &&
                MouseManager.Y <= fileManagerY + 30)
            {
                isFileManagerOpen = false;
                MouseManager.MouseState = MouseState.None;
                fileManagerContent.Clear();
            }
        }

        private void DrawSettings()
        {
            canvas.DrawFilledRectangle(Color.DarkBlue, settingsX, settingsY, settingsWidth, 30);
            canvas.DrawString("Settings", Arial_, Color.White, settingsX + 10, settingsY + 5);
            canvas.DrawFilledRectangle(Color.Red, settingsX + settingsWidth - 30, settingsY, 30, 30);
            canvas.DrawString("X", Arial_, Color.White, settingsX + settingsWidth - 20, settingsY + 5);

            canvas.DrawFilledRectangle(Color.LightGray, settingsX, settingsY + 30, settingsWidth, settingsHeight - 30);

            canvas.DrawString("Taskbar Color: ", Arial_, Color.Black, settingsX + 10, settingsY + 40);
            canvas.DrawString("Date Format: ", Arial_, Color.Black, settingsX + 10, settingsY + 80);
            canvas.DrawString("Time Format: ", Arial_, Color.Black, settingsX + 10, settingsY + 120);

            if (MouseManager.MouseState == MouseState.Left && MouseManager.X >= settingsX + settingsWidth - 30 &&
                MouseManager.X <= settingsX + settingsWidth && MouseManager.Y >= settingsY &&
                MouseManager.Y <= settingsY + 30)
            {
                isSettingsOpen = false;
                MouseManager.MouseState = MouseState.None;
            }
        }

        private void LoadFileManagerContent(string path)
        {
            fileManagerContent.Clear();
            foreach (var directory in VFSManager.GetDirectoryListing(path))
            {
                fileManagerContent.Add(directory.mName);
            }
        }

        private void HandleKeyboardInput()
        {
            if (KeyboardManager.TryReadKey(out var key))
            {
                if (key.Key == ConsoleKeyEx.Backspace && notepadContent.Count > 0)
                {
                    notepadContent.RemoveAt(notepadContent.Count - 1);
                }
                else if (key.KeyChar != '\0')
                {
                    notepadContent.Add(key.KeyChar);
                }
            }
        }

        private void HandleWindowDragging(ref int windowX, ref int windowY, int windowWidth, int windowHeight)
        {
            if (MouseManager.MouseState == MouseState.Left && !isDragging && MouseManager.X >= windowX && MouseManager.X <= windowX + windowWidth &&
                MouseManager.Y >= windowY && MouseManager.Y <= windowY + 30)
            {
                isDragging = true;
                dragOffsetX = (int)MouseManager.X - windowX;
                dragOffsetY = (int)MouseManager.Y - windowY;
            }
            else if (isDragging)
            {
                windowX = (int)MouseManager.X - dragOffsetX;
                windowY = (int)MouseManager.Y - dragOffsetY;
                if (windowX < 0) windowX = 0;
                if (windowY < 50) windowY = 50;
                if (windowX + windowWidth > 1920) windowX = 1920 - windowWidth;
                if (windowY + windowHeight > 1080) windowY = 1080 - windowHeight;
            }
            else
            {
                isDragging = false;
            }
        }

        private void DrawDateTime()
        {
            DateTime currentTime = DateTime.Now;
            canvas.DrawString(currentTime.ToString(dateFormat), Arial_, Color.White, 1750, 10);
            canvas.DrawString(currentTime.ToString(timeFormat), Arial_, Color.White, 1750, 30);
        }
    }
}
